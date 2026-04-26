using System;
using System.Collections.Generic;
using System.Linq;
using Tomino.Model;
using Tomino.Shared;

namespace Tomino.Model
{
    public class BalancedRandomPieceProvider : IPieceProvider
    {
        private readonly Random _random = new();
        private readonly Deck _deck;

        private (PieceType type, int colorIndex, bool isBomb)? _queuedPiece;

        // YENİ: Sıradaki taşın indeksi (merkezi PieceDataList'ten)
        private int _nextPieceIndex = -1;

        public Deck Deck => _deck;

        public BalancedRandomPieceProvider(Deck deck)
        {
            _deck = deck;
            // Başlangıçta sıradaki taşı belirle
            RefreshNextPieceIndex();
        }

        /// <summary>
        /// Sıradaki taşın indeksini rastgele seç ve belirle.
        /// SADECE IsUsed=false olan parçalardan seçim yapar.
        /// </summary>
        private void RefreshNextPieceIndex()
        {
            if (_deck?.PieceDataList == null || _deck.PieceDataList.Count == 0)
            {
                _nextPieceIndex = -1;
                UnityEngine.Debug.Log("BalancedRandomPieceProvider.RefreshNextPieceIndex: PieceDataList boş!");
                return;
            }

            // Kullanılmamış parçaları filtrele
            var availableIndices = new List<int>();
            for (int i = 0; i < _deck.PieceDataList.Count; i++)
            {
                if (!_deck.PieceDataList[i].IsUsed)
                {
                    availableIndices.Add(i);
                }
            }

            if (availableIndices.Count == 0)
            {
                _nextPieceIndex = -1;
                UnityEngine.Debug.Log("BalancedRandomPieceProvider.RefreshNextPieceIndex: Kullanılmamış parça yok!");
                return;
            }

            // Kullanılmamış parçalardan rastgele seç
            _nextPieceIndex = availableIndices[_random.Next(availableIndices.Count)];

            // Debug log
            if (_nextPieceIndex >= 0 && _nextPieceIndex < _deck.PieceDataList.Count)
            {
                var nextData = _deck.PieceDataList[_nextPieceIndex];
                UnityEngine.Debug.Log($"BalancedRandomPieceProvider.RefreshNextPieceIndex: " +
                                      $"Sıradaki parça Index:{_nextPieceIndex} -> {nextData} (IsUsed:false olan {availableIndices.Count} adettir)");
            }
        }

        public Piece GetPiece()
        {
            // Hafızada (Next Piece olarak gösterilen) parça varsa onu al, yoksa yeni çek
            var data = _queuedPiece ?? _deck.DrawSpecificPiece();
            
            // Hafızayı temizle ki bir sonraki 'Next Piece' yeni parça çekebilsin
            _queuedPiece = null; 

            return CreatePieceFromTuple(data);
        }

        public Piece GetNextPiece()
        {
            // Eğer hafıza boşsa (parça yeni düştüyse) desteden sıradakini belirle
            if (_queuedPiece == null)
            {
                _queuedPiece = _deck.DrawSpecificPiece();
            }
            
            // Belirlenen parçayı sadece görüntüle (desteden silme, hafızada tut)
            return CreatePieceFromTuple(_queuedPiece);
        }

        private Piece CreatePieceFromTuple((PieceType type, int colorIndex, bool isBomb)? tuple)
        {
            if (tuple == null) return null;

            Piece template = AvailablePieces.All().FirstOrDefault(p => p.Type == tuple.Value.type);
            if (template == null) return null;

            // ÖNEMLİ: Şablonun pozisyonlarını ToList() ile kopyalayıp YENİ bir Piece nesnesi üretiyoruz.
            // Bu sayede şablonun kendisi (AvailablePieces.All içindeki) değişmiyor.
            Piece piece = new Piece(template.GetPositions().Values.ToList(), template.Type, template.canRotate);
            piece.ColorIndex = tuple.Value.colorIndex;
            piece.IsBomb = tuple.Value.isBomb;

            return piece;
        }

        public void Reset()
        {
            _deck?.Reset();
            RefreshNextPieceIndex();
        }
    }
}