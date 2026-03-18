using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Tomino.Model;

namespace Tomino.View
{
    public class LevelView : MonoBehaviour
    {
        public Text levelText;
        public Text linesText;
        public TextMeshProUGUI multiplierText;
        public TextMeshProUGUI targetScoreText;
        public TextMeshProUGUI pointDisplay;
        public TextMeshProUGUI deckCountText;
        public MenuManager menuManager;

        public Game game;
        public Board board;

        internal void Update()
        {
            if (game == null || game.Level == null) return;

            levelText.text = "BÖLÜM: " + game.Level.Number.ToString();
            linesText.text = "SATIR: " + game.Level.Lines.ToString();
            targetScoreText.text = "HEDEF: " + game.Level.TargetScore.ToString();

            int currentMult = game.Level.CurrentMultiplier;
            multiplierText.text = currentMult > 0 ? "ÇARPAN: x" + currentMult : "ÇARPAN: -";

            if (deckCountText != null && board != null && board.Deck != null)
            {
                deckCountText.text = "DESTE: " + board.Deck.TotalCount.ToString();
            }
        }

        public void ShowPointAnimation(int totalBlocks, int multiplier)
        {
            StartCoroutine(AnimatePointsRoutine(totalBlocks, multiplier));
        }

        private IEnumerator AnimatePointsRoutine(int totalBlocks, int multiplier)
        {
            game.Pause();

            int currentPoints = 0;
            pointDisplay.gameObject.SetActive(true);

            for (int i = 0; i < totalBlocks; i++)
            {
                currentPoints += 1;
                pointDisplay.text = currentPoints.ToString();
                yield return new WaitForSeconds(0.1f);
            }

            
            yield return new WaitForSeconds(0.3f);

            int finalPointsForThisTurn = currentPoints * multiplier;
            pointDisplay.text = finalPointsForThisTurn.ToString();

            yield return new WaitForSeconds(2.0f);

            game.Score.Add(finalPointsForThisTurn);

            pointDisplay.gameObject.SetActive(false);

            if (menuManager != null)
            {
                Debug.Log("Skor Kontrolü Yapılıyor... Mevcut Skor: " + game.Score.Value + " / Hedef: " + game.Level.TargetScore);

                if (game.Score.Value >= game.Level.TargetScore)
                {
                    Debug.Log("Hedef aşıldı! Mağazaya geçiliyor.");
                    menuManager.CheckScoreAndTransition(game, game.Level.TargetScore);
                }
            }
            else
            {
                Debug.LogError("MenuManager, LevelView'a atanmamış!");
            }

            game.Resume();
        }
    }
}