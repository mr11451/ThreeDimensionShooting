using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// プレイヤーの位置と進行方向を把握するためのワールド固定背景を生成する。
    /// </summary>
    public sealed class NavigationBackdrop : MonoBehaviour
    {
        private const int StarCount = 120;

        [SerializeField] private int _seed = 31415;
        [SerializeField] private float _starMinDistance = 400f;
        [SerializeField] private float _starMaxDistance = 1200f;
        [SerializeField] private float _starCollisionDamage = 100f;
        [SerializeField] private Material _starMaterial;

        private Transform _player;
        private PlayerShield _playerShield;
        private Transform[] _stars;
        private readonly HashSet<Transform> _starTransforms = new HashSet<Transform>();
        private System.Random _random;

        private void Awake()
        {
            var player = FindFirstObjectByType<PlayerShipController>();
            if (player != null)
            {
                _player = player.transform;
                _playerShield = player.GetComponent<PlayerShield>();
            }
            _random = new System.Random(_seed);
            BuildBackdrop();
        }

        private void Update()
        {
            if (_player == null || _stars == null) return;

            float maxDistanceSqr = _starMaxDistance * _starMaxDistance;
            foreach (var star in _stars)
            {
                float distanceSqr = (star.position - _player.position).sqrMagnitude;
                if (distanceSqr > maxDistanceSqr)
                {
                    PlaceStar(star);
                }
            }
        }

        public void HandleStarCollision(Collider starCollider)
        {
            if (starCollider == null || !_starTransforms.Contains(starCollider.transform)) return;

            _playerShield?.TakeDamage(_starCollisionDamage);
            PlaceStar(starCollider.transform);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBackdropAfterSceneLoad()
        {
            if (SceneManager.GetActiveScene().name != "Game") return;
            if (FindFirstObjectByType<NavigationBackdrop>() != null) return;

            var backdrop = new GameObject("NavigationBackdrop");
            backdrop.AddComponent<NavigationBackdrop>();
        }

        private void BuildBackdrop()
        {
            if (transform.Find("NavigationBackdrop (World Fixed)") != null) return;

            var root = new GameObject("NavigationBackdrop (World Fixed)");
            root.transform.SetParent(transform, false);

            CreateStars(root.transform);
        }

        private void CreateStars(Transform parent)
        {
            var starMaterial = _starMaterial != null
                ? _starMaterial
                : CreateMaterial("Unlit/Color", new Color(0.55f, 0.85f, 1f));
            _stars = new Transform[StarCount];

            for (int i = 0; i < StarCount; i++)
            {
                float size = Mathf.Lerp(0.4f, 1.2f, (float)_random.NextDouble());
                var star = CreatePrimitive("Star", PrimitiveType.Sphere, parent, Vector3.zero, size, starMaterial);
                star.transform.localScale = Vector3.one * size;
                _stars[i] = star.transform;
                _starTransforms.Add(star.transform);
                PlaceStar(star.transform);
            }
        }

        private void PlaceStar(Transform star)
        {
            var direction = RandomDirection(_random);
            float distance = Mathf.Lerp(_starMinDistance, _starMaxDistance, (float)_random.NextDouble());
            star.position = (_player != null ? _player.position : transform.position) + direction * distance;
        }

        private static GameObject CreatePrimitive(string objectName, PrimitiveType type, Transform parent, Vector3 position, float size, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = Vector3.one * size;
            go.GetComponent<Renderer>().sharedMaterial = material;

            return go;
        }

        private static Material CreateMaterial(string shaderName, Color color)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Debug.LogError($"[NavigationBackdrop] Shader not found: {shaderName}");
                return null;
            }

            var material = new Material(shader)
            {
                color = color,
            };
            if (material.HasProperty("_WireColor")) material.SetColor("_WireColor", color);
            return material;
        }

        private static Vector3 RandomDirection(System.Random random)
        {
            var direction = new Vector3(
                (float)(random.NextDouble() * 2d - 1d),
                (float)(random.NextDouble() * 2d - 1d),
                (float)(random.NextDouble() * 2d - 1d));
            return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        }
    }
}