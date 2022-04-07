using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class Constants
    {
        #region Ranks, Files & Diagonals

        // Empty files

        public const ulong EmptyFileA = 0x7F7F7F7F7F7F7F7F;
        public const ulong EmptyFileB = 0xBFBFBFBFBFBFBFBF;

        public const ulong EmptyFileG = 0xFDFDFDFDFDFDFDFD;
        public const ulong EmptyFileH = 0xFEFEFEFEFEFEFEFE;

        // Fill ranks

        public const ulong Rank5 = 0x000000FF00000000;
        public const ulong Rank4 = 0x00000000FF000000;

        // Diagonals

        public const ulong Diagonal = 0x8040201008040201;
        public const ulong AntiDiagonal = 0x0102040810204080;

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

        #region Castling

        // King to and from square that indicates castling
        public const ulong WhiteCastlingShortMask = 0xA;
        public const ulong WhiteCastlingLongMask = 0x28;
        public const ulong BlackCastlingShortMask = 0xA00000000000000;
        public const ulong BlackCastlingLongMask = 0x2800000000000000;

        // Rook destination squares after castling
        public const ulong WhiteRookShortCastlingDestination = 0x4;
        public const ulong WhiteRookLongCastlingDestination = 0x10;
        public const ulong BlackRookShortCastlingDestination = 0x400000000000000;
        public const ulong BlackRookLongCastlingDestination = 0x1000000000000000;

        // Check squares between king & rook are clear
        public const ulong WhiteCastlingShortClear = 0xFFFFFFFFFFFFFFF9;
        public const ulong WhiteCastlingLongClear = 0xFFFFFFFFFFFFFF9F;
        public const ulong BlackCastlingShortClear = 0xF9FFFFFFFFFFFFFF;
        public const ulong BlackCastlingLongClear = 0x9FFFFFFFFFFFFFFF;
        #endregion
    }
}
