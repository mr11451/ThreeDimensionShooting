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

        [Header("Arrival")]
        [SerializeField] private float _arrivalSpeed = 250f;
        [SerializeField] private float _arrivalEndDistance = 100f;

        private Rigidbody _rb;
        private Transform _player;
        private float _fireTimer;
        private float _currentSpeed;
        private bool _arrived;
        private bool _inFormation;
        private Vector3 _formationDirection;
        private System.Collections.Generic.List<ChaserEnemy> _formationMembers;

        /// <summary>編隊の一員として、到着フェーズでは全機が同じ方向へ平行に進む。</summary>
        public void JoinFormation(Vector3 travelDirection, System.Collections.Generic.List<ChaserEnemy> members)
        {
            _formationMembers = members;
            _inFormation = true;
            _formationDirection = travelDirection.normalized;
        }

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

            // 自機を原点に固定した設計では、敵は「自機の位置 = 0」を目標として振る舞う。
            Vector3 toPlayer = _player.position == Vector3.zero
                ? -transform.position
                : _player.position - transform.position;

            float playerDistance = toPlayer.magnitude;

            // 編隊は 1 機でも撃墜されるまで維持し、隊形のまま接近して射撃する
            if (_inFormation && IsFormationIntact())
            {
                _arrived = true;
                // 編隊の重心を自機へ向ける(全機共通の方向なので隊形は保たれる)
                Vector3 centroid = Vector3.zero;
                foreach (var m in _formationMembers) centroid += m.transform.position;
                centroid /= _formationMembers.Count;
                Vector3 toCentroid = (_player.position == Vector3.zero ? -centroid : _player.position - centroid);
                if (toCentroid.sqrMagnitude > 0.001f) _formationDirection = toCentroid.normalized;
                float centroidDistance = toCentroid.magnitude;
                _rb.MoveRotation(Quaternion.LookRotation(_formationDirection, Vector3.up));
                float speed = centroidDistance > _arrivalEndDistance ? _arrivalSpeed
                    : (centroidDistance > _minimumPlayerDistance ? _moveSpeed : 0f);
                _rb.linearVelocity = _formationDirection * speed;
                if (playerDistance <= _attackRange)
                {
                    _fireTimer -= Time.fixedDeltaTime;
                    if (_fireTimer <= 0f) { Fire(); _fireTimer = _fireInterval; }
                }
                return;
            }
            _inFormation = false;

            // 遠方から高速で接近し、一定距離まで来たら通常行動に移る
            if (!_arrived)
            {
                if (playerDistance > _arrivalEndDistance)
                {
                    Vector3 arrivalDirection = _inFormation ? _formationDirection : toPlayer / playerDistance;
                    _rb.MoveRotation(Quaternion.LookRotation(arrivalDirection, Vector3.up));
                    _rb.linearVelocity = arrivalDirection * _arrivalSpeed;
                    return;
                }
                _arrived = true;
                _currentSpeed = _moveSpeed;
            }
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

            if (playerDistance <= _attackRange)
            {
                _fireTimer -= Time.fixedDeltaTime;
                if (_fireTimer <= 0f)
                {
                    Fire();
                    _fireTimer = _fireInterval;
                }
            }
        }

        private bool IsFormationIntact()
        {
            foreach (var m in _formationMembers)
                if (m == null || !m.IsAlive) return false;
            return true;
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

            // 自機を原点固定する設計では、プレイヤー方向は 0 を基準に計算する。
            Vector3 targetDirection = _player.position == Vector3.zero
                ? -transform.position
                : _player.position - transform.position;

            Vector3 dir = targetDirection.sqrMagnitude > 0.001f
                ? targetDirection.normalized
                : transform.forward;

            Quaternion spread = Quaternion.Euler(
                Random.Range(-_bulletSpreadDeg, _bulletSpreadDeg),
                Random.Range(-_bulletSpreadDeg, _bulletSpreadDeg),
                0f);
            Quaternion rot = Quaternion.LookRotation(dir) * spread;

            Instantiate(_bulletPrefab, transform.position + dir * 1.5f, rot);
        }
    }
}

