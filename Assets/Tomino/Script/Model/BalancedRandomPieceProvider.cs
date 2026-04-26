using System;
using System.Collections.Generic;
using Tomino.Model;
using Tomino.Shared;

namespace Tomino.Model
{
    public class BalancedRandomPieceProvider : IPieceProvider
    {
        private readonly Random _random = new();
        private readonly Deck _deck;

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
            if (_deck?.PieceDataList == null || _deck.PieceDataList.Count == 0)
            {
                UnityEngine.Debug.Log("BalancedRandomPieceProvider.GetPiece: PieceDataList boş, parça üretilemiyor.");
                return null;
            }

            // Eğer sıradaki taş indeksi geçersiz ise, yeni bir tane seç
            if (_nextPieceIndex < 0 || _nextPieceIndex >= _deck.PieceDataList.Count)
            {
                RefreshNextPieceIndex();
            }

            if (_nextPieceIndex < 0)
                return null;

            // Merkezi listeden seçilen PieceData'yı al (KÖŞELİ ALINMADIĞINI, SADECE OKU)
            PieceData selectedData = _deck.PieceDataList[_nextPieceIndex];

            UnityEngine.Debug.Log($"BalancedRandomPieceProvider.GetPiece: SEÇIM YAPILDI" +
                                  $" Index:{_nextPieceIndex} -> {selectedData}");

            // Yeni Piece nesnesi oluştur (Block'lar paylaşılmasın diye)
            Piece standardPiece = AvailablePieces.All()[(int)selectedData.Type];
            Piece pieceToReturn = new Piece(standardPiece.GetPositions().Values, selectedData.Type, standardPiece.canRotate);

            // Parçanın rengini merkezi veriden al
            pieceToReturn.ColorIndex = selectedData.ColorIndex;

            // Parçanın bomba durumunu merkezi veriden al
            pieceToReturn.IsBomb = selectedData.IsBomb;

            UnityEngine.Debug.Log($"BalancedRandomPieceProvider.GetPiece: Döndürülen Piece" +
                                  $" Type:{selectedData.Type} Color:{selectedData.ColorIndex} IsBomb:{pieceToReturn.IsBomb}");

            // Sıradaki taşı seçme işlemi SONRASINDA indeksi güncelle
            RefreshNextPieceIndex();

            return pieceToReturn;
        }

        public Piece GetNextPiece()
        {
            if (_deck?.PieceDataList == null || _deck.PieceDataList.Count == 0)
                return null;

            // Eğer sıradaki taş indeksi geçersiz ise
            if (_nextPieceIndex < 0 || _nextPieceIndex >= _deck.PieceDataList.Count)
            {
                return null;
            }

            // Sıradaki PieceData'yı al (ÇIKARMADAN, sadece göster)
            PieceData nextData = _deck.PieceDataList[_nextPieceIndex];

            Piece standardPiece = AvailablePieces.All()[(int)nextData.Type];
            var nextPiece = new Piece(standardPiece.GetPositions().Values, nextData.Type, standardPiece.canRotate);

            // Rengini ve bomba durumunu merkezi veriden al
            nextPiece.ColorIndex = nextData.ColorIndex;
            nextPiece.IsBomb = nextData.IsBomb;

            return nextPiece;
        }

        public void Reset()
        {
            _deck?.Reset();
            RefreshNextPieceIndex();
        }
    }
}