using System;
using System.Collections.Generic;
using Tomino.Shared;

namespace Tomino.Model
{
    public class BalancedRandomPieceProvider : IPieceProvider
    {
        private readonly Random _random = new();
        private readonly List<int> _pool = new();
        private readonly Deck _deck; // Deste referansý
        private bool _hasPopulated = false;
        public Deck Deck => _deck;

        public BalancedRandomPieceProvider(Deck deck)
        {
            _deck = deck;
        }

        public Piece GetPiece()
        {
            var pool = GetPopulatedPool();
            
            // Eðer deste bittiyse null dön (Board bunu kontrol etmeli)
            if (pool.Count == 0) return null;

            return AvailablePieces.All()[pool.TakeFirst()];
        }

        public Piece GetNextPiece()
        {
            var pool = GetPopulatedPool();
            if (pool.Count == 0) return null;

            return AvailablePieces.All()[pool[0]];
        }

        private List<int> GetPopulatedPool()
        {
            // Sadece bir kez, destedeki mevcut sayýlara göre havuzu doldur
            if (!_hasPopulated)
            {
                PopulatePool();
                _hasPopulated = true;
            }
            return _pool;
        }

        private void PopulatePool()
        {
            var allAvailable = AvailablePieces.All();
            
            for (var index = 0; index < allAvailable.Length; ++index)
            {
                PieceType type = allAvailable[index].Type;
                
                // Destedeki sayý kadar bu parçanýn index'ini havuzuna ekle
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