using UnityEngine;
using TMPro;
using System.Collections;

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
        public Tomino.View.LevelView levelView;
        
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
    if (rowsCount <= 0) {
        if (game != null) { game.Resume(); game.AddPiece(); }
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
    game.AddPiece(); 
}

        private IEnumerator ShowWinSequence()
        {
            if (youWonPanel != null) youWonPanel.SetActive(true);
            yield return new WaitForSeconds(2f);
            if (youWonPanel != null) youWonPanel.SetActive(false);
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
            if (levelView != null && levelView.board != null && levelView.board.Deck != null)
            {
                levelView.board.Deck.BombCount++;
                Debug.Log("Bomba satın alındı! Destedeki bomba sayısı: " + levelView.board.Deck.BombCount);
            }
        }
    }
}