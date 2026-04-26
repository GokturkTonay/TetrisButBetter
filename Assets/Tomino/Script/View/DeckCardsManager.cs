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
                    Debug.LogError($"DeckCardsManager: colorRow_{i} NULL! Lütfen Inspector'da atayın.");
                }
            }
            
            // Başlangıçta desteyi temiz kurulumla yap
            ResetDeckCards();
        }

        /// <summary>
        /// Deck.AvailablePieces'teki ve BombPieces'teki her parça için card oluştur.
        /// </summary>
        private void CreateCardsForAvailablePieces()
        {
            if (_board?.Deck?.AvailablePieces == null) 
            {
                Debug.LogWarning("DeckCardsManager: _board.Deck.AvailablePieces null, oluşturulmadı.");
                return;
            }

            var pieceTypes = new[]
            {
                PieceType.I, PieceType.J, PieceType.L, PieceType.O,
                PieceType.S, PieceType.T, PieceType.Z, PieceType.Plus
            };

            // ÖNEMLİ: Renk loop'u Dışarıda, Piece loop'u İçerde
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
            
            // Fan efekti varsa güncelle
            if (fanLayout != null)
            {
                fanLayout.ArrangeCards();
            }
        }

        /// <summary>
        /// Belirli bir parça için card oluştur (doğru ColorRow'a ekle).
        /// Merkezi PieceDataList'ten deckIndex'i bulur ve set eder.
        /// </summary>
        private void CreateCardForPiece(PieceType pieceType, int colorIndex, bool isBomb = false)
        {
            int identifier = GetIdentifier(pieceType, colorIndex);

            // Zaten varsa, oluşturma
            if (_cardsByIdentifier.ContainsKey(identifier))
            {
                Debug.LogWarning($"DeckCardsManager: Card zaten var -> {pieceType} (Renk:{colorIndex}) ID:{identifier}");
                return;
            }

            // Doğru ColorRow'u seç
            Transform targetRow = _colorRows[colorIndex];
            
            if (targetRow == null)
            {
                Debug.LogError($"DeckCardsManager: Target row null for color {colorIndex}!");
                return;
            }

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

            // Merkezi listeden deckIndex'i bul
            if (_board?.Deck?.PieceDataList != null)
            {
                var pieceData = _board.Deck.PieceDataList.FirstOrDefault(p => p.Type == pieceType && p.ColorIndex == colorIndex);
                if (pieceData != null)
                {
                    card.deckIndex = pieceData.DeckIndex;
                    Debug.Log($"DeckCardsManager.CreateCardForPiece: {pieceType} (Renk:{colorIndex}) = DeckIndex: {pieceData.DeckIndex}");
                }
            }

            // Bomba ise işaretle
            if (isBomb)
            {
                card.MarkAsBomb();
            }

            // Dictionary'ye ekle
            _cardsByIdentifier[identifier] = card;

            Debug.Log($"DeckCardsManager: OLUŞTURULDU -> {cardGO.name} @ Row_{colorIndex}");
        }

        /// <summary>
        /// Tüm kartları güncelle - Deck'te olmayan kartları sil, yenilik varsa oluştur.
        /// </summary>
        public void RefreshAllCards()
        {
            if (_board?.Deck == null) return;

            Debug.Log("DeckCardsManager.RefreshAllCards çalışıyor...");

            // 1. Destede (normal veya bomba) olmayan kartları SİL
            var cardsToRemove = new List<int>();
            foreach (var (identifier, card) in _cardsByIdentifier)
            {
                if (!_board.Deck.ContainsPieceOrBomb(card.pieceType, card.colorIndex))
                {
                    Debug.Log($"Silinecek Kart Bulundu: {card.pieceType} Renk: {card.colorIndex}");
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
                    Debug.Log($"Eksik normal kart oluşturuluyor: {pieceType} Renk: {colorIndex}");
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
                    // Eğer bomba olduysa ve kart henüz bomba değilse bomba yap
                    bool isBombNow = _board.Deck.IsBombPiece(card.pieceType, card.colorIndex);
                    if (isBombNow && !card.isBomb)
                    {
                        Debug.Log($"Kart BOMBAYA güncellendi: {card.pieceType} Renk: {card.colorIndex}");
                        card.MarkAsBomb();
                    }

                    bool isAvailable = _board.Deck.ContainsPieceOrBomb(card.pieceType, card.colorIndex);
                    card.UpdateDisplay(isAvailable);

                    if (card.isBomb)
                        card.MarkAsBomb(); // Bomba efektini uygula

                    card.gameObject.SetActive(true);
                }
            }
            
            if (fanLayout != null)
            {
                fanLayout.ArrangeCards();
            }
        }

        private int GetIdentifier(PieceType type, int colorIndex)
        {
            return ((int)type * 31) + colorIndex;
        }

        /// <summary>
        /// Deste kartlarını hiyerarşiden tamamen koparıp siler ve güncel desteye göre sıfırdan oluşturur.
        /// </summary>
        public void ResetDeckCards()
        {
            Debug.Log("DeckCardsManager.ResetDeckCards: Deste reset başlıyor...");

            // 1. Dictionary temizle
            _cardsByIdentifier.Clear();

            // 2. Hiyerarşiden objeleri kopar ve sil 
            if (_colorRows != null)
            {
                foreach (Transform colorRow in _colorRows)
                {
                    if (colorRow == null) continue;
                    
                    // Eski UI objelerini tamamen sil ki üst üste binmesinler (Senin istediğin temiz hiyerarşi eklentisi)
                    foreach (Transform child in colorRow)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            Debug.Log("DeckCardsManager.ResetDeckCards: Tüm eski objeler hiyerarşiden silindi.");

            // 3. Mevcut Data'ya göre objeleri hiyerarşide sıfırdan var et
            CreateCardsForAvailablePieces();

            Debug.Log("DeckCardsManager.ResetDeckCards: Deste yeni objelerle başarıyla kuruldu!");
        }

        // ==========================================
        // BOMBA SEÇİM MODU
        // ==========================================

        public void EnterBombSelectionMode()
        {
            Debug.Log("DeckCardsManager: EnterBombSelectionMode çağrıldı.");
            _isBombSelectionMode = true;

            if (deckPanel != null)
                deckPanel.SetActive(true);

            // Seçim aşamasından önce hiyerarşiyi taze bir görünüme al
            ResetDeckCards();

            // Sadece normal (bomba OLMAYAN) kartlara tıklanabilirlik ekle
            foreach (var card in _cardsByIdentifier.Values)
            {
                if (card != null && card.gameObject.activeInHierarchy)
                {
                    if (!card.isBomb)
                    {
                        card.SetClickable(true, OnBombCardSelected);
                    }
                    else
                    {
                        // Bomba olanlara tıklanamaz
                        card.SetClickable(false, null);
                    }
                }
            }
        }

        private void OnBombCardSelected(PieceType type, int colorIndex)
        {
            Debug.Log($"DeckCardsManager.OnBombCardSelected: Bomba olarak seçildi -> {type} Renk: {colorIndex}");
            if (_board?.Deck == null) return;

            // MERKEZI SİSTEM: ReplacePieceWithBomb çağrıldığında merkezi PieceDataList güncellenir
            bool success = _board.Deck.ReplacePieceWithBomb(type, colorIndex);

            if (success)
            {
                Debug.Log($"DeckCardsManager.OnBombCardSelected: Başarıyla bombaya dönüştürüldü!");
                Debug.Log($"DeckCardsManager.OnBombCardSelected: PieceDataList artık güncellendi. " +
                          $"BalancedRandomPieceProvider sıradaki taşı yeniden seçecek.");
                
                // Dönüşüm bittikten sonra hiyerarşideki kartları sıfırdan yarat (Görseli tamamen günceller)
                ResetDeckCards();
                ExitBombSelectionMode();
            }
            else
            {
                Debug.LogError($"DeckCardsManager.OnBombCardSelected: Dönüştürme başarısız!");
            }
        }

        public void ExitBombSelectionMode()
        {
            Debug.Log("DeckCardsManager: ExitBombSelectionMode çağrıldı.");
            _isBombSelectionMode = false;

            // Tüm kartların tıklanabilirliğini kaldır
            foreach (var card in _cardsByIdentifier.Values)
            {
                if (card != null)
                    card.SetClickable(false, null);
            }

            if (deckPanel != null)
                deckPanel.SetActive(false);
        }
    }
}