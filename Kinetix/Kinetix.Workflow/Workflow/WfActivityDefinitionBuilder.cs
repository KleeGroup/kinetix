using Kinetix.Workflow.model;
using System;
using System.Diagnostics;

namespace Kinetix.Workflow
{
    public class WfActivityDefinitionBuilder
    {
        private int MyWfwdId;
        private string MyName;
	    private int? MyLevel;
        private WfCodeMultiplicityDefinition MyWfCodeMultiplicityDefinition;

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="wfadIdFrom">wfadIdFrom.</param>
        /// <param name="wfadIdTo">wfadIdTo.</param>
        public WfActivityDefinitionBuilder(string name, int wfwdId)
        {
            Debug.Assert(name != null);
            //---
            this.MyName = name;
            this.MyWfwdId = wfwdId;
            MyWfCodeMultiplicityDefinition = WfCodeMultiplicityDefinition.Sin;
        }

        /// <summary>
        /// Optionnal level.
        /// </summary>
        /// <param name="level">the level of the activity definition.</param>
        /// <returns>The Builder.</returns>
        public WfActivityDefinitionBuilder WithLevel(int level)
        {
            MyLevel = level;
            return this;
        }

        /**
         * Optionnal multiplicity
         * @param wfmdCode
         * @return the builder
         */
        public WfActivityDefinitionBuilder WithMultiplicity(string wfmdCode)
        {
            MyWfCodeMultiplicityDefinition = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), wfmdCode);
            return this;
        }

        public WfActivityDefinition Build()
        {
            WfActivityDefinition wfActivityDefinition = new WfActivityDefinition();
            wfActivityDefinition.Name = MyName;
            wfActivityDefinition.Level = MyLevel;
            // Multiplicity : Single by default
            wfActivityDefinition.WfmdCode = MyWfCodeMultiplicityDefinition.ToString();
            wfActivityDefinition.WfwdId = MyWfwdId;
            return wfActivityDefinition;
        }

    }
}
