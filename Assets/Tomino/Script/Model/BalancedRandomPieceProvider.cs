using System;
using System.Collections.Generic;
using Tomino.Shared;

namespace Tomino.Model
{
    public class BalancedRandomPieceProvider : IPieceProvider
    {
        private readonly Random _random = new();
        private readonly List<int> _pool = new();
        private readonly Deck _deck;
        private bool _hasPopulated = false;

        // YENİ: Sıradaki taşın bomba olup olmayacağını tutan bayrak
        private bool _nextIsBomb = false;

        public Deck Deck => _deck;

        public BalancedRandomPieceProvider(Deck deck)
        {
            _deck = deck;
        }

        public Piece GetPiece()
        {
            var pool = GetPopulatedPool();

            if (pool.Count == 0) return null;

            int pieceIndex = pool.TakeFirst();
            Piece standardPiece = AvailablePieces.All()[pieceIndex];

            Piece pieceToReturn;

            // HER ZAMAN yeni Piece nesnesi oluştur (Block'lar paylaşılmasın diye)
            pieceToReturn = new Piece(standardPiece.GetPositions().Values, standardPiece.Type, standardPiece.canRotate);
            
            // Eğer bu taş için bomba kararı verildiyse, bomba bayrağını aç
            if (_nextIsBomb)
            {
                _deck.RemoveBomb(); // Bombayı kullandığımız için desteden düşüyoruz
                pieceToReturn.IsBomb = true; // Bomba bayrağını aç
            }

            // Bir sonraki taş için zar at (Eğer destede bomba varsa %20 ihtimalle bomba gelsin)
            _nextIsBomb = (_deck.BombCount > 0 && _random.NextDouble() < 0.20);

            return pieceToReturn;
        }

        public Piece GetNextPiece()
        {
            var pool = GetPopulatedPool();
            if (pool.Count == 0) return null;

            Piece standardPiece = AvailablePieces.All()[pool[0]];

            // "Next" (Sıradaki Taş) panelinde de bombanın görünmesi için
            // HER ZAMAN yeni Piece nesnesi oluştur (Block'lar paylaşılmasın diye)
            var nextPiece = new Piece(standardPiece.GetPositions().Values, standardPiece.Type, standardPiece.canRotate);
            
            if (_nextIsBomb)
            {
                nextPiece.IsBomb = true;
            }

            return nextPiece;
        }

        public void Reset()
        {
            _deck.Reset();
            _pool.Clear(); 
            _hasPopulated = false; 

            // Oyun sıfırlandığında ilk taşın bomba olup olmayacağını belirle
            _nextIsBomb = (_deck.BombCount > 0 && _random.NextDouble() < 0.20);
        }

        private List<int> GetPopulatedPool()
        {
            if (!_hasPopulated)
            {
                PopulatePool();
                _hasPopulated = true;
                // Havuz ilk dolduğunda da sıradaki taş zarını atalım
                _nextIsBomb = (_deck.BombCount > 0 && _random.NextDouble() < 0.20);
            }
            return _pool;
        }

        private void PopulatePool()
        {
            _pool.Clear(); 
            var allAvailable = AvailablePieces.All();

            for (var index = 0; index < allAvailable.Length; ++index)
            {
                PieceType type = allAvailable[index].Type;

                if (_deck.PieceCounts.TryGetValue(type, out int count))
                {
                    for (int j = 0; j < count; j++)
                    {
                        _pool.Add(index);
                    }
                }
            }
            _pool.Shuffle(_random);
        }
    }
}