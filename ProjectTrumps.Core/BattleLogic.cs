using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class SoloBattleLogic : BattleLogic
    {       
        public static void EvaluateBattle(DataCard card1, DataCard card2, int attributeIndex, int damageLimitPerLevel, out BattleLog log)
        {
            log = new BattleLog();
            log.OnlyLogResult = true;

            var card1Attribute = card1.CurrentAttributes[attributeIndex];
            var card2Attribute = card2.CurrentAttributes[attributeIndex];

            var card1AttributeStat = (card1Attribute.AttributeValue * 10) + new Random().Next(-5, 6);
            var card2AttributeStat = (card2Attribute.AttributeValue * 10) + new Random().Next(-5, 6);

            log.AddMessage(true, $"Selected Attribute: {card1Attribute.AttributeName}");

            log.AddMessage(false, $"{card1.DisplayName} - {card1Attribute.AttributeName} - {card1Attribute.AttributeValue}");
            log.AddMessage(false, $"{card2.DisplayName} - {card2Attribute.AttributeName} - {card2Attribute.AttributeValue}");

            log.AddMessage(false, System.Environment.NewLine);
            log.AddMessage(false, $"Calculated Attribute stats: {card1Attribute.AttributeName}");

            log.AddMessage(false, $"{card1.DisplayName} - {card1Attribute.AttributeName} - {card1AttributeStat}");
            log.AddMessage(false, $"{card2.DisplayName} - {card2Attribute.AttributeName} - {card2AttributeStat}");

            // Set Advantage
            // Red > Blue
            // Blue > Green
            // Green > Red

            if (card1Attribute.AttributeType != card2Attribute.AttributeType)
            {
                log.AddMessage(false, System.Environment.NewLine);
                log.AddMessage(false, $"{card1.DisplayName} - type - {card1.Type.ToString()}");
                log.AddMessage(false, $"{card2.DisplayName} - type - {card2.Type.ToString()}");

                bool card1Advantage = (card1Attribute.AttributeType == ColourType.Red && card2Attribute.AttributeType == ColourType.Blue)
                              || (card1Attribute.AttributeType == ColourType.Blue && card2Attribute.AttributeType == ColourType.Green)
                              || (card1Attribute.AttributeType == ColourType.Green && card2Attribute.AttributeType == ColourType.Red);

                bool card2Advantage = (card2Attribute.AttributeType == ColourType.Red && card1Attribute.AttributeType == ColourType.Blue)
                              || (card2Attribute.AttributeType == ColourType.Blue && card1Attribute.AttributeType == ColourType.Green)
                              || (card2Attribute.AttributeType == ColourType.Green && card1Attribute.AttributeType == ColourType.Red);

                if (card1Advantage)
                {
                    log.AddMessage(false, $"{card1.DisplayName} - Has battle advantage - {card1.Type.ToString()} >>> {card2.Type.ToString()} ");
                    card1AttributeStat += 10;
                    card2AttributeStat -= 10;
                }
                else if (card2Advantage)
                {
                    log.AddMessage(false, $"{card2.DisplayName} - Has battle advantage - {card2.Type.ToString()} >>> {card1.Type.ToString()}");
                    card2AttributeStat += 10;
                    card1AttributeStat -= 10;
                }
            }

            var damage = 0;
            bool damageCard1 = false;
            bool noDamage = false;

            // Card 1 Wins
            if (card1AttributeStat > card2AttributeStat)
            {
                damageCard1 = false;
                damage = card1AttributeStat - card2AttributeStat;

                if (damage > card1.Level * damageLimitPerLevel)
                    damage = card1.Level * damageLimitPerLevel;

                log.AddMessage(true, $"Damage to deal - {damage}");
            }
            // Card 2 Wins
            else if (card1AttributeStat < card2AttributeStat)
            {
                damageCard1 = true;
                damage = card2AttributeStat - card1AttributeStat;

                if (damage > card2.Level * damageLimitPerLevel)
                    damage = card2.Level * damageLimitPerLevel;

                log.AddMessage(true, $"Damage to deal - {damage}");
            }
            // No win
            else
            {
                noDamage = true;
                log.AddMessage(false, $"No damage to deal");
            }

            // Comduct battle
            if (!noDamage)
            {
                log.AddMessage(false, System.Environment.NewLine);
                if (damageCard1)
                {
                    ConductBattle(card1, card2, log, damage);
                }
                else
                {
                    ConductBattle(card2, card1, log, damage);
                }
            }

            var skipHeal = (card1.Health <= 0) || (card2.Health <= 0);

            if (!skipHeal)
            {
                ConductPostHeal(card1, log);
                ConductPostHeal(card2, log);                
            }
        }
   
        public static void ConductBattle(DataCard losingCard, DataCard winningCard, BattleLog log, int damage)
        {
            damage = ConductEvasion(losingCard, damage, log);

            if (damage == 0)
            {
                ConductParry(losingCard, winningCard, log);
            }
            else
            {
                ConductStandardDamage(losingCard, log, damage);
            }
            
            ConductAdditionalBurn(losingCard, winningCard, log);
        }

        private static void ConductAdditionalBurn(DataCard losingCard, DataCard winningCard, BattleLog log)
        {
            if (winningCard.Type == ColourType.Red)
            {
                var count = winningCard.CurrentAttributes.Where(p => p.AttributeType == ColourType.Red).Count();
                var additionalDamage = count * 3;

                var healthBefore = winningCard.Health;
                losingCard.Health -= additionalDamage;
                log.AddMessage(true, $"{losingCard.DisplayName} TOOK ADDITIONAL BURN DAMAGE");
                log.AddMessage(true, $"{losingCard.DisplayName} Health: {healthBefore} >>>> {losingCard.Health}");
            }
        }

        private static void ConductStandardDamage(DataCard losingCard, BattleLog log, int damage)
        {
            var healthBefore = losingCard.Health;
            losingCard.Health -= damage;
            log.AddMessage(true, $"{losingCard.DisplayName} TOOK DAMAGE: {healthBefore} >>>> {losingCard.Health}");
        }

        private static void ConductParry(DataCard losingCard, DataCard winningCard, BattleLog log)
        {
            var count = losingCard.CurrentAttributes.Where(p => p.AttributeType == ColourType.Blue).Count();
            var threshold = 100 - (count * 8);
            var parry = new Random().Next(0, 101) > threshold;

            if (parry)
            {
                var healthBefore = winningCard.Health;
                var reverseDamage = 15 + count;
                winningCard.Health -= reverseDamage;
                log.AddMessage(true, $"{losingCard.DisplayName} PARRIED for {reverseDamage}!");
                log.AddMessage(true, $"{winningCard.DisplayName} TOOK DAMAGE: {healthBefore} >>>> {winningCard.Health}");
            }
        }

        private static int ConductEvasion(DataCard losingCard, int damage, BattleLog log)
        {
            if (losingCard.Type == ColourType.Blue)
            {
                var count = losingCard.CurrentAttributes.Where(p => p.AttributeType == ColourType.Blue).Count();
                var threshold = 100 - (count * 8);
                var evade = new Random().Next(0, 101) > threshold;
                damage = evade ? 0 : damage;
                
                if (evade)
                    log.AddMessage(true, $"{losingCard.DisplayName} evaded!");
            }
            return damage;
        }

        private static void ConductPostHeal(DataCard card1, BattleLog log)
        {
            if (card1.Type == ColourType.Green)
            {
                var count = card1.CurrentAttributes.Where(p => p.AttributeType == ColourType.Green).Count();
                var healAmount = count * 3;
                var healthBefore = card1.Health;
                card1.Health += healAmount;

                // Handle Overheal
                // Check if healed above maxhealth
                // if so reduce by amount and if reduced below max health, assign max 
                if (card1.Health > card1.MaxHealth)
                    card1.Health = card1.Health - count < card1.MaxHealth ? card1.MaxHealth : card1.Health - count;

                card1.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count), 1);
                log.AddMessage(true, $"{card1.DisplayName} healed {healthBefore} >>>> {card1.Health}");
            }
        }

        public static void ReportHP(List<DataCard> cards, BattleLog log)
        {
            log.AddMessage(true, System.Environment.NewLine);

            foreach (var card in cards)
            {
                var segmentPercValue = 1;
                var currentHealth = card.Health;
                var maxHealth = card.MaxHealth;
                var lostHealth = maxHealth - currentHealth;

                var percentage = Math.Round(((double)currentHealth / (double)maxHealth) * 100);
                var percRemaining = 100 - percentage;

                var hpBarStart = "[";
                var segmentCount = 0;

                var hpBar = "";

                while (percentage >= segmentPercValue)
                {
                    hpBar += "=";
                    percentage -= segmentPercValue;
                }

                while (percRemaining >= segmentPercValue)
                {
                    hpBar += " ";
                    percRemaining -= segmentPercValue;
                }

                var hpBarEnd= "]";
                var finalBar = hpBarStart + hpBar + hpBarEnd;

                log.AddMessage(true, $"{card.DisplayName} Health: {card.Health}");
                log.AddMessage(true, $"{finalBar}");            
            }


        }

        public static void ModifyAttribute(DataCard card, int attributeIndex, int modifier)
        {
            card.CurrentAttributes[attributeIndex].AttributeValue += modifier;
        }
    }

    public class BattleLog
    {
        public IList<string> Messages { get; set; } = new List<string>();
        public bool OnlyLogResult { get; set; }

        public void AddMessage(bool isResult, string message)
        {
            if (OnlyLogResult && !isResult)
                return;

            Messages.Add(message); 
        }        

        public void DisplayConsoleMessages()
        {
            foreach (var msg in Messages)
            {
                Console.WriteLine(msg);
            }

        }
    }

    internal interface BattleLogic
    {        
    }

    public enum Difficulty
    {
        Easy = 1,
        Normal = 2,
        Hard = 3,
        VertHard = 4,
    }
}
