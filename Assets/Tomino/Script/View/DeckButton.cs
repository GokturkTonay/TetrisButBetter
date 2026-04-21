using UnityEngine;
using UnityEngine.UI;

namespace Tomino.View
{
    /// <summary>
    /// Deck UI Panel'ini açıp kapatan buton.
    /// "Deck Aç" butonuna attach edilir.
    /// </summary>
    public class DeckButton : MonoBehaviour
    {
        [Header("UI Bileşenleri")]
        public GameObject deckPanel;        // Açılıp kapanacak panel
        public Button closeButton;          // Panel içindeki kapat butonu
        public Button thisButton;           // Bu butonun kendisi (otomatik bulunabilir)

        private MenuManager _menuManager;

        private void Awake()
        {
            // MenuManager'ı bul (Singleton gibi)
            _menuManager = Object.FindFirstObjectByType<MenuManager>();

            // Bu butonun references'ı otomatik set
            if (thisButton == null)
                thisButton = GetComponent<Button>();

            // DeckPanel başta gizli olsun
            if (deckPanel != null)
                deckPanel.SetActive(false);

            // Button listener'ları ekle
            if (thisButton != null)
                thisButton.onClick.AddListener(OpenDeck);

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseDeck);
        }

        /// <summary>
        /// Deck panel'ini aç ve oyunu durdur.
        /// </summary>
        public void OpenDeck()
        {
            if (deckPanel == null) return;

            deckPanel.SetActive(true);

            // Oyunu durdur
            if (_menuManager != null)
                _menuManager.PauseGameForDeck();
        }

        /// <summary>
        /// Deck panel'ini kapat ve oyunu devam ettir.
        /// </summary>
        public void CloseDeck()
        {
            if (deckPanel == null) return;

            deckPanel.SetActive(false);

            // Oyunu devam ettir
            if (_menuManager != null)
                _menuManager.ResumeGameFromDeck();
        }
    }
}
