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

        // Mağazadan alınan bombalar (hangi parçanın bomba olduğunu takip eder)
        public List<(PieceType type, int colorIndex)> BombPieces { get; private set; } = new();

        /// <summary>
        /// Toplam bomba sayısı (BombPieces listesinin boyutu)
        /// </summary>
        public int BombCount => BombPieces.Count;

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
        /// Toplam kalan parça sayısı (normal + bomba parçalar)
        /// </summary>
        public int TotalCount => AvailablePieces.Count + BombPieces.Count;

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
        /// Belirli bir tür ve renkteki parçanın destede (normal) olup olmadığını kontrol et.
        /// </summary>
        public bool ContainsPiece(PieceType type, int colorIndex)
        {
            return AvailablePieces.Any(p => p.type == type && p.colorIndex == colorIndex);
        }

        /// <summary>
        /// Belirli bir parçanın bomba olarak işaretlenip işaretlenmediğini kontrol et.
        /// </summary>
        public bool IsBombPiece(PieceType type, int colorIndex)
        {
            return BombPieces.Any(p => p.type == type && p.colorIndex == colorIndex);
        }

        /// <summary>
        /// Parçanın destede olup olmadığını kontrol et (normal VEYA bomba).
        /// </summary>
        public bool ContainsPieceOrBomb(PieceType type, int colorIndex)
        {
            return ContainsPiece(type, colorIndex) || IsBombPiece(type, colorIndex);
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
        /// Belirli bir parçayı bombaya dönüştür.
        /// AvailablePieces'tan çıkarır, BombPieces'a ekler.
        /// Mağazadaki "Bomba Satın Al" akışında kullanılır.
        /// </summary>
        public bool ReplacePieceWithBomb(PieceType type, int colorIndex)
        {
            var index = AvailablePieces.FindIndex(p => p.type == type && p.colorIndex == colorIndex);
            if (index < 0) return false;

            // Normal listeden çıkar
            AvailablePieces.RemoveAt(index);

            // Bomba listesine ekle
            BombPieces.Add((type, colorIndex));

            // Eski sistem uyumluluğu: PieceCounts'tan da düş
            if (PieceCounts.ContainsKey(type) && PieceCounts[type] > 0)
                PieceCounts[type]--;

            return true;
        }

        /// <summary>
        /// Desteden bomba çekilince BombPieces'tan bir tanesini kaldır.
        /// </summary>
        public void RemoveBomb()
        {
            if (BombPieces.Count > 0)
                BombPieces.RemoveAt(BombPieces.Count - 1);
        }

        /// <summary>
        /// Destesini sıfırla.
        /// </summary>
        public void Reset()
        {
            InitializeDeck();
            // BombPieces'ı sıfırlamıyoruz - mağazadan parayla alındı
            // BombPieces listesi korunur
        }
    }
}