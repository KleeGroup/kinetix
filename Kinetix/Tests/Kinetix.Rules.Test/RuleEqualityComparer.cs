using System;
using System.Collections.Generic;

namespace Kinetix.Rules.Test
{
    internal class RuleEqualityComparer : IEqualityComparer<RuleDefinition>
    {
        public bool Equals(RuleDefinition x, RuleDefinition y)
        {
            if (object.ReferenceEquals(x, y)) return true;

            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) return false;

            return x.Id == y.Id && x.ItemId == y.ItemId && x.Label == y.Label;
        }

        public int GetHashCode(RuleDefinition obj)
        {
            if (object.ReferenceEquals(obj, null)) return 0;

            int hashCreationDate = obj.CreationDate == null ? 0 : obj.CreationDate.GetHashCode();
            int hashId = obj.Id.GetHashCode();
            int hashItemId = obj.ItemId.GetHashCode();
            int hashLabel = obj.Label.GetHashCode();
            
            return hashCreationDate ^ hashId ^ hashItemId ^ hashLabel;
        }
    }
}