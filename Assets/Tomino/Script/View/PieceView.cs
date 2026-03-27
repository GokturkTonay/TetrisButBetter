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

        private void Awake() => Settings.changedEvent += () => _forceRender = true;

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
            foreach (var block in piece.blocks)
            {
                var view = _blockViewPool.GetAndActivate();
                view.SetSprite(blockSprite);
                view.SetSize(blockSize);
                view.SetColor(piece.IsBomb ? Color.black : BlockColor(block.Type));
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

        private float BlockSize(Piece p) => Mathf.Min(container.rect.width / 4, container.rect.height / 4);
        private Color BlockColor(PieceType t) => themeProvider.currentTheme.BlockColors[(int)t];
    }
}