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

            // Önceki kartları temizle
            _cardsByIdentifier.Clear();
            foreach (Transform colorRow in _colorRows)
            {
                foreach (Transform child in colorRow)
                {
                    Destroy(child.gameObject);
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
                    CreateCardForPiece(pieceType, colorIndex);
                }
            }

            Debug.Log($"DeckCardsManager: {_cardsByIdentifier.Count} card başarıyla oluşturuldu.");
        }

        /// <summary>
        /// Belirli bir parça için card oluştur (doğru ColorRow'a ekle).
        /// </summary>
        private void CreateCardForPiece(PieceType pieceType, int colorIndex)
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

            // Card GameObject oluştur
            GameObject cardGO = new GameObject($"Card_{pieceType}_{colorIndex}");
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

            // Dictionary'ye ekle
            _cardsByIdentifier[identifier] = card;

            Debug.Log($"Card oluşturuldu: {pieceType} Renk: {colorIndex} -> ColorRow_{colorIndex}");
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

            // 1. Destede olmayan kartları SILT ve deaktive et
            var cardsToRemove = new List<int>();
            foreach (var (identifier, card) in _cardsByIdentifier)
            {
                if (!_board.Deck.ContainsPiece(card.pieceType, card.colorIndex))
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

            // 2. Destede olan ama HashMap'te olmayan kartları OLUŞTUR
            foreach (var (pieceType, colorIndex) in _board.Deck.AvailablePieces)
            {
                int identifier = GetIdentifier(pieceType, colorIndex);
                if (!_cardsByIdentifier.ContainsKey(identifier))
                {
                    Debug.Log($"Yeni kart oluşturuluyor: {pieceType} Renk: {colorIndex}");
                    CreateCardForPiece(pieceType, colorIndex);
                }
            }

            // 3. Mevcut kartların rengini güncelle (opak/normal)
            foreach (var card in _cardsByIdentifier.Values)
            {
                if (card != null && card.gameObject != null)
                {
                    bool isAvailable = _board.Deck.ContainsPiece(card.pieceType, card.colorIndex);
                    card.UpdateDisplay(isAvailable);
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
    }
}
