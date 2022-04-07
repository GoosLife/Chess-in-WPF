using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class Ray
    {
        // Constants naming the ulong[][] rays indexes.
        #region Direction names to int
        public const int North = 0;
        public const int East = 1;
        public const int South = 2;
        public const int West = 3;
        public const int NorthEast = 4;
        public const int SouthEast = 5;
        public const int SouthWest = 6;
        public const int NorthWest = 7;
        #endregion

        // These rays will help determine the attacks of sliding pieces (rook, bishop and queen).
        public static ulong[][] Rays = new ulong[8][];

        /// <summary>
        /// Array of arrays containing all relevant information on which files, ranks and (anti-)diagonals can be reached from any given square.
        /// </summary>
        /// <returns></returns>
        public static ulong[][] GenerateRays()
        {
            Rays[East] = GetRaysEast();
            Rays[West] = GetRaysWest();
            Rays[North] = GetRaysNorth();
            Rays[South] = GetRaysSouth();
            Rays[NorthEast] = GetRaysNorthEast();
            Rays[NorthWest] = GetRaysNorthWest();
            Rays[SouthEast] = GetRaysSouthEast();
            Rays[SouthWest] = GetRaysSouthWest();

            return Rays;
        }

        /// <summary>
        /// Array containing all squares directly east of any given square.
        /// </summary>
        /// <returns></returns>
        public static ulong[] GetRaysEast()
        {
            ulong[] eastRays = new ulong[64];
            ulong ray = 0;
            ulong squareToAdd = 0;

            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    ray = 0;
                }
                else
                {
                    squareToAdd = (ulong)1 << i - 1;
                    ray = ray | squareToAdd;
                }

                eastRays[i] = ray;
            }
            return eastRays;
        }

        /// <summary>
        /// Array containing all squares directly west of any given square.
        /// </summary>
        /// <returns></returns>
        public static ulong[] GetRaysWest()
        {
            ulong[] westRays = new ulong[64];
            ulong ray = 0;
            ulong squareToAdd = 0;

            for (int i = 64; i > 0; i--)
            {
                if (i % 8 == 0)
                {
                    ray = 0;
                }
                else
                {
                    squareToAdd = (ulong)1 << i;
                    ray = ray | squareToAdd;
                }
                westRays[i - 1] = ray;
            }

            return westRays;
        }

        /// <summary>
        /// Array containing all squares directly north of any given square.
        /// </summary>
        /// <returns></returns>
        public static ulong[] GetRaysNorth()
        {
            ulong[] northRays = new ulong[64];

            ulong north = 0x0101010101010100;

            for (int i = 0; i < 64; i++, north <<= 1)
            {
                northRays[i] = north;
            }
            return northRays;
        }

        /// <summary>
        /// Array containing all squares directly south of any given square.
        /// </summary>
        /// <returns></returns>
        public static ulong[] GetRaysSouth()
        {
            ulong[] southRays = new ulong[64];

            ulong south = 0x0080808080808080;

            for (int i = 63; i >= 0; i--, south >>= 1)
            {
                southRays[i] = south;
            }


            return southRays;
        }


        // TODO: These all currently return empty movesets
        public static ulong[] GetRaysNorthEast()
        {
            //ulong[] northEastRays = new ulong[64];

            //ulong northEast = 0x8040201008040200;

            //for (int i = 0; i < 8; i++, northEast <<= 1)
            //{
            //    ulong moves = northEast;

            //    for (int j = 0; j < 8 * 8; j += 8, moves <<= 8)
            //    {
            //        northEastRays[j + i] = moves;
            //    }
            //}

            //return northEastRays;

            return new ulong[64];
        }

        public static ulong[] GetRaysNorthWest()
        {
            ulong[] northEastRays = new ulong[64];

            ulong northEast = 0x102040810204000;

            for (int i = 8; i > 0; i--, northEast <<= -1)
            {
                ulong moves = northEast;

                for (int j = 64; j > 0; j -= 8, moves <<= 8)
                {
                    northEastRays[j - i] = moves;
                }
            }

            return northEastRays;
        }

        public static ulong[] GetRaysSouthEast()
        {
            return new ulong[64];
        }

        public static ulong[] GetRaysSouthWest()
        {
            return new ulong[64];
        }
    }
}
