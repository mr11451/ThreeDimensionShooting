using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 自機のシールド。被弾で減少し、最後の被弾から一定時間後に自動回復する。
    /// シールド 0 で撃墜(ゲームオーバー)。
    /// </summary>
    public class PlayerShield : MonoBehaviour
    {
        [SerializeField] private float _maxShield = 100f;
        [SerializeField] private float _regenDelay = 3f;
        [SerializeField] private float _regenPerSecond = 20f;

        public float Current { get; private set; }
        public float Max => _maxShield;
        public bool IsDestroyed => Current <= 0f;

        private float _lastDamageTime = -999f;

        private void Awake()
        {
            Current = _maxShield;
        }

        private void Update()
        {
            if (IsDestroyed) return;

            if (Time.time - _lastDamageTime >= _regenDelay && Current < _maxShield)
            {
                Current = Mathf.Min(_maxShield, Current + _regenPerSecond * Time.deltaTime);
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDestroyed) return;

            Current -= amount;
            _lastDamageTime = Time.time;

            GameManager.Instance?.ResetCombo();
            AudioManager.Instance?.PlayHit();

            if (Current <= 0f)
            {
                Current = 0f;
                OnDestroyed();
            }
        }

        private void OnDestroyed()
        {
            // TODO: 撃墜エフェクト
            GameManager.Instance?.GameOver();
        }
    }
}
