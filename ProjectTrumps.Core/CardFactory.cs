using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class CardFactory
    {
        public static CardFactory Instance { get; private set; } = new CardFactory();

        public DataCard CreateCard(string[] args)
        {
            var card = new DataCard();
            TrumpsAttribute temp = null;
            TrumpsAttribute origTemp = null;

            card.OriginalName = args[1];
            card.AffiliatedName = args[1];
            card.Type = ConvertToTrumpsType(args[2]);

            for (int i = 3; i < args.Length; i++)
            {
                var paramType = i % 3;

                switch (paramType)
                {
                    case 0:
                        temp = new TrumpsAttribute();
                        origTemp = new TrumpsAttribute();

                        temp.AttributeName = args[i];
                        origTemp.AttributeName = args[i];

                        card.Attributes.Add(temp);
                        card.OriginalAttributes.Add(temp);
                        break;
                    case 1:
                        temp.AttributeValue = int.Parse(args[i]);
                        origTemp.AttributeValue = int.Parse(args[i]);
                        break;
                    case 2:
                        temp.AttributeType = ConvertToTrumpsType(args[i]);
                        origTemp.AttributeType = ConvertToTrumpsType(args[i]);
                        break;
                    default:
                        break;
                }

            }

            return card;
        }

        public DataCard GenerateCard (string name, IList<TrumpsAttribute> attributes, TrumpsType type) 
        {
            var card = new DataCard();

            card.Attributes = CopyAttributes(attributes);
            card.OriginalName = name;
            card.Type= type;

            foreach (var attr in card.Attributes)
            {
                attr.AttributeValue = 5;
                attr.AttributeType = (TrumpsType)new Random().Next(1, 3);
            }

            card.OriginalAttributes = CopyAttributes(card.Attributes);

            return card;
        }

        public IList<TrumpsAttribute> CopyAttributes(IList<TrumpsAttribute> attributes) 
        { 
            var list = new List<TrumpsAttribute>();
            foreach (var attr in attributes)
                list.Add(new TrumpsAttribute 
                { 
                    AttributeName = attr.AttributeName, 
                    AttributeValue = attr.AttributeValue, 
                    AttributeType = attr.AttributeType 
                });

            return list;
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
