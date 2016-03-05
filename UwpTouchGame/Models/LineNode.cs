using System;
using System.Linq;
using System.Reactive.Linq;

namespace UwpTouchGame.Models
{

    public class LineNode : ILine
    {
        public MarkerType Type => MarkerType.Node;
        public HitType HitType
            => (this.Parent?.IsCaptured ?? false) ? (HitType.Down | HitType.Hold) : HitType.Down;
        public LineNode Parent { get; }

        public bool IsHandlable => true;
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
}
