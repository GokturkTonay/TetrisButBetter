using System.Collections.Generic;
using System.Linq; // Toplam hesaplamak için gerekli

namespace Tomino.Model
{
    public class Deck
    {
        public Dictionary<PieceType, int> PieceCounts { get; private set; }

        // YENİ: Mağazadan alınan bombaların sayısını burada tutacağız
        public int BombCount { get; set; } = 0; 

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

        // Toplam taş sayısına bombaları da ekliyoruz ki ekranda doğru sayı yazsın
        public int TotalCount => PieceCounts.Values.Sum() + BombCount;

        public void RemovePiece(PieceType type)
        {
            if (PieceCounts.ContainsKey(type) && PieceCounts[type] > 0)
            {
                PieceCounts[type]--;
            }
        }

        // YENİ: Desteden bomba çekilince sayıyı düşmek için
        public void RemoveBomb()
        {
            if (BombCount > 0) BombCount--;
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

            // Not: Reset atıldığında bombayı sıfırlamıyoruz çünkü mağazadan parayla alındı,
            // oyuncu onu kullanana kadar destesinde beklemeli!
        }
    }
}