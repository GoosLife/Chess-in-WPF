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
        /// <summary>
        /// Index for De Bruijn multiplication.
        /// </summary>
        static readonly int[] index64 = { 
            0, 47,  1, 56, 48, 27,  2, 60,
            57, 49, 41, 37, 28, 16,  3, 61,
            54, 58, 35, 52, 50, 42, 21, 44,
            38, 32, 29, 23, 17, 11,  4, 62,
            46, 55, 26, 59, 40, 36, 15, 53,
            34, 51, 20, 43, 31, 22, 10, 45,
            25, 39, 14, 33, 19, 30,  9, 24,
            13, 18,  8, 12,  7,  6,  5, 63 
        };

        const ulong deBruijn64 = 0x03f79d71b4cb0a89;

        public static int GetLeastSignificant1Bit(ulong value)
        {
            value &= ~(value - 1);

            // Get the board index value of the ls1b square by finding the exponent of the least significant bits value
            return (int)(Math.Log(value) / Math.Log(2));
        }

        /// <summary>
        /// Get most significant 1 bit, utilizing De Bruijn multiplication.
        /// </summary>
        /// <param name="value">Bitboard to scan</param>
        /// <returns>Index of proposed end square for move.</returns>
        public static int GetMostSignificant1Bit(ulong value)
        {
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;

            return index64[(value * deBruijn64) >> 58];
        }
    }
}
