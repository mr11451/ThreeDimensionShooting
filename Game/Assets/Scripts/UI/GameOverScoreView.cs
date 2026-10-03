using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ゲームオーバー画面のスコア表示。
    /// </summary>
    public class GameOverScoreView : MonoBehaviour
    {
        private TMP_Text _tmpText;
        private Text _legacyText;

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            _legacyText = GetComponent<Text>();
        }

        public void SetScore(int score, int? bestScore = null)
        {
            var content = bestScore.HasValue
                ? $"SCORE  {score:N0}\nBEST  {bestScore.Value:N0}"
                : $"SCORE  {score:N0}";

            if (_tmpText != null)
            {
                _tmpText.text = content;
                return;
            }

            if (_legacyText != null)
            {
                _legacyText.text = content;
            }
        }
    }
}
