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
                .DistinctUntilChanged()
                .Where(x => x)
                //.Take(1)
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
    }
}
