using TMPro;
using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ゲームオーバー画面のスコア表示。
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class GameOverScoreView : MonoBehaviour
    {
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        public void SetScore(int score)
        {
            if (_text != null)
            {
                _text.text = $"SCORE  {score:N0}";
            }
        }
    }
}
