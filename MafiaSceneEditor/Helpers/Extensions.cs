using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAMSE.Helpers
{
    public static class Extensions
    {
        public static string[] SplitByCorrect(this string str)
        {
            int howManyR = str.Count(x => x == '\r');
            int howManyN = str.Count(x => x == '\n');

            if (howManyR < (howManyN - 10))
            {
                return str.Split("\n");
            }
            else
            {
                return str.Split("\r\n");
            }
        }
    }
}
