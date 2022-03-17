using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class Constants
    {
        #region Ranks and Files

        // Empty files

        public const ulong EmptyFileA = 0x7F7F7F7F7F7F7F7F;
        public const ulong EmptyFileB = 0xBFBFBFBFBFBFBFBF;

        public const ulong EmptyFileG = 0xFDFDFDFDFDFDFDFD;
        public const ulong EmptyFileH = 0xFEFEFEFEFEFEFEFE;

        // Fill ranks

        public const ulong Rank5 = 0x000000FF00000000;
        public const ulong Rank4 = 0x00000000FF000000;

        #endregion

        #region Starting Positions

        public const ulong WhiteKing = 8;
        public const ulong WhiteQueens = 16;
        public const ulong WhiteRooks = 129;
        public const ulong WhiteBishops = 36;
        public const ulong WhiteKnights = 66;
        public const ulong WhitePawns = 65280;

        public const ulong BlackKing = 576460752303423488;
        public const ulong BlackQueens = 1152921504606846976;
        public const ulong BlackRooks = 9295429630892703744;
        public const ulong BlackBishops = 2594073385365405696;
        public const ulong BlackKnights = 4755801206503243776;
        public const ulong BlackPawns = 71776119061217280;

        #endregion

        #region BitBoard Names
        public const string bbWhiteKing = "WhiteKing";
        public const string bbWhiteQueens = "WhiteQueens";
        public const string bbWhiteRooks = "WhiteRooks";
        public const string bbWhiteBishops = "WhiteBishops";
        public const string bbWhiteKnights = "WhiteKnights";
        public const string bbWhitePawns = "WhitePawns";
        public const string bbWhitePieces = "WhitePieces";

        public const string bbBlackKing = "BlackKing";
        public const string bbBlackQueens = "BlackQueens";
        public const string bbBlackRooks = "BlackRooks";
        public const string bbBlackBishops = "BlackBishops";
        public const string bbBlackKnights = "BlackKnights";
        public const string bbBlackPawns = "BlackPawns";
        public const string bbBlackPieces = "BlackPieces";

        public const string bbSquaresOccupied = "SquaresOccupied";
        #endregion
    }
}
