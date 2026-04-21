using Tomino.Model;
using UnityEngine;
using Text = UnityEngine.UI.Text;

namespace Tomino.View
{
    public class ScoreView : MonoBehaviour
    {
        public Text scoreText;
        public Game game;
        public Board board;

        internal void Update()
        {
            if (game == null)
            {
                if (scoreText != null)
                    scoreText.text = "0";
                return;
            }

            const int padLength = ScoreFormat.Length;
            const char padCharacter = ScoreFormat.PadCharacter;
            scoreText.text = game.Score.Value.ToString().PadLeft(padLength, padCharacter);
        }
    }
}
