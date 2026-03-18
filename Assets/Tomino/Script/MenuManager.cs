using UnityEngine;
using TMPro;
using Tomino;

public class MenuManager : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject levelSelectPanel;
    public GameObject gamePanel;
    public TextMeshProUGUI levelSelectTargetText;
    public Tomino.View.LevelView levelView;

    private int _nextTargetScore = 100;

    public void CheckScoreAndTransition(Tomino.Game game, int currentLevelTarget)
    {
        if (game.Score.Value >= currentLevelTarget)
        {
            _nextTargetScore = currentLevelTarget + 100;
            if (game != null) game.Pause();
            OpenShop();
        }
    }

    private void OpenShop()
    {
        gamePanel.SetActive(false);
        shopPanel.SetActive(true);
    }

    public void OnNextLevelButtonClicked()
    {
        shopPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void StartLevel()
    {
        shopPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        gamePanel.SetActive(true);

        if (levelView != null && levelView.board != null)
        {
            if (levelView.game != null)
            {
                levelView.game.Start();
            }

            if (levelView.game != null)
            {
                levelView.game.Level.TargetScore = _nextTargetScore;
                if (levelView.targetScoreText != null)
                    levelView.targetScoreText.text = "HEDEF: " + _nextTargetScore;
            }

            levelView.board.ResetDeck();

            if (levelView.deckCountText != null && levelView.board.Deck != null)
                levelView.deckCountText.text = "DESTE: " + levelView.board.Deck.TotalCount;

            if (levelView.game != null)
            {
                levelView.game.Resume();
            }
        }
    }
}