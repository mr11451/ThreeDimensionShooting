using System.Collections.Generic;
using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ウェーブ制の敵スポーン管理。全滅で次ウェーブへ進む。
    /// 5ウェーブごとに大型ボスを出す。
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        private const int BossWaveInterval = 5;

        [Header("Spawn")]
        [SerializeField] private GameObject _chaserPrefab;
        [SerializeField] private GameObject _bossPrefab;
        [SerializeField] private GameObject _missileSupplyPodPrefab;
        [SerializeField] private float _spawnRadius = 80f;
        [SerializeField] private float _supplyPodSpawnRadius = 30f;
        [SerializeField] private int _baseEnemyCount = 3;
        [SerializeField] private int _enemyCountPerWave = 1;
        [SerializeField] private Transform _player;

        [Header("Wave Interval")]
        [SerializeField] private float _waveInterval = 3f;
        [SerializeField] private float _bossClearInterval = 8f;

        private float _nextWaveTime;
        private readonly List<EnemyBase> _alive = new List<EnemyBase>();
        private GameObject _missileSupplyPod;
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
                if (Time.time >= _nextWaveTime)
                {
                    SpawnWave(GameManager.Instance.CurrentWave);
                }
            }
            else if (_alive.Count == 0)
            {
                _waveActive = false;
                int clearedWave = GameManager.Instance.CurrentWave;
                bool wasBossWave = clearedWave > 0 && clearedWave % BossWaveInterval == 0;
                _nextWaveTime = Time.time + (wasBossWave ? _bossClearInterval : _waveInterval);
                GameManager.Instance.AdvanceToNextWave();
            }
        }

        private void SpawnWave(int wave)
        {
            if (_player == null) return;

            SpawnMissileSupplyPod();

            if (wave > 0 && wave % BossWaveInterval == 0)
            {
                if (_bossPrefab == null) return;

                Vector3 bossDirection = Random.onUnitSphere;
                Vector3 bossPosition = _player.position + bossDirection * _spawnRadius;
                var boss = Instantiate(_bossPrefab, bossPosition, Quaternion.LookRotation(-bossDirection));
                if (boss.TryGetComponent<EnemyBase>(out var bossEnemy))
                {
                    _alive.Add(bossEnemy);
                }
                _waveActive = true;
                return;
            }

            if (_chaserPrefab == null) return;

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

        private void SpawnMissileSupplyPod()
        {
            if (_missileSupplyPodPrefab == null || _missileSupplyPod != null) return;

            Vector3 direction = Random.onUnitSphere;
            Vector3 position = _player.position + direction * _supplyPodSpawnRadius;
            _missileSupplyPod = Instantiate(
                _missileSupplyPodPrefab,
                position,
                Quaternion.identity);
        }
    }
}
