using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// タイトル画面。Start ボタン or Space でゲームシーンへ遷移する。
    /// </summary>
    public class TitleController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "Game";

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
    }
}
