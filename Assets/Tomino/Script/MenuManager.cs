using UnityEngine;
using TMPro;
using System.Collections;
using Tomino.View;

namespace Tomino
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Paneller (Inspector'dan Sürükle Bırak)")]
        public GameObject gamePanel;
        public GameObject youWonPanel;
        public GameObject shopPanel;
        public GameObject levelSelectPanel;

        [Header("Level Ayarları")]
        public int[] levelTargetScores = { 100, 250, 500, 1000, 2000 };
        private int _currentLevelIndex = 0;

        [Header("Referanslar")]
        public TextMeshProUGUI levelSelectTargetText;
        public LevelView levelView;
        [Tooltip("DeckCardsManager component'ini sürükleyin.")]
        public DeckCardsManager deckCardsManager;

        [Header("Balatro Çarpan Sistemi")]
        public TextMeshProUGUI multiplierText;

        public void CheckScoreAndTransition(Game game, int targetScore)
        {
            int currentTarget = levelTargetScores[_currentLevelIndex];

            if (game.Score.Value >= currentTarget)
            {
                game.Pause();
                StartCoroutine(ShowWinSequence());
            }
        }

        // CS1061 Hatasını Çözen Metot: Çarpan hesaplama ve ekranda gösterme sekansı
        public IEnumerator CalculateMultiplierSequence(Game game, int rowsCount)
        {
            // ÖNLEYİCİ: Eğer oyun zaten durduysa veya rowsCount saçma bir rakamsa çık
            if (rowsCount <= 0)
            {
                if (game != null) { game.Resume(); }
                yield break;
            }

            int basePuan = rowsCount * 10;
            int carpan = rowsCount;
            int kazanilanPuan = basePuan * carpan;

            // Puanı SADECE BİR KEZ ekle (Döngüye girmesin)
            game.Score.Value += kazanilanPuan;

            if (multiplierText != null)
            {
                multiplierText.gameObject.SetActive(true);
                multiplierText.text = $"{basePuan} x {carpan}\n+ {kazanilanPuan}!";
            }

            // Animasyon beklerken Update'in şişmesini engellemek için kısa tut
            yield return new WaitForSecondsRealtime(0.5f);

            if (multiplierText != null) multiplierText.gameObject.SetActive(false);

            // DİKKAT: Burada CheckScoreAndTransition çağırırken dikkatli ol!
            // Eğer o fonksiyon içinde tekrar coroutine başlatıyorsan RAM patlar.
            CheckScoreAndTransition(game, game.Level.TargetScore);

            game.Resume();
            // AddPiece() BURADAN KALKADı - SafeBombSequence veya HandleNormalRowClear'da çağrılacak
        }

        private IEnumerator ShowWinSequence()
        {
            if (youWonPanel != null) youWonPanel.SetActive(true);
            yield return new WaitForSeconds(2f);
            if (youWonPanel != null) youWonPanel.SetActive(false);

            // Deste kartlarını reset et (ColorRow'daki objeleri sil ve yeniden spawn et)
            if (deckCardsManager != null)
            {
                deckCardsManager.ResetDeckCards();
                Debug.Log("MenuManager.ShowWinSequence: Deste kartları reset edildi!");
            }
            else
            {
                Debug.LogWarning("MenuManager.ShowWinSequence: deckCardsManager NULL! Inspector'dan bağla.");
            }

            gamePanel.SetActive(false);
            shopPanel.SetActive(true);
        }

        public void OnNextLevelButtonClicked()
        {
            shopPanel.SetActive(false);
            // Haritayı es geçip doğrudan sıradaki leveli başlatıyoruz
            StartLevel();
        }

        public void StartLevel()
        {
            _currentLevelIndex++;
            if (_currentLevelIndex >= levelTargetScores.Length)
            {
                _currentLevelIndex = levelTargetScores.Length - 1;
            }

            int nextTargetScore = levelTargetScores[_currentLevelIndex];

            shopPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            gamePanel.SetActive(true);

            if (levelView != null && levelView.board != null && levelView.game != null)
            {
                levelView.game.Start();
                levelView.game.Level.TargetScore = nextTargetScore;

                if (levelView.targetScoreText != null)
                    levelView.targetScoreText.text = "HEDEF: " + nextTargetScore;

                levelView.board.ResetDeck();

                // Deste kartlarını yeni deck durumuna göre yeniden oluştur
                if (deckCardsManager != null)
                {
                    deckCardsManager.ResetDeckCards();
                    Debug.Log("MenuManager.StartLevel: Deste kartları yeniden oluşturuldu.");
                }

                if (levelView.deckCountText != null && levelView.board.Deck != null)
                    levelView.deckCountText.text = "DESTE: " + levelView.board.Deck.TotalCount;

                levelView.game.Resume();
            }
        }

        // ==========================================
        // MAĞAZA SİSTEMİ: Bomba Satın Alma Butonu İçin
        // ==========================================
        public void BuyBomb()
        {
            if (deckCardsManager != null)
            {
                deckCardsManager.EnterBombSelectionMode();
                Debug.Log("MenuManager.BuyBomb: Bomba seçim modu açıldı. Kullanıcı bir kart seçecek.");
            }
            else
            {
                Debug.LogError("MenuManager.BuyBomb: deckCardsManager NULL! Inspector'dan bağla.");
            }
        }

        // ==========================================
        // DECK UI SİSTEMİ: Pause/Resume Metodları
        // ==========================================
        /// <summary>
        /// Deck paneli açıldığında oyunu durdur.
        /// </summary>
        public void PauseGameForDeck()
        {
            if (levelView != null && levelView.game != null)
            {
                levelView.game.Pause();
            }
        }

        /// <summary>
        /// Deck paneli kapatıldığında oyunu devam ettir.
        /// </summary>
        public void ResumeGameFromDeck()
        {
            if (levelView != null && levelView.game != null)
            {
                levelView.game.Resume();
            }
        }
    }
}