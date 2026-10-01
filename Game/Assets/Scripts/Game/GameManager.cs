using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ゲーム全体の進行管理。シーン状態(タイトル/ゲーム/ゲームオーバー)と
    /// スコア・ウェーブを統括する。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Title,
            InGame,
            GameOver,
        }

        [Header("Score")]
        [SerializeField] private int _score;
        [SerializeField] private int _comboCount;
        [SerializeField] private float _comboResetSeconds = 4f;

        [Header("Wave")]
        [SerializeField] private int _currentWave = 0;

        [Header("Lives")]
        [SerializeField] private int _startingLives = 3;

        public GameState State { get; private set; } = GameState.Title;
        public int Score => _score;
        public int ComboCount => _comboCount;
        public int ComboMultiplier => 1 << Mathf.Min(_comboCount, 4); // x1, x2, x4, x8, x16
        public int CurrentWave => _currentWave;
        public int Lives { get; private set; }

        private float _comboTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioManager が存在しなければ自動生成
            if (AudioManager.Instance == null)
            {
                var audioGo = new GameObject("AudioManager");
                audioGo.AddComponent<AudioManager>();
            }
        }

        private void Update()
        {
            if (State != GameState.InGame) return;

            if (_comboCount > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            _score = 0;
            _comboCount = 0;
            _currentWave = 0;
            Lives = Mathf.Max(1, _startingLives);
            State = GameState.InGame;
            AdvanceWave();
        }

        public bool LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            return Lives > 0;
        }

        public void AddScore(int baseScore)
        {
            _comboCount++;
            _comboTimer = _comboResetSeconds;
            _score += baseScore * ComboMultiplier;
        }

        public void ResetCombo()
        {
            _comboCount = 0;
        }

        public void GameOver()
        {
            Time.timeScale = 1f;
            State = GameState.GameOver;
            AudioManager.Instance?.StopBGM();
            // ゲームオーバーシーンへ遷移
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }

        public void ReturnToTitle()
        {
            State = GameState.Title;
            // TODO: タイトル画面表示
        }

        /// <summary>
        /// ウェーブを1つ進める。WaveManager から呼ばれる。
        /// </summary>
        public void AdvanceToNextWave()
        {
            _currentWave++;
        }

        private void AdvanceWave()
        {
            _currentWave++;
            // TODO: WaveManager へ出撃指示
        }
    }
}
