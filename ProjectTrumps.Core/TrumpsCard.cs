using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTrumps.Core
{
    public class TrumpsCard : ITrumpsCard
    {
        public string Name { get; set; }
        public TrumpsType Type { get; set; }
        public IList<TrumpsAttribute> Attributes { get; set; } = new List<TrumpsAttribute>();
        public int Health { get; set; } = 100;
    }

    public interface ITrumpsCard
    {        
        string Name { get; set; }
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
