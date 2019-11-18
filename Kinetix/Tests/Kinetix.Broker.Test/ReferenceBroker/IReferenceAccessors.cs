using System.Collections.Generic;
using System.ServiceModel;
using Kinetix.ServiceModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Reference accessors.
    /// </summary>
    [ServiceContract]
    public interface IReferenceAccessors {
        /// <summary>
        /// Reference accessor for type ReferenceBean.
        /// </summary>
        [ReferenceAccessor]
        [OperationContract]
        ICollection<ReferenceBean> LoadReferenceBeanList();
    }
}
