using System.Linq;
using System.Reactive.Linq;
using Boredbone.XamlTools;
using Reactive.Bindings.Extensions;
using UwpTouchGame.Resources;

namespace UwpTouchGame.Models
{
    /// <summary>
    /// Main Model
    /// </summary>
    public class CoreModel : DisposableBase
    {
        public MarkerManager Markers { get; }
        public ScoreProvider Score { get; }

        public CoreModel()
        {
            this.Markers = new MarkerManager(GameViewDefinition.HitEventBufferSize).AddTo(this.Disposables);
            this.Score = new ScoreProvider(this.Markers).AddTo(this.Disposables);
            ScoreDefinition.Set(this.Score);
        }
    }
}
