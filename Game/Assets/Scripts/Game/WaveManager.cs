using System.Collections.Generic;
using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ウェーブ制の敵スポーン管理。全滅で次ウェーブへ進む。
    /// 数ウェーブごとに大型ボスを出す(未実装)。
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Spawn")]
        [SerializeField] private GameObject _chaserPrefab;
        [SerializeField] private float _spawnRadius = 80f;
        [SerializeField] private int _baseEnemyCount = 3;
        [SerializeField] private int _enemyCountPerWave = 1;
        [SerializeField] private Transform _player;

        private readonly List<EnemyBase> _alive = new List<EnemyBase>();
        private bool _waveActive;

        private void Start()
        {
            if (_player == null)
            {
                var pc = FindFirstObjectByType<PlayerShipController>();
                if (pc != null) _player = pc.transform;
            }

            // ゲームシーン開始時にゲームを開始する
            if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.InGame)
            {
                GameManager.Instance.StartGame();
                AudioManager.Instance?.StartBGM();
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.InGame)
            {
                return;
            }

            _alive.RemoveAll(e => e == null);

            if (!_waveActive)
            {
                SpawnWave(GameManager.Instance.CurrentWave);
            }
            else if (_alive.Count == 0)
            {
                _waveActive = false;
                GameManager.Instance.AdvanceToNextWave();
            }
        }

        private void SpawnWave(int wave)
        {
            if (_chaserPrefab == null || _player == null) return;

            int count = _baseEnemyCount + wave * _enemyCountPerWave;
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Random.onUnitSphere;
                Vector3 pos = _player.position + dir * _spawnRadius;
                var go = Instantiate(_chaserPrefab, pos, Quaternion.LookRotation(-dir));
                if (go.TryGetComponent<EnemyBase>(out var enemy))
                {
                    _alive.Add(enemy);
                }
            }
            _waveActive = true;
        }
    }
}
