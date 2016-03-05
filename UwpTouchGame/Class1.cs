using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Boredbone.XamlTools;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UwpTouchGame.Models.Helpers;
using UwpTouchGame.Resources;

namespace UwpTouchGame
{
    public class LineTrace : DisposableBase
    {
        private Subject<LineNode> LineCompletedSubject { get; }
        public IObservable<LineNode> LineCompleted => this.LineCompletedSubject.AsObservable();


        public LineTrace(IObservable<MarkerContainer> hitObservable)
        {

            var hitMarker = hitObservable.Select(x => x.Marker);

            this.LineCompletedSubject = new Subject<LineNode>().AddTo(this.Disposables);

            // Line Hit
            var lineObservable = hitMarker
                .Where(x => x.Type == MarkerType.Node || x.Type == MarkerType.Line)
                .Select(x => (ILine)x);

            // Node Hit
            hitMarker
                .Where(x => x.Type == MarkerType.Node)
                .Select(x => (LineNode)x)
                .Do(x => x.Capture(lineObservable))
                .Where(x => x.Parent?.IsCaptured ?? false)
                .Subscribe(this.LineCompletedSubject)
                .AddTo(this.Disposables);
        }

    }

    public class ScoreProvider : DisposableBase
    {
        public int SmallMarkerScore { get; set; }
        public int BigMarkerScore { get; set; }
        public int LineNodeScore { get; set; }
        public int LineHoldScore { get; set; }
        public int LineCompleteScore { get; set; }

        public ReactiveProperty<int> Score { get; }


        public ScoreProvider(IObservable<MarkerContainer> hitObservable)
        {

            var lineTrace = new LineTrace(hitObservable).AddTo(this.Disposables);

            this.Score = hitObservable
                .Select(x => this.GetScore(x.Marker))
                .Merge(lineTrace.LineCompleted.Select(_ => this.LineCompleteScore))
                .Sum()
                .ToReactiveProperty()
                .AddTo(this.Disposables);
        }

        private int GetScore(IMarker marker)
        {
            switch (marker.Type)
            {
                case MarkerType.Node:
                    return this.LineNodeScore;
                case MarkerType.Line:
                    return this.LineHoldScore;
                case MarkerType.Big:
                    return this.BigMarkerScore;
                case MarkerType.Small:
                    return this.SmallMarkerScore;
                default:
                    return 0;
            }
        }
    }

    public class CoreModel : DisposableBase
    {

        public MarkerManager Markers { get; }
        public ScoreProvider Score { get; }

        public CoreModel()
        {
            this.Markers = new MarkerManager(8).AddTo(this.Disposables);
            this.Score = new ScoreProvider(this.Markers.Hit.Select(x => x.Target)).AddTo(this.Disposables);
            ScoreDefinition.Set(this.Score);
        }

    }
    /*
    public interface ICollisionManager : IDisposable
    {
        IObservable<HitEventArgs> Hit { get; }
    }

    public class CollisionManager1 : DisposableBase, ICollisionManager
    {


        private Subject<HitEventArgs> HitSubject { get; }
        public IObservable<HitEventArgs> Hit => this.HitSubject.AsObservable();

        private HitEventArgs[] Pool { get; }
        private int poolIndex;


        public CollisionManager1(int poolSize)
        {
            this.HitSubject = new Subject<HitEventArgs>().AddTo(this.Disposables);

            this.Pool = Enumerable.Range(0, poolSize).Select(_ => new HitEventArgs()).ToArray();
            this.poolIndex = 0;
        }


        public void CheckCollision()
        {
            var arg = this.Pool[this.poolIndex / this.Pool.Length];
            this.poolIndex++;
            this.HitSubject.OnNext(arg);
        }
    }*/

    public class MarkerManager : DisposableBase
    {
        private HashSet<MarkerContainer> Markers { get; }


        private Subject<HitEventArgs> HitSubject { get; }
        public IObservable<HitEventArgs> Hit => this.HitSubject.AsObservable();

        private HitEventArgs[] Pool { get; }
        private int poolIndex;

        private CompositeDisposable markerUnsubscribers;


        public MarkerManager(int eventPoolSize)
        {
            this.HitSubject = new Subject<HitEventArgs>().AddTo(this.Disposables);

            this.Pool = Enumerable.Range(0, eventPoolSize).Select(_ => new HitEventArgs()).ToArray();
            this.poolIndex = 0;

            this.Markers = new HashSet<MarkerContainer>();

            this.markerUnsubscribers = new CompositeDisposable();
            Disposable.Create(this.Clear).AddTo(this.Disposables);

        }

        public void Add(MarkerContainer item)
        {
            item.IsHandled
                .Where(x => x)
                .Take(1)
                .Subscribe(_ =>
                {
                    var arg = this.Pool[this.poolIndex / this.Pool.Length];
                    this.poolIndex++;

                    arg.Target = item;
                    this.HitSubject.OnNext(arg);
                })
                .AddTo(this.markerUnsubscribers);

            this.Markers.Add(item);
        }

        public void Clear()
        {
            this.markerUnsubscribers.Clear();
            this.Markers.Clear();
        }

        public IEnumerable<MarkerContainer> GetAll() => this.Markers.AsEnumerable();
    }



    public class HitEventArgs
    {
        public MarkerContainer Target { get; set; }
    }



    public interface IMarker
    {
        MarkerType Type { get; }
        HitType HitType { get; }
        bool IsHandlable { get; }
    }

    public enum MarkerType
    {
        Node,
        Line,
        Big,
        Small,
    }

    public enum HitType
    {
        Down,
        Hold,
    }

    public class MarkerContainer : DisposableBase
    {
        public ReactiveProperty<double> X { get; }
        public ReactiveProperty<double> Y { get; }

        private Subject<bool> IsHandledSubject { get; }
        public ReadOnlyReactiveProperty<bool> IsHandled { get; }

        public IMarker Marker { get; }




        public MarkerContainer(IMarker source, double x, double y)
        {

            this.Marker = source;

            this.X = new ReactiveProperty<double>(x).AddTo(this.Disposables);
            this.Y = new ReactiveProperty<double>(y).AddTo(this.Disposables);

            this.IsHandledSubject = new Subject<bool>().AddTo(this.Disposables);
            this.IsHandled = this.IsHandledSubject.ToReadOnlyReactiveProperty(false).AddTo(this.Disposables);
        }



        public void Handle()
        {
            if (this.Marker.IsHandlable && !this.IsHandled.Value)
            {
                this.IsHandledSubject.OnNext(true);
            }
        }

    }


    public interface ILine : IMarker
    {
        LineNode Parent { get; }
    }

    public class LineNode : ILine
    {
        public MarkerType Type => MarkerType.Node;
        public HitType HitType => HitType.Hold;
        public LineNode Parent { get; }

        public bool IsHandlable => true;

        public bool IsCaptured { get; private set; }

        private bool isCapturable;
        private double livingTime;


        public LineNode(LineNode parent, double livingTime)
        {
            this.isCapturable = true;
            this.livingTime = livingTime;
            this.Parent = parent;
        }

        public void Capture(IObservable<ILine> observableForContinueCapturing)
        {
            if (this.isCapturable)
            {
                this.isCapturable = false;
                this.IsCaptured = true;

                observableForContinueCapturing
                    .Where(x => x.Parent == this)
                    .Throttle(TimeSpan.FromMilliseconds(this.livingTime))
                    .Take(1)
                    .Subscribe(x => this.IsCaptured = false);
            }
        }

    }

    public class Line : ILine
    {
        public MarkerType Type => MarkerType.Line;
        public HitType HitType => (this.Parent == null) ? HitType.Down : HitType.Hold;
        public LineNode Parent { get; }

        public bool IsHandlable => this.Parent?.IsCaptured ?? false;

        public Line(LineNode parent)
        {
            this.Parent = parent;
        }

    }

    public class NormalMarker : IMarker
    {
        public HitType HitType => HitType.Down;
        public bool IsHandlable => true;
        public MarkerType Type { get; }

        public NormalMarker(bool isBig)
        {
            this.Type = isBig ? MarkerType.Big : MarkerType.Small;
        }
    }

}
