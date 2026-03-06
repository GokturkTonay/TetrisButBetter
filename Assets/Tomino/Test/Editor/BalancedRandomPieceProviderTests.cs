using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tomino.Model;

namespace Tomino.Test.Editor
{
    public class BalancedRandomPieceProviderTests
    {
        [Test]
        public void GeneratesRandomPiecesFromDeckCorrectly()
        {
            // ADIM 1: Yeni Deck yapýsýný oluþturuyoruz.
            var deck = new Deck();

            // ADIM 2: Provider'a bu desteyi veriyoruz (Hataný bu çözer).
            var provider = new BalancedRandomPieceProvider(deck);

            var pieceCount = new Dictionary<PieceType, int>();

            // ADIM 3: Test miktarýný destedeki toplam kart sayýsýna çekiyoruz.
            // 1000 yazarsak deste biter ve 'null' hatasý alýrýz.
            int totalCardsInDeck = 0;
            foreach (var count in deck.PieceCounts.Values) totalCardsInDeck += count;

            for (var i = 0; i < totalCardsInDeck; i++)
            {
                var piece = provider.GetPiece();

                // Deste bittiyse null gelebilir, kontrol edelim.
                if (piece == null) break;

                var pieceType = piece.Type;

                if (!pieceCount.TryAdd(pieceType, 1))
                {
                    pieceCount[pieceType] += 1;
                }
            }

            // ADIM 4: Kontrol. Desteden her parçadan tam olarak 4 tane (NumDuplicates) gelmeli.
            foreach (var type in deck.PieceCounts.Keys)
            {
                Assert.AreEqual(deck.PieceCounts[type], pieceCount[type],
                    $"{type} tipi için beklenen sayý ile gelen sayý tutmuyor!");
            }
        }
    }
}