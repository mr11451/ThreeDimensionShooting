using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float _regenAmount = 1f;
        [SerializeField] private float _regenInterval = 1f;
        [SerializeField] private float _downedEffectSeconds = 3f;
        [SerializeField] private float _respawnInvulnerabilitySeconds = 2f;

        public float Current { get; private set; }
        public float Max => _maxShield;
        public bool IsDestroyed => Current <= 0f;
        public bool IsDowned { get; private set; }

        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private float _lastDamageTime = -999f;
        private float _regenTimer;
        private float _invulnerableUntil;
        private float _downedStartedAt;
        private Coroutine _downedRoutine;
        private readonly List<Vector2[]> _cracks = new List<Vector2[]>();

        private void Awake()
        {
            Current = _maxShield;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        private void Update()
        {
            if (IsDestroyed) return;

            if (Time.time - _lastDamageTime >= _regenDelay && Current < _maxShield)
            {
                _regenTimer += Time.deltaTime;
                if (_regenTimer >= _regenInterval)
                {
                    _regenTimer -= _regenInterval;
                    Current = Mathf.Min(_maxShield, Current + _regenAmount);
                }
            }
            else
            {
                _regenTimer = 0f;
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDestroyed || Time.unscaledTime < _invulnerableUntil) return;

            Current -= amount;
            _lastDamageTime = Time.time;
            _regenTimer = 0f;

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
            if (_downedRoutine != null) return;

            IsDowned = true;
            _downedStartedAt = Time.unscaledTime;
            BuildCracks();
            AudioManager.Instance?.PlayPlayerDestroyed();
            _downedRoutine = StartCoroutine(HandleDowned());
        }

        private IEnumerator HandleDowned()
        {
            yield return new WaitForSecondsRealtime(_downedEffectSeconds);

            var gameManager = GameManager.Instance;
            if (gameManager == null || !gameManager.LoseLife())
            {
                _downedRoutine = null;
                gameManager?.GameOver();
                yield break;
            }

            var body = GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = _spawnPosition;
                body.rotation = _spawnRotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            Current = _maxShield;
            _lastDamageTime = Time.time;
            _regenTimer = 0f;
            _invulnerableUntil = Time.unscaledTime + _respawnInvulnerabilitySeconds;
            IsDowned = false;
            _cracks.Clear();
            _downedRoutine = null;
        }

        private void OnGUI()
        {
            if (!IsDowned) return;

            float elapsed = Time.unscaledTime - _downedStartedAt;
            float remaining = Mathf.Clamp01(1f - elapsed / _downedEffectSeconds);
            var previousColor = GUI.color;
            GUI.color = new Color(0.65f, 0.015f, 0.025f, Mathf.Lerp(0.3f, 0.42f, remaining));
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;

            float crackAlpha = Mathf.Lerp(0.5f, 1f, remaining);
            foreach (var crack in _cracks)
            {
                for (int i = 1; i < crack.Length; i++)
                {
                    Vector2 start = new Vector2(crack[i - 1].x * Screen.width, crack[i - 1].y * Screen.height);
                    Vector2 end = new Vector2(crack[i].x * Screen.width, crack[i].y * Screen.height);
                    DrawCrackLine(start, end, 3f, new Color(0.22f, 0f, 0.01f, crackAlpha * 0.7f));
                    DrawCrackLine(start, end, 1.2f, new Color(1f, 0.78f, 0.8f, crackAlpha));
                }
            }
        }

        private void BuildCracks()
        {
            _cracks.Clear();
            var random = new System.Random(Time.frameCount);
            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f + (float)(random.NextDouble() - 0.5d) * 0.25f;
                float edgeDistance = DistanceToScreenEdge(center, angle);
                float length = edgeDistance * Mathf.Lerp(0.7f, 1f, (float)random.NextDouble());
                var mainCrack = BuildCrackPath(center, angle, length, 5 + random.Next(4), random);
                _cracks.Add(mainCrack);

                if (mainCrack.Length > 3)
                {
                    int branchIndex = random.Next(2, mainCrack.Length - 1);
                    Vector2 branchStart = new Vector2(mainCrack[branchIndex].x * Screen.width, mainCrack[branchIndex].y * Screen.height);
                    float branchAngle = angle + (random.Next(2) == 0 ? -1f : 1f) * Mathf.Lerp(0.55f, 1.15f, (float)random.NextDouble());
                    _cracks.Add(BuildCrackPath(branchStart, branchAngle, length * 0.28f, 2 + random.Next(3), random));
                }
            }
        }

        private Vector2[] BuildCrackPath(Vector2 start, float angle, float length, int steps, System.Random random)
        {
            var points = new Vector2[steps + 1];
            Vector2 position = start;
            points[0] = ToNormalizedScreenPoint(position);

            for (int i = 1; i <= steps; i++)
            {
                angle += (float)(random.NextDouble() - 0.5d) * 0.4f;
                float stepLength = length / steps * Mathf.Lerp(0.75f, 1.25f, (float)random.NextDouble());
                position += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * stepLength;
                position.x = Mathf.Clamp(position.x, 1f, Screen.width - 1f);
                position.y = Mathf.Clamp(position.y, 1f, Screen.height - 1f);
                points[i] = ToNormalizedScreenPoint(position);
            }

            return points;
        }

        private Vector2 ToNormalizedScreenPoint(Vector2 point)
        {
            return new Vector2(point.x / Mathf.Max(1, Screen.width), point.y / Mathf.Max(1, Screen.height));
        }

        private float DistanceToScreenEdge(Vector2 origin, float angle)
        {
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float horizontal = Mathf.Abs(direction.x) > 0.001f
                ? (direction.x > 0f ? Screen.width - origin.x : origin.x) / Mathf.Abs(direction.x)
                : float.MaxValue;
            float vertical = Mathf.Abs(direction.y) > 0.001f
                ? (direction.y > 0f ? Screen.height - origin.y : origin.y) / Mathf.Abs(direction.y)
                : float.MaxValue;
            return Mathf.Min(horizontal, vertical);
        }

        private static void DrawCrackLine(Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 delta = end - start;
            Vector2 center = (start + end) * 0.5f;
            var previousMatrix = GUI.matrix;
            var previousColor = GUI.color;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, center);
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x - delta.magnitude * 0.5f, center.y - width * 0.5f, delta.magnitude, width), Texture2D.whiteTexture);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }
    }
}
