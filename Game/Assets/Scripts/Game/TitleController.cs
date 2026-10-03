using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// タイトル画面。Start ボタン or Space でゲームシーンへ遷移する。
    /// </summary>
    public class TitleController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "Game";
        [SerializeField] private TMP_Text _highScoreText;
        [SerializeField] private Text _legacyHighScoreText;

        private void Start()
        {
            UpdateHighScoreText();
        }

        private void Update()
        {
            bool start = false;

            var pad = Gamepad.current;
            if (pad != null && pad.startButton.wasPressedThisFrame)
            {
                start = true;
            }

            var kb = Keyboard.current;
            if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
            {
                start = true;
            }

            if (start)
            {
                SceneManager.LoadScene(_gameSceneName);
            }
        }

        private void UpdateHighScoreText()
        {
            object target = _highScoreText != null ? (object)_highScoreText : _legacyHighScoreText;
            if (target == null)
            {
                return;
            }

            var content = $"HIGH SCORE  {HighScoreStore.BestScore:N0}";
            if (target is TMP_Text tmpText)
            {
                tmpText.text = content;
                return;
            }

            if (target is Text uiText)
            {
                uiText.text = content;
            }
        }
    }
}
