using TMPro;
using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ゲーム中の HUD。シールド、スコア、コンボ倍率、ミサイル残弾、ウェーブ数を表示する。
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _shieldText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _comboText;
        [SerializeField] private TMP_Text _missileText;
        [SerializeField] private TMP_Text _waveText;

        [Header("Sources")]
        [SerializeField] private PlayerShield _playerShield;
        [SerializeField] private PlayerWeapons _playerWeapons;

        private GUIStyle _consoleLabelStyle;
        private GUIStyle _consoleValueStyle;

        private void Start()
        {
            if (_playerShield == null)
            {
                _playerShield = FindFirstObjectByType<PlayerShield>();
            }
            if (_playerWeapons == null)
            {
                _playerWeapons = FindFirstObjectByType<PlayerWeapons>();
            }
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (_shieldText != null && _playerShield != null)
            {
                _shieldText.text = $"SHIELD {(int)_playerShield.Current,3}/{ (int)_playerShield.Max}";
            }

            if (_scoreText != null)
            {
                _scoreText.text = $"SCORE {gm.Score,8:N0}";
            }

            if (_comboText != null)
            {
                int mult = gm.ComboMultiplier;
                _comboText.text = mult > 1 ? $"COMBO x{mult}" : string.Empty;
            }

            if (_missileText != null && _playerWeapons != null)
            {
                _missileText.text = $"MISSILE {_playerWeapons.MissileStock}";
            }

            if (_waveText != null)
            {
                _waveText.text = $"WAVE {gm.CurrentWave}";
            }
        }

        private void OnGUI()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            _consoleLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
            };
            _consoleValueStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
            };

            float panelTop = Screen.height * 0.885f;
            var panel = new Rect(Screen.width * 0.06f, panelTop, Screen.width * 0.88f, Screen.height - panelTop);
            float labelSize = Mathf.Clamp(Screen.height * 0.012f, 10f, 16f);
            float valueSize = Mathf.Clamp(Screen.height * 0.019f, 14f, 24f);
            _consoleLabelStyle.fontSize = Mathf.RoundToInt(labelSize);
            _consoleLabelStyle.normal.textColor = new Color(0.48f, 0.82f, 0.86f);
            _consoleValueStyle.fontSize = Mathf.RoundToInt(valueSize);

            DrawConsoleRect(panel, new Color(0.012f, 0.035f, 0.045f, 0.22f));
            DrawConsoleRect(new Rect(panel.x, panel.y, panel.width, 2f), new Color(0.1f, 0.9f, 1f, 0.9f));

            float columnWidth = panel.width / 6f;
            for (int i = 1; i < 6; i++)
            {
                float dividerX = panel.x + columnWidth * i;
                DrawConsoleRect(new Rect(dividerX, panel.y + panel.height * 0.2f, 1f, panel.height * 0.6f),
                    new Color(0.1f, 0.65f, 0.7f, 0.4f));
            }

            string shieldValue = _playerShield != null
                ? $"{(int)_playerShield.Current} / {(int)_playerShield.Max}"
                : "--- / ---";
            string missileValue = _playerWeapons != null ? _playerWeapons.MissileStock.ToString() : "--";
            DrawConsoleCell(panel, 0, "SHIELD", shieldValue, new Color(0.3f, 1f, 0.6f));
            DrawConsoleCell(panel, 1, "SCORE", gm.Score.ToString("N0"), Color.white);
            DrawConsoleCell(panel, 2, "COMBO", $"x{gm.ComboMultiplier}", new Color(1f, 0.8f, 0.2f));
            DrawConsoleCell(panel, 3, "MISSILE", missileValue, new Color(0.3f, 0.9f, 1f));
            DrawConsoleCell(panel, 4, "WAVE", gm.CurrentWave.ToString(), Color.white);
            DrawConsoleCell(panel, 5, "LIVES", gm.Lives.ToString(), new Color(1f, 0.55f, 0.35f));
        }

        private void DrawConsoleCell(Rect panel, int index, string label, string value, Color valueColor)
        {
            float columnWidth = panel.width / 6f;
            float x = panel.x + columnWidth * index;
            var labelRect = new Rect(x, panel.y + panel.height * 0.15f, columnWidth, panel.height * 0.3f);
            var valueRect = new Rect(x, panel.y + panel.height * 0.43f, columnWidth, panel.height * 0.45f);
            _consoleValueStyle.normal.textColor = valueColor;
            GUI.Label(labelRect, label, _consoleLabelStyle);
            GUI.Label(valueRect, value, _consoleValueStyle);
        }

        private static void DrawConsoleRect(Rect rect, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
