using System.Collections.Generic;
using System.Linq;
using Tomino.Model;
using Tomino.Shared;
using UnityEngine;

namespace Tomino.View
{
    public class PieceView : MonoBehaviour
    {
        public GameObject blockPrefab;
        public Sprite blockSprite;
        public ThemeProvider themeProvider;
        public RectTransform container;

        private Board _board;
        private GameObjectPool<BlockView> _blockViewPool;
        private PieceType? _renderedPieceType;
        private const int BlockPoolSize = 10;
        private bool _forceRender;

        private void Awake() 
        { 
            // UI koordinatlarını ve boyutlarını hesaplamak için gerekli olan bileşeni alıyoruz
            container = GetComponent<RectTransform>(); 

            // Ayarlar değiştiğinde Board'un yeniden çizilmesini sağlayan olay aboneliği
            Settings.changedEvent += () => _forceRender = true; 

            _blockViewPool = new GameObjectPool<BlockView>(blockPrefab, 10, gameObject);
        }

        public void SetPiece(Piece piece)
        {
            // Önce eski blokları temizle
            if (_blockViewPool != null)
                _blockViewPool.DeactivateAll();

            if (piece == null || themeProvider == null || themeProvider.currentTheme == null)
                return;

            float size = BlockSize();

            foreach (var pos in piece.GetPositions().Values)
            {
                var view = _blockViewPool.GetAndActivate();
                
                // Temadan parçaya uygun sprite'ı çek
                Sprite sprite = themeProvider.currentTheme.GetBlockSprite(piece.Type, piece.ColorIndex) ?? blockSprite;
                view.SetSprite(sprite);
                view.SetSize(size);
                
                // Parça bombaysa siyah yap
                view.SetColor(piece.IsBomb ? Color.black : Color.white);

                // Pozisyonu hesapla ve yerleştir
                view.SetPosition(CalculateBlockPosition(pos.Row, pos.Column));
            }
        }

        private float BlockSize()
        {
            // Önizleme kutusunu 4 birim genişlikte varsayarak boyutu hesaplıyoruz
            return container != null ? container.rect.size.x / 4f : 0.1f;
        }

        private Vector3 CalculateBlockPosition(int r, int c)
        {
            float size = BlockSize();
            // Parçayı kutunun içinde ortalamak için (0,0) noktasından offset ekliyoruz
            // BoardView'deki mantığın sadeleşmiş hali
            return new Vector3((c + 0.5f) * size, (r + 0.5f) * size, 0) - PivotOffset();
        }

        private Vector3 PivotOffset()
        {
            return new Vector3(container.rect.size.x * container.pivot.x, 
                               container.rect.size.y * container.pivot.y);
        }

        public void SetBoard(Board board)
        {
            _board = board;
            _blockViewPool = new GameObjectPool<BlockView>(blockPrefab, BlockPoolSize, gameObject);
        }

        internal void Update()
        {
            if (_board == null || _board.NextPiece == null)
            {
                if (_renderedPieceType != null) { _blockViewPool?.DeactivateAll(); _renderedPieceType = null; }
                return;
            }
            if (_renderedPieceType != null && !_forceRender && _board.NextPiece.Type == _renderedPieceType) return;
            RenderPiece(_board.NextPiece);
            _renderedPieceType = _board.NextPiece.Type;
            _forceRender = false;
        }

        private void RenderPiece(Piece piece)
        {
            _blockViewPool.DeactivateAll();
            var blockSize = BlockSize(piece);
            var activeBlocks = new List<BlockView>();
            // Get custom sprite if available, otherwise use the default block sprite
            Sprite spriteToUse = themeProvider.currentTheme.GetBlockSprite(piece.Type, piece.ColorIndex) ?? blockSprite;
            foreach (var block in piece.blocks)
            {
                var view = _blockViewPool.GetAndActivate();
                view.SetSprite(spriteToUse);
                view.SetSize(blockSize);
                view.SetColor(piece.IsBomb ? Color.black : Color.white);
                view.SetPosition(new Vector3(block.Position.Column * blockSize, block.Position.Row * blockSize));
                activeBlocks.Add(view);
            }
            if (activeBlocks.Count == 0) return;
            float minX = activeBlocks.Min(b => b.transform.localPosition.x) - blockSize/2;
            float maxX = activeBlocks.Max(b => b.transform.localPosition.x) + blockSize/2;
            float minY = activeBlocks.Min(b => b.transform.localPosition.y) - blockSize/2;
            float maxY = activeBlocks.Max(b => b.transform.localPosition.y) + blockSize/2;
            Vector3 offset = new Vector3(-(maxX + minX) / 2f, -(maxY + minY) / 2f);
            foreach (var b in activeBlocks) b.transform.localPosition += offset;
        }

        private float BlockSize(Piece p) => container != null ? Mathf.Min(container.rect.width / 4, container.rect.height / 4) : 20f;
        private Color BlockColor(PieceType t) => themeProvider.currentTheme.BlockColors[(int)t];
    }
}