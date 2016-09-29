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
        /// <param name="itemId">itemId to track.</param>
        /// <param name="item">item to track.</param>
        void AddItem(int itemId, object item);

        /// <summary>
        /// Get an item
        /// </summary>
        /// <param name="itemId">ItemId.</param>
        /// <returns>the object corresponding to the itemId.</returns>
        object ReadItem(int itemId);
    }
}
