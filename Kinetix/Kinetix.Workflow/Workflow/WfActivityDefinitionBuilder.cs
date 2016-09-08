using Kinetix.Workflow.model;
using System;
using System.Diagnostics;

namespace Kinetix.Workflow
{
    public class WfActivityDefinitionBuilder
    {
        private int myWfwdId;
        private string myName;
	    private int? myLevel;
        private WfCodeMultiplicityDefinition myWfCodeMultiplicityDefinition;

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="wfadIdFrom">wfadIdFrom.</param>
        /// <param name="wfadIdTo">wfadIdTo.</param>
        public WfActivityDefinitionBuilder(string name, int wfwdId)
        {
            Debug.Assert(name != null);
            //---
            this.myName = name;
            this.myWfwdId = wfwdId;
            myWfCodeMultiplicityDefinition = WfCodeMultiplicityDefinition.Sin;
        }

        /// <summary>
        /// Optionnal level.
        /// </summary>
        /// <param name="level">the level of the activity definition.</param>
        /// <returns>The Builder.</returns>
        public WfActivityDefinitionBuilder WithLevel(int level)
        {
            myLevel = level;
            return this;
        }

        /**
         * Optionnal multiplicity
         * @param wfmdCode
         * @return the builder
         */
        public WfActivityDefinitionBuilder WithMultiplicity(string wfmdCode)
        {
            myWfCodeMultiplicityDefinition = (WfCodeMultiplicityDefinition)Enum.Parse(typeof(WfCodeMultiplicityDefinition), wfmdCode);
            return this;
        }

        public WfActivityDefinition Build()
        {
            WfActivityDefinition wfActivityDefinition = new WfActivityDefinition();
            wfActivityDefinition.Name = myName;
            wfActivityDefinition.Level = myLevel;
            // Multiplicity : Single by default
            wfActivityDefinition.WfmdCode = myWfCodeMultiplicityDefinition.ToString();
            wfActivityDefinition.WfwdId = myWfwdId;
            return wfActivityDefinition;
        }

    }
}
