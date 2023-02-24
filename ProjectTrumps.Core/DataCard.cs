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
        public int EnhanceCountdown { get; set; }

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

        private string _affilatedName;
        public string AffiliatedName 
        {
            get => IsEnhanced ? "ENHANCED" : _affilatedName;
            set => _affilatedName = value; 
        }

        public bool IsEnhanced { get; set; } = false;

        public float CurrentPowerRating => (CurrentAttributes.Sum(p => p.AttributeValue) / CurrentAttributes.Count) + (CurrentAttributes.Count(p => p.AttributeValue > 9));
        public float OriginalPowerRating => (OriginalAttributes.Sum(p => p.AttributeValue) / CurrentAttributes.Count) + (CurrentAttributes.Count(p => p.AttributeValue > 9));

        public ColourType Type { get; set; }
        public ColourType OriginalType { get; set; }


        /// <summary>
        /// Attributes Currently in use
        /// </summary>
        public IList<DataCardAttributes> CurrentAttributes { get; set; } = new List<DataCardAttributes>();

        /// <summary>
        /// Original Card values
        /// </summary>
        public IList<DataCardAttributes> OriginalAttributes { get; set; } = new List<DataCardAttributes>();

        /// <summary>
        /// Attributes of Main card pre change
        /// </summary>
        public IList<DataCardAttributes> StoredAttributes { get; set; } = new List<DataCardAttributes>();       

        public bool HasSwapped => StoredAttributes.Any();

        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        public void ReplaceAttributes(DataCard newCard)
        {
            AffiliatedName = newCard.OriginalName;

            CurrentAttributes.Clear();
            foreach (var newAttr in newCard.CurrentAttributes)
            {
                CurrentAttributes.Add(new DataCardAttributes
                {
                    AttributeName = newAttr.AttributeName,
                    AttributeType = newAttr.AttributeType,
                    AttributeValue= newAttr.AttributeValue,
                });
            }
        }
        public void RestoreStoredAttributes(bool restoreHealth = true)
        {
            AffiliatedName = "";

            if (restoreHealth)
                FullHeal();

            Type = OriginalType;

            CurrentAttributes.Clear();
            foreach (var newAttr in StoredAttributes)
            {
                CurrentAttributes.Add(new DataCardAttributes
                {
                    AttributeName = newAttr.AttributeName,
                    AttributeType = newAttr.AttributeType,
                    AttributeValue = newAttr.AttributeValue,
                });
            }
            StoredAttributes.Clear();
        }

        public void CopyStoredAttributesToCurrent()
        {
            Type = OriginalType;

            CurrentAttributes.Clear();
            foreach (var newAttr in StoredAttributes)
            {
                CurrentAttributes.Add(new DataCardAttributes
                {
                    AttributeName = newAttr.AttributeName,
                    AttributeType = newAttr.AttributeType,
                    AttributeValue = newAttr.AttributeValue,
                });
            }            
        }

        public void RestoreAttributes(bool restoreHealth = true)
        {
            AffiliatedName = "";

            if (restoreHealth)
                FullHeal();

            CurrentAttributes.Clear();
            foreach (var newAttr in OriginalAttributes)
            {
                CurrentAttributes.Add(new DataCardAttributes
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

        public void FuseAttributes(DataCard fuseCard)
        {
            RestoreStoredAttributes(false);
            var newHp = (fuseCard.MaxHealth + Health) / 2;
            Health = Health < newHp ? newHp : Health;

            for (int i = 0; i < CurrentAttributes.Count; i++)
            {
                var attrValue = (CurrentAttributes[i].AttributeValue + fuseCard.OriginalAttributes[i].AttributeValue) / 2;
                CurrentAttributes[i].AttributeValue = attrValue;
            }
        }

        public void EnhanceUsingTributes(List<DataCard> tributes)
        {
            CopyStoredAttributesToCurrent();

            AffiliatedName = "ENHANCED";

            for (int i = 0; i < CurrentAttributes.Count; i++)
            {
                var enhancementValue = tributes.Sum(p => p.CurrentAttributes[i].AttributeValue) / tributes.Count;
                CurrentAttributes[i].AttributeValue += enhancementValue;
            }

            EnhanceCountdown = 3;
        }

        public void EvaluateEnhance()
        {
            if (EnhanceCountdown > 0)
            {
                EnhanceCountdown--;
            }

            if (EnhanceCountdown == 0)
            {
                AffiliatedName = "";
                RestoreStoredAttributes(false);
            }
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
        IList<DataCardAttributes> CurrentAttributes { get; set; }
    }

    public enum ColourType
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
    }

    public class DataCardAttributes
    {        
        public string AttributeName { get; set; }        
        public int AttributeValue { get; set; }
        public ColourType AttributeType { get; set; }

        public override string ToString()
        {
            return $"{AttributeName},{AttributeValue},{AttributeType.ToString()},";
        }
    }

    public class DataDeck : List<DataCard>
    {
        public bool HasCards(int count)
        {
            return Count >= count;
        }

        public DataCard DrawTopCard()
        {
            if (Count <= 0)
                return null;
                
            var topCard = this[0];
            RemoveAt(0);

            return topCard;
        }

        public void Shuffle()
        {
            if (Count <= 1)            
                return;            

            for (int i = 0; i < Count; i++)
            {
                var card = this[i];

                var random = new Random().Next(0, this.Count);
                
                while(i != random)
                {
                    var tmp = this[random];
                    this[random] = card;
                    this[i] = tmp;
                }                
            }
        }
    }
}
