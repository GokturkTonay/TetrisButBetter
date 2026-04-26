using System.Collections.Generic;
using System.Linq;

namespace Tomino.Model
{
    /// <summary>
    /// Deste sistemi - her parçanın tür, renk ve bomba durumunu merkezi olarak takip eder.
    /// MERKEZI SİSTEM: PieceDataList tüm veriyi tutar, diğer listeler buna bağlı kalır.
    /// </summary>
    public class Deck
    {
        // ========== MERKEZİ VERİ YAPISI ==========
        /// <summary>
        /// Tüm parçaların verilerini tutar: (Type, ColorIndex, IsBomb, DeckIndex)
        /// UI ve kod bu listeyi doğrudan kullanır. HER ŞEY BİLAKIS SİSTEMİN KAYNAĞI!
        /// </summary>
        public List<PieceData> PieceDataList { get; private set; } = new();

        // ========== ESKI SİSTEM (Uyumluluk için) ==========
        // Eski sistem (BalancedRandomPieceProvider uyumluluğu için)
        public Dictionary<PieceType, int> PieceCounts { get; private set; }

        // Yeni sistem (Deste UI için parça + renk takibi) - PieceDataList tarafından generate edilir
        public List<(PieceType type, int colorIndex)> AvailablePieces { get; private set; }

        // Aktif bölümdeki bombalar (hangi parçanın bomba olduğunu takip eder)
        public List<(PieceType type, int colorIndex)> BombPieces { get; private set; } = new();

        // MİNİMAL EKLEME: Satın aldığın bombaları unutmamak için kalıcı liste
        public List<(PieceType type, int colorIndex)> PurchasedBombs { get; private set; } = new();

        /// <summary>
        /// Toplam bomba sayısı
        /// </summary>
        public int BombCount => BombPieces.Count;

        /// <summary>
        /// Toplam kalan parça sayısı (normal + bomba parçalar)
        /// </summary>
        public int TotalCount => AvailablePieces.Count + BombPieces.Count;

        public Deck()
        {
            InitializeDeck();
        }

        /// <summary>
        /// Destesini başlat - merkezi PieceDataList'i ve eski sistemi başlat.
        /// </summary>
        private void InitializeDeck()
        {
            // ========== MERKEZI SİSTEM BAŞLATMA ==========
            PieceDataList.Clear();
            
            var pieceTypes = new[]
            {
                PieceType.I, PieceType.J, PieceType.L, PieceType.O,
                PieceType.S, PieceType.T, PieceType.Z, PieceType.Plus
            };

            int deckIndex = 0;

            // Her piece türü için 4 renk ekle
            foreach (var pType in pieceTypes)
            {
                for (int colorIndex = 0; colorIndex < 4; colorIndex++)
                {
                    // Bu parça önceden satın alınmış bir bombaysa bomba olarakstart et
                    bool isBomb = PurchasedBombs.Contains((pType, colorIndex));

                    PieceData data = new PieceData(pType, colorIndex, isBomb, deckIndex);
                    PieceDataList.Add(data);

                    deckIndex++;
                }
            }

            // ========== ESKI SİSTEM (PieceDataList'den generate edilir) ==========
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

            // Yeni sistem (type + color kombinasyonları) - PieceDataList'ten türetilir
            AvailablePieces = new List<(PieceType, int)>();
            BombPieces.Clear();

            // PieceDataList'e göre AvailablePieces ve BombPieces'ı doldur
            foreach (var pieceData in PieceDataList)
            {
                if (pieceData.IsBomb)
                {
                    BombPieces.Add((pieceData.Type, pieceData.ColorIndex));
                }
                else
                {
                    AvailablePieces.Add((pieceData.Type, pieceData.ColorIndex));
                }
            }

            // PieceCounts'ı sadece normal (bomba olmayan) parçalara göre set et
            foreach (var type in pieceTypes)
            {
                int count = PieceDataList.Count(p => p.Type == type && !p.IsBomb);
                PieceCounts[type] = count;
            }

            UnityEngine.Debug.Log($"Deck.InitializeDeck: PieceDataList {PieceDataList.Count} parça ile başlatıldı. " +
                                  $"Normal: {AvailablePieces.Count}, Bomba: {BombPieces.Count}");
        }

        /// <summary>
        /// Belirli bir tür ve renkteki parçayı "kullanıldı" olarak işaretle.
        /// PieceDataList'ten ÇIKARMA, sadece IsUsed = true yap.
        /// </summary>
        public void RemovePieceWithColor(PieceType type, int colorIndex)
        {
            // Merkezi listeden bul
            var pieceData = PieceDataList.FirstOrDefault(p => p.Type == type && p.ColorIndex == colorIndex && !p.IsUsed);
            
            if (pieceData != null)
            {
                pieceData.IsUsed = true; // Çıkarma yerine, sadece IsUsed = true yap
                UnityEngine.Debug.Log($"Deck.RemovePieceWithColor: {type} (Renk:{colorIndex}) IsUsed=true yapıldı. " +
                                      $"PieceDataList'te kalıyor, IsBomb:{pieceData.IsBomb}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Deck.RemovePieceWithColor: {type} (Renk:{colorIndex}) bulunamadı veya zaten kullanıldı!");
            }

            // Eski sistemi de güncelle (uyumluluk için)
            var index = AvailablePieces.FindIndex(p => p.type == type && p.colorIndex == colorIndex);
            if (index >= 0)
            {
                AvailablePieces.RemoveAt(index);
                if (PieceCounts.ContainsKey(type) && PieceCounts[type] > 0)
                {
                    PieceCounts[type]--;
                }
            }
            else
            {
                // Normal destede yoksa bombayı kontrol et
                var bombIndex = BombPieces.FindIndex(p => p.type == type && p.colorIndex == colorIndex);
                if (bombIndex >= 0)
                {
                    BombPieces.RemoveAt(bombIndex);
                }
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
            // Hem aktif bombalara hem de satın alınmışlara bakar
            return BombPieces.Any(p => p.type == type && p.colorIndex == colorIndex) || PurchasedBombs.Contains((type, colorIndex));
        }

        /// <summary>
        /// Parçanın destede olup olmadığını kontrol et (normal VEYA bomba).
        /// </summary>
        public bool ContainsPieceOrBomb(PieceType type, int colorIndex)
        {
            return ContainsPiece(type, colorIndex) || BombPieces.Any(p => p.type == type && p.colorIndex == colorIndex);
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
        /// Belirli bir parçayı bombaya dönüştür. MERKEZI SİSTEMİ GÜNCELLERİ.
        /// PieceDataList'teki ilgili PieceData'nın IsBomb bayrağını true yapır.
        /// Buna göre AvailablePieces ve BombPieces otomatik güncellenir.
        /// </summary>
        public bool ReplacePieceWithBomb(PieceType type, int colorIndex)
        {
            // Merkezi listeden bul
            var pieceData = PieceDataList.FirstOrDefault(p => p.Type == type && p.ColorIndex == colorIndex && !p.IsBomb);
            
            if (pieceData == null)
            {
                UnityEngine.Debug.LogError($"Deck.ReplacePieceWithBomb: {type} (Renk:{colorIndex}) merkezi listede bulunamadı!");
                return false;
            }

            // İş BURADA: PieceData'nın IsBomb bayrağını true yap
            pieceData.IsBomb = true;

            // Eski sistemi de güncelle (AvailablePieces ve BombPieces)
            var index = AvailablePieces.FindIndex(p => p.type == type && p.colorIndex == colorIndex);
            if (index >= 0)
            {
                AvailablePieces.RemoveAt(index);
                BombPieces.Add((type, colorIndex));

                // eski sistem uyumluluğu
                if (PieceCounts.ContainsKey(type) && PieceCounts[type] > 0)
                    PieceCounts[type]--;
            }

            // Kalıcı liste güncelle (bir sonraki levelda da bomba olsun)
            if (!PurchasedBombs.Contains((type, colorIndex)))
            {
                PurchasedBombs.Add((type, colorIndex));
            }

            UnityEngine.Debug.Log($"Deck.ReplacePieceWithBomb: {type} (Renk:{colorIndex}) başarıyla bombaya dönüştürüldü! " +
                                  $"PieceDataList'te güncellendi (Index: {pieceData.DeckIndex})");

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
        /// Destesini sıfırla - tüm IsUsed flaglarını false yap ve bomba bilgisini restore et.
        /// </summary>
        public void Reset()
        {
            // Tüm PieceData'daki IsUsed flaglarını false yap
            foreach (var pieceData in PieceDataList)
            {
                pieceData.IsUsed = false;
            }

            // InitializeDeck zaten bomba bilgisini PurchasedBombs'dan restore ediyor
            InitializeDeck();

            UnityEngine.Debug.Log($"Deck.Reset: Deste reset edildi! Tüm {PieceDataList.Count} parça yeniden hazır.");
        }

        /// <summary>
        /// Merkezi PieceDataList'in durumunu debug log'la.
        /// </summary>
        public void LogDeckStatus()
        {
            UnityEngine.Debug.Log("========== DECK STATUS ==========");
            UnityEngine.Debug.Log($"Merkezi PieceDataList Toplam: {PieceDataList.Count} parça");
            
            int normalCount = PieceDataList.Count(p => !p.IsBomb && !p.IsUsed);
            int bombCount = PieceDataList.Count(p => p.IsBomb && !p.IsUsed);
            int usedCount = PieceDataList.Count(p => p.IsUsed);
            
            UnityEngine.Debug.Log($"  Normal (hazır): {normalCount}, Bomba (hazır): {bombCount}, Kullanıldı: {usedCount}");
            UnityEngine.Debug.Log($"AvailablePieces: {AvailablePieces.Count}, BombPieces: {BombPieces.Count}");
            
            if (PieceDataList.Count <= 10)
            {
                foreach (var data in PieceDataList)
                {
                    string usedStr = data.IsUsed ? "[USED]" : "[OK]";
                    UnityEngine.Debug.Log($"  [{data.DeckIndex}] {data} {usedStr}");
                }
            }
            
            UnityEngine.Debug.Log("================================");
        }
    }
}