using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ロックオンミサイル。ターゲットを追尾し、命中で大ダメージ。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Missile : MonoBehaviour
    {
        [SerializeField] private int _damage = 30;
        [SerializeField] private float _speed = 80f;
        [SerializeField] private float _turnSpeed = 200f;
        [SerializeField] private float _lifeSeconds = 6f;

        private Transform _target;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
        }

        private void Start()
        {
            Destroy(gameObject, _lifeSeconds);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void FixedUpdate()
        {
            if (_target != null)
            {
                Vector3 toTarget = _target.position - transform.position;
                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
                _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime));
            }
            _rb.linearVelocity = transform.forward * _speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
