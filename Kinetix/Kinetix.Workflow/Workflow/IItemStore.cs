using System.Collections.Generic;

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

        /// <summary>
        /// Get a list of items
        /// </summary>
        /// <param name="itemIds">List of Items Ids.</param>
        /// <returns>A dictionary with the itemId as a key the the object as the associated value.</returns>
        IDictionary<int, object> ReadItems(IList<int> itemIds);

    }
}
