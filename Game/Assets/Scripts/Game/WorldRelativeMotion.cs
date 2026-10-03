using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 自機を原点に留めたまま、敵・星・弾を相対移動させるための補助機能。
    /// 自機が入力に応じて移動したように見せるため、世界全体を逆方向へずらす。
    /// </summary>
    public static class WorldRelativeMotion
    {
        public static void ApplyDelta(Vector3 delta)
        {
            if (delta == Vector3.zero)
            {
                return;
            }

            var enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    enemies[i].transform.position += delta;
                }
            }

            var bullets = Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);
            for (int i = 0; i < bullets.Length; i++)
            {
                if (bullets[i] != null)
                {
                    bullets[i].transform.position += delta;
                }
            }

            var enemyBullets = Object.FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None);
            for (int i = 0; i < enemyBullets.Length; i++)
            {
                if (enemyBullets[i] != null)
                {
                    enemyBullets[i].transform.position += delta;
                }
            }

            var missiles = Object.FindObjectsByType<Missile>(FindObjectsSortMode.None);
            for (int i = 0; i < missiles.Length; i++)
            {
                if (missiles[i] != null)
                {
                    missiles[i].transform.position += delta;
                }
            }

            var backdrop = Object.FindFirstObjectByType<NavigationBackdrop>();
            backdrop?.ApplyWorldDelta(delta);
        }
    }
}
