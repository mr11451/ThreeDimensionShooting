using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 敵の弾。直進し、プレイヤーのシールドに当たるとダメージを与える。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyBullet : MonoBehaviour
    {
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _speed = 50f;
        [SerializeField] private float _lifeSeconds = 5f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
        }

        private void Start()
        {
            _rb.linearVelocity = transform.forward * _speed;
            Destroy(gameObject, _lifeSeconds);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerShield>(out var shield))
            {
                shield.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
