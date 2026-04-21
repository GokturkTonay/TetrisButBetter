using Tomino.Model;
using Tomino.Shared;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Tomino.View
{
    /// <summary>
    /// Deck sistemini görsel kart destesi olarak gösterir (8 parça × 4 renk = 32 blok).
    /// Tüm renkli parçaları gösterir.
    /// Tüketilen parçalar (deck'te olmayan) %50 opak gösterilir.
    /// BlockView prefab ve sprite'ları BoardView'den otomatik çeker.
    /// </summary>
    public class DeckUIView : MonoBehaviour
    {
        [Header("Referanslar")]
        public BoardView boardView;

        [Header("Grid Ayarları")]
        public float cellSize = 30f;
        public float cellSpacing = 5f;
        public float rowSpacing = 20f;

        private Board _board;
        private GameObjectPool<BlockView> _blockViewPool;
        private RectTransform _rectTransform;
        private List<BlockView> _renderedBlocks = new();
        private GameObject _blockPrefab;
        private Sprite _blockSprite;
        private ThemeProvider _themeProvider;

        /// <summary>
        /// Tüm renk + tür kombinasyonlarını (8 × 4 = 32)
        /// </summary>
        private List<(PieceType type, int colorIndex)> _pieceDisplayOrder;

        public void Initialize(Board board)
        {
            _board = board;
            _rectTransform = GetComponent<RectTransform>();

            // BoardView'den referansları çek
            if (boardView == null)
                boardView = Object.FindFirstObjectByType<BoardView>();

            if (boardView != null)
            {
                _blockPrefab = boardView.blockPrefab;
                _blockSprite = boardView.blockSprite;
                _themeProvider = boardView.themeProvider;
            }

            if (_blockPrefab == null || _themeProvider == null)
            {
                Debug.LogError("DeckUIView: BlockView prefab veya ThemeProvider bulunamadı!");
                return;
            }

            // Pool oluştur
            _blockViewPool = new GameObjectPool<BlockView>(_blockPrefab, 35, gameObject);

            // Görüntülenecek parça sırasını oluştur
            InitializeDisplayOrder();

            Render();
        }

        /// <summary>
        /// Görünecek parçaların sırasını belirle: 8 parça × 4 renk
        /// </summary>
        private void InitializeDisplayOrder()
        {
            _pieceDisplayOrder = new List<(PieceType, int)>();

            var pieceTypes = new[]
            {
                PieceType.I, PieceType.J, PieceType.L, PieceType.O,
                PieceType.S, PieceType.T, PieceType.Z, PieceType.Plus
            };

            // Her piece türü için 4 renk sırasında ekle
            foreach (var pType in pieceTypes)
            {
                for (int colorIndex = 0; colorIndex < 4; colorIndex++)
                {
                    _pieceDisplayOrder.Add((pType, colorIndex));
                }
            }
        }

        /// <summary>
        /// Tüm deck UI'ı render et.
        /// </summary>
        public void Render()
        {
            if (_board?.Deck == null || _blockViewPool == null) return;

            _blockViewPool.DeactivateAll();
            _renderedBlocks.Clear();

            // Tüm 32 parçayı sırasıyla render et
            for (int index = 0; index < _pieceDisplayOrder.Count; index++)
            {
                var (pieceType, colorIndex) = _pieceDisplayOrder[index];
                RenderBlockCard(pieceType, colorIndex, index);
            }
        }

        /// <summary>
        /// Bir blok kartını render et (grid konumunu hesapla).
        /// 4 sütun × 8 satır
        /// </summary>
        private void RenderBlockCard(PieceType pieceType, int colorIndex, int index)
        {
            // Grid konumu hesapla: 4 sütun
            int row = index / 4;
            int col = index % 4;

            float cellXOffset = col * (cellSize + cellSpacing);
            float rowYOffset = -(row * (cellSize + rowSpacing));

            // Block view al
            var blockView = _blockViewPool.GetAndActivate();
            _renderedBlocks.Add(blockView);

            // Sprite ayarla
            Sprite spriteToUse = _themeProvider.currentTheme.GetBlockSprite(pieceType, colorIndex) 
                ?? _blockSprite;
            blockView.SetSprite(spriteToUse);
            blockView.SetSize(cellSize);

            // Renk: Destede varsa normal, yoksa %50 opak
            bool isAvailable = _board.Deck.ContainsPiece(pieceType, colorIndex);
            Color displayColor = isAvailable 
                ? Color.white 
                : new Color(1f, 1f, 1f, 0.5f);

            blockView.SetColor(displayColor);

            // Pozisyonu ayarla
            Vector3 localPosition = new Vector3(cellXOffset, rowYOffset, 0);
            blockView.transform.localPosition = localPosition;

            // RectTransform düz tutması için
            var blockRectTransform = blockView.GetComponent<RectTransform>();
            if (blockRectTransform != null)
            {
                blockRectTransform.anchorMin = Vector2.zero;
                blockRectTransform.anchorMax = Vector2.zero;
                blockRectTransform.pivot = Vector2.zero;
            }
        }

        /// <summary>
        /// Deck değiştiğinde çağrıl, UI'ı güncelle.
        /// </summary>
        public void RefreshDeckView()
        {
            Render();
        }
    }
}
