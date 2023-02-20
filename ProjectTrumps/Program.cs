using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using ProjectTrumps.Core;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var deck = new List<DataCard>();
            var path = @"C:\\Users\\AKEEM\\Documents\\TrumpsGen.csv";
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var card = CardFactory.Instance.CreateCard(line.Split(','));
                    deck.Add(card);
                }
            }

            var sCard1 = deck[new Random().Next(0, deck.Count - 1)];
            var sCard2 = deck[new Random().Next(0, deck.Count - 1)];

            var hp = 100;

            var card1 = new DataCard() { OriginalName = sCard1.OriginalName, Attributes = CardFactory.Instance.CopyAttributes(sCard1.Attributes), Health = hp, MaxHealth = hp };
            var card2 = new DataCard() { OriginalName = sCard2.OriginalName, Attributes = CardFactory.Instance.CopyAttributes(sCard2.Attributes), Health = hp+20, MaxHealth = hp+20 };
        
            var playerTurn = true;
            var prevAttr = -1;

            var useModifier = -2;

            var damageLimit = 100;
            var aiDamageChangeThreshold = 20;
            var aiMoraleDamage = 0;
            var aiChangeAtMoraleBreak = 6;
            var aiTakenDamageCounter = 0;
            var aiChangeAtNTimesDamageTaken = 10;
            var aiChangeNext = false;

            Console.WriteLine("Select Difficulty:");
            Console.WriteLine();

            Console.WriteLine("1: Easy");
            Console.WriteLine("2: Normal");
            Console.WriteLine("3: Hard");
            Console.WriteLine("4: Very Hard");

            var aiChosenCommands = new List<int>();
            var diffInput = Console.ReadLine();
            var difficulty = Difficulty.Normal;

            if (diffInput == "1")
            {
                difficulty = Difficulty.Easy;
            }
            else if (diffInput == "2")
            {
                difficulty = Difficulty.Normal;
            }
            else if (diffInput == "3")
            {
                difficulty = Difficulty.Hard;
            }
            else if (diffInput == "4")
            {
                difficulty = Difficulty.VertHard;
            }
            else
            {
                difficulty = Difficulty.Normal;
            }

            InitialiseDifficulty(card1, card2, aiChosenCommands, difficulty);

            foreach (var item in aiChosenCommands)
            {
                Console.Write(item + ",");
            }

            Console.WriteLine();
            var c1Average = card1.CurrentPowerRating;
            var c2Average = card2.CurrentPowerRating;

            Console.WriteLine($"{card1.DisplayName} power: {c1Average}");
            Console.WriteLine($"{card2.DisplayName} power: {c2Average}");
            
            while (card1.Health > 0 && card2.Health > 0)
            {
                var playerTurnAtStart = playerTurn;

                Console.WriteLine();
                Console.WriteLine();

                var aiHealthBefore = card2.Health;

                if (playerTurn)
                {
                    Console.WriteLine("------------------------------- List of Attributes: -----------------------------------------------------------");
                    Console.WriteLine();
                    Console.WriteLine($"---- {card1.DisplayName} ----");

                    for (int i = 0; i < card1.Attributes.Count; i++)
                    {
                        if (i == prevAttr)
                        {
                            Console.WriteLine($"{(i + 1)} - {card1.Attributes[i].AttributeName} - {card1.Attributes[i].AttributeValue} - CANNOT USE SAME ATTRIBUTE TWICE IN A ROW");
                        }
                        else
                        {
                            Console.WriteLine($"{(i + 1)} - {card1.Attributes[i].AttributeName} - {card1.Attributes[i].AttributeValue} - ({card1.Attributes[i].AttributeType})");
                        }
                    }

                    if (prevAttr >= 0)
                    {
                        Console.WriteLine($"Previous used: {card1.Attributes[prevAttr].AttributeName}");
                    }
                    Console.WriteLine();

                    var hasSelected = false;
                    var selectedAttribute = 0;
                    var conductBattle = false;
                    var changeCard = false;

                    while (!hasSelected)
                    {
                        Console.WriteLine("Enter a valid value - cannot use prev:");
                        var input = Console.ReadLine();

                        if (input == "C")
                        {
                            changeCard = true;
                            conductBattle = false;
                            hasSelected = true;
                        }
                        else if (int.TryParse(input, out var res))
                        {
                            res--;

                            if (prevAttr == res)
                                continue;


                            if (res < card1.Attributes.Count)
                            {
                                hasSelected = true;
                                prevAttr = res;
                            }

                            selectedAttribute = res;
                            changeCard = false;
                            conductBattle = true;
                        }
                    }

                    if (conductBattle)
                    {
                        Console.WriteLine();
                        SoloBattleLogic.EvaluateBattle(card1, card2, selectedAttribute, damageLimit, out var log);
                        SoloBattleLogic.ModifyAttribute(card1, selectedAttribute, useModifier);

                        foreach (var msg in log.Messages)
                        {
                            Console.WriteLine(msg);
                        }

                        InitialiseDifficulty(card1, card2, aiChosenCommands, difficulty);
                    }

                    if (changeCard)
                    {
                        ChangeCard(deck, card1);
                    }

                    playerTurn = false;
                }
                else
                {
                    Console.WriteLine("CPU turn - Press to continue");
                    Console.ReadLine();

                    if (aiChangeNext)
                    {
                        Console.WriteLine("CPU Changed card - Press to continue");
                        ChangeCard(deck, card2, false);
                        aiChangeNext = false;

                        Console.ReadLine();
                    }
                    else
                    {
                        var randomAttributeSelected = aiChosenCommands[new Random().Next(0, aiChosenCommands.Count - 1)];
                        SoloBattleLogic.EvaluateBattle(card1, card2, randomAttributeSelected, damageLimit, out var log);
                        SoloBattleLogic.ModifyAttribute(card2, randomAttributeSelected, useModifier);

                        foreach (var msg in log.Messages)
                        {
                            Console.WriteLine(msg);
                        }                        
                    }


                    playerTurn = true;
                }

                var aiHealthAfter = card2.Health;
                if (aiHealthAfter < aiHealthBefore)
                {
                    aiTakenDamageCounter++;
                }

                if (aiHealthBefore - aiHealthAfter >= aiDamageChangeThreshold)
                {
                    var diff = aiHealthBefore - aiHealthAfter;

                    if (diff*2 > aiDamageChangeThreshold)
                    {
                        aiMoraleDamage += ((aiChangeAtMoraleBreak / 2) + 1);
                        Console.WriteLine($"Inflicted SIGNIFICANT damage");
                    }
                    else if (playerTurnAtStart)
                    {
                        aiMoraleDamage++;
                        Console.WriteLine($"Inflicted considerable damage");
                    }
                    else
                    {
                        aiMoraleDamage = aiMoraleDamage + 2;
                        Console.WriteLine($"Self-Inflicted considerable damage");
                    }
                }
                else
                {
                    if (aiMoraleDamage > 0)
                        aiMoraleDamage--;
                }

                if (aiMoraleDamage < aiChangeAtMoraleBreak)
                {
                    double morale = ((aiChangeAtMoraleBreak - aiMoraleDamage) / (double)aiChangeAtMoraleBreak) * 100;
                    Console.WriteLine($"Morale - {morale.ToString("0")}%");
                }
                else
                {
                    Console.WriteLine($"Opponenet has Broken - Morale - 0%");
                }

                if (aiMoraleDamage >= aiChangeAtMoraleBreak || 
                    aiTakenDamageCounter == aiChangeAtNTimesDamageTaken)
                { 
                    aiTakenDamageCounter = 0;
                    aiMoraleDamage = 0;
                    aiChangeNext = true;
                }

                Console.WriteLine("Press to continue");
                Console.ReadLine();
            }

            DisplayComparison(card1, card2);
        }

        private static void InitialiseDifficulty(DataCard card1, DataCard card2, List<int> aiChosenCommands, Difficulty difficulty)
        {
            aiChosenCommands.Clear();
            switch (difficulty)
            {
                case Difficulty.Easy:
                    InitialiseEasy(card1, aiChosenCommands);
                    break;
                case Difficulty.Normal:
                    InitialiseNormal(card1, card2, aiChosenCommands);
                    break;
                case Difficulty.Hard:
                    InitialiseHard(card1, card2, aiChosenCommands);
                    break;
                case Difficulty.VertHard:
                    IntialiseVeryHard(card1, card2, aiChosenCommands);
                    break;
                default:
                    break;
            }
        }

        private static void IntialiseVeryHard(DataCard card1, DataCard card2, List<int> aiChosenCommands)
        {
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                if (card1.Attributes[i].AttributeValue < card2.Attributes[i].AttributeValue)
                {
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                }
            }
        }

        private static void InitialiseHard(DataCard card1, DataCard card2, List<int> aiChosenCommands)
        {
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                if (card1.Attributes[i].AttributeValue < card2.Attributes[i].AttributeValue)
                {
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                }
            }
        }

        private static void InitialiseNormal(DataCard card1, DataCard card2, List<int> aiChosenCommands)
        {
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                if (card1.Attributes[i].AttributeValue < card2.Attributes[i].AttributeValue)
                {
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                }
            }
        }

        private static void InitialiseEasy(DataCard card1, List<int> aiChosenCommands)
        {
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
        }

        private static void DisplayComparison(DataCard card1, DataCard card2)
        {
            Console.WriteLine($"---- {card1.DisplayName} vs {card2.DisplayName} ----");
            var idx = 0;
            for (int i = 0; i < card1.Attributes.Count; i++)
            {
                var winnerName = card1.Attributes[i].AttributeValue == card2.Attributes[i].AttributeValue
                    ? ""
                    : card1.Attributes[i].AttributeValue > card2.Attributes[i].AttributeValue
                    ? card1.OriginalName
                    : card2.OriginalName;

                Console.WriteLine($"{card1.Attributes[i].AttributeValue} - {card1.Attributes[i].AttributeName} - {card2.Attributes[i].AttributeValue} ({winnerName})");
            }
        }

        private static void ChangeCard(List<DataCard> deck, DataCard card, bool displayChangedStats = true)
        {

            var newCard = deck[new Random().Next(0, deck.Count - 1)];
            card.Health += 15;
            card.ReplaceAttributes(newCard);


            if (displayChangedStats )
            {
                Console.WriteLine("Changed card: ");
                Console.WriteLine();
                Console.WriteLine("List of Attributes:");
                Console.WriteLine();
                Console.WriteLine($"---- {card.DisplayName} ----");

                for (int i = 0; i < card.Attributes.Count; i++)
                {
                    Console.WriteLine($"{(i + 1)} - {card.Attributes[i].AttributeName} - {card.Attributes[i].AttributeValue} - ({card.Attributes[i].AttributeType})");
                }
            }
        }
    }

    public enum Difficulty
    {
        Easy = 1,
        Normal = 2,
        Hard = 3,
        VertHard = 4,
    }
}