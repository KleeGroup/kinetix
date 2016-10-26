using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kinetix.Workflow.Test
{
    public class MemoryItemStorePlugin : IItemStorePlugin
    {
        private IDictionary<int, object> inMemoryItemStore = new ConcurrentDictionary<int, object>();

        public void AddItem(int itemId, object item)
        {
            inMemoryItemStore[itemId] = item;
        }

        public object ReadItem(int itemId)
        {
            return inMemoryItemStore[itemId];
        }

        public IDictionary<int, object> ReadItems(IList<int> itemIds)
        {
            return inMemoryItemStore;
        }
    }
}
