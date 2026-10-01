using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 敵の位置を示すマーカー。画面内なら敵位置に、画面外なら画面端に方向矢印を表示する。
    /// </summary>
    public class TargetIndicators : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _container;
        [SerializeField] private Image _markerPrefab;    // 画面内マーカー
        [SerializeField] private Image _arrowPrefab;     // 画面外矢印

        [Header("Settings")]
        [SerializeField] private float _edgeMarginPx = 40f;
        [SerializeField] private Color _markerColor = new Color(1f, 0.4f, 0.4f, 0.9f);
        [SerializeField] private Color _arrowColor = new Color(1f, 0.6f, 0.2f, 0.9f);

        private Camera _cam;
        private readonly List<Image> _markerPool = new List<Image>();
        private readonly List<Image> _arrowPool = new List<Image>();

        private void Awake()
        {
            // ランタイムでスプライトを生成して確実に適用
            if (_markerPrefab != null) _markerPrefab.sprite = UISpriteFactory.Diamond(32);
            if (_arrowPrefab != null) _arrowPrefab.sprite = UISpriteFactory.Arrow(64);
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (_cam == null || _container == null) return;

            var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            EnsurePool(_markerPool, _markerPrefab, enemies.Length);
            EnsurePool(_arrowPool, _arrowPrefab, enemies.Length);

            float halfW = Screen.width * 0.5f;
            float halfH = Screen.height * 0.5f;

            for (int i = 0; i < enemies.Length; i++)
            {
                Vector3 viewport = _cam.WorldToViewportPoint(enemies[i].transform.position);
                bool inFront = viewport.z > 0f;
                bool onScreen = inFront && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;

                var marker = _markerPool[i];
                var arrow = _arrowPool[i];

                if (onScreen)
                {
                    // 画面内: 敵位置にマーカー
                    Vector3 screenPos = _cam.WorldToScreenPoint(enemies[i].transform.position);
                    marker.gameObject.SetActive(true);
                    marker.rectTransform.position = screenPos;
                    marker.color = _markerColor;
                    arrow.gameObject.SetActive(false);
                }
                else
                {
                    // 画面外: 画面端に矢印(方向を示す)
                    marker.gameObject.SetActive(false);

                    Vector3 screenPos = _cam.WorldToScreenPoint(enemies[i].transform.position);
                    if (!inFront)
                    {
                        // 背後の場合は反転
                        screenPos = new Vector3(Screen.width - screenPos.x, Screen.height - screenPos.y, 0f);
                    }

                    Vector2 center = new Vector2(halfW, halfH);
                    Vector2 dir = ((Vector2)screenPos - center).normalized;
                    if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;

                    // 画面端に沿った位置(中心から枠まで)
                    float tx = (halfW - _edgeMarginPx) / Mathf.Abs(dir.x);
                    float ty = (halfH - _edgeMarginPx) / Mathf.Abs(dir.y);
                    float t = Mathf.Min(tx, ty);
                    Vector2 edgePos = center + dir * t;

                    arrow.gameObject.SetActive(true);
                    arrow.rectTransform.position = edgePos;
                    arrow.rectTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
                    arrow.color = _arrowColor;
                }
            }

            // 余剰プールを非表示
            for (int i = enemies.Length; i < _markerPool.Count; i++) _markerPool[i].gameObject.SetActive(false);
            for (int i = enemies.Length; i < _arrowPool.Count; i++) _arrowPool[i].gameObject.SetActive(false);
        }

        private void EnsurePool(List<Image> pool, Image prefab, int count)
        {
            if (prefab == null) return;
            while (pool.Count < count)
            {
                pool.Add(Instantiate(prefab, _container));
            }
        }
    }
}
