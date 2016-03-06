using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpTouchGame.Models
{
    /// <summary>
    /// Traceable Line
    /// </summary>
    public class Line : ILine
    {
        public MarkerType Type => MarkerType.Line;
        public HitType HitType => HitType.Hold;
        public LineNode Parent { get; }

        public bool IsHandlable => this.Parent?.IsCaptured ?? false;
        public bool IsOneShot => false;

        public Line(LineNode parent)
        {
            this.Parent = parent;
        }

        public void Reset()
        {
        }
    }
}
