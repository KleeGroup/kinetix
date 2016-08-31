using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Rules
{
    public sealed class SimpleRuleValidatorPlugin : IRuleValidatorPlugin
    {

        public bool IsRuleValid(long idActivityDefinition, IList<RuleDefinition> rules, RuleContext ruleContext)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
