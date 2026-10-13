using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// メインショットの弾。直進し、敵に当たるとダメージを与える。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private int _damage = 5;
        [SerializeField] private float _lifeSeconds = 3f;

        private void Awake()
        {
            _damage = FlightSettingsFile.GetInt("PlayerShotDamage", _damage, 0);
        }

        private void Start()
        {
            Destroy(gameObject, _lifeSeconds);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<BossHomingMissile>(out var homingMissile))
            {
                homingMissile.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }

            if (other.TryGetComponent<EnemyBase>(out var enemy))
            {
                enemy.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
