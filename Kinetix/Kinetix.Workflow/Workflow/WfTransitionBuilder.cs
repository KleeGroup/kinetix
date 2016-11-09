using System.Diagnostics;
using Kinetix.Workflow.model;


namespace Kinetix.Workflow {
    public class WfTransitionBuilder {
        private string MyName;
        private int WfadIdFrom;
        private int WfadIdTo;
        private int WfwdId;

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="wfadIdFrom">wfadIdFrom.</param>
        /// <param name="wfadIdTo">wfadIdTo.</param>
        public WfTransitionBuilder(int? wfwdId, int? wfadIdFrom, int? wfadIdTo) {
            Debug.Assert(wfadIdFrom != null);
            Debug.Assert(wfadIdTo != null);
            //---
            this.WfadIdFrom = (int)wfadIdFrom;
            this.WfadIdTo = (int)wfadIdTo;
            this.WfwdId = wfwdId.Value;
        }

        /// <summary>
        /// Builder for transitions.
        /// </summary>
        /// <param name="name">name.</param>
        /// <returns>The Builder.</returns>
        public WfTransitionBuilder WithName(string name) {
            MyName = name;
            return this;
        }

        public WfTransitionDefinition Build() {
            WfTransitionDefinition wfTransitionDefinition = new WfTransitionDefinition();
            wfTransitionDefinition.Name = (MyName == null ? WfCodeTransition.Default.ToString() : MyName);
            wfTransitionDefinition.WfadIdFrom = WfadIdFrom;
            wfTransitionDefinition.WfadIdTo = WfadIdTo;
            wfTransitionDefinition.WfwdId = WfwdId;
            return wfTransitionDefinition;
        }

    }
}
