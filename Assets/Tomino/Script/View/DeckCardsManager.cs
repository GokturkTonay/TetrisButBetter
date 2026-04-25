using Tomino.Model;
using Tomino.Shared;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Tomino.View
{
    /// <summary>
    /// DeckPanel altında Deck'te olan parçaların kartlarını dinamik olarak oluşturur/siler.
    /// Her renk için ayrı Row Container'da organize edilir.
    /// </summary>
    public class DeckCardsManager : MonoBehaviour
    {
        [Header("Referanslar")]
        public BoardView boardView;
        public ThemeProvider themeProvider;

        [Header("Row Containers (Her renk için 1 tane)")]
        public Transform colorRow_0; // Renk 0 için container
        public Transform colorRow_1; // Renk 1 için container
        public Transform colorRow_2; // Renk 2 için container
        public Transform colorRow_3; // Renk 3 için container

        [Header("Card Tasarımı")]
        public float cardSize = 35f;

        [Header("Fan Efekti (Opsiyonel)")]
        [Tooltip("DeckFanLayout component'i drag edin. Boş bırakılırsa fan efekti devre dışı.")]
        public DeckFanLayout fanLayout;

        [Header("Bomba Seçim Modu")]
        [Tooltip("DeckPanel GameObject'ini sürükleyin (bomba seçiminde açılıp kapanacak).")]
        public GameObject deckPanel;
        private bool _isBombSelectionMode = false;

        private Board _board;
        private Dictionary<int, DeckPieceCard> _cardsByIdentifier = new();
        private Transform[] _colorRows; // Tüm row containers

        public void Initialize(Board board)
        {
            _board = board;
            Debug.Log("DeckCardsManager.Initialize çağrıldı.");

            // BoardView'den referans çek
            if (boardView == null)
            {
                boardView = Object.FindFirstObjectByType<BoardView>();
                Debug.Log($"DeckCardsManager: BoardView FindFirstObjectByType -> {(boardView != null ? "FOUND" : "NULL")}");
            }

            if (boardView == null)
            {
                Debug.LogError("DeckCardsManager: BoardView NULL! Inspector'da set et!");
                return;
            }

            // ThemeProvider'ı boardView'den al
            if (boardView.themeProvider == null)
            {
                Debug.LogError("DeckCardsManager: boardView.themeProvider NULL! BoardView'de set et!");
                return;
            }

            themeProvider = boardView.themeProvider;
            Debug.Log($"DeckCardsManager: ThemeProvider alındı - {(themeProvider != null ? "OK" : "NULL")}");

            // ColorRow references kontrol et
            _colorRows = new Transform[] { colorRow_0, colorRow_1, colorRow_2, colorRow_3 };
            
            for (int i = 0; i < _colorRows.Length; i++)
            {
                if (_colorRows[i] == null)
                {
                    Debug.LogError($"DeckCardsManager: colorRow_{i} NULL! Inspector'da set et!");
                    return;
                }
            }

            Debug.Log("DeckCardsManager: Tüm ColorRow references kontrol edildi - OK");

            // İlk olarak Deck'teki tüm parçalar için card oluştur
            CreateCardsForAvailablePieces();
        }

        /// <summary>
        /// Deck.AvailablePieces'teki her parça için card oluştur.
        /// </summary>
        private void CreateCardsForAvailablePieces()
        {
            if (_board?.Deck?.AvailablePieces == null) 
            {
                Debug.LogError("DeckCardsManager: Deck veya AvailablePieces NULL!");
                return;
            }

            Debug.Log($"DeckCardsManager: Deck'te {_board.Deck.AvailablePieces.Count} parça var");

            // Önceki kartları temizle — sadece DeckPieceCard olanları sil,
            // PivotRow gibi diğer child'lara dokunma.
            _cardsByIdentifier.Clear();
            foreach (Transform colorRow in _colorRows)
            {
                foreach (DeckPieceCard card in colorRow.GetComponentsInChildren<DeckPieceCard>(includeInactive: true))
                {
                    Destroy(card.gameObject);
                }
            }

            // SIRALAMA: 4 Renk × 8 Piece Türü
            var pieceTypes = new[]
            {
                PieceType.I, PieceType.J, PieceType.L, PieceType.O,
                PieceType.S, PieceType.T, PieceType.Z, PieceType.Plus
            };

            // ÖNEMLİ: Renk loop'u DÖŞ (dışarıda), Piece loop'u İÇER (içerde)
            // Her renk için 8 piece'i kendi row'a ekle
            for (int colorIndex = 0; colorIndex < 4; colorIndex++)
            {
                Debug.Log($"DeckCardsManager: Renk {colorIndex} için kartlar oluşturuluyor");
                
                foreach (var pieceType in pieceTypes)
                {
                    // Normal parçalar için kart oluştur
                    if (_board.Deck.ContainsPiece(pieceType, colorIndex))
                    {
                        CreateCardForPiece(pieceType, colorIndex, false);
                    }
                    // Bomba parçaları için kart oluştur
                    else if (_board.Deck.IsBombPiece(pieceType, colorIndex))
                    {
                        CreateCardForPiece(pieceType, colorIndex, true);
                    }
                }
            }

            Debug.Log($"DeckCardsManager: {_cardsByIdentifier.Count} card başarıyla oluşturuldu.");
        }

        /// <summary>
        /// Belirli bir parça için card oluştur (doğru ColorRow'a ekle).
        /// isBomb: true ise kartı bomba olarak işaretle.
        /// </summary>
        private void CreateCardForPiece(PieceType pieceType, int colorIndex, bool isBomb = false)
        {
            int identifier = GetIdentifier(pieceType, colorIndex);

            // Zaten varsa, oluşturma
            if (_cardsByIdentifier.ContainsKey(identifier))
                return;

            // Doğru ColorRow'u seç
            Transform targetRow = _colorRows[colorIndex];
            
            if (targetRow == null)
            {
                Debug.LogError($"DeckCardsManager: ColorRow_{colorIndex} NULL!");
                return;
            }

            // NOT: Kartlar doğrudan colorRow'a eklenir.
            // PivotRow (LayoutElement.IgnoreLayout) ayrı durur, grid onu yok sayar.

            // Card GameObject oluştur
            GameObject cardGO = new GameObject($"Card_{pieceType}_{colorIndex}{(isBomb ? "_BOMB" : "")}");
            cardGO.transform.SetParent(targetRow, false);
            
            // RectTransform ekle (Canvas UI için zorunlu!)
            RectTransform rectTransform = cardGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(cardSize, cardSize);
            rectTransform.anchoredPosition = Vector2.zero;
            
            cardGO.SetActive(true);

            // DeckPieceCard script ekle
            DeckPieceCard card = cardGO.AddComponent<DeckPieceCard>();

            // Card'ı initialize et
            card.Initialize(pieceType, colorIndex, themeProvider);

            // Bomba ise işaretle
            if (isBomb)
            {
                card.MarkAsBomb();
            }

            // Dictionary'ye ekle
            _cardsByIdentifier[identifier] = card;

            Debug.Log($"Card oluşturuldu: {pieceType} Renk: {colorIndex} Bomba: {isBomb} -> ColorRow_{colorIndex}");
        }

        /// <summary>
        /// Tüm kartları güncelle - Deck'te olmayan kartları sil, yenilik varsa oluştur.
        /// Kullanılan kartlar opak, kullanılmayan normal gösterilir.
        /// </summary>
        public void RefreshAllCards()
        {
            if (_board?.Deck == null) 
            {
                Debug.LogWarning("DeckCardsManager.RefreshAllCards: Deck NULL!");
                return;
            }

            Debug.Log($"DeckCardsManager.RefreshAllCards çağrıldı. Mevcut kartlar: {_cardsByIdentifier.Count}");

            // 1. Destede (normal veya bomba) olmayan kartları SİL
            var cardsToRemove = new List<int>();
            foreach (var (identifier, card) in _cardsByIdentifier)
            {
                if (!_board.Deck.ContainsPieceOrBomb(card.pieceType, card.colorIndex))
                {
                    Debug.Log($"Kart kaldırılıyor: {card.pieceType} Renk: {card.colorIndex}");
                    card.gameObject.SetActive(false);
                    Destroy(card.gameObject);
                    cardsToRemove.Add(identifier);
                }
            }

            foreach (var id in cardsToRemove)
            {
                _cardsByIdentifier.Remove(id);
            }

            // 2. Destede olan ama HashMap'te olmayan kartları OLUŞTUR (normal)
            foreach (var (pieceType, colorIndex) in _board.Deck.AvailablePieces)
            {
                int identifier = GetIdentifier(pieceType, colorIndex);
                if (!_cardsByIdentifier.ContainsKey(identifier))
                {
                    Debug.Log($"Yeni kart oluşturuluyor: {pieceType} Renk: {colorIndex}");
                    CreateCardForPiece(pieceType, colorIndex, false);
                }
            }

            // 3. Bomba parçaları için kartları oluştur/güncelle
            foreach (var (pieceType, colorIndex) in _board.Deck.BombPieces)
            {
                int identifier = GetIdentifier(pieceType, colorIndex);
                if (!_cardsByIdentifier.ContainsKey(identifier))
                {
                    Debug.Log($"Bomba kartı oluşturuluyor: {pieceType} Renk: {colorIndex}");
                    CreateCardForPiece(pieceType, colorIndex, true);
                }
            }

            // 4. Mevcut kartların görselini güncelle
            foreach (var card in _cardsByIdentifier.Values)
            {
                if (card != null && card.gameObject != null)
                {
                    // Bomba mı kontrol et
                    bool isBombNow = _board.Deck.IsBombPiece(card.pieceType, card.colorIndex);
                    if (isBombNow && !card.isBomb)
                    {
                        card.MarkAsBomb();
                    }

                    bool isAvailable = _board.Deck.ContainsPieceOrBomb(card.pieceType, card.colorIndex);
                    card.UpdateDisplay(isAvailable);

                    // Bomba görselini UpdateDisplay sonrası tekrar uygula (alpha değişmesini ezme)
                    if (card.isBomb)
                        card.MarkAsBomb();

                    card.gameObject.SetActive(true);
                }
            }

            Debug.Log($"Refresh tamamlandı. Son kartlar: {_cardsByIdentifier.Count}");
        }

        /// <summary>
        /// (PieceType, ColorIndex) kombinasyonunun unique ID'sini döndür.
        /// </summary>
        private int GetIdentifier(PieceType type, int colorIndex)
        {
            return ((int)type * 31) + colorIndex;
        }

        /// <summary>
        /// Deste kartlarını reset et: PivotRow'u koru, içindeki kartları sil ve yeniden spawn et.
        /// </summary>
        public void ResetDeckCards()
        {
            Debug.Log("DeckCardsManager.ResetDeckCards: Deste reset başlıyor...");

            // 1. Dictionary temizle
            _cardsByIdentifier.Clear();

            // 2. ColorRow'daki kartları sil (PivotRow ve diğer non-card objeler korunur)
            if (_colorRows != null)
            {
                foreach (Transform colorRow in _colorRows)
                {
                    if (colorRow == null) continue;
                    foreach (DeckPieceCard card in colorRow.GetComponentsInChildren<DeckPieceCard>(includeInactive: true))
                    {
                        Destroy(card.gameObject);
                    }
                }
            }

            Debug.Log("DeckCardsManager.ResetDeckCards: Tüm kartlar silindi");

            // 3. Yeni kartları spawn et
            CreateCardsForAvailablePieces();

            Debug.Log("DeckCardsManager.ResetDeckCards: Deste reset tamamlandı!");
        }

        // ==========================================
        // BOMBA SEÇİM MODU
        // ==========================================

        /// <summary>
        /// Mağazadan "Bomba Satın Al" butonuna basıldığında çağrılır.
        /// DeckPanel'i açar ve kartları tıklanabilir yapar.
        /// </summary>
        public void EnterBombSelectionMode()
        {
            Debug.Log("DeckCardsManager.EnterBombSelectionMode: Bomba seçim modu açılıyor...");

            _isBombSelectionMode = true;

            // Paneli aç
            if (deckPanel != null)
                deckPanel.SetActive(true);

            // FULL REBUILD: Kartları sıfırdan oluştur (güncel deste durumunu yansıtsın)
            ResetDeckCards();

            // Sadece normal (bomba OLMAYAN) kartlara tıklanabilirlik ekle
            foreach (var card in _cardsByIdentifier.Values)
            {
                if (card != null && card.gameObject.activeInHierarchy && !card.isBomb)
                    card.SetClickable(true, OnBombCardSelected);
            }

            Debug.Log("DeckCardsManager.EnterBombSelectionMode: Kartlar tıklanabilir yapıldı.");
        }

        /// <summary>
        /// Bir kart seçildiğinde çağrılan callback.
        /// Seçilen parçayı bombaya dönüştürür, UI'ı günceller ve paneli kapatır.
        /// </summary>
        private void OnBombCardSelected(PieceType type, int colorIndex)
        {
            if (_board?.Deck == null)
            {
                Debug.LogError("DeckCardsManager.OnBombCardSelected: Deck NULL!");
                return;
            }

            Debug.Log($"DeckCardsManager.OnBombCardSelected: {type} Renk:{colorIndex} seçildi → Bombaya dönüştürülüyor...");

            bool success = _board.Deck.ReplacePieceWithBomb(type, colorIndex);

            if (success)
            {
                Debug.Log($"Bomba dönüşümü başarılı! Yeni BombCount: {_board.Deck.BombCount}");
            }
            else
            {
                Debug.LogWarning($"DeckCardsManager.OnBombCardSelected: {type} Renk:{colorIndex} destede bulunamadı!");
            }

            // Kartları sıfırdan oluştur (bomba görselini yansıtsın)
            ResetDeckCards();
            ExitBombSelectionMode();
        }

        /// <summary>
        /// Bomba seçim modundan çık. Kartları normal moda döndür ve paneli kapat.
        /// </summary>
        public void ExitBombSelectionMode()
        {
            Debug.Log("DeckCardsManager.ExitBombSelectionMode: Bomba seçim modu kapatılıyor...");

            _isBombSelectionMode = false;

            // Tüm kartlardan tıklanabilirliği kaldır
            foreach (var card in _cardsByIdentifier.Values)
            {
                if (card != null)
                    card.SetClickable(false, null);
            }

            // Paneli kapat
            if (deckPanel != null)
                deckPanel.SetActive(false);

            Debug.Log("DeckCardsManager.ExitBombSelectionMode: Panel kapatıldı.");
        }
    }
}
