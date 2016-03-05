using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpTouchGame.Models
{
    public class NormalMarker : IMarker
    {
        public HitType HitType => HitType.Down;
        public bool IsHandlable => true;
        public MarkerType Type { get; }

        public NormalMarker(bool isBig)
        {
            this.Type = isBig ? MarkerType.Big : MarkerType.Small;
        }

        public void Reset()
        {
        }
    }
}
