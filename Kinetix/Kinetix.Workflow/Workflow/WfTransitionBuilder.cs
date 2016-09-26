using System.Diagnostics;
using Kinetix.Workflow.model;


namespace Kinetix.Workflow {
    public class WfTransitionBuilder {
        private readonly static string DEFAULT_VALUE_NAME = "default";

        private string myName;
        private int wfadIdFrom;
        private int wfadIdTo;
        private int wfwdId;

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="wfadIdFrom">wfadIdFrom.</param>
        /// <param name="wfadIdTo">wfadIdTo.</param>
        public WfTransitionBuilder(int? wfwdId, int? wfadIdFrom, int? wfadIdTo) {
            Debug.Assert(wfadIdFrom != null);
            Debug.Assert(wfadIdTo != null);
            //---
            this.wfadIdFrom = (int)wfadIdFrom;
            this.wfadIdTo = (int)wfadIdTo;
            this.wfwdId = wfwdId.Value;
        }

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="name">name.</param>
        /// <returns>The Builder.</returns>
        public WfTransitionBuilder WithName(string name) {
            myName = name;
            return this;
        }

        public WfTransitionDefinition build() {
            WfTransitionDefinition wfTransitionDefinition = new WfTransitionDefinition();
            wfTransitionDefinition.Name = (myName == null ? DEFAULT_VALUE_NAME : myName);
            wfTransitionDefinition.WfadIdFrom = wfadIdFrom;
            wfTransitionDefinition.WfadIdTo = wfadIdTo;
            wfTransitionDefinition.WfwdId = wfwdId;
            return wfTransitionDefinition;
        }

    }
}
