using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAMSE
{
    public static class ByteOperationExtensions
    {
        private static bool IsEmptyLocate(byte[] array, byte[] candidate) => array == null
                                                                             || candidate == null
                                                                             || array.Length == 0
                                                                             || candidate.Length == 0
                                                                             || candidate.Length > array.Length;

        private static bool IsMatch(byte[] array,
                                    long position,
                                    byte[] candidate) =>
            candidate.Length <= array.Length - position && !candidate.Where((t, i) => array[position + i] != t).Any();

        public static IEnumerable<long> FindIndexOf(this byte[] self, byte[] candidate)
        {
            if (!IsEmptyLocate(self, candidate))
                for (var i = 0; i < self.Length; i++)
                {
                    if (!IsMatch(self, i, candidate))
                        continue;

                    yield return i;
                }
        }
    }
}
