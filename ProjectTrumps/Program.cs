using System;
using System.Reflection.PortableExecutable;
using ProjectTrumps.Core;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var deck = new List<TrumpsCard>();

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

            var card1 = new TrumpsCard() { Name = sCard1.Name, Attributes = sCard1.Attributes, Health = 100 };
            var card2 = new TrumpsCard() { Name = sCard2.Name, Attributes = sCard2.Attributes, Health = 100 };

            var playerTurn = true;
            var prevAttr = -1;

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

            switch (difficulty)
            {
                case Difficulty.Easy:
                    for (int i = 0; i < card1.Attributes.Count; i++)
                    {
                        aiChosenCommands.Add(i);
                    }
                    break;
                case Difficulty.Normal:
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
                    break;
                case Difficulty.Hard:
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
                    break;
                case Difficulty.VertHard:
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
                    break;
                default:
                    break;
            }

            foreach (var item in aiChosenCommands)
            {
                Console.Write(item + ",");
            }

            Console.WriteLine();            
            var c1Average = ((card1.Attributes.Sum(p => p.AttributeValue)) / card1.Attributes.Count) + (card1.Attributes.Count(p => p.AttributeValue > 9));
            var c2Average = ((card2.Attributes.Sum(p => p.AttributeValue)) / card2.Attributes.Count) + (card1.Attributes.Count(p => p.AttributeValue > 9));

            Console.WriteLine($"{card1.Name} power: {c1Average}");
            Console.WriteLine($"{card2.Name} power: {c2Average}");


            while (card1.Health > 0 && card2.Health > 0)
            {
                Console.WriteLine();
                Console.WriteLine();

                if (playerTurn)
                {
                    Console.WriteLine("List of Attributes:");
                    Console.WriteLine();
                    Console.WriteLine($"---- {card1.Name} ----");

                    for (int i = 0; i < card1.Attributes.Count; i++)
                    {
                        Console.WriteLine($"{(i + 1)} - {card1.Attributes[i].AttributeName} - {card1.Attributes[i].AttributeValue} - ({card1.Attributes[i].AttributeType})");
                    }

                    Console.WriteLine();
                    Console.WriteLine();

                    var hasSelected = false;
                    var selectedAttribute = 0;

                    while (!hasSelected)
                    {
                        Console.WriteLine("Enter a valid value - cannot use prev:");
                        var input = Console.ReadLine();

                        if (int.TryParse(input, out var res))
                        {
                            res--;
                            
                            if (prevAttr == res)
                                continue;


                            if (res < card1.Attributes.Count)
                            {
                                hasSelected = true;
                            }
                            selectedAttribute = res;
                        }
                    }

                    Console.WriteLine();
                    SoloBattleLogic.EvaluateBattle(card1, card2, selectedAttribute, out var log);

                    foreach (var msg in log.Messages)
                    {
                        Console.WriteLine(msg);
                    }
                    
                    playerTurn = false;
                }
                else
                {
                    Console.WriteLine("CPU turn - Press to continue");
                    Console.ReadLine();

                    var randomAttributeSelected = aiChosenCommands[new Random().Next(0, aiChosenCommands.Count - 1)];
                    SoloBattleLogic.EvaluateBattle(card1, card2, randomAttributeSelected, out var log);

                    foreach (var msg in log.Messages)
                    {
                        Console.WriteLine(msg);
                    }

                    Console.WriteLine("Press to continue");
                    Console.ReadLine();

                    playerTurn = true;
                }
            }

            Console.WriteLine($"---- {card2.Name} ----");
            var idx = 0;
            foreach (var attr in card2.Attributes)
            {
                Console.WriteLine($"{(idx + 1)} - {card2.Attributes[idx].AttributeName} - {card2.Attributes[idx].AttributeValue} - ({card2.Attributes[idx].AttributeType})");
                idx++;
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