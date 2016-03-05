using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Boredbone.XamlTools;
using Boredbone.Utility.Extensions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UwpTouchGame.Resources;
using Windows.UI.Xaml;
using UwpTouchGame.Models;
using Boredbone.XamlTools.Extensions;

namespace UwpTouchGame.ViewModels
{
    public class MainPageViewModel : NotificationBase
    {

        public ObservableCollection<MarkerViewModel> Markers { get; }

        public ReadOnlyReactiveProperty<int> Score { get; }
        public ReactiveProperty<int> Count { get; }
        public ReadOnlyReactiveProperty<Thickness> DisplayPosition { get; }
        public ReactiveProperty<double> TotalHeight { get; }
        public ReadOnlyReactiveProperty<Visibility> ViewVisibility { get; }

        private ReactiveProperty<double> ViewHeight { get; }


        public MainPageViewModel()
        {
            var core = ((App)Application.Current).Core;

            this.Markers = new ObservableCollection<MarkerViewModel>();

            this.Score = core.Score.Score.ToReadOnlyReactiveProperty().AddTo(this.Disposables);

            this.ViewHeight = new ReactiveProperty<double>(0).AddTo(this.Disposables);
            this.TotalHeight = new ReactiveProperty<double>(1).AddTo(this.Disposables);

            Disposable.Create(ClearMarkers).AddTo(this.Disposables);


            // start game after load markers
            Observable
                .FromAsync(() => new MarkerLoader().LoadAsync(core.Markers))
                .Select(_ => core.Markers.GetAll()
                    .OrderByDescending(x => x.Y.Value)
                    .Select(y => new MarkerViewModel(y, MarkerDefinition.Style[y.Marker.Type],
                        GameViewDefinition.XScale, GameViewDefinition.YScale,
                        GameViewDefinition.WidthScale, GameViewDefinition.HeightScale))
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

            // initialized event
            var ready = this.ViewHeight
                .Where(h => h > 10)
                .Zip(this.TotalHeight.Where(h => h > 10), (_, __) => true);

            // game timer
            var time = Observable
                .Interval(TimeSpan.FromMilliseconds(GameViewDefinition.RefreshInterval))
                .SkipUntil(ready)
                .Scan(0, (prev, current) =>
                    (prev * GameViewDefinition.ScrollSpeed > this.TotalHeight.Value + this.ViewHeight.Value) 
                    ? 0 : (prev + 1))
                .Publish();

            // scroll
            this.DisplayPosition = time
                .Select(t => new Thickness(0, 0, 0, -t * GameViewDefinition.ScrollSpeed + this.ViewHeight.Value))
                .ObserveOnUIDispatcher()
                .ToReadOnlyReactiveProperty()
                .AddTo(this.Disposables);

            // Hide view before initializing
            this.ViewVisibility = this.DisplayPosition
                .Skip(1)
                .Select(x => Visibility.Visible)
                .ToReadOnlyReactiveProperty(Visibility.Collapsed)
                .AddTo(this.Disposables);

            // Loop
            time.Pairwise()
                .Where(x => x.NewItem < x.OldItem)
                .Subscribe(_ => this.Reset())
                .AddTo(this.Disposables);

            // count
            this.Count = time.Select(_ => this.Markers
                     .Where(x => x.Source.Marker.Type == MarkerType.Node)
                     .Count(x => ((LineNode)x.Source.Marker).IsCaptured))
                .ToReactiveProperty()
                .AddTo(this.Disposables);
            
            // start timer
            time.Connect().AddTo(this.Disposables);
            
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
}
