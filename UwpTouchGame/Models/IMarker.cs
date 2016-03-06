using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpTouchGame.Models
{
    public interface IMarker
    {
        MarkerType Type { get; }
        HitType HitType { get; }
        bool IsHandlable { get; }
        bool IsOneShot { get; }
        void Reset();
    }
}
