using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 一人称視点の自機先端マーカーとコックピット窓枠を表示する。
    /// Vキーまたはゲームパッド右スティック押下で表示を切り替える。
    /// </summary>
    public sealed class CockpitFrame : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;
        private Transform _cameraTransform;
        private GameObject _frameRoot;
        private bool _wasTogglePressed;
        private Texture2D _overlayTexture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCockpitFrame()
        {
            if (SceneManager.GetActiveScene().name != "Game") return;
            var player = FindFirstObjectByType<PlayerShipController>();
            if (player != null && player.GetComponent<CockpitFrame>() == null)
            {
                player.gameObject.AddComponent<CockpitFrame>();
            }
        }

        private void Start()
        {
            TryCreateFrame();
        }

        private void TryCreateFrame()
        {
            if (_frameRoot != null) return;
            var camera = Camera.main;
            if (camera == null) return;

            _cameraTransform = camera.transform;
            _overlayTexture = CreateOverlayTexture();
            _frameRoot = new GameObject("CockpitFrame").transform.gameObject;
            _frameRoot.transform.SetParent(_cameraTransform, false);
            _frameRoot.SetActive(_visible);
        }

        private void Update()
        {
            TryCreateFrame();
            var keyboardToggle = Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame;
            var padToggle = Gamepad.current != null && Gamepad.current.rightStickButton.wasPressedThisFrame;
            var togglePressed = keyboardToggle || padToggle;
            if (togglePressed && !_wasTogglePressed && _frameRoot != null)
            {
                _visible = !_visible;
                _frameRoot.SetActive(_visible);
            }
            _wasTogglePressed = togglePressed;
        }

        private void OnGUI()
        {
            if (!_visible || _overlayTexture == null || Event.current.type != EventType.Repaint) return;

            float beam = Mathf.Max(1.5f, Screen.width * 0.0015f);
            float top = Screen.height * 0.16f;
            float bottom = Screen.height * 0.88f;
            Vector2 topLeft = new Vector2(Screen.width * 0.24f, top);
            Vector2 topRight = new Vector2(Screen.width * 0.76f, top);
            Vector2 bottomLeft = new Vector2(Screen.width * 0.06f, bottom);
            Vector2 bottomRight = new Vector2(Screen.width * 0.94f, bottom);

            DrawBeamBetween(topLeft, bottomLeft, beam);
            DrawBeamBetween(topRight, bottomRight, beam);
            DrawBeamBetween(topLeft, topRight, beam);
            DrawBeamBetween(bottomLeft, bottomRight, beam);

            // 台形の各頂点から画面の四隅へ放射状に伸びる窓枠
            DrawBeamBetween(topLeft, new Vector2(0f, 0f), beam);
            DrawBeamBetween(topRight, new Vector2(Screen.width, 0f), beam);
            DrawBeamBetween(bottomLeft, new Vector2(0f, Screen.height), beam);
            DrawBeamBetween(bottomRight, new Vector2(Screen.width, Screen.height), beam);
        }

            private void DrawBeam(Vector2 center, Vector2 size, float angle)
            {
                var previousMatrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(angle, center);
                GUI.DrawTexture(new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y), _overlayTexture);
                GUI.matrix = previousMatrix;
            }

            private void DrawBeamBetween(Vector2 start, Vector2 end, float width)
            {
                Vector2 delta = end - start;
                DrawBeam((start + end) * 0.5f, new Vector2(delta.magnitude, width), Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            }

        private static Texture2D CreateOverlayTexture()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, new Color(0.1f, 0.9f, 1f, 0.85f));
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            if (_overlayTexture != null) Destroy(_overlayTexture);
        }
    }
}