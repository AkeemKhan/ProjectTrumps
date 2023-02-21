using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class SoloBattleLogic : IBattleLogic
    {       
        public static void EvaluateBattle(DataCard card1, DataCard card2, int attributeIndex, int damageLimit, out BattleLog log)
        {
            if (damageLimit == 0)
                damageLimit = 100;

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

                if (damage > damageLimit)
                    damage = damageLimit;

                log.AddMessage(true, $"Damage to deal - {damage}");
            }
            // Card 2 Wins
            else if (card1AttributeStat < card2AttributeStat)
            {
                damageCard1 = true;
                damage = card2AttributeStat - card1AttributeStat;

                if (damage > damageLimit)
                    damage = damageLimit;

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
                    if (card1.Type == ColourType.Blue)
                    {
                        var count = card1.CurrentAttributes.Where(p => p.AttributeType == ColourType.Blue).Count();
                        var threshold = 100 - (count * 8);
                        var evade = new Random().Next(0, 101) > threshold;
                        damage = evade ? 0 : damage;
                    }

                    if (damage == 0)
                    {
                        log.AddMessage(true, $"{card1.DisplayName} evaded!");

                        var count = card1.CurrentAttributes.Where(p => p.AttributeType == ColourType.Blue).Count();
                        var threshold = 100 - (count * 8);
                        var parry = new Random().Next(0, 101) > threshold;

                        if (parry)
                        {
                            var healthBefore = card2.Health;
                            var reverseDamage = 15 + count;
                            card2.Health -= reverseDamage;
                            log.AddMessage(true, $"{card1.DisplayName} parried for {reverseDamage}!");
                            log.AddMessage(true, $"{card2.DisplayName} TOOK DAMAGE: {healthBefore} >>>> {card2.Health}");
                        }
                    }
                    else
                    {
                        var healthBefore = card1.Health;
                        card1.Health -= damage;                        
                        log.AddMessage(true, $"{card1.DisplayName} TOOK DAMAGE: {healthBefore} >>>> {card1.Health}");
                    }

                    // Additional burn
                    if (card2.Type == ColourType.Red)
                    {
                        var count = card2.CurrentAttributes.Where(p => p.AttributeType == ColourType.Red).Count();
                        var additionalDamage = count * 3;

                        var healthBefore = card2.Health;
                        card1.Health -= additionalDamage;
                        log.AddMessage(true, $"{card1.DisplayName} suffers additional burn damage");
                        log.AddMessage(true, $"{card1.DisplayName} Health: {healthBefore} >>>> {card1.Health}");
                    }
                }
                else
                {
                    if (card2.Type == ColourType.Blue)
                    {
                        var count = card2.CurrentAttributes.Where(p => p.AttributeType == ColourType.Blue).Count();
                        var threshold = 100 - (count * 10);
                        var evade = new Random().Next(0, 101) > threshold;
                        damage = evade ? 0 : damage;
                    }

                    if (damage == 0)
                    {
                        log.AddMessage(true, $"{card2.DisplayName} evaded!");

                        var count = card1.CurrentAttributes.Where(p => p.AttributeType == ColourType.Blue).Count();
                        var threshold = 100 - (count * 8);
                        var parry = new Random().Next(0, 101) > threshold;

                        if (parry)
                        {
                            var healthBefore = card2.Health;
                            var reverseDamage = 15 + count;
                            card1.Health -= reverseDamage;
                            log.AddMessage(true, $"{card2.DisplayName} parried for {reverseDamage}!");
                            log.AddMessage(true, $"{card1.DisplayName} TOOK DAMAGE: {healthBefore} >>>> {card1.Health}");
                        }
                    }
                    else
                    {
                        var healthBefore = card2.Health;
                        card2.Health -= damage;
                        log.AddMessage(true, $"{card2.DisplayName} TOOK DAMAGE: {healthBefore} >>>> {card2.Health}");
                    }

                    // Additional burn
                    if (card1.Type == ColourType.Red)
                    {
                        var count = card1.CurrentAttributes.Where(p => p.AttributeType == ColourType.Red).Count();
                        var additionalDamage = count * 3;

                        var healthBefore = card2.Health;
                        card2.Health -= additionalDamage;
                        log.AddMessage(true, $"{card2.DisplayName} suffers additional burn damage");
                        log.AddMessage(true, $"{card2.DisplayName} TOOK DAMAGE: {healthBefore} >>>> {card2.Health}");
                    }
                }
            }

            var skipHeal = false;

            if (card1.Health <= 0)
            {
                log.AddMessage(true, $"{card1.DisplayName} has been defeated - Health: {card1.Health}");
                skipHeal = true;
            }
            if (card2.Health <= 0)
            {
                log.AddMessage(true, $"{card2.DisplayName} has been defeated - Health: {card2.Health}");
                skipHeal = true;
            }

            if (!skipHeal)
            {
                // Post heal
                if (card1.Type == ColourType.Green)
                {
                    var count = card1.CurrentAttributes.Where(p => p.AttributeType == ColourType.Green).Count();
                    var healAmount = count * 3;
                    var healthBefore = card1.Health;
                    card1.Health += healAmount;
                    log.AddMessage(true, $"{card1.DisplayName} healed {healthBefore} >>>> {card1.Health}");
                }
                if (card2.Type == ColourType.Green)
                {
                    var count = card2.CurrentAttributes.Where(p => p.AttributeType == ColourType.Green).Count();
                    var healAmount = count;
                    var healthBefore = card2.Health;
                    card2.Health += healAmount;
                    log.AddMessage(true, $"{card2.DisplayName} healed {healthBefore} >>>> {card2.Health}");
                }
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

    internal interface IBattleLogic
    {        
    }
}
