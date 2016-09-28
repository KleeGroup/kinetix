using Kinetix.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.Workflow
{
    public class WfActivityCriteria
    {
        public IList<RuleConditionCriteria> conditionCriteria
        {
            get;
            set;
        }

    }
}
