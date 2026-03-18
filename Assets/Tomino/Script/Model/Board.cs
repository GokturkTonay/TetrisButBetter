using System.Collections.Generic;
using System.Linq;
using Tomino.Shared;

namespace Tomino.Model
{
    public class Board
    {
        public readonly int width;
        public readonly int height;

        public List<Block> Blocks { get; } = new();
        public Piece Piece { get; private set; }

        public Piece NextPiece => _pieceProvider?.GetNextPiece();

        public Deck Deck => (_pieceProvider as BalancedRandomPieceProvider)?.Deck;

        private readonly IPieceProvider _pieceProvider;
        private int Top => height - 1;

        public Board(int width, int height)
            : this(width, height, new BalancedRandomPieceProvider(new Deck()))
        {
        }

        public Board(int width, int height, IPieceProvider pieceProvider)
        {
            this.width = width;
            this.height = height;
            _pieceProvider = pieceProvider;
        }

        public void AddPiece()
        {
            Piece = _pieceProvider.GetPiece();

            if (Piece == null)
            {
                UnityEngine.Debug.Log("Deste bitti! Yeni parça üretilemiyor.");
                return;
            }

            // DESTE GÜNCELLEME: Çekilen parçayı destedeki sayıdan düşer.
            Deck?.RemovePiece(Piece.Type);

            var offsetRow = Top - Piece.Top;
            var offsetCol = (width - Piece.Width) / 2;

            foreach (var block in Piece.blocks)
            {
                block.MoveBy(offsetRow, offsetCol);
            }

            Blocks.AddRange(Piece.blocks);
        }

        public bool HasCollisions() => HasBoardCollisions() || HasBlockCollisions();

        private bool HasBlockCollisions()
        {
            // Orijinal Map ve HashSet yapısı
            var allPositions = Blocks.Map(block => block.Position);
            var uniquePositions = new HashSet<Position>(allPositions);
            return allPositions.Length != uniquePositions.Count;
        }

        private bool HasBoardCollisions() => Blocks.Find(CollidesWithBoard) != null;

        private bool CollidesWithBoard(Block block)
        {
            return block.Position.Row < 0 ||
                   block.Position.Row >= height ||
                   block.Position.Column < 0 ||
                   block.Position.Column >= width;
        }

        public override int GetHashCode()
        {
            return (from block in Blocks
                    let row = block.Position.Row
                    let column = block.Position.Column
                    let offset = width * height * (int)block.Type
                    select offset + row * width + column).Sum();
        }

        public ICollection<Position> GetPieceShadow()
        {
            if (Piece == null) return new List<Position>();
            var positions = Piece.GetPositions();
            _ = FallPiece();
            var shadowPositions = Piece.GetPositions().Values.Map(p => p);
            RestoreSavedPiecePosition(positions);
            return shadowPositions;
        }

        public bool MovePieceLeft() => MovePiece(0, -1);
        public bool MovePieceRight() => MovePiece(0, 1);
        public bool MovePieceDown() => MovePiece(-1, 0);

        private bool MovePiece(int rowOffset, int columnOffset)
        {
            if (Piece == null) return false;
            foreach (var block in Piece.blocks) block.MoveBy(rowOffset, columnOffset);
            if (!HasCollisions()) return true;
            foreach (var block in Piece.blocks) block.MoveBy(-rowOffset, -columnOffset);
            return false;
        }

        public bool RotatePiece()
        {
            if (Piece == null || !Piece.canRotate) return false;
            var piecePosition = Piece.GetPositions();
            var offset = Piece.blocks[0].Position;
            foreach (var block in Piece.blocks)
            {
                var row = block.Position.Row - offset.Row;
                var column = block.Position.Column - offset.Column;
                block.MoveTo(-column + offset.Row, row + offset.Column);
            }
            if (!HasCollisions() || ResolveCollisionsAfterRotation()) return true;
            RestoreSavedPiecePosition(piecePosition);
            return false;
        }

        private bool ResolveCollisionsAfterRotation()
        {
            var columnOffsets = new[] { -1, -2, 1, 2 };
            foreach (var offset in columnOffsets)
            {
                _ = MovePiece(0, offset);
                if (HasCollisions()) _ = MovePiece(0, -offset);
                else return true;
            }
            return false;
        }

        private void RestoreSavedPiecePosition(IReadOnlyDictionary<Block, Position> piecePosition)
        {
            if (Piece == null) return;
            foreach (var block in Piece.blocks) block.MoveTo(piecePosition[block]);
        }

        public int FallPiece()
        {
            if (Piece == null) return 0;
            var rowsCount = 0;
            while (MovePieceDown()) rowsCount++;
            return rowsCount;
        }

        public (int rowsRemoved, int totalScore) RemoveFullRows()
        {
            int rowsRemoved = 0;
            int totalBlocksRemoved = 0;
            for (int row = height - 1; row >= 0; --row)
            {
                var rowBlocks = GetBlocksFromRow(row);
                if (rowBlocks.Count == width)
                {
                    totalBlocksRemoved += rowBlocks.Count;
                    Remove(rowBlocks);
                    MoveDownBlocksBelowRow(row);
                    rowsRemoved += 1;
                }
            }
            int scoreForTurn = totalBlocksRemoved * rowsRemoved;
            return (rowsRemoved, scoreForTurn);
        }

        public void RemoveAllBlocks() => Blocks.Clear();

        private List<Block> GetBlocksFromRow(int row) => Blocks.FindAll(block => block.Position.Row == row);

        private void Remove(ICollection<Block> blocksToRemove) => _ = Blocks.RemoveAll(blocksToRemove.Contains);

        private void MoveDownBlocksBelowRow(int row)
        {
            foreach (var block in Blocks.Where(block => block.Position.Row > row))
            {
                block.MoveBy(-1, 0);
            }
        }

        public void ResetDeck()
        {
            if (_pieceProvider is BalancedRandomPieceProvider provider)
            {
                provider.Reset();
            }
        }
    }
}