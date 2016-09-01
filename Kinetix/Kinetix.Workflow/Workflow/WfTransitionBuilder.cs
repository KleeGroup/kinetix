using Kinetix.Workflow.model;
using System.Diagnostics;



namespace Kinetix.Workflow
{
    public class WfTransitionBuilder
    {
        private readonly static string DEFAULT_VALUE_NAME = "default";

        private string myName;
        private long wfadIdFrom;
    	private long wfadIdTo;

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="wfadIdFrom">wfadIdFrom.</param>
        /// <param name="wfadIdTo">wfadIdTo.</param>
        public WfTransitionBuilder(long? wfadIdFrom, long? wfadIdTo)
        {
            Debug.Assert(wfadIdFrom != null);
            Debug.Assert(wfadIdTo != null);
            //---
            this.wfadIdFrom = (long) wfadIdFrom;
            this.wfadIdTo = (long) wfadIdTo;
        }

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="name">name.</param>
        /// <returns>The Builder.</returns>
        public WfTransitionBuilder WithName(string name)
        {
            myName = name;
            return this;
        }

        public WfTransitionDefinition build()
        {
            WfTransitionDefinition wfTransitionDefinition = new WfTransitionDefinition();
            wfTransitionDefinition.Name = (myName == null ? DEFAULT_VALUE_NAME : myName);
            wfTransitionDefinition.WfadIdFrom = wfadIdFrom;
            wfTransitionDefinition.WfadIdTo = wfadIdTo;
            return wfTransitionDefinition;
        }

    }
}
