using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class MatchController
    {
        public BattleLog BattleLog { get; set; } = new BattleLog();
        public int DamageLimitPerLevel { get; set; }
        public int UseCost { get; set; }
        public bool Inspect { get; set; }
    }
}
