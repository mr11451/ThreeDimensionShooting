using UnityEngine;
using UnityEngine.UI;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 照準(レティクル)。ランタイムで十字スプライトを生成して表示する。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class Reticle : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(0.4f, 1f, 0.7f, 0.9f);

        private void Awake()
        {
            var img = GetComponent<Image>();
            img.sprite = null;
            img.color = Color.clear;

            CreateBar("Top", new Vector2(2f, 10f), new Vector2(0f, 7f));
            CreateBar("Bottom", new Vector2(2f, 10f), new Vector2(0f, -7f));
            CreateBar("Left", new Vector2(10f, 2f), new Vector2(-7f, 0f));
            CreateBar("Right", new Vector2(10f, 2f), new Vector2(7f, 0f));
        }

        private void CreateBar(string name, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<Image>();
            image.color = _color;
            image.raycastTarget = false;
        }
    }
}
