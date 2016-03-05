using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boredbone.Utility.Extensions;
using UwpTouchGame.Resources;

namespace UwpTouchGame.Models
{

    public class MarkerLoader
    {

        private enum MarkerSettingType
        {
            Small = 0,
            Big = 1,
            LineStart = 2,
            Node = 3,
        }

        private struct MarkerSetting
        {
            public MarkerSettingType Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        private async Task<IEnumerable<MarkerSetting>> LoadDummyAsync()
        {
            var data = new List<int[]>()
            {
                new[] {0,10,0},
                new[] {0,20,300},
                new[] {1,40,150},
                new[] {1,10,250},
                new[] {1,10,500},
                new[] {2,20,150},
                new[] {3,40,300},
                new[] {3,20,500},
                new[] {2,50,200},
                new[] {3,50,400},
            };

            await Task.Delay(100).ConfigureAwait(false);

            return data.Select(item => new MarkerSetting() { Type = (MarkerSettingType)item[0], X = item[1], Y = item[2] });
        }


        public async Task LoadAsync(MarkerManager manager)
        {
            var data = await this.LoadDummyAsync().ConfigureAwait(false);


            var list = new List<MarkerContainer>();

            MarkerContainer prev = null;

            foreach (var item in data)
            {
                IMarker marker = null;
                switch (item.Type)
                {
                    case MarkerSettingType.Small:
                        marker = new NormalMarker(false);
                        break;
                    case MarkerSettingType.Big:
                        marker = new NormalMarker(true);
                        break;
                    case MarkerSettingType.LineStart:
                        marker = new LineNode(null, MarkerDefinition.LineLivingTime);
                        break;
                    case MarkerSettingType.Node:
                        marker = new LineNode((LineNode)prev.Marker, MarkerDefinition.LineLivingTime);
                        break;
                    default:
                        continue;
                }

                var container = new MarkerContainer(marker, item.X, item.Y);

                if (prev != null && item.Type == MarkerSettingType.Node)
                {
                    var width = container.X.Value - prev.X.Value;
                    var height = container.Y.Value - prev.Y.Value;

                    var length = Math.Abs(height / MarkerDefinition.LineResolution);

                    Enumerable.Range(1, (int)length - 1)
                        .Select(i => new MarkerContainer(new Line((LineNode)prev.Marker),
                            prev.X.Value + width * i / length,
                            prev.Y.Value + height * i / length))
                        .ForEach(c => list.Add(c));

                }

                prev = (container.Marker.Type == MarkerType.Node) ? container : null;

                list.Add(container);

            }

            manager.Clear();
            list.ForEach(x => manager.Add(x));
        }
    }

}
