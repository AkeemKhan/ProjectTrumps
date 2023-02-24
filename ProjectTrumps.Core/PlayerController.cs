using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class PlayerController
    {
        public DataCard MainCard { get => PlayerInventory.MainCard; set => PlayerInventory.MainCard = value; }
        public PlayerInventory PlayerInventory { get; set; } = new PlayerInventory();
        public int PreviousAttribute { get; set; }
        public PlayerActionParams PlayerActionParams { get; set; } = new PlayerActionParams();

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

        public void EvaluatePostTurn(bool playerTurn)
        {
            MainCard.EvaluateEnhance(playerTurn);
        }
    }

    public class PlayerInventory : IInventory
    {
        public PlayerInventory()
        {
            InsightType = 1;
            InsightStats = 1;
            Fusion = 1;
            Tribute = 1;
            Heal = 1;
            Replenish= 1;
        }

        public DataCard MainCard { get; set;}
        public DataDeck Deck { get; set; } = new DataDeck();


        public int InsightType { get; set; }
        public int InsightStats { get; set; }        
        public int Fusion { get; set; }
        public int Tribute { get; set; }
        public int Heal { get; set; }
        public int Replenish { get; set; }
    }

    public class PlayerActionParams
    {
        public int PreviousAttribute { get; set; }
        public int SelectedAttribute { get; set; }

        public bool InsightStats { get; set; }
        public bool InsightType { get; set; }
        public bool Retreat { get; set; }
        public bool ConductBattle { get; set; }
        public bool ChangeCard { get; set; }        
        public bool Fuse { get; set; }
        public bool Tribute { get; set; }
        public bool Heal { get; set; }
        public bool Replenish { get; set; }

        public void InitialiseAtStartOfTurn()
        {
            Retreat = false;
            ConductBattle= false;
            ChangeCard = false;
            Fuse = false;
            Tribute = false;
            Heal = false;
            Replenish = false;
        }
    }

    public enum TurnAction
    {
        ConductBattle,
        ChangeCard,
        Retreat,
        Fuse,
        Tribute
    }

    public interface IInventory
    {
        DataDeck Deck { get; set; }
    }
}
