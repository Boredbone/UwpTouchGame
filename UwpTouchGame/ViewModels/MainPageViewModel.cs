using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Boredbone.XamlTools;
using Boredbone.Utility.Extensions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UwpTouchGame.Resources;
using Windows.UI.Xaml;
using Windows.UI;
using UwpTouchGame.Models;
using System.Diagnostics;
using Windows.UI.Xaml.Input;
using Boredbone.XamlTools.Extensions;

namespace UwpTouchGame.ViewModels
{
    public class MainPageViewModel : NotificationBase
    {
        private const double xScale = 8.0;
        private const double yScale = 2.0;
        private const double widthScale = 1;
        private const double heightScale = 1;

        private const double refreshInterval = 50;

        private const double scrollSpeed = 5.0;

        public ObservableCollection<MarkerViewModel> Markers { get; }

        public ReadOnlyReactiveProperty<int> Score { get; }
        public ReactiveProperty<string> Text { get; }
        public ReadOnlyReactiveProperty<Thickness> DisplayPosition { get; }
        public ReactiveProperty<double> TotalHeight { get; }
        public ReadOnlyReactiveProperty<Visibility> ViewVisibility { get; }

        private ReactiveProperty<double> ViewHeight { get; }


        public MainPageViewModel()
        {
            var core = ((App)Application.Current).Core;

            this.Markers = new ObservableCollection<MarkerViewModel>();

            this.Score = core.Score.Score.ToReadOnlyReactiveProperty().AddTo(this.Disposables);

            Disposable.Create(ClearMarkers).AddTo(this.Disposables);

            this.Text = new ReactiveProperty<string>("text").AddTo(this.Disposables);

            this.ViewHeight = new ReactiveProperty<double>(0).AddTo(this.Disposables);

            this.TotalHeight = new ReactiveProperty<double>(1).AddTo(this.Disposables);



            Observable
                .FromAsync(() => new MarkerLoader().LoadAsync(core.Markers))
                .Select(_ => core.Markers.GetAll()
                    .OrderByDescending(x => x.Y.Value)
                    .Select(y => new MarkerViewModel(y, MarkerDefinition.Style[y.Marker.Type],
                        xScale, yScale, widthScale, heightScale))
                    .ToArray())
                .ObserveOnUIDispatcher()
                .Subscribe(array =>
                {
                    this.ClearMarkers();

                    var topItem = array.FirstOrDefault();
                    if (topItem != null)
                    {
                        this.TotalHeight.Value = topItem.Y + topItem.Height * 2;
                    }
                    array.ForEach(x => this.Markers.Add(x));
                })
                .AddTo(this.Disposables);


            var ready = this.ViewHeight
                .Where(h => h > 10)
                .Zip(this.TotalHeight.Where(h => h > 10), (_, __) => true);

            var time = Observable
                .Interval(TimeSpan.FromMilliseconds(refreshInterval))
                .SkipUntil(ready)
                .Scan(0, (prev, current) => (prev * scrollSpeed > this.TotalHeight.Value + this.ViewHeight.Value) ? 0 : (prev + 1))
                .Publish();

            this.DisplayPosition = time
                .Select(t => new Thickness(0, 0, 0, -t * scrollSpeed + this.ViewHeight.Value))
                .ObserveOnUIDispatcher()
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);


            this.ViewVisibility = this.DisplayPosition
                .Skip(1)
                .Select(x => Visibility.Visible)
                .ToReadOnlyReactiveProperty(Visibility.Collapsed)
                .AddTo(this.Disposables);

            time.Pairwise()
                .Where(x => x.NewItem < x.OldItem)
                .Subscribe(_ => this.Reset())
                .AddTo(this.Disposables);

            //ready.Connect().AddTo(this.Disposables);
            time.Connect().AddTo(this.Disposables);

            //this.DisplayPosition = Observable
            //    .Interval(TimeSpan.FromMilliseconds(refreshInterval))
            //    .SkipUntil(ready)
            //    .Scan(0, (prev, current) => (prev * scrollSpeed > this.TotalHeight.Value + this.ViewHeight.Value) ? 0 : (prev + 1))
            //    .Select(t => new Thickness(0, 0, 0, -t * scrollSpeed + this.ViewHeight.Value))
            //    .ObserveOnUIDispatcher()
            //    .ToReadOnlyReactiveProperty()
            //    .AddTo(this.Disposables);

        }

        private void ClearMarkers()
        {
            this.Markers.ForEach(x => x.Dispose());
            this.Markers.Clear();
        }

        private void Reset()
        {
            this.Markers.ForEach(x => x.Reset());
        }

        public void OnViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ViewHeight.Value = e.NewSize.Height;
        }
    }


    public class MarkerViewModel : DisposableBase
    {
        public ReadOnlyReactiveProperty<Color> Color { get; }
        public ReadOnlyReactiveProperty<Thickness> Margin { get; }

        public double Width { get; }
        public double Height { get; }

        public double X => this.Margin.Value.Left;
        public double Y => this.Margin.Value.Bottom;

        public ReactiveCommand DownCommand { get; }
        public ReactiveCommand HoldCommand { get; }

        public ReadOnlyReactiveProperty<bool> IsEnabled { get; }


        private MarkerContainer Source { get; }

        public MarkerViewModel(MarkerContainer source, MarkerStyleItem resource,
            double xScale, double yScale, double widthScale, double heightScale)
        {
            this.Source = source;

            this.Width = resource.Width * widthScale;
            this.Height = resource.Height * heightScale;


            this.Color = source.IsHandled
                .Select(x => !x ? resource.PrimaryColor : resource.SecondaryColor)
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);

            this.Margin = source.X
                .CombineLatest(source.Y, (x, y) => new Thickness(x * xScale, 0, 0, y * yScale))
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);

            this.IsEnabled = this.Source.IsHandled.Select(x => !x).ToReadOnlyReactiveProperty().AddTo(this.Disposables);

            this.DownCommand = new ReactiveCommand().WithSubscribe(e => this.Down((PointerRoutedEventArgs)e), this.Disposables);
            this.HoldCommand = new ReactiveCommand().WithSubscribe(e => this.Hold((PointerRoutedEventArgs)e), this.Disposables);

        }

        public void Down(PointerRoutedEventArgs e)
        {
            if (this.Source.Marker.HitType.HasFlag(HitType.Down))
            {
                this.Source.Handle();
            }
        }

        public void Hold(PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && this.Source.Marker.HitType.HasFlag(HitType.Hold))
            {
                this.Source.Handle();
            }
        }

        public void Reset()
        {
            this.Source.Reset();
        }
    }
}
