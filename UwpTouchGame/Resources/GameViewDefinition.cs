using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpTouchGame.Resources
{
    class GameViewDefinition
    {
        public static double XScale { get; } = 14.0;
        public static double YScale { get; } = 2.0;
        public static double WidthScale { get; } = 1;
        public static double HeightScale { get; } = 1;

        public static double RefreshInterval { get; } = 50;

        public static double ScrollSpeed { get; } = 5.0;

        public static int HitEventBufferSize { get; } = 8;
    }
}
