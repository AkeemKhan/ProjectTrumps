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
            DataCardAttributes temp = null;
            DataCardAttributes origTemp = null;

            // TODO: Validate string args

            var saveCluster = args[0].Split("|");
            string id = "";

            if (saveCluster.Length > 1)
            {
                // TODO: Validate cluster

                card.Id = saveCluster[0];
                card.MaxHealth = int.Parse(saveCluster[1]);
                card.Health = int.Parse(saveCluster[1]);
                card.Level = int.Parse(saveCluster[2]);
            }
            else
            {
                card.Id = Guid.NewGuid().ToString();
                card.MaxHealth = 100;
                card.Health = 100;
            }

            card.OriginalName = args[1];
            card.AffiliatedName = args[1];
            card.OriginalType = ConvertToTrumpsType(args[2]);
            card.Type = ConvertToTrumpsType(args[2]);            

            for (int i = 3; i < args.Length; i++)
            {
                var paramType = i % 3;

                if (args[i] == "")
                    break;

                switch (paramType)
                {
                    case 0:
                        temp = new DataCardAttributes();
                        origTemp = new DataCardAttributes();

                        temp.AttributeName = args[i];
                        origTemp.AttributeName = args[i];

                        card.CurrentAttributes.Add(temp);
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

        public DataCard GenerateCard(string name, IList<DataCardAttributes> attributes, ColourType type) 
        {
            var card = new DataCard();

            card.Id = Guid.NewGuid().ToString();
            card.Level = 1;
            card.CurrentAttributes = CopyAttributes(attributes);
            card.OriginalName = name;
            card.OriginalType = type;
            card.Type = type;

            foreach (var attr in card.CurrentAttributes)
            {
                attr.AttributeValue = 5;
                attr.AttributeType = (ColourType)new Random().Next(1, 4);
            }

            card.OriginalAttributes = CopyAttributes(card.CurrentAttributes);

            return card;
        }

        public IList<DataCardAttributes> CopyAttributes(IList<DataCardAttributes> attributes) 
        { 
            var list = new List<DataCardAttributes>();
            foreach (var attr in attributes)
                list.Add(new DataCardAttributes 
                { 
                    AttributeName = attr.AttributeName, 
                    AttributeValue = attr.AttributeValue, 
                    AttributeType = attr.AttributeType 
                });

            return list;
        }

        public static ColourType ConvertToTrumpsType(string val)
        {
            switch (val)
            {
                case "Red":
                    return ColourType.Red;
                    break;
                case "Blue":
                    return ColourType.Blue;
                    break;
                case "Green":
                    return ColourType.Green;
                    break;
                default:
                    return (ColourType)new Random().Next(1, 4);
                    break;
            }
        }
    }
}
