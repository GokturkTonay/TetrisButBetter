using System.Collections.Generic;

namespace Tomino.Model
{
    public class Deck
    {
        // Hangi parça tipinden (I, T, S vb.) kaç adet kaldýðýný tutar
        public Dictionary<PieceType, int> PieceCounts { get; private set; }

        public Deck()
        {
            PieceCounts = new Dictionary<PieceType, int>
            {
                { PieceType.I, 4 },
                { PieceType.J, 4 },
                { PieceType.L, 4 },
                { PieceType.O, 4 },
                { PieceType.S, 4 },
                { PieceType.T, 4 },
                { PieceType.Z, 4 },
                { PieceType.Plus, 1 }
            };
        }

    }
}