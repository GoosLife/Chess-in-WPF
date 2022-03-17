using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class BitHelper
    {
        public static int GetLeastSignificant1Bit(ulong value)
        {
            value &= ~(value - 1);

            // Get the board index value of the ls1b square by finding the exponent of the least significant bits value
            return (int)(Math.Log(value) / Math.Log(2));
        }
    }
}
