using System;
using System.Linq;
using System.Reactive.Linq;
using Boredbone.XamlTools;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace UwpTouchGame.Models
{

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
                .Scan((p, c) => p + c)
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
}
