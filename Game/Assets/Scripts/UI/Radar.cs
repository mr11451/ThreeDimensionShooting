using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// レーダー。プレイヤー中心の相対位置(前方を上)で敵を2Dマップにプロットする。
    /// 敵1機につき1ドット(Image)を使い回す。
    /// </summary>
    public class Radar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _blipContainer;
        [SerializeField] private Image _blipPrefab;

        [Header("Settings")]
        [SerializeField] private float _range = 150f;   // レーダー表示範囲(m)
        [SerializeField] private float _radarRadiusPx = 90f; // レーダー円の半径(px)
        [SerializeField] private Color _enemyColor = new Color(1f, 0.3f, 0.3f);

        private Transform _player;
        private readonly List<Image> _blipPool = new List<Image>();
        private Texture2D _overlayTexture;
        private Texture2D _playerMarkerTexture;
        private Texture2D _enemyMarkerTexture;

        private void Awake()
        {
            // ランタイムで円スプライトを生成して確実に適用
            if (_blipPrefab != null)
            {
                _blipPrefab.sprite = UISpriteFactory.Circle(16, false);
            }

            var center = transform.Find("Center")?.GetComponent<Image>();
            if (center != null)
            {
                center.sprite = UISpriteFactory.Circle(16, false);
                center.color = new Color(0.3f, 1f, 0.6f, 1f);
                center.raycastTarget = false;
                center.gameObject.SetActive(false);
            }

            _overlayTexture = CreateOverlayTexture(256);
            _playerMarkerTexture = CreateSolidCircleTexture(12, new Color(0.3f, 1f, 0.6f, 1f));
            _enemyMarkerTexture = CreateSolidCircleTexture(8, _enemyColor);
            ConfigureBlipContainer();
            // 背景を塗りつぶした円にして、Image の四隅が表示されないようにする
            var bg = GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = null;
                bg.color = Color.clear;
                if (transform.Find("CircularSurface") == null) CreateCircularSurface();
            }
        }

        private void OnGUI()
        {
            if (_overlayTexture == null || Event.current.type != EventType.Repaint) return;

            const float size = 200f;
            const float windowFrameBottom = 0.88f;
            const float frameGap = 18f;
            var rect = new Rect(
                (Screen.width - size) * 0.5f,
                Screen.height * windowFrameBottom - size - frameGap,
                size,
                size);
            GUI.DrawTexture(rect, _overlayTexture, ScaleMode.StretchToFill, true);

            const float markerSize = 10f;
            GUI.DrawTexture(
                new Rect(rect.center.x - markerSize * 0.5f, rect.center.y - markerSize * 0.5f, markerSize, markerSize),
                _playerMarkerTexture,
                ScaleMode.StretchToFill,
                true);

            DrawEnemyMarkers(rect);
        }

        private void DrawEnemyMarkers(Rect radarRect)
        {
            if (_player == null || _enemyMarkerTexture == null) return;

            float yaw = _player.eulerAngles.y;
            Quaternion inverseYaw = Quaternion.Euler(0f, -yaw, 0f);
            var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            const float markerSize = 8f;

            foreach (var enemy in enemies)
            {
                Vector3 relative = inverseYaw * (enemy.transform.position - _player.position);
                Vector2 mapPosition = new Vector2(relative.x, relative.z) / _range;
                mapPosition = Vector2.ClampMagnitude(mapPosition, 1f) * _radarRadiusPx;
                var position = radarRect.center + new Vector2(mapPosition.x, mapPosition.y);
                GUI.DrawTexture(
                    new Rect(position.x - markerSize * 0.5f, position.y - markerSize * 0.5f, markerSize, markerSize),
                    _enemyMarkerTexture,
                    ScaleMode.StretchToFill,
                    true);
            }
        }

        private static Texture2D CreateOverlayTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center - 2f;
            float outlineWidth = 5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    Color pixel = Color.clear;
                    if (distance <= radius)
                    {
                        pixel = distance >= radius - outlineWidth
                            ? new Color(0.1f, 0.9f, 1f, 1f)
                            : Color.clear;
                    }
                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateSolidCircleTexture(int size, Color circleColor)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    pixels[y * size + x] = distance <= radius ? circleColor : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            if (_overlayTexture != null) Destroy(_overlayTexture);
            if (_playerMarkerTexture != null) Destroy(_playerMarkerTexture);
            if (_enemyMarkerTexture != null) Destroy(_enemyMarkerTexture);
        }

        private void CreateCircularSurface()
        {
            var surfaceGo = new GameObject("CircularSurface", typeof(RectTransform), typeof(CircularRadarGraphic));
            surfaceGo.transform.SetParent(transform, false);
            surfaceGo.transform.SetAsFirstSibling();

            var rect = surfaceGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var surface = surfaceGo.GetComponent<CircularRadarGraphic>();
            surface.color = new Color(0.015f, 0.06f, 0.09f, 0.9f);
            surface.raycastTarget = false;
        }

        private void Start()
        {
            var pc = FindFirstObjectByType<PlayerShipController>();
            if (pc != null) _player = pc.transform;
        }

        private void Update()
        {
            if (_player == null || _blipContainer == null || _blipPrefab == null) return;

            var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            EnsurePool(enemies.Length);

            foreach (var blip in _blipPool)
            {
                blip.gameObject.SetActive(false);
            }
        }

        private void EnsurePool(int count)
        {
            while (_blipPool.Count < count)
            {
                var blip = Instantiate(_blipPrefab, _blipContainer);
                var rect = blip.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(6f, 6f);
                _blipPool.Add(blip);
            }
        }

        private void ConfigureBlipContainer()
        {
            if (_blipContainer == null) return;

            _blipContainer.anchorMin = new Vector2(0.5f, 0.5f);
            _blipContainer.anchorMax = new Vector2(0.5f, 0.5f);
            _blipContainer.pivot = new Vector2(0.5f, 0.5f);
            _blipContainer.anchoredPosition = Vector2.zero;
            _blipContainer.sizeDelta = GetComponent<RectTransform>().rect.size;
        }
    }
}
