using UnityEngine;
using UnityEngine.UI;

namespace ThreeDimensionShooter
{
    public sealed class CircularRadarGraphic : MaskableGraphic
    {
        [SerializeField] private int _segments = 64;
        [SerializeField] private float _outlineWidth = 3f;
        [SerializeField] private Color _outlineColor = new Color(0.1f, 0.9f, 1f, 1f);

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = rectTransform.rect;
            var center = rect.center;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            float innerRadius = Mathf.Max(0f, radius - _outlineWidth);
            int segments = Mathf.Max(16, _segments);

            vertexHelper.AddVert(center, color, new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                var position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * innerRadius;
                vertexHelper.AddVert(position, color, Vector2.zero);
            }

            for (int i = 0; i < segments; i++)
            {
                vertexHelper.AddTriangle(0, i + 1, i + 2);
            }

            int outerStart = vertexHelper.currentVertCount;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                var position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vertexHelper.AddVert(position, _outlineColor, Vector2.zero);
            }

            int innerStart = outerStart + segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                var position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * innerRadius;
                vertexHelper.AddVert(position, _outlineColor, Vector2.zero);
            }

            for (int i = 0; i < segments; i++)
            {
                vertexHelper.AddTriangle(outerStart + i, outerStart + i + 1, innerStart + i + 1);
                vertexHelper.AddTriangle(outerStart + i, innerStart + i + 1, innerStart + i);
            }
        }
    }
}