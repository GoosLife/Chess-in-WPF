using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class EnPassantAndCastling
    {
        private const byte WhiteShortCastleMask = 1;
        private const byte WhiteLongCastleMask = 2;
        private const byte BlackShortCastleMask = 4;
        private const byte BlackLongCastleMask = 8;

        private byte CastlingRights = 0x0f; // The last 4 bits in the byte (8 4 2 1) represent each players castling rights.

        // Get castling rights

        public bool CanWhiteCastleShort()
        {
            return (CastlingRights & WhiteShortCastleMask) > 0;
        }

        public bool CanWhiteCastleLong()
        {
            return (CastlingRights & WhiteLongCastleMask) > 0;
        }

        public bool CanBlackCastleShort()
        {
            return (CastlingRights & BlackShortCastleMask) > 0;
        }

        public bool CanBlackCastleLong()
        {
            return (CastlingRights & BlackLongCastleMask) > 0;
        }

        // Enable castling rights

        public void EnableWhiteShortCastling()
        {
            EnableCastlingRights(WhiteShortCastleMask);
        }

        public void EnableWhiteLongCastling()
        {
            EnableCastlingRights(WhiteLongCastleMask);
        }
        
        public void EnableBlackShortCastling()
        {
            EnableCastlingRights(BlackShortCastleMask);
        }

        public void EnableBlackLongCastling()
        {
            EnableCastlingRights(BlackLongCastleMask);
        }

        // Disable castling rights 

        public void DisableWhiteShortCastling()
        {
            DisableCastlingRights(WhiteShortCastleMask);
        }

        public void DisableWhiteLongCastling()
        {
            DisableCastlingRights(WhiteLongCastleMask);
        }

        public void DisableBlackShortCastling()
        {
            DisableCastlingRights(BlackShortCastleMask);
        }

        public void DisableBlackLongCastling()
        {
            DisableCastlingRights(BlackLongCastleMask);
        }

        // Toggle castling rights

        private void EnableCastlingRights(byte castlingMask)
        {
            CastlingRights = (byte)(CastlingRights | castlingMask);
        }

        private void DisableCastlingRights(byte castlingMask)
        {
            CastlingRights = (byte)(CastlingRights & (~castlingMask));
        }
    }
}
