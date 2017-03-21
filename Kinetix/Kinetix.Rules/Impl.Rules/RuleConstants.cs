using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Rules
{
    public class RuleConstants
    {
        private static readonly RuleConstants EmptyConstants = new RuleConstants();
        private readonly IDictionary<string, string> constants = new Dictionary<string, string>();

        public static RuleConstants EmptyRuleConstants
        {
            get
            {
                return EmptyConstants;
            }
        }

        public void AddConstant(string key, string value)
        {
            constants.Add(key, value);
        }


        public IEnumerable<KeyValuePair<string, string>> GetValues() 
        {
            return constants.AsEnumerable();
        }
    }
}
