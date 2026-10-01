using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ゲームオーバー画面。スコア表示とリトライ/タイトル遷移を担当する。
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "Game";
        [SerializeField] private string _titleSceneName = "Title";
        [SerializeField] private GameOverScoreView _scoreView;

        private void Start()
        {
            if (_scoreView != null && GameManager.Instance != null)
            {
                _scoreView.SetScore(GameManager.Instance.Score);
            }
        }

        private void Update()
        {
            var pad = Gamepad.current;
            var kb = Keyboard.current;

            bool retry = (pad != null && pad.startButton.wasPressedThisFrame)
                         || (kb != null && kb.enterKey.wasPressedThisFrame);
            bool toTitle = (pad != null && pad.selectButton.wasPressedThisFrame)
                           || (kb != null && kb.escapeKey.wasPressedThisFrame);

            if (retry)
            {
                SceneManager.LoadScene(_gameSceneName);
            }
            else if (toTitle)
            {
                SceneManager.LoadScene(_titleSceneName);
            }
        }
    }
}
