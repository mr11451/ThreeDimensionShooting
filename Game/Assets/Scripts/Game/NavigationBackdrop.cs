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
        [SerializeField] private Material _starMaterial;

        private void Awake()
        {
            BuildBackdrop();
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
            var random = new System.Random(_seed);

            for (int i = 0; i < StarCount; i++)
            {
                var direction = RandomDirection(random);
                float distance = Mathf.Lerp(_starMinDistance, _starMaxDistance, (float)random.NextDouble());
                float size = Mathf.Lerp(0.4f, 1.2f, (float)random.NextDouble());
                var star = CreatePrimitive("Star", PrimitiveType.Sphere, parent, direction * distance, size, starMaterial);
                star.transform.localScale = Vector3.one * size;
            }
        }

        private static GameObject CreatePrimitive(string objectName, PrimitiveType type, Transform parent, Vector3 position, float size, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = Vector3.one * size;
            go.GetComponent<Renderer>().sharedMaterial = material;

            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
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