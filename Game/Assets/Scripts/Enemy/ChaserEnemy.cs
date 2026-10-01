using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 追跡型の敵。プレイヤーを追尾しながら接近・射撃する基本敵。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ChaserEnemy : EnemyBase
    {
        [SerializeField] private float _moveSpeed = 25f;
        [SerializeField] private float _turnSpeed = 90f;
        [SerializeField] private float _attackRange = 60f;
        [SerializeField] private float _fireInterval = 1.5f;

        [Header("Separation")]
        [SerializeField] private float _separationDistance = 6f;
        [SerializeField] private float _separationStrength = 2.5f;

        [Header("Player Avoidance")]
        [SerializeField] private float _minimumPlayerDistance = 25f;
        [SerializeField] private float _speedChangeRate = 35f;

        [Header("Weapon")]
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private float _bulletSpreadDeg = 4f;

        private Rigidbody _rb;
        private Transform _player;
        private float _fireTimer;
        private float _currentSpeed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _currentSpeed = _moveSpeed;
        }

        private void Start()
        {
            var pc = FindFirstObjectByType<PlayerShipController>();
            if (pc != null) _player = pc.transform;
        }

        private void FixedUpdate()
        {
            if (_player == null || !IsAlive) return;

            Vector3 toPlayer = _player.position - transform.position;
            float playerDistance = toPlayer.magnitude;
            Vector3 approachDirection = (toPlayer.sqrMagnitude > 0.001f)
                ? toPlayer.normalized
                : transform.forward;

            Vector3 desiredDirection = approachDirection + GetSeparationDirection();
            if (playerDistance < _minimumPlayerDistance)
            {
                float retreatStrength = 1f - playerDistance / _minimumPlayerDistance;
                desiredDirection -= approachDirection * (retreatStrength * 2f);
            }
            desiredDirection = desiredDirection.sqrMagnitude > 0.001f
                ? desiredDirection.normalized
                : transform.forward;

            Quaternion targetRot = Quaternion.LookRotation(desiredDirection, Vector3.up);
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime));

            float targetSpeed = playerDistance < _minimumPlayerDistance
                ? _moveSpeed * 0.5f
                : _moveSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _speedChangeRate * Time.fixedDeltaTime);
            _rb.linearVelocity = _rb.rotation * Vector3.forward * _currentSpeed;

            if (toPlayer.magnitude <= _attackRange)
            {
                _fireTimer -= Time.fixedDeltaTime;
                if (_fireTimer <= 0f)
                {
                    Fire();
                    _fireTimer = _fireInterval;
                }
            }
        }

        private Vector3 GetSeparationDirection()
        {
            Vector3 separation = Vector3.zero;
            var nearbyEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

            foreach (var enemy in nearbyEnemies)
            {
                if (enemy == this || !enemy.IsAlive) continue;

                Vector3 offset = transform.position - enemy.transform.position;
                float distance = offset.magnitude;
                if (distance <= 0.001f || distance >= _separationDistance) continue;

                float influence = 1f - distance / _separationDistance;
                separation += offset / distance * (influence * influence * _separationStrength);
            }

            return separation;
        }

        private void Fire()
        {
            if (_bulletPrefab == null || _player == null) return;

            // プレイヤー方向にわずかな拡散を付けて発射
            Vector3 dir = (_player.position - transform.position).normalized;
            Quaternion spread = Quaternion.Euler(
                Random.Range(-_bulletSpreadDeg, _bulletSpreadDeg),
                Random.Range(-_bulletSpreadDeg, _bulletSpreadDeg),
                0f);
            Quaternion rot = Quaternion.LookRotation(dir) * spread;

            Instantiate(_bulletPrefab, transform.position + dir * 1.5f, rot);
        }
    }
}
