using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class DataCard : IDataCard
    {
        public string Id { get; set; }
        public int Level { get; set; } = 1;    

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

        public float CurrentPowerRating => (CurrentAttributes.Sum(p => p.AttributeValue) / CurrentAttributes.Count) + (CurrentAttributes.Count(p => p.AttributeValue > 9));
        public float OriginalPowerRating => (OriginalAttributes.Sum(p => p.AttributeValue) / CurrentAttributes.Count) + (CurrentAttributes.Count(p => p.AttributeValue > 9));

        public ColourType Type { get; set; }
        public ColourType OriginalType { get; set; }


        public IList<TrumpsAttribute> CurrentAttributes { get; set; } = new List<TrumpsAttribute>();
        public IList<TrumpsAttribute> OriginalAttributes { get; set; } = new List<TrumpsAttribute>();
        public IList<TrumpsAttribute> MainAttributes { get; set; } = new List<TrumpsAttribute>();

        public bool HasSwapped => MainAttributes.Any();

        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        public void ReplaceAttributes(DataCard newCard)
        {
            AffiliatedName = newCard.OriginalName;

            CurrentAttributes.Clear();
            foreach (var newAttr in newCard.CurrentAttributes)
            {
                CurrentAttributes.Add(new TrumpsAttribute
                {
                    AttributeName = newAttr.AttributeName,
                    AttributeType = newAttr.AttributeType,
                    AttributeValue= newAttr.AttributeValue,
                });
            }
        }
        public void RestoreMainAttributes(bool restoreHealth = true)
        {
            AffiliatedName = "";

            if (restoreHealth)
                FullHeal();

            Type = OriginalType;

            CurrentAttributes.Clear();
            foreach (var newAttr in MainAttributes)
            {
                CurrentAttributes.Add(new TrumpsAttribute
                {
                    AttributeName = newAttr.AttributeName,
                    AttributeType = newAttr.AttributeType,
                    AttributeValue = newAttr.AttributeValue,
                });
            }
            MainAttributes.Clear();
        }

        public void RestoreAttributes(bool restoreHealth = true)
        {
            AffiliatedName = "";

            if (restoreHealth)
                FullHeal();

            CurrentAttributes.Clear();
            foreach (var newAttr in OriginalAttributes)
            {
                CurrentAttributes.Add(new TrumpsAttribute
                {
                    AttributeName = newAttr.AttributeName,
                    AttributeType = newAttr.AttributeType,
                    AttributeValue = newAttr.AttributeValue,
                });
            }
        }

        public void EnhanceAllAttributes(int value)
        {
            for (int i = 0; i < CurrentAttributes.Count; i++)
            {
                EnhanceAttribute(i, value);
            }
        }

        public void EnhanceOriginalAttribute(int index, int value)
        {
            CurrentAttributes[index].AttributeValue = OriginalAttributes[index].AttributeValue;
            CurrentAttributes[index].AttributeValue += value;
        }

        public void EnhanceAttribute(int index, int value)
        {
            CurrentAttributes[index].AttributeValue += value;
        }

        public void ModifyAttributeType(int index, ColourType type)
        {
            CurrentAttributes[index].AttributeType = type;
        }

        public void UnifyAttributesToCardType(bool restore = false)
        {
            if (restore)
                RestoreAttributes(false);

            for (int i = 0; i < CurrentAttributes.Count; i++)
            {
                ModifyAttributeType(i, Type);
            }
        }

        public void EnhanceHealth(int value)
        {
            MaxHealth += value;
            Health += value;
        }

        public void Heal(int value)
        {
            Health += value;
        }

        public void FullHeal()
        {
            Health = MaxHealth;
        }

        public void AllEnhance(int hpValue, int attrValue)
        {
            EnhanceHealth(hpValue);
            EnhanceAllAttributes(attrValue);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            // ID|MAXHP|LEVEL
            var saveSegment = $"{Id}|{MaxHealth.ToString()}|{Level.ToString()}";
            sb.Append(saveSegment);
            sb.Append(",");
            sb.Append(OriginalName);
            sb.Append(",");
            sb.Append(Type.ToString());
            sb.Append(",");

            foreach (var attr in CurrentAttributes)
            {
                sb.Append(attr.ToString());
            }

            return sb.ToString();
        }
    }

    public interface IDataCard
    {        
        string OriginalName { get; set; }
        ColourType Type { get; set; }
        IList<TrumpsAttribute> CurrentAttributes { get; set; }
    }

    public enum ColourType
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
        public ColourType AttributeType { get; set; }

        public override string ToString()
        {
            return $"{AttributeName},{AttributeValue},{AttributeType.ToString()},";
        }
    }
}
