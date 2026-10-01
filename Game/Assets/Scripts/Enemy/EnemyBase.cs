using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 敵機の基底クラス。HP と撃墜時のスコア付与を持つ。
    /// 派生: 追跡型 / 編隊型 / 大型ボス。
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField] protected int _hp = 10;
        [SerializeField] protected int _scoreValue = 100;

        public bool IsAlive => _hp > 0;

        public virtual void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            _hp -= amount;
            if (_hp <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            GameManager.Instance?.AddScore(_scoreValue);
            AudioManager.Instance?.PlayExplosion();
            // TODO: 爆発エフェクト
            Destroy(gameObject);
        }
    }
}
