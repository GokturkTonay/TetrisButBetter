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
        public Piece Piece { get;  set; }

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

            // Offset hesaplaması: Piece'ı en üstten başlatıp ortalıyoruz
            var offsetRow = Top - Piece.Top;  // En üste konumlandır
            var offsetCol = (width - Piece.Width) / 2;  // Merkezde başlat

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
            int hash = 0;
            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                int row = block.Position.Row;
                int column = block.Position.Column;
                int offset = width * height * (int)block.Type;
                hash += offset + row * width + column;
            }
            return hash;
        }

        public ICollection<Position> GetPieceShadow()
        {
            if (Piece == null) return new List<Position>();
            
            var savedPositions = Piece.GetPositions();
            var shadowPositions = new List<Position>();
            
            while (MovePieceDown()) { }
            
            foreach (var block in Piece.blocks)
            {
                shadowPositions.Add(new Position(block.Position.Row, block.Position.Column));
            }
            
            RestoreSavedPiecePosition(savedPositions);
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
            
            // Optimize: Silinecek satırları önce topla (aşağıdan yukarıya işleme)
            var rowsToRemove = new List<int>();
            for (int row = 0; row < height; row++)
            {
                var rowBlocks = GetBlocksFromRow(row);
                if (rowBlocks.Count == width)
                {
                    rowsToRemove.Add(row);
                    totalBlocksRemoved += rowBlocks.Count;
                }
            }
            
            // Silinecek satırları (ters sırada) sil
            foreach (int row in rowsToRemove)
            {
                var rowBlocks = GetBlocksFromRow(row);
                Remove(rowBlocks);
                MoveDownBlocksBelowRow(row);
                rowsRemoved++;
            }
            
            int scoreForTurn = totalBlocksRemoved * rowsRemoved;
            return (rowsRemoved, scoreForTurn);
        }

        public void RemoveAllBlocks() => Blocks.Clear();

        private List<Block> GetBlocksFromRow(int row) => Blocks.FindAll(block => block.Position.Row == row);

        private void Remove(ICollection<Block> blocksToRemove) => _ = Blocks.RemoveAll(blocksToRemove.Contains);

        private void MoveDownBlocksBelowRow(int row)
        {
            // Optimize: Sadece ilgili satırın altında bulunan blokları tarama
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

        // ==========================================
        // ---- YENİ: BOMBA SİSTEMİ METOTLARI ----
        // ==========================================

        public (int blocksDestroyed, int scoreEarned) ExplodeContactBomb(Piece bombPiece)
        {
            if (bombPiece == null) return (0, 0);

            // Bomba bloklarını HashSet'e ekle (O(1) lookup)
            var bombBlockSet = new HashSet<Block>(bombPiece.blocks);
            
            // Position-based lookup dictionary oluştur (O(1) position lookup)
            var positionToBlockMap = new Dictionary<(int, int), Block>();
            foreach (var block in Blocks)
            {
                var pos = (block.Position.Row, block.Position.Column);
                positionToBlockMap[pos] = block;
            }

            var blocksToRemove = new HashSet<Block>();

            // 1. AŞAMA: Bombanın etrafındaki (Sağ, Sol, Üst, Alt) blokları tespit et
            foreach (var bombBlock in bombPiece.blocks)
            {
                int r = bombBlock.Position.Row;
                int c = bombBlock.Position.Column;

                // Etraftaki hücreleri kontrol et (4 yön) - O(1) lookup
                CheckAndMarkNeighborForDestruction(r + 1, c, bombBlockSet, blocksToRemove, positionToBlockMap);
                CheckAndMarkNeighborForDestruction(r - 1, c, bombBlockSet, blocksToRemove, positionToBlockMap);
                CheckAndMarkNeighborForDestruction(r, c + 1, bombBlockSet, blocksToRemove, positionToBlockMap);
                CheckAndMarkNeighborForDestruction(r, c - 1, bombBlockSet, blocksToRemove, positionToBlockMap);
            }

            // 2. AŞAMA: Tespit edilen komşu blokları Board'dan sil
            int destroyedNeighborCount = blocksToRemove.Count;
            if (destroyedNeighborCount > 0)
            {
                Remove(blocksToRemove);
            }

            // 3. AŞAMA: Bombanın kendisini Board'dan sil
            Remove(bombBlockSet);

            // 4. AŞAMA: Havada kalan blokları aşağı düşür (Yerçekimi) - Optimize edilmiş
            ApplyGravityAfterExplosion();

            // Puan hesaplaması: Yok edilen her komşu blok için x10 puan verelim
            int scoreEarned = destroyedNeighborCount * 10;

            return (destroyedNeighborCount, scoreEarned);
        }

        private void CheckAndMarkNeighborForDestruction(int row, int col, HashSet<Block> bombBlockSet, HashSet<Block> blocksToRemove, Dictionary<(int, int), Block> positionMap)
        {
            // Koordinatlar Board içinde mi?
            if (row < 0 || row >= height || col < 0 || col >= width) return;

            // O(1) lookup position map'ten
            if (positionMap.TryGetValue((row, col), out var neighborBlock))
            {
                if (!bombBlockSet.Contains(neighborBlock))
                {
                    blocksToRemove.Add(neighborBlock);
                }
            }
        }

        private void ApplyGravityAfterExplosion()
        {
            // Basit gravity: her satır için, blokları aşağıda boş yerlere düşür
            bool moved = true;
            while (moved)
            {
                moved = false;
                for (int i = 0; i < Blocks.Count; i++)
                {
                    var block = Blocks[i];
                    int row = block.Position.Row;
                    int col = block.Position.Column;
                    
                    // Altında blok var mı kontrol et
                    bool blockBelow = Blocks.Any(b => 
                        b.Position.Row == row - 1 && 
                        b.Position.Column == col);
                    
                    // Altında blok yok ve board limiti içinde ise düşür
                    if (!blockBelow && row > 0)
                    {
                        block.MoveBy(-1, 0);
                        moved = true;
                    }
                }
            }
        }
    }
}