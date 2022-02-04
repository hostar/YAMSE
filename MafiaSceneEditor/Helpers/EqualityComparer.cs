using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using YAMSE.DataLayer;

namespace YAMSE.Helpers
{
    public class EqualityComparer : IEqualityComparer<Dnc>
    {
        public bool Equals([AllowNull] Dnc x, [AllowNull] Dnc y)
        {
            if (x.Name != y.Name)
            {
                return false;
            }

            if (!DncMethods.RawDataEqual(x, y))
            {
                return false;
            }
            return true;
        }

        public int GetHashCode([DisallowNull] Dnc obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
