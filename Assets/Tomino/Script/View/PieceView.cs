using System;
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
            Settings.changedEvent += () => _forceRender = true;
        }

        public void SetBoard(Board board)
        {
            _board = board;
            _blockViewPool = new GameObjectPool<BlockView>(blockPrefab, BlockPoolSize, gameObject);
        }

        internal void Update()
        {
            // GÜVENLİK KONTROLÜ 1: Board veya NextPiece null ise işlem yapma
            if (_board == null || _board.NextPiece == null)
            {
                // Eğer daha önce bir parça çizildiyse ve artık parça yoksa ekranı temizle
                if (_renderedPieceType != null)
                {
                    _blockViewPool?.DeactivateAll();
                    _renderedPieceType = null;
                }
                return;
            }

            // GÜVENLİK KONTROLÜ 2: Aynı parçayı tekrar tekrar çizme
            if (_renderedPieceType != null && !_forceRender && _board.NextPiece.Type == _renderedPieceType) return;

            RenderPiece(_board.NextPiece);
            _renderedPieceType = _board.NextPiece.Type;
            _forceRender = false;
        }

        internal void OnRectTransformDimensionsChange()
        {
            _forceRender = true;
        }

        private void RenderPiece(Piece piece)
        {
            // GÜVENLİK KONTROLÜ 3: Fonksiyona gelen parça null ise havuzu boşalt ve çık
            if (piece == null)
            {
                _blockViewPool.DeactivateAll();
                return;
            }

            _blockViewPool.DeactivateAll();

            var blockSize = BlockSize(piece);

            foreach (var block in piece.blocks)
            {
                var blockView = _blockViewPool.GetAndActivate();
                blockView.SetSprite(blockSprite);
                blockView.SetSize(blockSize);
                blockView.SetColor(BlockColor(block.Type));
                blockView.SetPosition(BlockPosition(block.Position, blockSize));
            }

            // Parça blokları listesini al
            var pieceBlocks = _blockViewPool.Items.Take(piece.blocks.Length).ToList();
            if (pieceBlocks.Count == 0) return;

            var xValues = pieceBlocks.Select(b => b.transform.localPosition.x).ToList();
            var yValues = pieceBlocks.Select(b => b.transform.localPosition.y).ToList();

            var halfBlockSize = blockSize / 2.0f;
            var minX = xValues.Min() - halfBlockSize;
            var maxX = xValues.Max() + halfBlockSize;
            var minY = yValues.Min() - halfBlockSize;
            var maxY = yValues.Max() + halfBlockSize;

            var width = maxX - minX;
            var height = maxY - minY;
            var offsetX = -width / 2.0f - minX;
            var offsetY = -height / 2.0f - minY;

            foreach (var block in pieceBlocks)
            {
                block.transform.localPosition += new Vector3(offsetX, offsetY);
            }
        }

        private static Vector3 BlockPosition(Position position, float blockSize)
        {
            return new Vector3(position.Column * blockSize, position.Row * blockSize);
        }

        private float BlockSize(Piece piece)
        {
            if (container == null || piece == null) return 0;

            var rect = container.rect;
            var width = rect.size.x;
            var height = rect.size.y;
            var numBlocks = piece.blocks.Length;
            return Mathf.Min(width / numBlocks, height / numBlocks);
        }

        private Color BlockColor(PieceType type)
        {
            return themeProvider.currentTheme.BlockColors[(int)type];
        }
    }
}