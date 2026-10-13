using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ゆるく自機を追尾し、プレイヤー弾で破壊できるボス誘導弾。
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class BossHomingMissile : MonoBehaviour
    {
        [SerializeField] private int _hp = 30;
        [SerializeField] private float _damage = 20f;
        [SerializeField] private float _speed = 24f;
        [SerializeField] private float _turnSpeed = 32f;
        [SerializeField] private float _aimErrorRadius = 8f;

        private Rigidbody _rb;
        private Transform _target;
        private Vector3 _aimOffset;

        public bool IsAlive => _hp > 0;

        public static readonly System.Collections.Generic.List<BossHomingMissile> Active =
            new System.Collections.Generic.List<BossHomingMissile>();

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;

            var missileCollider = GetComponent<SphereCollider>();
            missileCollider.isTrigger = true;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            _aimOffset = Random.insideUnitSphere * _aimErrorRadius;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            _hp -= amount;
            if (_hp <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (_target == null) return;

            Vector3 toAimPoint = _target.position + _aimOffset - _rb.position;
            if (toAimPoint.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toAimPoint.normalized);
                _rb.MoveRotation(Quaternion.RotateTowards(
                    _rb.rotation,
                    targetRotation,
                    _turnSpeed * Time.fixedDeltaTime));
            }

            _rb.linearVelocity = _rb.rotation * Vector3.forward * _speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            var shield = other.GetComponentInParent<PlayerShield>();
            if (shield == null) return;

            shield.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
