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

        private GUIStyle _hudStyle;

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

            _hudStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                normal = { textColor = Color.white },
            };

            if (_shieldText == null && _playerShield != null)
            {
                GUI.Label(new Rect(30f, 25f, 300f, 32f),
                    $"SHIELD {(int)_playerShield.Current}/{(int)_playerShield.Max}", _hudStyle);
            }

            if (_scoreText == null)
            {
                _hudStyle.alignment = TextAnchor.UpperRight;
                GUI.Label(new Rect(Screen.width - 330f, 25f, 300f, 32f),
                    $"SCORE {gm.Score:N0}", _hudStyle);
            }

            if (_missileText == null && _playerWeapons != null)
            {
                _hudStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(30f, Screen.height - 55f, 300f, 32f),
                    $"MISSILE {_playerWeapons.MissileStock}", _hudStyle);
            }

            if (_waveText == null)
            {
                _hudStyle.alignment = TextAnchor.UpperCenter;
                GUI.Label(new Rect(Screen.width * 0.5f - 150f, 25f, 300f, 32f),
                    $"WAVE {gm.CurrentWave}", _hudStyle);
            }

            _hudStyle.alignment = TextAnchor.UpperRight;
        }
    }
}
