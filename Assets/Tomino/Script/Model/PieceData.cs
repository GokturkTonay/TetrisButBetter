using System;

namespace Tomino.Model
{
    /// <summary>
    /// Bir parçanın tam verilerini tutar: tip, renk, bomba durumu ve index.
    /// Bu, Deck'te merkezi olarak tutulan ve tüm sistem tarafından kullanılan veri yapısıdır.
    /// </summary>
    [Serializable]
    public class PieceData
    {
        /// <summary>
        /// Parça türü (I, J, L, O, S, T, Z, Plus)
        /// </summary>
        public PieceType Type { get; set; }

        /// <summary>
        /// Parça renk indeksi (0-3)
        /// </summary>
        public int ColorIndex { get; set; }

        /// <summary>
        /// Bu parçanın bomba olup olmadığı
        /// </summary>
        public bool IsBomb { get; set; }

        /// <summary>
        /// Bu parçanın Deck listesindeki indeksi (UI ve kod senkronizasyonu için kullanılan)
        /// </summary>
        public int DeckIndex { get; set; }

        /// <summary>
        /// Bu parçanın oyunda kullanılıp kullanılmadığı (True = çekildi ve oyunun içinde)
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public PieceData(PieceType type, int colorIndex, bool isBomb = false, int deckIndex = -1)
        {
            Type = type;
            ColorIndex = colorIndex;
            IsBomb = isBomb;
            DeckIndex = deckIndex;
        }

        /// <summary>
        /// Bu parçanın (Type, ColorIndex, IsBomb) kombinasyonunu string olarak döndür (debug için)
        /// </summary>
        public override string ToString()
        {
            return $"{Type} Color:{ColorIndex} Bomb:{IsBomb} DeckIdx:{DeckIndex}";
        }

        /// <summary>
        /// İki PieceData'nın eşit olup olmadığını kontrol et (type + color)
        /// </summary>
        public bool EqualsTypeAndColor(PieceData other)
        {
            if (other == null) return false;
            return Type == other.Type && ColorIndex == other.ColorIndex;
        }

        /// <summary>
        /// İki PieceData'nın eşit olup olmadığını kontrol et (type + color + bomb)
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is PieceData other)) return false;
            return Type == other.Type && ColorIndex == other.ColorIndex && IsBomb == other.IsBomb;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ColorIndex, IsBomb);
        }
    }
}
