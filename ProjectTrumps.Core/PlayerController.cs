using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    internal class PlayerController
    {
        public int PreviousAttribute { get; set; }
        public PlayerActionParams PlayerActionParams { get; set; }

        public void InitialiseActions()
        {
            var prevSelectedAttribute = PlayerActionParams?.PreviousAttribute ?? -1;

            PlayerActionParams = new PlayerActionParams
            {
                PreviousAttribute = prevSelectedAttribute,
                SelectedAttribute = -1,
                Retreat = false,
                ChangeCard= false,
                ConductBattle = false,
            };
        }
    }

    public class PlayerActionParams
    {
        public int PreviousAttribute { get; set; }
        public int SelectedAttribute { get; set; }
        public bool Retreat { get; set; }
        public bool ConductBattle { get; set; }
        public bool ChangeCard { get; set; }
    }
}
