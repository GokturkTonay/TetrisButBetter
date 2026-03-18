using System.Collections.Generic;
using System.Linq; // Toplam hesaplamak için gerekli

namespace Tomino.Model
{
    public class Deck
    {
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

        public int TotalCount => PieceCounts.Values.Sum();

        public void RemovePiece(PieceType type)
        {
            if (PieceCounts.ContainsKey(type) && PieceCounts[type] > 0)
            {
                PieceCounts[type]--;
            }
        }

        public void Reset()
        {
            PieceCounts[PieceType.I] = 4;
            PieceCounts[PieceType.J] = 4;
            PieceCounts[PieceType.L] = 4;
            PieceCounts[PieceType.O] = 4;
            PieceCounts[PieceType.S] = 4;
            PieceCounts[PieceType.T] = 4;
            PieceCounts[PieceType.Z] = 4;
            PieceCounts[PieceType.Plus] = 1;
        }
    }
}