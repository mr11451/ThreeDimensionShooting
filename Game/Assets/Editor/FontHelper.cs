using UnityEditor;
using UnityEngine;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// プロジェクト組み込みフォントの読み込みヘルパー。
    /// ビルド版で確実に描画されるよう、Assets 内の Arial.ttf を使う。
    /// </summary>
    public static class FontHelper
    {
        private const string FontPath = "Assets/Fonts/Arial.ttf";

        public static Font GetFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogError($"[FontHelper] Font not found at {FontPath}. Falling back to builtin.");
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return font;
        }
    }
}
