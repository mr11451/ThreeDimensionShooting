using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 周囲を旋回しながらプレイヤーへ扇状に弾を撃つ大型ボス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BossEnemy : EnemyBase
    {
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private float _orbitRadius = 65f;
        [SerializeField] private float _orbitSpeed = 12f;
        [SerializeField] private float _turnSpeed = 45f;
        [SerializeField] private float _fireInterval = 1.2f;
        [SerializeField] private int _bulletsPerVolley = 5;
        [SerializeField] private float _volleySpreadDeg = 32f;

        [Header("Homing Missiles")]
        [SerializeField] private GameObject _homingMissilePrefab;
        [SerializeField] private float _homingMissileInterval = 60f;
        [SerializeField] private int _homingMissilesPerLaunch = 2;

        private Rigidbody _rb;
        private Transform _player;
        private float _fireTimer;
        private float _homingMissileTimer;
        private float _orbitAngle;
        private bool _arrived;

        [Header("Arrival")]
        [SerializeField] private float _arrivalSpeed = 150f;
        protected override float ExplosionScale => 4f;

        protected override void Die()
        {
            foreach (var missile in BossHomingMissile.Active.ToArray())
            {
                if (missile != null) Destroy(missile.gameObject);
            }

            ClearBanner.Show();
            base.Die();
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = true;
            _orbitAngle = Random.Range(0f, 360f);
            _hp = FlightSettingsFile.GetInt("BossHp", _hp, 1);
        }

        private void Start()
        {
            var settings = FlightSettingsFile.Load();
            if (settings.TryGetValue("BossHomingMissileInterval", out float missileInterval))
            {
                _homingMissileInterval = Mathf.Max(0.1f, missileInterval);
            }
            if (settings.TryGetValue("BossHomingMissilesPerLaunch", out float missilesPerLaunch))
            {
                _homingMissilesPerLaunch = Mathf.Max(0, Mathf.RoundToInt(missilesPerLaunch));
            }

            var playerController = FindFirstObjectByType<PlayerShipController>();
            if (playerController != null)
            {
                _player = playerController.transform;
            }

            _fireTimer = _fireInterval;
            _homingMissileTimer = _homingMissileInterval;
        }

        private void FixedUpdate()
        {
            if (_player == null || !IsAlive) return;

            Vector3 playerPosition = _player.position;
            _orbitAngle += _orbitSpeed * Time.fixedDeltaTime;
            float radians = _orbitAngle * Mathf.Deg2Rad;
            Vector3 orbitOffset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians * 0.7f) * 0.25f, Mathf.Sin(radians));
            Vector3 targetPosition = playerPosition + orbitOffset * _orbitRadius;

            // 遠方から高速で周回位置まで到着するまでは攻撃しない
            if (!_arrived)
            {
                _rb.MovePosition(Vector3.MoveTowards(_rb.position, targetPosition, _arrivalSpeed * Time.fixedDeltaTime));
                Vector3 toTarget = playerPosition - _rb.position;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    _rb.MoveRotation(Quaternion.LookRotation(toTarget));
                }
                _arrived = (targetPosition - _rb.position).sqrMagnitude < 4f;
                return;
            }
            _rb.MovePosition(Vector3.MoveTowards(
                _rb.position,
                targetPosition,
                _orbitSpeed * Time.fixedDeltaTime));

            Vector3 toPlayer = playerPosition - _rb.position;
            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
                _rb.MoveRotation(Quaternion.RotateTowards(
                    _rb.rotation,
                    targetRotation,
                    _turnSpeed * Time.fixedDeltaTime));
            }

            _fireTimer -= Time.fixedDeltaTime;
            if (_fireTimer <= 0f)
            {
                FireVolley(playerPosition);
                _fireTimer = _fireInterval;
            }

            _homingMissileTimer -= Time.fixedDeltaTime;
            if (_homingMissileTimer <= 0f)
            {
                FireHomingMissiles();
                _homingMissileTimer = _homingMissileInterval;
            }
        }

        private void FireVolley(Vector3 targetPosition)
        {
            if (_bulletPrefab == null || _bulletsPerVolley <= 0) return;

            Vector3 direction = targetPosition - _rb.position;
            if (direction.sqrMagnitude <= 0.001f) return;
            Quaternion aimRotation = Quaternion.LookRotation(direction.normalized);
            float halfSpread = _volleySpreadDeg * 0.5f;

            for (int i = 0; i < _bulletsPerVolley; i++)
            {
                float spread = _bulletsPerVolley == 1
                    ? 0f
                    : Mathf.Lerp(-halfSpread, halfSpread, (float)i / (_bulletsPerVolley - 1));
                Quaternion bulletRotation = aimRotation * Quaternion.Euler(0f, spread, 0f);
                Instantiate(_bulletPrefab, _rb.position + bulletRotation * Vector3.forward * 5f, bulletRotation);
            }
        }

        private void FireHomingMissiles()
        {
            if (_homingMissilePrefab == null || _homingMissilesPerLaunch <= 0 || _player == null) return;

            Vector3 toPlayer = _player.position - _rb.position;
            if (toPlayer.sqrMagnitude <= 0.001f) return;

            Quaternion aimRotation = Quaternion.LookRotation(toPlayer.normalized);
            for (int i = 0; i < _homingMissilesPerLaunch; i++)
            {
                float side = i == 0 ? -1f : 1f;
                Vector3 spawnOffset = aimRotation * new Vector3(side * 2f, 0f, 0f);
                Vector3 launchDirection = (toPlayer.normalized + aimRotation * new Vector3(side * 0.12f, 0f, 0f)).normalized;
                Quaternion launchRotation = Quaternion.LookRotation(launchDirection);
                var missileObject = Instantiate(
                    _homingMissilePrefab,
                    _rb.position + toPlayer.normalized * 8f + spawnOffset,
                    launchRotation);
                if (missileObject.TryGetComponent<BossHomingMissile>(out var missile))
                {
                    missile.SetTarget(_player);
                }
            }
        }
    }
}
