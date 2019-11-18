using System.Collections.Generic;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Reference accessors.
    /// </summary>
    public partial class ReferenceAccessors : IReferenceAccessors {
        /// <summary>
        /// Reference accessor for type ReferenceBean.
        /// </summary>
        /// <returns></returns>
        public ICollection<ReferenceBean> LoadReferenceBeanList() {
            return TestStore<ReferenceBean>.PutList;
        }
    }
}
