using System.Reactive.Subjects;
using Boredbone.XamlTools;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace UwpTouchGame.Models
{
    /// <summary>
    /// Marker status
    /// </summary>
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

        public void Reset()
        {
            this.Marker.Reset();
            this.IsHandledSubject.OnNext(false);
        }

        public void Handle()
        {
            if (this.Marker.IsHandlable && !this.IsHandled.Value)
            {
                this.IsHandledSubject.OnNext(true);
            }
        }
    }
}
