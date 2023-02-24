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
        public List<DataCard> Deck { get; set; }
        public Difficulty GlobalDifficulty { get; set; }
        public bool UsingHero { get; set; } = false;

        public int ArcadeLength { get; set; } = 8;
        public int ChampionsLength { get; set; } = 5;
        public Queue<CPUController> ArcadeOpponents { get; set; }
        public int Round { get; set; }

        public int LoadedSessionDifficultyModifier { get; set; } = 0;

        public void RunCoreSession()
        {
            LoadSession(@"C:\\Users\\AKEEM\\Documents\\TrumpsGen.csv");

            Deck = SaveState.Instance.FullDeck;

            DataCard selectedCard1 = SelectMode();

            var maxOpponents = ArcadeOpponents.Count;

            Round = 1;
            DataCard playerCard = null;
            var playerController = new PlayerController();

            while (ArcadeOpponents.Count > 0)
            {
                if (Round == 1)
                {
                    playerCard = InitialiseCard(selectedCard1, selectedCard1.MaxHealth, selectedCard1.Level);
                    playerController.MainCard = playerCard;
                    playerController.PlayerInventory.Deck.AddCardsToDeck(Deck, 10);
                }

                var cpuController = ArcadeOpponents.Dequeue();
                var opponentCard = cpuController.CPUCard;

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
                    UseCost = -3,
                    Inspect = false
                };

                playerController.PlayerActionParams.InitialiseAtStartOfTurn();

                var playerTurn = true;

                cpuController.RefreshCommands(playerCard, opponentCard);

                Console.WriteLine();

                var c1Average = playerCard.CurrentPowerRating;
                var c2Average = opponentCard.CurrentPowerRating;

                Console.WriteLine($"{playerCard.DisplayName} power: {c1Average}");
                Console.WriteLine($"{opponentCard.DisplayName} power: {c2Average}");

                while (playerCard.Health > 0 && opponentCard.Health > 0)
                {                   
                    Console.WriteLine();

                    var aiHealthBefore = opponentCard.Health;

                    if (playerTurn)
                    {
                        playerController.InitialiseActions();

                        SelectAction(playerCard, opponentCard, matchController, playerController);
                        ImplementAction(playerCard, opponentCard, matchController, cpuController, playerController);

                        playerTurn = false;

                        if (playerController.PlayerActionParams.Retreat)
                            break;
                    }
                    else
                    {
                        Console.WriteLine("CPU turn - Press to continue");
                        Console.ReadLine();

                        if (cpuController.ChangeCard)
                        {
                            Console.WriteLine("CPU Changed card - Press to continue");
                            CPUChangeCard(Deck, opponentCard, false);
                            cpuController.ChangeCard = false;
                            matchController.Inspect = false;
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

                    if (playerController.PlayerActionParams.Retreat)
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
                    playerController.EvaluatePostTurn(playerTurn  playerCard, opponentCard);
                    Console.ReadLine();
                }

                if (playerController.PlayerActionParams.Retreat)
                {
                    Console.WriteLine("Saving card --- Ending run");
                    DisplayCardDetails(playerCard, 0, playerController, true);
                    SaveState.Instance.SaveCard(playerCard);
                    break;
                }

                playerCard.EnhanceCountdown = 0;

                PostGameSummary(playerCard, opponentCard, Round);

                if (playerCard.Health <= 0)
                {
                    Console.WriteLine($"{playerCard.DisplayName} Defeated");
                    break;
                }
                else
                {
                    Console.WriteLine($"{opponentCard.DisplayName} Defeated");

                    playerCard.Level++;
                    Console.WriteLine();
                    DisplayCardDetails(playerCard, 0, playerController, true);

                    if (UsingHero && playerCard.StoredAttributes.Any())
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
                            playerCard.RestoreStoredAttributes();
                            Console.WriteLine();
                            Console.WriteLine("Restored Main card...");
                            DisplayCardDetails(playerCard, 0, playerController, true);
                            Console.WriteLine();
                            Console.ReadLine();
                        }                        
                    }

                    if (ArcadeOpponents.Any())
                    {
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

                        DisplayCardDetails(playerCard, 0, playerController, true);

                        Console.WriteLine("Continue to next opponent...");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.ReadLine();
                        Console.WriteLine("VICTORY - Enhancing your card");
                        playerCard.EnhanceAttribute(new Random().Next(0, playerCard.CurrentAttributes.Count), 7);

                        SaveState.Instance.SaveCard(playerCard);
                        Console.ReadLine();
                    }

                    Round++;
                }
            }
        }

        private void ImplementAction(DataCard playerCard, DataCard opponentCard, MatchController matchController, CPUController cpuController, PlayerController playerController)
        {
            if (playerController.PlayerActionParams.ConductBattle)
            {
                Console.WriteLine("******************************************************************");
                SoloBattleLogic.EvaluateBattle(playerCard, opponentCard, playerController.PlayerActionParams.SelectedAttribute, matchController.DamageLimitPerLevel, out var log);
                SoloBattleLogic.ModifyAttribute(playerCard, playerController.PlayerActionParams.SelectedAttribute, matchController.UseCost);
                log.DisplayConsoleMessages();

                cpuController.RefreshCommands(playerCard, opponentCard);
            }

            if (playerController.PlayerActionParams.ChangeCard)
            {
                PlayerChangeCard(playerController, true);
            }

            if (playerController.PlayerActionParams.Fuse)
            {
                playerController.PlayerInventory.Fusion--;
                playerCard.FuseAttributes(playerController.PlayerInventory.Deck.DrawTopCard());                
            }

            if (playerController.PlayerActionParams.Heal)
            {
                playerCard.FullHeal();
                playerController.PlayerInventory.Heal--;
            }

            if (playerController.PlayerActionParams.Replenish)
            {
                playerCard.RestoreAttributes();
                playerController.PlayerInventory.Replenish--;                
            }

            if (playerController.PlayerActionParams.Tribute)
            {
                var tributes = new List<DataCard>();
                for (int i = 0; i < 5; i++)
                {
                    tributes.Add(playerController.PlayerInventory.Deck.DrawTopCard());
                }
                playerCard.EnhanceUsingTributes(tributes);
                playerController.PlayerInventory.Tribute--;
            }
        }

        private void SelectAction(DataCard playerCard, DataCard opponentCard, MatchController matchController, PlayerController playerController)
        {
            Console.WriteLine("------------------------------- List of Attributes: -----------------------------------------------------------");
            Console.WriteLine();

            DisplayCardDetails(playerCard, playerController.PreviousAttribute, playerController, false, playerController.PlayerActionParams.InsightType ? opponentCard : null);

            if (playerController.PreviousAttribute >= 0)
            {
                Console.WriteLine($"Previous used: {playerCard.CurrentAttributes[playerController.PreviousAttribute].AttributeName}");
            }

            Console.WriteLine();

            var hasSelected = false;
            while (!hasSelected)
            {
                Console.WriteLine("Enter a valid value - cannot use prev:");
                var input = Console.ReadLine();

                if (input.ToLower() == "c")
                {
                    playerController.PlayerActionParams.ChangeCard = true;
                    playerController.PlayerActionParams.ConductBattle = false;
                    hasSelected = true;
                }
                else if (input.ToLower() == "f")
                {
                    if (playerController.PlayerInventory.Deck.HasCards(1) && playerController.PlayerInventory.Fusion > 0)
                    {
                        playerController.PlayerActionParams.Fuse = true;
                        hasSelected = true;
                    }
                }
                else if (input.ToLower() == "t")
                {
                    if (playerController.PlayerInventory.Deck.HasCards(5) && playerController.PlayerInventory.Tribute > 0)
                    {
                        playerController.PlayerActionParams.Tribute = true;
                        hasSelected = true;
                    }
                }
                else if (input.ToLower() == "h")
                {
                    if (playerController.PlayerInventory.Heal > 0)
                    {
                        playerController.PlayerActionParams.Heal = true;
                        hasSelected = true;
                    }
                }
                else if (input.ToLower() == "a")
                {
                    if (playerController.PlayerInventory.Replenish > 0)
                    {
                        playerController.PlayerActionParams.Replenish = true;
                        hasSelected = true;
                    }
                }
                else if (input.ToLower() == "i")
                {
                    playerController.PlayerActionParams.InsightType = true;                    
                    playerController.PlayerActionParams.ConductBattle = false;
                    hasSelected = true;
                }
                else if (input.ToLower() == "r")
                {
                    if (UsingHero)
                        playerController.PlayerActionParams.Retreat = true;
                }
                else if (int.TryParse(input, out var res))
                {
                    res--;

                    if (playerController.PreviousAttribute == res)
                        continue;


                    if (res < playerCard.CurrentAttributes.Count)
                    {
                        hasSelected = true;
                        playerController.PreviousAttribute = res;
                    }

                    playerController.PlayerActionParams.SelectedAttribute = res;
                    playerController.PlayerActionParams.ChangeCard = false;
                    playerController.PlayerActionParams.ConductBattle = true;
                }

                if (playerController.PlayerActionParams.Retreat)
                    break;
            }
        }

        private DataCard SelectMode()
        {
            GameMode mode = GameMode.None;

            Console.WriteLine("Choose Ladder starter:");
            Console.WriteLine();
            Console.WriteLine("1: Ladder Mode - New Game");
            Console.WriteLine("2: Ladder Mode - Existing Heroes");
            if (SaveState.Instance.AdditionalDeck.Any())
            {

                Console.WriteLine("3: Ladder Mode - Load Game");
                Console.WriteLine("4: Champions Ladder Mode - Load Game");
            }

            var startInput = Console.ReadLine();
            Console.WriteLine();

            DataCard selectedCard1 = null;
            DataCard selectedCard2 = null;

            switch (startInput)
            {
                case "1":
                    mode = GameMode.LadderModeNewGame;
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
                    mode = GameMode.LadderModeExistingHero;
                    selectedCard1 = Deck[new Random().Next(0, Deck.Count)];
                    break;
                case "3":
                    mode = GameMode.LadderModeLoadGame;
                    if (SaveState.Instance.AdditionalDeck.Any())
                    {
                        selectedCard1 = SelectLSavedHero(out var loadedHero);
                        UsingHero = loadedHero;
                    }
                    else
                    {
                        Console.WriteLine("No cards to load - selecting existing hero");
                        selectedCard1 = Deck[new Random().Next(0, Deck.Count)];
                    }
                    break;
                case "4":
                    mode = GameMode.ChampionsLadder;
                    if (SaveState.Instance.AdditionalDeck.Any())
                    {
                        selectedCard1 = SelectLSavedHero(out var loadedHero);
                        UsingHero = loadedHero;
                    }
                    else
                    {
                        Console.WriteLine("No cards to load - selecting existing hero");
                        selectedCard1 = Deck[new Random().Next(0, Deck.Count)];
                    }
                    GlobalDifficulty = Difficulty.Hard;
                    break;
                default:
                    selectedCard1 = Deck[new Random().Next(0, Deck.Count)];
                    break;
            }

            if (mode != GameMode.ChampionsLadder)
            {
                Console.WriteLine("Select Difficulty:");
                Console.WriteLine();
                Console.WriteLine("1: Easy");
                Console.WriteLine("2: Normal");
                Console.WriteLine("3: Hard");
                Console.WriteLine("4: Very Hard");
                GlobalDifficulty = SelectDifficulty();
            }

            if (mode == GameMode.LadderModeNewGame || mode == GameMode.LadderModeLoadGame || mode == GameMode.LadderModeExistingHero)
                InitialiseArcadeOpponents(selectedCard1.Level);
            else
                InitialiseChampionsLadder();
            return selectedCard1;
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
                OriginalType = sCard1.Type,
                Health = hp,
                MaxHealth = hp,
                Level = level,
            };
        }

        private void InitialiseArcadeOpponents(int level)
        {
            var startLevel = level;
            ArcadeOpponents = new Queue<CPUController>();
            var arcadeOpponentList = new List<DataCard>();

            for (int i = 0; i < ArcadeLength; i++)
            {
                if (i != ArcadeLength- 1)
                {
                    var arcadeOpponentHp = 100 + (i * 15);
                    var arcadeOpponentCard = InitialiseCard(Deck[new Random().Next(0, Deck.Count)], arcadeOpponentHp, i + startLevel);

                    for (int j = 1; j <= i + startLevel - 1; j++)
                    {
                        arcadeOpponentCard.EnhanceAttribute(new Random().Next(0, arcadeOpponentCard.CurrentAttributes.Count), 2);
                    }

                    var arcadeOpponent = new CPUController()
                    {
                        CPUCard = arcadeOpponentCard,
                        MoraleDropDamageThreshold = 20,
                        ChangeCard = false,
                        ChangeAtNTimesDamageTaken = 10,
                        DamageTakenCounter = 0,
                        DifficultyLevel = GlobalDifficulty
                    };

                    ArcadeOpponents.Enqueue(arcadeOpponent);
                }
                // Final
                else
                {
                    var finalOpponentHp = 400;
                    var finalOpponentCard = InitialiseCard(Deck[new Random().Next(0, Deck.Count)], finalOpponentHp, i + startLevel);

                    for (int j = 1; j <= i + startLevel - 1; j++)
                    {
                        finalOpponentCard.EnhanceAttribute(new Random().Next(0, finalOpponentCard.CurrentAttributes.Count), 2);
                    }

                    finalOpponentCard.EnhanceAttribute(new Random().Next(0, finalOpponentCard.CurrentAttributes.Count), 5);
                    finalOpponentCard.EnhanceAttribute(new Random().Next(0, finalOpponentCard.CurrentAttributes.Count), 5);
                    finalOpponentCard.EnhanceAttribute(new Random().Next(0, finalOpponentCard.CurrentAttributes.Count), 5);

                    var finalOpponent = new CPUController(400)
                    {
                        CPUCard = finalOpponentCard,
                        MoraleDropDamageThreshold = 20,
                        ChangeCard = false,
                        ChangeAtNTimesDamageTaken = 100,
                        DamageTakenCounter = 0,
                        DifficultyLevel = GlobalDifficulty
                    };

                    ArcadeOpponents.Enqueue(finalOpponent);
                }
            }           
        }

        private void InitialiseChampionsLadder()
        {
            var level = 30;

            ArcadeOpponents = new Queue<CPUController>();            

            for (int i = 0; i < ChampionsLength; i++)
            {
                var arcadeOpponentHp = 400;
                var arcadeOpponentCard = InitialiseCard(Deck[new Random().Next(0, Deck.Count)], arcadeOpponentHp, i + level);

                for (int j = 1; j <= i + level - 1; j++)
                {
                    arcadeOpponentCard.EnhanceAttribute(new Random().Next(0, arcadeOpponentCard.CurrentAttributes.Count), 2);
                }

                var arcadeOpponent = new CPUController(400)
                {
                    CPUCard = arcadeOpponentCard,
                    MoraleDropDamageThreshold = 20,
                    ChangeCard = false,
                    ChangeAtNTimesDamageTaken = 100,
                    DamageTakenCounter = 0,
                    DifficultyLevel = GlobalDifficulty
                };

                ArcadeOpponents.Enqueue(arcadeOpponent);
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

        public void DisplayCardDetails(DataCard card1, int prevAttr, PlayerController player, bool displayOnly = false, DataCard card2 = null)
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
                    var inspect = "";
                    if (card2 != null)
                    {
                        inspect = $" <--->  ({card2.CurrentAttributes[i].AttributeType.ToString()})";
                    }

                    Console.WriteLine($"{(i + 1)} - {card1.CurrentAttributes[i].AttributeName} - {card1.CurrentAttributes[i].AttributeValue} - ({card1.CurrentAttributes[i].AttributeType}){inspect}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("R - Retreat (Hero)");
            Console.WriteLine($"F - Fuse with next card                 - {player.PlayerInventory.Fusion} remaining");
            Console.WriteLine($"T - Enhance by tributing with next card - {player.PlayerInventory.Tribute} remaining");
            Console.WriteLine($"H - Heal                                - {player.PlayerInventory.Heal} remaining");
            Console.WriteLine($"A - Replenish Attributes                - {player.PlayerInventory.Replenish} remaining");
            Console.WriteLine();
            Console.WriteLine($"Cards remaining in player deck          - {player.PlayerInventory.Deck.Count}");

            if (player.MainCard.EnhanceCountdown > 0)
                Console.WriteLine($"Enhanced turns remaining                - {player.MainCard.EnhanceCountdown}");
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

        public void PostGameSummary(DataCard card1, DataCard card2, int round)
        {
            Console.WriteLine($"------------- POST MATCH SUMMARY -------------------");
            Console.WriteLine($"------------- Round {round}");
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

        public bool PlayerChangeCard(PlayerController playerController, bool displayChangedStats)
        {
            if (playerController.MainCard == null)            
                return false;
            

            if (!playerController.PlayerInventory.Deck.HasCards(1))
            {
                Console.WriteLine("No Cards available... ");
                return false;
            }

            if (!playerController.MainCard.StoredAttributes.Any())
            {
                playerController.MainCard.StoredAttributes = CardFactory.Instance.CopyAttributes(playerController.MainCard.CurrentAttributes);
            }

            var newCard = playerController.PlayerInventory.Deck.DrawTopCard();
            playerController.MainCard.Health += 15;
            playerController.MainCard.ReplaceAttributes(newCard);
            playerController.MainCard.Type = newCard.Type;

            if (displayChangedStats)
            {
                Console.WriteLine("Changed card: ");
                Console.WriteLine();
                Console.WriteLine("List of Attributes:");
                Console.WriteLine();
                Console.WriteLine($"---- {playerController.MainCard.DisplayName} ----");

                for (int i = 0; i < playerController.MainCard.CurrentAttributes.Count; i++)
                {
                    Console.WriteLine($"{(i + 1)} - {playerController.MainCard.CurrentAttributes[i].AttributeName} - {playerController.MainCard.CurrentAttributes[i].AttributeValue} - ({playerController.MainCard.CurrentAttributes[i].AttributeType})");
                }
            }

            return true;
        }

        public void CPUChangeCard(List<DataCard> deck, DataCard card, bool displayChangedStats = true)
        {
            if (!card.StoredAttributes.Any())
            {
                card.StoredAttributes = CardFactory.Instance.CopyAttributes(card.CurrentAttributes);
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

    public enum GameMode
    {
        None = 0,
        LadderModeNewGame = 1,
        LadderModeExistingHero = 2,
        LadderModeLoadGame = 3,
        ChampionsLadder = 4,
    }
}
