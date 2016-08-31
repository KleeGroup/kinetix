using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow
{
    public interface IItemStore
    {

        /// <summary>
        /// Track a new item.
        ///  /!\ No item will be created. It will only be tracked
        /// </summary>
        /// <param name="auditTraceCriteria">Criteria.</param>
        /// <returns>The matching audit traces.</returns>
        void AddItem(long itemId, object item);

        /// <summary>
        /// Get an item
        /// </summary>
        /// <param name="itemId">ItemId.</param>
        /// <returns>The matching audit traces.</returns>
        object ReadItem(long itemId);
    }
}
