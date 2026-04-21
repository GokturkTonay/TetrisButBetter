using Tomino.Model;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tomino.View
{
    /// <summary>
    /// Bir piece kartını temsil eden UI elemanı.
    /// Piece'in şekline göre blokları renders (tek blok değil, piece şekli).
    /// </summary>
    public class DeckPieceCard : MonoBehaviour
    {
        [Header("Piece Bilgisi")]
        public PieceType pieceType;
        public int colorIndex;

        private ThemeProvider _themeProvider;
        private List<Image> _blockImages = new();
        private Color _normalColor = Color.white;
        private Color _usedColor = new Color(1f, 1f, 1f, 0.5f);

        private float _blockSize = 8f; // Her mini block'un boyutu

        /// <summary>
        /// Card'ı başlat. Piece şekline göre mini blokları render et.
        /// </summary>
        public void Initialize(PieceType type, int color, ThemeProvider themeProvider)
        {
            pieceType = type;
            colorIndex = color;
            _themeProvider = themeProvider;

            if (_themeProvider == null)
                Debug.LogError("DeckPieceCard.Initialize: ThemeProvider NULL!");

            // Mevcut blokları temizle
            foreach (var img in _blockImages)
            {
                Destroy(img.gameObject);
            }
            _blockImages.Clear();

            // Piece şekline göre blokları oluştur
            CreatePieceShape();

            Debug.Log($"DeckPieceCard Initialize: {type} Color: {color} - Piece shape oluşturuldu");

            // İlk display'i ayarla
            UpdateDisplay(true);
        }

        /// <summary>
        /// Piece türüne göre blok pozisyonlarını al (normalized, 0,0 den başlayan).
        /// </summary>
        private List<(int row, int col)> GetPieceBlockPositions()
        {
            var positions = new List<(int, int)>();

            switch (pieceType)
            {
                case PieceType.I: // Horizontal bar: 4 blocks
                    positions.Add((0, 0));
                    positions.Add((0, 1));
                    positions.Add((0, 2));
                    positions.Add((0, 3));
                    break;

                case PieceType.O: // Square: 2x2
                    positions.Add((0, 0));
                    positions.Add((0, 1));
                    positions.Add((1, 0));
                    positions.Add((1, 1));
                    break;

                case PieceType.T: // T shape
                    positions.Add((0, 1));
                    positions.Add((1, 0));
                    positions.Add((1, 1));
                    positions.Add((1, 2));
                    break;

                case PieceType.L: // L shape
                    positions.Add((0, 0));
                    positions.Add((1, 0));
                    positions.Add((2, 0));
                    positions.Add((2, 1));
                    break;

                case PieceType.J: // J shape
                    positions.Add((0, 1));
                    positions.Add((1, 1));
                    positions.Add((2, 0));
                    positions.Add((2, 1));
                    break;

                case PieceType.S: // S shape
                    positions.Add((0, 1));
                    positions.Add((0, 2));
                    positions.Add((1, 0));
                    positions.Add((1, 1));
                    break;

                case PieceType.Z: // Z shape
                    positions.Add((0, 0));
                    positions.Add((0, 1));
                    positions.Add((1, 1));
                    positions.Add((1, 2));
                    break;

                case PieceType.Plus: // Plus/Cross shape
                    positions.Add((0, 1));
                    positions.Add((1, 0));
                    positions.Add((1, 1));
                    positions.Add((1, 2));
                    positions.Add((2, 1));
                    break;

                default:
                    break;
            }

            return positions;
        }

        /// <summary>
        /// Piece şekline göre blokları UI'da oluştur ve yerleştir.
        /// </summary>
        private void CreatePieceShape()
        {
            if (_themeProvider == null)
            {
                Debug.LogError("DeckPieceCard.CreatePieceShape: ThemeProvider NULL! Piece render edilemiyor.");
                return;
            }

            if (_themeProvider.currentTheme == null)
            {
                Debug.LogError("DeckPieceCard.CreatePieceShape: currentTheme NULL! Piece render edilemiyor.");
                return;
            }

            var positions = GetPieceBlockPositions();
            
            if (positions == null || positions.Count == 0)
            {
                Debug.LogWarning($"DeckPieceCard.CreatePieceShape: Positions empty or null for {pieceType}");
                return;
            }

            Debug.Log($"DeckPieceCard.CreatePieceShape: {pieceType} Color:{colorIndex} - {positions.Count} blok oluşturulacak");
            
            // Her pozisyon için bir Image oluştur
            foreach (var (row, col) in positions)
            {
                try
                {
                    GameObject blockGO = new GameObject($"Block_{row}_{col}");
                    
                    if (blockGO == null)
                    {
                        Debug.LogError($"DeckPieceCard: blockGO oluşturulamadı!");
                        continue;
                    }

                    // ÖNEMLİ: RectTransform'u ÖNCE ekle, SONRA SetParent yap
                    RectTransform rectTransform = blockGO.AddComponent<RectTransform>();
                    if (rectTransform == null)
                    {
                        Debug.LogError($"DeckPieceCard: RectTransform component eklenemedi!");
                        Destroy(blockGO);
                        continue;
                    }

                    // Şimdi parent'a ekle
                    blockGO.transform.SetParent(transform, false);

                    // Image component ekle
                    Image image = blockGO.AddComponent<Image>();
                    
                    if (image == null)
                    {
                        Debug.LogError($"DeckPieceCard: Image component eklenemedi!");
                        Destroy(blockGO);
                        continue;
                    }

                    // Sprite'ı ThemeProvider'dan çek
                    Sprite sprite = _themeProvider.currentTheme.GetBlockSprite(pieceType, colorIndex);
                    
                    if (sprite == null)
                    {
                        Debug.LogError($"DeckPieceCard: Sprite NULL! {pieceType} Color:{colorIndex} - ThemeProvider.GetBlockSprite hatası!");
                    }
                    else
                    {
                        image.sprite = sprite;
                        Debug.Log($"DeckPieceCard: Sprite set - {pieceType} Color:{colorIndex}");
                    }
                    
                    image.raycastTarget = false;

                    // RectTransform'u ayarla
                    rectTransform.sizeDelta = new Vector2(_blockSize, _blockSize);

                    // Pozisyonu ayarla (grid'de sıralanmış gibi)
                    float posX = col * (_blockSize + 1);
                    float posY = -row * (_blockSize + 1);
                    rectTransform.anchoredPosition = new Vector2(posX, posY);

                    _blockImages.Add(image);
                    Debug.Log($"DeckPieceCard: Block added successfully - Row:{row} Col:{col}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"DeckPieceCard.CreatePieceShape: Exception! {e.Message}\n{e.StackTrace}");
                }
            }

            Debug.Log($"DeckPieceCard: {_blockImages.Count} blok oluşturuldu");
        }

        /// <summary>
        /// Tüm blokları güncelle - destede varsa normal, yoksa opak göster.
        /// Sprite'ın rengini korur, sadece alpha channel'ı değiştirir.
        /// </summary>
        public void UpdateDisplay(bool isAvailable)
        {
            // Alpha: 1 = normal, 0.5 = opak (used)
            float alpha = isAvailable ? 1f : 0.5f;

            foreach (var image in _blockImages)
            {
                if (image != null)
                {
                    // Sprite'ın rengini koruyup sadece alpha'yı değiştir
                    Color currentColor = image.color;
                    currentColor.a = alpha;
                    image.color = currentColor;
                    
                    Debug.Log($"DeckPieceCard.UpdateDisplay: {pieceType} Color:{colorIndex} Alpha:{alpha}");
                }
            }

            Debug.Log($"Card güncellendi: {pieceType} Renk: {colorIndex} | Available: {isAvailable}");
        }
    }
}
