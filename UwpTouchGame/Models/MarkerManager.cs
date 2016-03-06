using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Boredbone.XamlTools;
using Reactive.Bindings.Extensions;

namespace UwpTouchGame.Models
{

    public class MarkerManager : DisposableBase
    {
        private HashSet<MarkerContainer> Markers { get; }

        private Subject<HitEventArgs> HitSubject { get; }

        public IObservable<MarkerContainer> Hit { get; }
        public IObservable<LineNode> LineCompleted { get; }

        private HitEventArgs[] Pool { get; }
        private int poolIndex;

        private CompositeDisposable markerUnsubscribers;


        public MarkerManager(int eventPoolSize)
        {
            this.HitSubject = new Subject<HitEventArgs>().AddTo(this.Disposables);
            this.Hit = this.HitSubject.Select(x => x.Target).Publish().RefCount();

            this.Pool = Enumerable.Range(0, eventPoolSize).Select(_ => new HitEventArgs()).ToArray();
            this.poolIndex = 0;

            this.Markers = new HashSet<MarkerContainer>();

            this.markerUnsubscribers = new CompositeDisposable();
            Disposable.Create(this.Clear).AddTo(this.Disposables);
            
            var lineTrace = new LineTrace(this.Hit).AddTo(this.Disposables);
            this.LineCompleted = lineTrace.LineCompleted;
        }

        public void Add(MarkerContainer item)
        {
            item.IsHandled
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ =>
                {
                    var arg = this.Pool[this.poolIndex % this.Pool.Length];
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


        /// <summary>
        /// Provide a completion event of line trace 
        /// </summary>
        private class LineTrace : DisposableBase
        {
            private Subject<LineNode> LineCompletedSubject { get; }
            public IObservable<LineNode> LineCompleted => this.LineCompletedSubject.AsObservable();


            public LineTrace(IObservable<MarkerContainer> hitObservable)
            {
                this.LineCompletedSubject = new Subject<LineNode>().AddTo(this.Disposables);

                var hitMarker = hitObservable
                    .Select(x => x.Marker)
                    .Publish()
                    .RefCount();

                // Line Hit
                var lineObservable = hitMarker
                    .Where(x => x.Type == MarkerType.Node || x.Type == MarkerType.Line)
                    .Select(x => (ILine)x)
                    .Publish()
                    .RefCount();

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
    }
}
