using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 自機の武装。メインショット(連射)とロックオンミサイルを管理する。
    /// </summary>
    public class PlayerWeapons : MonoBehaviour
    {
        [Header("Main Shot")]
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _fireRate = 0.12f;
        [SerializeField] private float _bulletSpeed = 200f;

        [Header("Lock-on Missile")]
        [SerializeField] private GameObject _missilePrefab;
        [SerializeField] private int _missileMaxStock = 8;
        [SerializeField] private float _lockOnRadius = 120f;
        [SerializeField] private float _lockOnAngleDeg = 15f;
        [SerializeField] private float _maxLockOnAngleDeg = 45f;
        [SerializeField] private float _lockOnExpandSpeed = 20f;
        [SerializeField] private int _maxLockTargets = 4;

        [Header("Targeting")]
        [SerializeField] private LayerMask _enemyLayer = ~0; // デフォルト: 全レイヤー

        private readonly List<Transform> _lockTargets = new List<Transform>();
        private float _fireCooldown;
        private int _missileStock;
        private bool _wasLockHeld;
        private float _currentLockOnAngleDeg;
        private Texture2D _lockRangeTexture;

        public int MissileStock => _missileStock;
        public IReadOnlyList<Transform> LockTargets => _lockTargets;

        private void Awake()
        {
            _missileStock = _missileMaxStock;
            _currentLockOnAngleDeg = _lockOnAngleDeg;
            _lockRangeTexture = CreateRingTexture(256, 4f, new Color(1f, 0.8f, 0.2f, 0.9f));
        }

        private void OnGUI()
        {
            var pad = Gamepad.current;
            if (pad == null || !pad.leftShoulder.isPressed || _missileStock <= 0 || _lockRangeTexture == null) return;

            float expansion = Mathf.InverseLerp(_lockOnAngleDeg, _maxLockOnAngleDeg, _currentLockOnAngleDeg);
            float ringSize = Mathf.Lerp(180f, 300f, expansion);
            GUI.DrawTexture(
                new Rect(Screen.width * 0.5f - ringSize * 0.5f, Screen.height * 0.5f - ringSize * 0.5f, ringSize, ringSize),
                _lockRangeTexture,
                ScaleMode.StretchToFill,
                true);
        }

        private void Update()
        {
            var pad = Gamepad.current;
            if (pad == null) return;

            _fireCooldown -= Time.deltaTime;

            // RB: メインショット
            if (pad.rightShoulder.isPressed && _fireCooldown <= 0f)
            {
                FireMainShot();
                _fireCooldown = _fireRate;
            }

            // LB: ホールドでロックオン、離してミサイル発射
            bool lockHeld = pad.leftShoulder.isPressed;
            if (lockHeld && _missileStock > 0)
            {
                _currentLockOnAngleDeg = Mathf.MoveTowards(
                    _currentLockOnAngleDeg,
                    _maxLockOnAngleDeg,
                    _lockOnExpandSpeed * Time.deltaTime);
                UpdateLockOn(_currentLockOnAngleDeg);
            }
            else if (_wasLockHeld && _lockTargets.Count > 0 && _missileStock > 0)
            {
                FireMissiles();
                _currentLockOnAngleDeg = _lockOnAngleDeg;
            }
            else if (!lockHeld)
            {
                _lockTargets.Clear();
                _currentLockOnAngleDeg = _lockOnAngleDeg;
            }
            _wasLockHeld = lockHeld;
        }

        private void FireMainShot()
        {
            if (_bulletPrefab == null || _muzzle == null) return;
            var go = Instantiate(_bulletPrefab, _muzzle.position, _muzzle.rotation);
            if (go.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = _muzzle.forward * _bulletSpeed;
            }
            AudioManager.Instance?.PlayShot();
        }

        /// <summary>
        /// 視線前方のロックオン範囲内の敵を捕捉する。
        /// </summary>
        private void UpdateLockOn(float lockOnAngleDeg)
        {
            _lockTargets.Clear();
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;

            var hits = Physics.OverlapSphere(origin, _lockOnRadius, _enemyLayer);
            var candidates = new List<(Transform t, float dist)>();

            foreach (var h in hits)
            {
                if (!h.TryGetComponent<EnemyBase>(out var enemy) || !enemy.IsAlive) continue;

                Vector3 to = h.transform.position - origin;
                float angle = Vector3.Angle(forward, to);
                if (angle <= lockOnAngleDeg)
                {
                    candidates.Add((h.transform, to.magnitude));
                }
            }

            candidates.Sort((a, b) => a.dist.CompareTo(b.dist));
            for (int i = 0; i < candidates.Count && i < _maxLockTargets; i++)
            {
                _lockTargets.Add(candidates[i].t);
            }
        }

        /// <summary>
        /// ロックした全ターゲットにミサイルを発射する。
        /// </summary>
        private void FireMissiles()
        {
            if (_missilePrefab == null || _missileStock <= 0) return;

            foreach (var target in _lockTargets)
            {
                if (_missileStock <= 0) break;
                if (target == null) continue;

                var go = Instantiate(_missilePrefab, transform.position, transform.rotation);
                if (go.TryGetComponent<Missile>(out var missile))
                {
                    missile.SetTarget(target);
                }
                _missileStock--;
            }
            AudioManager.Instance?.PlayMissile();
            _lockTargets.Clear();
        }

        private static Texture2D CreateRingTexture(int size, float thickness, Color ringColor)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    pixels[y * size + x] = Mathf.Abs(distance - radius) <= thickness
                        ? ringColor
                        : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            if (_lockRangeTexture != null) Destroy(_lockRangeTexture);
        }

        /// <summary>
        /// ミサイルを補充する(ウェーブクリア時など)。
        /// </summary>
        public void RestockMissiles()
        {
            _missileStock = _missileMaxStock;
        }
    }
}
