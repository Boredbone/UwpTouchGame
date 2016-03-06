using System.Linq;
using System.Reactive.Linq;
using Boredbone.XamlTools;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UwpTouchGame.Resources;
using Windows.UI.Xaml;
using Windows.UI;
using UwpTouchGame.Models;
using Windows.UI.Xaml.Input;
using Boredbone.XamlTools.Extensions;

namespace UwpTouchGame.ViewModels
{


    public class MarkerViewModel : DisposableBase
    {
        public ReadOnlyReactiveProperty<Color> Color { get; }
        public ReadOnlyReactiveProperty<Thickness> Margin { get; }
        public ReadOnlyReactiveProperty<bool> IsEnabled { get; }

        public double Width { get; }
        public double Height { get; }

        public double X => this.Margin.Value.Left;
        public double Y => this.Margin.Value.Bottom;

        public ReactiveCommand DownCommand { get; }
        public ReactiveCommand HoldCommand { get; }

        public MarkerContainer Source { get; private set; }


        public MarkerViewModel(MarkerContainer source, MarkerStyleItem resource,
            double xScale, double yScale, double widthScale, double heightScale)
        {
            this.Source = source;

            // marker size
            this.Width = resource.Width * widthScale;
            this.Height = resource.Height * heightScale;

            // color
            this.Color = source.IsHandled
                .Select(x => !x ? resource.PrimaryColor : resource.SecondaryColor)
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);

            // position
            this.Margin = source.X
                .CombineLatest(source.Y, (x, y) => new Thickness(x * xScale, 0, 0, y * yScale))
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);

            this.IsEnabled = this.Source.IsHandled.Select(x => !x).ToReadOnlyReactiveProperty().AddTo(this.Disposables);

            this.DownCommand = new ReactiveCommand().WithSubscribe(e => this.Source.Down(), this.Disposables);

            this.HoldCommand = new ReactiveCommand().WithSubscribe(e =>
            {
                if (((PointerRoutedEventArgs)e).Pointer.IsInContact)
                {
                    this.Source.Hold();
                }
            }, this.Disposables);

        }

        public void Reset()
        {
            this.Source.Reset();
        }
    }
}
