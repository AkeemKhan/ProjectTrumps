﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class CardFactory
    {
        public static CardFactory Instance { get; private set; } = new CardFactory();

        public TrumpsCard CreateCard(string[] args)
        {
            var card = new TrumpsCard();
            TrumpsAttribute temp = null;

            card.Name = args[1];
            card.Type = ConvertToTrumpsType(args[2]);

            for (int i = 3; i < args.Length; i++)
            {
                var paramType = i % 3;

                switch (paramType)
                {
                    case 0:
                        temp = new TrumpsAttribute();
                        temp.AttributeName = args[i];
                        card.Attributes.Add(temp);
                        break;
                    case 1:
                        temp.AttributeValue = int.Parse(args[i]);
                        break;
                    case 2:
                        temp.AttributeType = ConvertToTrumpsType(args[i]);
                        break;
                    default:
                        break;
                }

            }

            return card;
        }

        public static TrumpsType ConvertToTrumpsType(string val)
        {
            switch (val)
            {
                case "Red":
                    return TrumpsType.Red;
                    break;
                case "Blue":
                    return TrumpsType.Blue;
                    break;
                case "Green":
                    return TrumpsType.Green;
                    break;
                default:
                    return (TrumpsType)new Random().Next(1, 3);
                    break;
            }
        }
    }
}
