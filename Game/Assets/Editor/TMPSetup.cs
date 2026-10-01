using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// TextMeshPro への移行。Arial から TMP フォントアセットを生成し、
    /// 全シーンの UI Text を TMP_Text に置き換える。
    /// メニュー: Tools > ThreeDimension > Migrate to TextMeshPro
    /// </summary>
    public static class TMPSetup
    {
        private const string FontSourcePath = "Assets/Fonts/Arial.ttf";
        private const string TMPFontPath = "Assets/Fonts/ArialTMP.asset";

        [MenuItem("Tools/ThreeDimension/Step1 Import TMP Essentials")]
        public static void Step1ImportEssentials()
        {
            // TMP Essential Resources.unitypackage をインポート(シェーダー・Settings・フォント等を含む)
            string packagePath = FindTMPEssentialsPackage();
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogError("[TMP] Essentials package not found.");
                EditorApplication.Exit(1);
                return;
            }
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[TMP] Essentials imported.");
        }

        [MenuItem("Tools/ThreeDimension/Step2 Migrate to TextMeshPro")]
        public static void Migrate()
        {
            // 1. TMP フォントアセット生成
            var fontAsset = CreateTMPFontAsset();
            if (fontAsset == null)
            {
                Debug.LogError("[TMP] Failed to create font asset.");
                return;
            }

            // 2. 全シーンを TMP に移行
            MigrateScene("Assets/Scenes/Title.unity", fontAsset);
            MigrateScene("Assets/Scenes/Game.unity", fontAsset);
            MigrateScene("Assets/Scenes/GameOver.unity", fontAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TMP] Migration complete.");
        }

        private static string FindTMPEssentialsPackage()
        {
            string cacheDir = "Library/PackageCache";
            if (System.IO.Directory.Exists(cacheDir))
            {
                foreach (var dir in System.IO.Directory.GetDirectories(cacheDir, "com.unity.ugui*"))
                {
                    string p = System.IO.Path.Combine(dir, "Package Resources", "TMP Essential Resources.unitypackage");
                    if (System.IO.File.Exists(p)) return System.IO.Path.GetFullPath(p);
                }
            }
            return null;
        }

        private static TMP_FontAsset CreateTMPFontAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TMPFontPath);
            if (existing != null) return existing;

            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);
            if (sourceFont == null)
            {
                Debug.LogError($"[TMP] Source font not found: {FontSourcePath}");
                return null;
            }

            // 動的フォントアセットを作成(全文字対応)
            var fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            fontAsset.name = "ArialTMP";
            AssetDatabase.CreateAsset(fontAsset, TMPFontPath);

            // アトラステクスチャとマテリアルをサブアセットとして明示的に保存
            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = "ArialTMP Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }
            if (fontAsset.material != null)
            {
                fontAsset.material.name = "ArialTMP Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(TMPFontPath);
            return fontAsset;
        }

        private static void MigrateScene(string scenePath, TMP_FontAsset fontAsset)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 既存の Text を全て収集(子も含む)
            var texts = Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in texts)
            {
                var go = t.gameObject;
                var rt = go.GetComponent<RectTransform>();
                string content = t.text;
                int fontSize = t.fontSize;
                Color color = t.color;
                TextAnchor anchor = t.alignment;
                string name = go.name;
                Vector2 size = rt != null ? rt.sizeDelta : new Vector2(600, fontSize * 1.6f);
                Vector2 pos = rt != null ? rt.anchoredPosition : Vector2.zero;
                Vector2 anchorMin = rt != null ? rt.anchorMin : new Vector2(0.5f, 0.5f);
                Vector2 anchorMax = rt != null ? rt.anchorMax : new Vector2(0.5f, 0.5f);
                Vector2 pivot = rt != null ? rt.pivot : new Vector2(0.5f, 0.5f);

                // GameOverScoreView など参照保持のため、コンポーネントを入れ替える
                // 既存 Text を削除して TextMeshProUGUI を追加
                var parent = go.transform.parent;
                Object.DestroyImmediate(t);

                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = content;
                tmp.fontSize = fontSize;
                tmp.color = color;
                tmp.font = fontAsset;
                tmp.alignment = ConvertAnchor(anchor);
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.overflowMode = TextOverflowModes.Overflow;

                if (rt != null)
                {
                    rt.sizeDelta = size;
                    rt.anchoredPosition = pos;
                    rt.anchorMin = anchorMin;
                    rt.anchorMax = anchorMax;
                    rt.pivot = pivot;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static TextAlignmentOptions ConvertAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
