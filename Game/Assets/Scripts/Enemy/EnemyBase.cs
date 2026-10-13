using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 敵機の基底クラス。HP と撃墜時のスコア付与、エフェクト再生を持つ。
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
            SpawnExplosionEffect();
            Destroy(gameObject);
        }

        protected virtual float ExplosionScale => 1f;

        private void SpawnExplosionEffect()
        {
            float scale = ExplosionScale;
            var effectObject = new GameObject("EnemyExplosion");
            effectObject.transform.position = transform.position;

            var particles = effectObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f * Mathf.Sqrt(scale), 0.55f * Mathf.Sqrt(scale));
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f * scale, 8f * scale);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f * scale, 0.45f * scale);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.4f, 0.05f),
                new Color(1f, 0.9f, 0.25f));
            int burstCount = Mathf.RoundToInt(40 * scale);
            main.maxParticles = burstCount * 3;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(scale > 1f
                ? new[]
                {
                    new ParticleSystem.Burst(0f, burstCount),
                    new ParticleSystem.Burst(0.15f, burstCount),
                    new ParticleSystem.Burst(0.3f, burstCount),
                }
                : new[] { new ParticleSystem.Burst(0f, burstCount) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.25f, 0.02f), 1f),
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f),
                });
            colorOverLifetime.color = gradient;

            particles.Play();
        }
    }
}
