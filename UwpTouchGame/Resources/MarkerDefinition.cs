using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpTouchGame.Models;
using Windows.UI;

namespace UwpTouchGame.Resources
{
    class MarkerDefinition
    {
        public static Dictionary<MarkerType, MarkerStyleItem> Style
            = new Dictionary<MarkerType, MarkerStyleItem>()
            {
                [MarkerType.Small] = new MarkerStyleItem(Colors.Blue, Colors.Cyan, 50, 50),
                [MarkerType.Big] = new MarkerStyleItem(Colors.Red, Colors.Magenta, 100, 100),
                [MarkerType.Line] = new MarkerStyleItem(Colors.Yellow, Colors.Orange, 40, 80),
                [MarkerType.Node] = new MarkerStyleItem(Colors.Green, Colors.GreenYellow, 100, 100),
            };


        public static double LineLivingTime { get; } = 1000;
        public static double LineResolution { get; } = 10;
    }

    public class MarkerStyleItem
    {
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public MarkerStyleItem(Color color1, Color color2, double width, double height)
        {
            this.PrimaryColor = color1;
            this.SecondaryColor = color2;
            this.Width = width;
            this.Height = height;
        }
    }
}
