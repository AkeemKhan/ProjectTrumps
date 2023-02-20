using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class DataCard : IDataCard
    {

        public string DisplayName 
        { 
            get 
            { 
                if (OriginalName == AffiliatedName || string.IsNullOrEmpty(AffiliatedName))
                    return OriginalName; 

                return $"{OriginalName} ({AffiliatedName})";
            } 
        }

        public string OriginalName { get; set; }
        public string AffiliatedName { get; set; }

        public float CurrentPowerRating => (Attributes.Sum(p => p.AttributeValue) / Attributes.Count) + (Attributes.Count(p => p.AttributeValue > 9));
        public float OriginalPowerRating => (OriginalAttributes.Sum(p => p.AttributeValue) / Attributes.Count) + (Attributes.Count(p => p.AttributeValue > 9));


        public TrumpsType Type { get; set; }
        public IList<TrumpsAttribute> Attributes { get; set; } = new List<TrumpsAttribute>();
        public IList<TrumpsAttribute> OriginalAttributes { get; set; } = new List<TrumpsAttribute>();
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        public void ReplaceAttributes(DataCard newCard)
        {
            AffiliatedName = newCard.OriginalName;

            Attributes.Clear();
            foreach (var newAttr in newCard.Attributes)
            {
                Attributes.Add(new TrumpsAttribute
                {
                    AttributeName = newAttr.AttributeName,
                    AttributeType = newAttr.AttributeType,
                    AttributeValue= newAttr.AttributeValue,
                });
            }
        }
    }

    public interface IDataCard
    {        
        string OriginalName { get; set; }
        TrumpsType Type { get; set; }
        IList<TrumpsAttribute> Attributes { get; set; }
    }

    public enum TrumpsType
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
    }

    public class TrumpsAttribute
    {        
        public string AttributeName { get; set; }        
        public int AttributeValue { get; set; }
        public TrumpsType AttributeType { get; set; }
    }
}
