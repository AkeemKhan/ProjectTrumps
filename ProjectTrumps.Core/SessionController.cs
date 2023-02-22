using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class SessionController
    {
        public SessionController() { }

        public List<DataCard> Deck { get; set; }
        public Difficulty GlobalDifficulty { get; set; }
        public bool UsingHero { get; set; } = false;

        public int ArcadeLength { get; set; } = 8;
        public Queue<DataCard> ArcadeOpponents { get; set; }
        public int Round { get; set; }

        public int LoadedSessionDifficultyModifier { get; set; } = 0;

        public void RunCoreSession()
        {
            LoadSession(@"C:\\Users\\AKEEM\\Documents\\TrumpsGen.csv");

            Deck = SaveState.Instance.FullDeck;

            Console.WriteLine("Choose Ladder starter:");
            Console.WriteLine();
            Console.WriteLine("1: New Game");
            Console.WriteLine("2: Existing Heroes");
            Console.WriteLine("3: Load Game");

            var startInput = Console.ReadLine();
            Console.WriteLine();

            DataCard selectedCard1 = null;
            DataCard selectedCard2 = null;

            switch (startInput)
            {
                case "1":
                    UsingHero = true;
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

                    selectedCard1 = CardFactory.Instance.GenerateCard(nameInput, Deck.FirstOrDefault().CurrentAttributes, (ColourType)typeInt);
                    break;
                case "2":
                    selectedCard1 = Deck[new Random().Next(0, Deck.Count)];
                    break;
                case "3":
                    selectedCard1 = SelectLSavedHero(out var loadedHero);
                    UsingHero = loadedHero;
                    break;
                default:
                    selectedCard1 = Deck[new Random().Next(0, Deck.Count)];
                    break;
            }

            Console.WriteLine("Select Difficulty:");
            Console.WriteLine();
            Console.WriteLine("1: Easy");
            Console.WriteLine("2: Normal");
            Console.WriteLine("3: Hard");
            Console.WriteLine("4: Very Hard");

            GlobalDifficulty = SelectDifficulty();

            InitialiseArcadeOpponents(selectedCard1.Level);

            var maxOpponents = ArcadeOpponents.Count;

            Round = 1;
            DataCard playerCard = null;

            while (ArcadeOpponents.Count > 0)
            {                
                if (Round == 1)
                {
                    playerCard = InitialiseCard(selectedCard1, 100, selectedCard1.Level);
                }

                var opponentCard = ArcadeOpponents.Dequeue();

                if (Round < maxOpponents)
                {
                    Console.WriteLine($"------------------------------------------------------------");
                    Console.WriteLine($"----------- ROUND {Round} - {opponentCard.DisplayName}");
                    Console.WriteLine($"------------------------------------------------------------");
                }
                else if (Round == maxOpponents)
                {
                    Console.WriteLine($"------------------------------------------------------------");
                    Console.WriteLine($"----------- FINAL ROUND - {opponentCard.DisplayName} is the final enemy!");
                    Console.WriteLine($"------------------------------------------------------------");
                }

                var matchController = new MatchController()
                {
                    DamageLimitPerLevel = 10,
                    UseCost = -3
                };

                var cpuController = new CPUController()
                {
                    DamageThreshold = 20,
                    ChangeCard = false,
                    ChangeAtNTimesDamageTaken = 10,
                    DamageTakenCounter = 0,
                    DifficultyLevel = GlobalDifficulty
                };

                bool retreat = false;
                var playerTurn = true;
                var prevAttr = -1;
                
                cpuController.RefreshCommands(playerCard, opponentCard);

                Console.WriteLine();

                var c1Average = playerCard.CurrentPowerRating;
                var c2Average = opponentCard.CurrentPowerRating;

                Console.WriteLine($"{playerCard.DisplayName} power: {c1Average}");
                Console.WriteLine($"{opponentCard.DisplayName} power: {c2Average}");

                while (playerCard.Health > 0 && opponentCard.Health > 0)
                {
                    var playerTurnAtStart = playerTurn;

                    Console.WriteLine();
                    Console.WriteLine();

                    var aiHealthBefore = opponentCard.Health;

                    if (playerTurn)
                    {
                        Console.WriteLine("------------------------------- List of Attributes: -----------------------------------------------------------");
                        Console.WriteLine();
                        DisplayCardDetails(playerCard, prevAttr);

                        if (prevAttr >= 0)
                        {
                            Console.WriteLine($"Previous used: {playerCard.CurrentAttributes[prevAttr].AttributeName}");
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
                            else if (input.ToLower() == "r")
                            {
                                if (UsingHero)
                                    retreat = true;
                            }
                            else if (int.TryParse(input, out var res))
                            {
                                res--;

                                if (prevAttr == res)
                                    continue;


                                if (res < playerCard.CurrentAttributes.Count)
                                {
                                    hasSelected = true;
                                    prevAttr = res;
                                }

                                selectedAttribute = res;
                                changeCard = false;
                                conductBattle = true;
                            }

                            if (retreat)
                                break;
                        }


                        if (conductBattle)
                        {
                            Console.WriteLine("******************************************************************");
                            SoloBattleLogic.EvaluateBattle(playerCard, opponentCard, selectedAttribute, matchController.DamageLimitPerLevel, out var log);
                            SoloBattleLogic.ModifyAttribute(playerCard, selectedAttribute, matchController.UseCost);
                            log.DisplayConsoleMessages();

                            cpuController.RefreshCommands(playerCard, opponentCard);
                        }

                        if (changeCard)
                        {
                            ChangeCard(Deck, playerCard);
                        }

                        playerTurn = false;

                        if (retreat)
                            break;
                    }
                    else
                    {
                        Console.WriteLine("CPU turn - Press to continue");
                        Console.ReadLine();

                        if (cpuController.ChangeCard)
                        {
                            Console.WriteLine("CPU Changed card - Press to continue");
                            ChangeCard(Deck, opponentCard, false);
                            cpuController.ChangeCard = false;

                            Console.ReadLine();
                        }
                        else
                        {
                            Console.WriteLine("******************************************************************");
                            var randomAttributeSelected = cpuController.ChooseCommand();
                            SoloBattleLogic.EvaluateBattle(playerCard, opponentCard, randomAttributeSelected, matchController.DamageLimitPerLevel, out var log);
                            SoloBattleLogic.ModifyAttribute(opponentCard, randomAttributeSelected, matchController.UseCost);
                            log.DisplayConsoleMessages();
                        }

                        playerTurn = true;
                    }

                    if (retreat)
                        break;

                    var aiHealthAfter = opponentCard.Health;
                    if (aiHealthAfter < aiHealthBefore)
                    {
                        cpuController.DamageTakenCounter++;
                    }

                    cpuController.Morale.EvaluateMorale(aiHealthBefore, aiHealthAfter, playerTurn, out var moraleLog);
                    moraleLog.DisplayConsoleMessages();

                    Console.WriteLine("******************************************************************");

                    var blog = new BattleLog();
                    SoloBattleLogic.ReportHP(new List<DataCard> { playerCard, opponentCard }, blog);
                    blog.DisplayConsoleMessages();

                    Console.WriteLine("Press to continue");
                    Console.ReadLine();
                }

                if (retreat)
                {
                    Console.WriteLine("Saving card --- Ending run");
                    DisplayCardDetails(playerCard, 0, true);
                    SaveState.Instance.SaveCard(playerCard);
                    break;
                }

                PostGameSummary(playerCard, opponentCard);

                if (playerCard.Health <= 0)
                {
                    Console.WriteLine($"{playerCard.DisplayName} Defeat");
                    break;
                }
                else
                {
                    playerCard.Level++;
                    Console.WriteLine();
                    DisplayCardDetails(playerCard, 0, true);

                    if (UsingHero && playerCard.MainAttributes.Any())
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
                            playerCard.RestoreMainAttributes();
                            Console.WriteLine();
                            Console.WriteLine("Restored Main card...");
                            DisplayCardDetails(playerCard, 0, true);
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

                    if (ArcadeOpponents.Any())
                    {
                        var input = Console.ReadLine();
                        var heroUpgrade = 0;
                        switch (input)
                        {
                            case "1":
                                Console.WriteLine("Restored and Enhanced Health");
                                playerCard.FullHeal();
                                playerCard.EnhanceHealth(100);
                                break;
                            case "2":
                                Console.WriteLine("Restored attributes to original values");
                                playerCard.Heal(20);
                                playerCard.RestoreAttributes();

                                if (UsingHero)
                                {
                                    heroUpgrade++;
                                    playerCard.EnhanceAllAttributes(heroUpgrade + 1);
                                }

                                break;
                            case "3":
                                Console.WriteLine("Enhanced current attributes");
                                // card1.Heal(20);
                                playerCard.EnhanceAllAttributes(2);
                                // card1.EnhanceAttribute(new Random().Next(0, card1.CurrentAttributes.Count), 1);
                                break;
                            case "4":
                                Console.WriteLine("Greatly enhanced a single attribute");
                                var extraModifier = 0;

                                if (UsingHero)
                                {
                                    heroUpgrade++;
                                    extraModifier = 1 + heroUpgrade;
                                }

                                playerCard.EnhanceAttribute(new Random().Next(0, playerCard.CurrentAttributes.Count), 5 + extraModifier);
                                break;
                            case "5":
                                Console.WriteLine($"Specialised and restored all attributes to type {playerCard.Type.ToString()}");
                                playerCard.Heal(15);
                                playerCard.UnifyAttributesToCardType(true);
                                break;
                            case "6":
                                Console.WriteLine($"Specialised all attributes to type {playerCard.Type.ToString()}");
                                playerCard.Heal(15);
                                playerCard.UnifyAttributesToCardType();
                                playerCard.EnhanceAttribute(new Random().Next(0, playerCard.CurrentAttributes.Count), 3);
                                break;
                            case "7":
                                Console.WriteLine("Lighlty enhanced all attributes and health");
                                playerCard.Heal(15);
                                playerCard.EnhanceHealth(25);
                                playerCard.EnhanceAllAttributes(1);
                                break;
                            default:
                                Console.WriteLine("Restored and Enhanced Health");
                                playerCard.FullHeal();
                                playerCard.EnhanceHealth(100);
                                break;
                        }

                        Console.ReadLine();

                        DisplayCardDetails(playerCard, 0, true);

                        Console.WriteLine("Continue to next opponent...");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.ReadLine();
                        Console.WriteLine("VICTORY - your card will be saved");
                        SaveState.Instance.SaveCard(playerCard);
                        Console.ReadLine();
                    }

                    Round++;
                }
            }
        }

        private static DataCard InitialiseCard(DataCard sCard1, int hp, int level)
        {
            return new DataCard()
            {
                Id = sCard1.Id,
                OriginalName = sCard1.OriginalName,
                CurrentAttributes = CardFactory.Instance.CopyAttributes(sCard1.CurrentAttributes),
                OriginalAttributes = CardFactory.Instance.CopyAttributes(sCard1.CurrentAttributes),
                Type = sCard1.Type,
                Health = hp,
                MaxHealth = hp,
                Level = level,
            };
        }

        private void InitialiseArcadeOpponents(int level)
        {
            var startLevel = level - 1;
            ArcadeOpponents = new Queue<DataCard>();
            var arcadeOpponentList = new List<DataCard>();

            for (int i = 0; i < ArcadeLength; i++)
            {
                if (i != ArcadeLength- 1)
                {
                    var arcadeOpponentHp = 100 + (i * 15);
                    var arcadeOpponent = InitialiseCard(Deck[new Random().Next(0, Deck.Count)], arcadeOpponentHp, i + startLevel);

                    for (int j = 1; j <= i + startLevel; j++)
                    {
                        arcadeOpponent.EnhanceAttribute(new Random().Next(0, arcadeOpponent.CurrentAttributes.Count), 2);
                    }

                    ArcadeOpponents.Enqueue(arcadeOpponent);
                }
                // Final
                else
                {
                    var finalOpponentHp = 400;
                    var finalOpponent = InitialiseCard(Deck[new Random().Next(0, Deck.Count)], finalOpponentHp, i + startLevel);

                    for (int j = 1; j <= i + startLevel; j++)
                    {
                        finalOpponent.EnhanceAttribute(new Random().Next(0, finalOpponent.CurrentAttributes.Count), 2);
                    }

                    finalOpponent.EnhanceAttribute(new Random().Next(0, finalOpponent.CurrentAttributes.Count), 5);
                    finalOpponent.EnhanceAttribute(new Random().Next(0, finalOpponent.CurrentAttributes.Count), 5);
                    finalOpponent.EnhanceAttribute(new Random().Next(0, finalOpponent.CurrentAttributes.Count), 5);
                    
                    ArcadeOpponents.Enqueue(finalOpponent);
                }
            }           
        }

        public void LoadSession(string path)
        {            
            SaveState.Instance.MainCardsLocation = path ;
            SaveState.Instance.LoadMainDeck();
            SaveState.Instance.LoadAdditionalDeck();
        }

        public DataCard SelectLSavedHero(out bool loadSuccess)
        {
            loadSuccess = false;
            var deck = SaveState.Instance.AdditionalDeck;
            DataCard selectedCard = null;

            foreach (var card in deck)
            {
                var index = deck.IndexOf(card);
                Console.WriteLine($"{index + 1}. {card.DisplayName} - {card.Type.ToString()} - {card.OriginalPowerRating} - {card.Id}");
            }

            var selected = false;

            while (!selected)
            {
                Console.WriteLine("Select card - Enter the number:");

                var input = Console.ReadLine();

                if (input.ToLower() == "c")
                {
                    Console.WriteLine("Cancelled Selection - picking random card");
                    break;
                }
                if (int.TryParse(input, out var res) && (res - 1) <= deck.Count)
                {
                    selectedCard = deck[res - 1];
                    loadSuccess = selected = true;
                }
            }

            return selectedCard ?? deck[new Random().Next(0, deck.Count)];
        }

        public void DisplayCardDetails(DataCard card1, int prevAttr, bool displayOnly = false)
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

        public Difficulty SelectDifficulty()
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

        public void PostGameSummary(DataCard card1, DataCard card2)
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

        public void ChangeCard(List<DataCard> deck, DataCard card, bool displayChangedStats = true)
        {
            if (!card.MainAttributes.Any())
            {
                card.MainAttributes = CardFactory.Instance.CopyAttributes(card.CurrentAttributes);
            }

            var newCard = deck[new Random().Next(0, deck.Count)];
            card.Health += 15;
            card.ReplaceAttributes(newCard);
            card.Type = newCard.Type;

            if (displayChangedStats)
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
}
