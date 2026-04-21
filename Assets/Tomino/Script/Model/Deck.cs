using System.Collections.Generic;
using System.Linq;

namespace Tomino.Model
{
    /// <summary>
    /// Deste sistemi - her parçanın tür ve rengini takip eder.
    /// Eski PieceCounts sistemi + yeni List<(type, color)> birlikte çalışır.
    /// </summary>
    public class Deck
    {
        // Eski sistem (BalancedRandomPieceProvider uyumluluğu için)
        public Dictionary<PieceType, int> PieceCounts { get; private set; }
        
        // Yeni sistem (Deste UI için parça + renk takibi)
        public List<(PieceType type, int colorIndex)> AvailablePieces { get; private set; }
        
        // Mağazadan alınan bombalar
        public int BombCount { get; set; } = 0;

        public Deck()
        {
            InitializeDeck();
        }

        /// <summary>
        /// Destesini başlat - eski + yeni sistemi başlat.
        /// </summary>
        private void InitializeDeck()
        {
            // Eski sistem (type sayıları)
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

            // Yeni sistem (type + color kombinasyonları)
            AvailablePieces = new List<(PieceType, int)>();
            
            var pieceTypes = new[]
            {
                PieceType.I, PieceType.J, PieceType.L, PieceType.O,
                PieceType.S, PieceType.T, PieceType.Z, PieceType.Plus
            };

            // Her piece türü için 4 renk ekle
            foreach (var pType in pieceTypes)
            {
                for (int colorIndex = 0; colorIndex < 4; colorIndex++)
                {
                    AvailablePieces.Add((pType, colorIndex));
                }
            }
        }

        /// <summary>
        /// Toplam kalan parça sayısı (bombalar dahil)
        /// </summary>
        public int TotalCount => AvailablePieces.Count + BombCount;

        /// <summary>
        /// Belirli bir tür ve renkteki parçayı kaldır (yeni sistem için).
        /// </summary>
        public void RemovePieceWithColor(PieceType type, int colorIndex)
        {
            // List'ten bul ve kaldır
            var indexToRemove = AvailablePieces.FindIndex(p => p.type == type && p.colorIndex == colorIndex);
            if (indexToRemove >= 0)
            {
                AvailablePieces.RemoveAt(indexToRemove);
            }
            
            // Aynı zamanda Dictionary'den de azalt (eski sistem uyumluluğu)
            if (PieceCounts.ContainsKey(type) && PieceCounts[type] > 0)
            {
                PieceCounts[type]--;
            }
        }

        /// <summary>
        /// Belirli bir tür ve renkteki parçanın destede olup olmadığını kontrol et.
        /// </summary>
        public bool ContainsPiece(PieceType type, int colorIndex)
        {
            return AvailablePieces.Any(p => p.type == type && p.colorIndex == colorIndex);
        }

        /// <summary>
        /// Bir piece türü için kalan parçaların sayısını döndür.
        /// </summary>
        public int GetCountByType(PieceType type)
        {
            return AvailablePieces.Count(p => p.type == type);
        }

        /// <summary>
        /// Eski RemovePiece metodunu korumak (uyumluluk için).
        /// </summary>
        public void RemovePiece(PieceType type)
        {
            if (PieceCounts.ContainsKey(type) && PieceCounts[type] > 0)
            {
                PieceCounts[type]--;
            }
        }

        /// <summary>
        /// Desteden bomba çekilince sayıyı düş.
        /// </summary>
        public void RemoveBomb()
        {
            if (BombCount > 0) BombCount--;
        }

        /// <summary>
        /// Destesini sıfırla.
        /// </summary>
        public void Reset()
        {
            InitializeDeck();
            // Bombayı sıfırlamıyoruz - mağazadan parayla alındı
        }
    }
}