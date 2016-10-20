using System;
using System.Collections.Generic;

namespace Kinetix.Rules.Test
{
    internal class SelectorEqualityComparer : IEqualityComparer<SelectorDefinition>
    {
        public bool Equals(SelectorDefinition x, SelectorDefinition y)
        {
            if (object.ReferenceEquals(x, y)) return true;

            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) return false;

            return x.Id == y.Id && x.ItemId == y.ItemId && x.GroupId == y.GroupId;
        }

        public int GetHashCode(SelectorDefinition obj)
        {
            if (object.ReferenceEquals(obj, null)) return 0;

            int hashCreationDate = obj.CreationDate == null ? 0 : obj.CreationDate.GetHashCode();
            int hashId = obj.Id.GetHashCode();
            int hashItemId = obj.ItemId.GetHashCode();
            int hashGroupId = obj.GroupId.GetHashCode();
            
            return hashCreationDate ^ hashId ^ hashItemId ^ hashGroupId;
        }
    }
}