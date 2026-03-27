using Tomino.Input;
using Tomino.Model;
using Tomino.Shared;
using UnityEngine;
using System.Linq;

namespace Tomino.View
{
    public class BoardView : MonoBehaviour
    {
        private enum Layer { Blocks, PieceShadow }
        public GameObject blockPrefab;
        public Sprite blockSprite;
        public ThemeProvider themeProvider;
        public Sprite shadowBlockSprite;
        public readonly TouchInput touchInput = new();

        private Board _gameBoard;
        private int _renderedBoardHash = -1;
        private bool _forceRender;
        private GameObjectPool<BlockView> _blockViewPool;
        private RectTransform _rectTransform;

        public void SetBoard(Board board)
        {
            _gameBoard = board;
            _blockViewPool = new GameObjectPool<BlockView>(blockPrefab, board.width * board.height + 20, gameObject);
            _forceRender = true;
        }

        internal void Update()
        {
            if (_gameBoard == null) return;
            touchInput.blockSize = BlockSize();
            var hash = _gameBoard.GetHashCode();
            if (!_forceRender && hash == _renderedBoardHash) return;
            RenderGameBoard();
            _renderedBoardHash = hash;
            _forceRender = false;
        }

        private void RenderGameBoard()
        {
            _blockViewPool.DeactivateAll();
            RenderPieceShadow();
            RenderBlocks();
        }

        private void RenderBlocks()
        {
            bool isBomb = _gameBoard.Piece != null && _gameBoard.Piece.IsBomb;
            foreach (var block in _gameBoard.Blocks)
            {
                Color colorToRender = BlockColor(block.Type);
                if (isBomb)
                {
                    foreach (var b in _gameBoard.Piece.blocks)
                    {
                        if (ReferenceEquals(b, block)) { colorToRender = Color.black; break; }
                    }
                }
                RenderBlock(blockSprite, block.Position, colorToRender, Layer.Blocks);
            }
        }

        private void RenderPieceShadow()
        {
            if (_gameBoard.Piece == null) return;
            Color shadowColor = _gameBoard.Piece.IsBomb ? new Color(0,0,0,0.4f) : themeProvider.currentTheme.blockShadowColor;
            foreach (var position in _gameBoard.GetPieceShadow())
                RenderBlock(shadowBlockSprite, position, shadowColor, Layer.PieceShadow);
        }

        private void RenderBlock(Sprite sprite, Position position, Color color, Layer layer)
        {
            var view = _blockViewPool.GetAndActivate();
            view.SetSprite(sprite);
            view.SetSize(BlockSize());
            view.SetColor(color);
            view.SetPosition(BlockPosition(position.Row, position.Column, layer));
        }

        internal void Awake() { _rectTransform = GetComponent<RectTransform>(); Settings.changedEvent += () => _forceRender = true; }
        private Vector3 BlockPosition(int r, int c, Layer l) => new Vector3(c*BlockSize(), r*BlockSize(), (float)l) + new Vector3(BlockSize()/2, BlockSize()/2, 0) - PivotOffset();
        private float BlockSize() => _rectTransform.rect.size.x / _gameBoard.width;
        private Color BlockColor(PieceType t) => themeProvider.currentTheme.BlockColors[(int)t];
        private Vector3 PivotOffset() => new Vector3(_rectTransform.rect.size.x * _rectTransform.pivot.x, _rectTransform.rect.size.y * _rectTransform.pivot.y);
    }
}