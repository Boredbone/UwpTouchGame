using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace UwpTouchGame.Models
{
    /// <summary>
    /// Node of line
    /// </summary>
    public class LineNode : ILine
    {
        public MarkerType Type => MarkerType.Node;
        public HitType HitType
            => (this.Parent?.IsCaptured ?? false) ? (HitType.Down | HitType.Hold) : HitType.Down;
        public LineNode Parent { get; }

        public bool IsHandlable => true;
        public bool IsOneShot => true;
        public bool IsCaptured { get; private set; }

        private bool isCapturable;
        private double livingTime;


        public LineNode(LineNode parent, double livingTime)
        {
            this.livingTime = livingTime;
            this.Parent = parent;
            this.Reset();
        }

        public void Reset()
        {
            this.isCapturable = true;
            this.IsCaptured = false;
        }

        /// <summary>
        /// start line tracing
        /// </summary>
        /// <param name="observableForContinueCapturing"></param>
        public void Capture(IObservable<ILine> observableForContinueCapturing)
        {
            if (this.isCapturable)
            {
                this.isCapturable = false;
                this.IsCaptured = true;

                // cancel tracing
                observableForContinueCapturing
                    .Where(x => x.Parent == this)
                    .Select(_ => Unit.Default)
                    .Merge(Observable.Return(Unit.Default))
                    .Throttle(TimeSpan.FromMilliseconds(this.livingTime))
                    .Take(1)
                    .Subscribe(_ => this.IsCaptured = false);
            }
        }

    }
}
