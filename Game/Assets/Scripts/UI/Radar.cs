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
        private Texture2D _podMarkerTexture;
        private Texture2D _homingMarkerTexture;
        private Texture2D _linePixel;
        private PlayerWeapons _weapons;
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
            _linePixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _linePixel.SetPixel(0, 0, Color.white);
            _linePixel.Apply();
            _playerMarkerTexture = CreateSolidCircleTexture(12, new Color(0.3f, 1f, 0.6f, 1f));
            _enemyMarkerTexture = CreateSolidCircleTexture(8, _enemyColor);
            _podMarkerTexture = CreateSolidCircleTexture(8, new Color(1f, 0.85f, 0.2f));
            _homingMarkerTexture = CreateSolidCircleTexture(6, new Color(1f, 0.5f, 0.1f));
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

            DrawLockOnFan(rect);
            DrawEnemyMarkers(rect);
        }

        /// <summary>ロックオン中、前方(上)を中心にロック範囲を扇形でレーダーに重ねる。</summary>
        private void DrawLockOnFan(Rect radarRect)
        {
            if (_weapons == null || !_weapons.IsLockOnActive) return;

            float radius = Mathf.Min(_weapons.LockOnRadius / _range, 1f) * _radarRadiusPx;
            float halfAngle = _weapons.CurrentLockOnAngleDeg;
            Vector2 center = radarRect.center;
            var edgeColor = new Color(1f, 0.8f, 0.2f, 0.9f);
            var fillColor = new Color(1f, 0.8f, 0.2f, 0.12f);

            const int arcSegments = 16;
            const int fillRays = 12;

            for (int i = 0; i <= fillRays; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)fillRays);
                DrawRadarLine(center, center + FanPoint(angle, radius), 2f, fillColor);
            }

            Vector2 previous = center + FanPoint(-halfAngle, radius);
            DrawRadarLine(center, previous, 1.5f, edgeColor);
            for (int i = 1; i <= arcSegments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)arcSegments);
                Vector2 next = center + FanPoint(angle, radius);
                DrawRadarLine(previous, next, 1.5f, edgeColor);
                previous = next;
            }
            DrawRadarLine(previous, center, 1.5f, edgeColor);
        }

        // 画面座標は下向きがプラスなので、前方(上)を負のYで表す
        private static Vector2 FanPoint(float angleDeg, float radius)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)) * radius;
        }

        private void DrawRadarLine(Vector2 a, Vector2 b, float thickness, Color color)
        {
            Vector2 delta = b - a;
            float length = delta.magnitude;
            if (length < 0.01f || _linePixel == null) return;

            var savedMatrix = GUI.matrix;
            var savedColor = GUI.color;
            GUI.color = color;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, a);
            GUI.DrawTexture(new Rect(a.x, a.y - thickness * 0.5f, length, thickness), _linePixel);
            GUI.matrix = savedMatrix;
            GUI.color = savedColor;
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
                DrawMarker(radarRect, inverseYaw, enemy.transform.position, _enemyMarkerTexture, markerSize);
            }

            foreach (var pod in MissileSupplyPod.Active)
            {
                DrawMarker(radarRect, inverseYaw, pod.transform.position, _podMarkerTexture, markerSize);
            }

            foreach (var missile in BossHomingMissile.Active)
            {
                DrawMarker(radarRect, inverseYaw, missile.transform.position, _homingMarkerTexture, markerSize);
            }
        }

        private void DrawMarker(Rect radarRect, Quaternion inverseYaw, Vector3 worldPosition, Texture2D texture, float markerSize)
        {
            Vector3 relative = inverseYaw * (worldPosition - _player.position);
            Vector2 mapPosition = new Vector2(relative.x, relative.z) / _range;
            mapPosition = Vector2.ClampMagnitude(mapPosition, 1f) * _radarRadiusPx;
            var position = radarRect.center + new Vector2(mapPosition.x, -mapPosition.y);
            GUI.DrawTexture(
                new Rect(position.x - markerSize * 0.5f, position.y - markerSize * 0.5f, markerSize, markerSize),
                texture,
                ScaleMode.StretchToFill,
                true);
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
            if (_podMarkerTexture != null) Destroy(_podMarkerTexture);
            if (_linePixel != null) Destroy(_linePixel);
            if (_homingMarkerTexture != null) Destroy(_homingMarkerTexture);
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
            _weapons = FindFirstObjectByType<PlayerWeapons>();
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
