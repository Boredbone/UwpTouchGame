using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpTouchGame.Models;

namespace UwpTouchGame.Resources
{
    class ScoreDefinition
    {
        public static void Set(ScoreProvider target)
        {
            target.SmallMarkerScore = 10;
            target.BigMarkerScore = 5;
            target.LineNodeScore = 3;
            target.LineHoldScore = 1;
            target.LineCompleteScore = 20;
        }
    }
}
