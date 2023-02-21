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
            var opponents = new Queue<DataCard>();
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

            Console.WriteLine("Choose Ladder starter:");
            Console.WriteLine();
            Console.WriteLine("1: New Hero");
            Console.WriteLine("2: Existing Heroes");

            bool usingHero = false;
            var startInput = Console.ReadLine();
            Console.WriteLine();

            DataCard sCard1 = null;
            DataCard sCard2 = null;

            switch (startInput)
            {
                case "1":
                    usingHero = true;
                    Console.WriteLine("Enter the name:");
                    var nameInput = Console.ReadLine();

                    if (string.IsNullOrEmpty(nameInput))
                        nameInput = "Hero";

                    Console.WriteLine();
                    Console.WriteLine("Choose Type:");
                    Console.WriteLine();
                    Console.WriteLine("1: Red - Burn");
                    Console.WriteLine("2: Blue - Evade/Parry");
                    Console.WriteLine("3: Green - Heal");
                    Console.WriteLine();

                    var typeInput = Console.ReadLine();
                    var typeInt = 1;
                    switch (typeInput)
                    {   
                        case "1":
                            typeInt = 1;
                            break;
                        case "2":
                            typeInt = 2;
                            break;
                        case "3":
                            typeInt = 3;
                            break;
                        default:
                            break;
                    }

                    sCard1 = CardFactory.Instance.GenerateCard(nameInput, deck.FirstOrDefault().CurrentAttributes, (ColourType)typeInt);
                    break;
                case "2":
                default:
                    sCard1 = deck[new Random().Next(0, deck.Count - 1)];
                    break;
            }


            Console.WriteLine("Select Difficulty:");
            Console.WriteLine();
            Console.WriteLine("1: Easy");
            Console.WriteLine("2: Normal");
            Console.WriteLine("3: Hard");
            Console.WriteLine("4: Very Hard");

            var difficulty = SelectDifficulty();

            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);
            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);
            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);
            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);
            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);
            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);
            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);
            opponents.Enqueue(deck[new Random().Next(0, deck.Count - 1)]);

            var maxOpponents = opponents.Count;

            var round = 1;
            DataCard card1 = null;

            while (opponents.Count > 0)
            {
                sCard2 = opponents.Dequeue();
                var hp = 100 + ((round - 1) * 20);
                
                if (round == 1)
                {
                    card1 = new DataCard() 
                    {
                        OriginalName = sCard1.OriginalName, 
                        CurrentAttributes = CardFactory.Instance.CopyAttributes(sCard1.CurrentAttributes), 
                        OriginalAttributes = CardFactory.Instance.CopyAttributes(sCard1.CurrentAttributes),
                        Type = sCard1.Type,
                        Health = hp, 
                        MaxHealth = hp 
                    };
                }

                var card2 = new DataCard() 
                { 
                    OriginalName = sCard2.OriginalName, 
                    CurrentAttributes = CardFactory.Instance.CopyAttributes(sCard2.CurrentAttributes),
                    OriginalAttributes = CardFactory.Instance.CopyAttributes(sCard2.CurrentAttributes),
                    Type = sCard2.Type,
                    Health = hp, 
                    MaxHealth = hp 
                };

                for(int i = 1; i <= round; i++)
                {
                    card2.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count - 1),1);
                }

                if (round < maxOpponents)
                {
                    Console.WriteLine($"------------------------------------------------------------");
                    Console.WriteLine($"----------- ROUND {round} - {card2.DisplayName}");
                    Console.WriteLine($"------------------------------------------------------------");
                }
                if (round == maxOpponents)
                {
                    Console.WriteLine($"------------------------------------------------------------");
                    Console.WriteLine($"----------- FINAL ROUND - {card2.DisplayName} is the final enemy!");
                    Console.WriteLine($"------------------------------------------------------------");
                    
                    card2.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count - 1), 5);
                    card2.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count - 1), 5);
                    card2.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count - 1), 5);
                }

                var playerTurn = true;
                var prevAttr = -1;

                var useModifier = - 3;
                var heroUseModifier = -1;

                var damageLimit = 10 * round;
                var aiDamageChangeThreshold = 20;
                var aiMoraleDamage = 0;
                var aiChangeAtMoraleBreak = 6;
                var aiTakenDamageCounter = 0;
                var aiChangeAtNTimesDamageTaken = 10;
                var aiChangeNext = false;
           
                var aiChosenCommands = new List<int>();
                InitialiseDifficulty(card1, card2, aiChosenCommands, difficulty);

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
                        DisplayCardDetails(card1, prevAttr);

                        if (prevAttr >= 0)
                        {
                            Console.WriteLine($"Previous used: {card1.CurrentAttributes[prevAttr].AttributeName}");
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

                            if (input.ToLower() == "c")
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


                                if (res < card1.CurrentAttributes.Count)
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
                            Console.WriteLine("******************************************************************");
                            SoloBattleLogic.EvaluateBattle(card1, card2, selectedAttribute, damageLimit, out var log);
                            SoloBattleLogic.ModifyAttribute(card1, selectedAttribute, usingHero? heroUseModifier : useModifier);

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
                            Console.WriteLine("******************************************************************");
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

                        if (diff * 2 > aiDamageChangeThreshold)
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
                    Console.WriteLine("******************************************************************");

                    var blog = new BattleLog();
                    SoloBattleLogic.ReportHP(new List<DataCard> { card1, card2 }, blog);
                    foreach (var msg in blog.Messages)
                    {
                        Console.WriteLine(msg);
                    }

                    Console.WriteLine("Press to continue");
                    Console.ReadLine();
                }

                PostGameSummary(card1, card2);

                if (card1.Health <= 0)
                {
                    Console.WriteLine($"{card1.DisplayName} Defeat");
                    break;
                }
                else
                {
                    Console.WriteLine();
                    DisplayCardDetails(card1, 0, true);

                    if (usingHero && card1.MainAttributes.Any())
                    {
                        Console.WriteLine();
                        Console.WriteLine("Restore main card?");
                        Console.WriteLine();
                        Console.WriteLine("1: Yes");
                        Console.WriteLine("2: No");

                        Console.WriteLine();

                        var resInput = Console.ReadLine();
                        if (resInput == "1") 
                        {
                            card1.RestoreMainAttributes();
                            Console.WriteLine();
                            Console.WriteLine("Restored Main card...");
                            DisplayCardDetails(card1, 0, true);
                            Console.WriteLine();
                            Console.ReadLine();
                        }
                    }

                    Console.WriteLine("Select Reward:");
                    Console.WriteLine();
                    Console.WriteLine("1: Enhance Health");
                    Console.WriteLine("2: Replenish Attributes (Heroes also slighly improve attributes)");
                    Console.WriteLine("3: Enhance All Attributes");
                    Console.WriteLine("4: Greatly Enhance Single Random Attributes");
                    Console.WriteLine("5: Specialise Types - (Restore attributes)");
                    Console.WriteLine("6: Specialise Types - (Keep attributes and Enhance a random attribute)");
                    Console.WriteLine("7: All Minor Enhance");

                    Console.WriteLine();

                    var input = Console.ReadLine();
                    var heroUpgrade = 0;
                    switch (input)
                    {
                        case "1":
                            Console.WriteLine("Restored and Enhanced Health");
                            card1.FullHeal();
                            card1.EnhanceHealth(100);
                            break;
                        case "2":
                            Console.WriteLine("Restored attributes to original values");
                            card1.Heal(20);
                            card1.RestoreAttributes();

                            if (usingHero)
                            {
                                heroUpgrade++;
                                card1.EnhanceAllAttributes(heroUpgrade+1);
                            }

                            break;
                        case "3":
                            Console.WriteLine("Enhanced current attributes");
                            card1.Heal(20);
                            card1.EnhanceAllAttributes(1);
                            card1.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count - 1), 1);
                            break;
                        case "4":
                            Console.WriteLine("Greatly enhanced a single attribute");
                            var extraModifier = 0;
                            if (usingHero)
                            {
                                heroUpgrade++;
                                extraModifier = 1 + heroUpgrade;
                            }

                            card1.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count - 1), 5 + extraModifier);
                            break;
                        case "5":
                            Console.WriteLine($"Specialised and restored all attributes to type {card1.Type.ToString()}");
                            card1.Heal(15);
                            card1.UnifyAttributesToCardType(true);
                            break;
                        case "6":
                            Console.WriteLine($"Specialised all attributes to type {card1.Type.ToString()}");
                            card1.Heal(15);
                            card1.UnifyAttributesToCardType();
                            card1.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count - 1), 3);
                            break;
                        case "7":
                            Console.WriteLine("Lighlty enhanced all attributes and health");
                            card1.Heal(15);
                            card1.EnhanceHealth(50);
                            card1.EnhanceAllAttributes(1);
                            break;
                        default:
                            Console.WriteLine("Restored and Enhanced Health");
                            card1.FullHeal();
                            card1.EnhanceHealth(100);
                            break;
                    }

                    Console.ReadLine();

                    DisplayCardDetails(card1, 0, true);

                    Console.WriteLine("Continue to next opponent...");
                    Console.ReadLine();

                    round++;
                }
            }

        }

        private static void DisplayCardDetails(DataCard card1, int prevAttr, bool displayOnly = false)
        {
            Console.WriteLine($"---- {card1.DisplayName} ({card1.Type.ToString()}) ----");

            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                if (i == prevAttr && !displayOnly)
                {
                    Console.WriteLine($"{(i + 1)} - {card1.CurrentAttributes[i].AttributeName} - {card1.CurrentAttributes[i].AttributeValue} - CANNOT USE SAME ATTRIBUTE TWICE IN A ROW");
                }
                else
                {
                    Console.WriteLine($"{(i + 1)} - {card1.CurrentAttributes[i].AttributeName} - {card1.CurrentAttributes[i].AttributeValue} - ({card1.CurrentAttributes[i].AttributeType})");
                }
            }
        }

        private static Difficulty SelectDifficulty()
        {
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

            return difficulty;
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
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                if (card1.CurrentAttributes[i].AttributeValue < card2.CurrentAttributes[i].AttributeValue)
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
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                if (card1.CurrentAttributes[i].AttributeValue < card2.CurrentAttributes[i].AttributeValue)
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
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                if (card1.CurrentAttributes[i].AttributeValue < card2.CurrentAttributes[i].AttributeValue)
                {
                    aiChosenCommands.Add(i);
                    aiChosenCommands.Add(i);
                }
            }
        }

        private static void InitialiseEasy(DataCard card1, List<int> aiChosenCommands)
        {
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                aiChosenCommands.Add(i);
            }
        }

        private static void PostGameSummary(DataCard card1, DataCard card2)
        {
            Console.WriteLine($"------------- POST MATCH SUMMARY -------------------");
            Console.WriteLine($"---- {card1.DisplayName} vs {card2.DisplayName} ----");
            var idx = 0;
            for (int i = 0; i < card1.CurrentAttributes.Count; i++)
            {
                var winnerName = card1.CurrentAttributes[i].AttributeValue == card2.CurrentAttributes[i].AttributeValue
                    ? ""
                    : card1.CurrentAttributes[i].AttributeValue > card2.CurrentAttributes[i].AttributeValue
                    ? card1.OriginalName
                    : card2.OriginalName;

                Console.WriteLine($"{card1.CurrentAttributes[i].AttributeValue} - {card1.CurrentAttributes[i].AttributeName} - {card2.CurrentAttributes[i].AttributeValue} ({winnerName})");
            }
            Console.WriteLine($"----------------------------------------------------");
            Console.WriteLine();
        }

        private static void ChangeCard(List<DataCard> deck, DataCard card, bool displayChangedStats = true)
        {
            if (!card.MainAttributes.Any()) 
            {
                card.MainAttributes = CardFactory.Instance.CopyAttributes(card.CurrentAttributes);                    
            }

            var newCard = deck[new Random().Next(0, deck.Count - 1)];
            card.Health += 15;
            card.ReplaceAttributes(newCard);
            card.Type = newCard.Type;

            if (displayChangedStats )
            {
                Console.WriteLine("Changed card: ");
                Console.WriteLine();
                Console.WriteLine("List of Attributes:");
                Console.WriteLine();
                Console.WriteLine($"---- {card.DisplayName} ----");

                for (int i = 0; i < card.CurrentAttributes.Count; i++)
                {
                    Console.WriteLine($"{(i + 1)} - {card.CurrentAttributes[i].AttributeName} - {card.CurrentAttributes[i].AttributeValue} - ({card.CurrentAttributes[i].AttributeType})");
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