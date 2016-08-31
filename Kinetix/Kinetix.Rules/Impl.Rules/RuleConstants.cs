using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules
{
    public class RuleConstants
    {
        private readonly IDictionary<string, string> constants = new Dictionary<string, string>();

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
