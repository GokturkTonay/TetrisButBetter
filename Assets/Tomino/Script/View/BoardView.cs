using Tomino.Input;
using Tomino.Model;
using Tomino.Shared;
using UnityEngine;
using System.Linq;

namespace Tomino.View
{
    public class BoardView : MonoBehaviour
    {
        private enum Layer { PieceShadow = -1, Blocks = 0 }
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
            
            // Eğer bomb ise piece block'larını HashSet'e ekle (O(1) lookup)
            // Block nesneleri kıyaslamak reference-based olup hashable olduğu için
            var bombBlockSet = isBomb ? new System.Collections.Generic.HashSet<Block>(_gameBoard.Piece.blocks) : null;
            
            foreach (var block in _gameBoard.Blocks)
            {
                Color colorToRender = Color.white;
                if (isBomb && bombBlockSet.Contains(block))
                {
                    colorToRender = Color.black;
                }
                
                // Get custom sprite if available, otherwise use the default block sprite
                Sprite spriteToUse = themeProvider.currentTheme.GetBlockSprite(block.Type, block.ColorIndex) ?? blockSprite;
                
                RenderBlock(spriteToUse, block.Position, colorToRender, Layer.Blocks);
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
        private Vector3 BlockPosition(int r, int c, Layer l) 
        { 
            float size = BlockSize();
            if (_rectTransform == null || size <= 0) return Vector3.zero;
            return new Vector3((c + 0.5f)*size, (r + 0.5f)*size, (float)l) - PivotOffset();
        }
        private float BlockSize() => _gameBoard != null && _gameBoard.width > 0 ? _rectTransform.rect.size.x / _gameBoard.width : 0.1f;
        private Color BlockColor(PieceType t) => themeProvider.currentTheme.BlockColors[(int)t];
        private Vector3 PivotOffset() => new Vector3(_rectTransform.rect.size.x * _rectTransform.pivot.x, _rectTransform.rect.size.y * _rectTransform.pivot.y);
    }
}