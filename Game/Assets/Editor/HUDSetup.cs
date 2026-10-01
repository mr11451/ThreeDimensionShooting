using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// Game シーンに HUD(Canvas + 各種テキスト)を配置し、参照を設定する。
    /// メニュー: Tools > ThreeDimension > Setup HUD
    /// </summary>
    public static class HUDSetup
    {
        [MenuItem("Tools/ThreeDimension/Setup HUD")]
        public static void SetupHUD()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);

            // 既存の HUD があれば削除して作り直す
            var existing = Object.FindFirstObjectByType<HUD>();
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            // Canvas
            var canvasGo = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var hud = canvasGo.AddComponent<HUD>();

            // 各テキスト生成
            var shield = CreateText(canvasGo.transform, "ShieldText", new Vector2(40, -40), TextAnchor.UpperLeft, 32, new Color(0.3f, 1f, 0.6f));
            var score = CreateText(canvasGo.transform, "ScoreText", new Vector2(-40, -40), TextAnchor.UpperRight, 32, Color.white);
            var combo = CreateText(canvasGo.transform, "ComboText", new Vector2(-40, -90), TextAnchor.UpperRight, 36, new Color(1f, 0.8f, 0.2f));
            var missile = CreateText(canvasGo.transform, "MissileText", new Vector2(40, 40), TextAnchor.LowerLeft, 28, new Color(0.3f, 0.9f, 1f));
            var wave = CreateText(canvasGo.transform, "WaveText", new Vector2(0, -40), TextAnchor.UpperCenter, 28, Color.white);

            // HUD 参照設定
            var so = new SerializedObject(hud);
            so.FindProperty("_shieldText").objectReferenceValue = shield;
            so.FindProperty("_scoreText").objectReferenceValue = score;
            so.FindProperty("_comboText").objectReferenceValue = combo;
            so.FindProperty("_missileText").objectReferenceValue = missile;
            so.FindProperty("_waveText").objectReferenceValue = wave;

            var playerShield = Object.FindFirstObjectByType<PlayerShield>();
            var playerWeapons = Object.FindFirstObjectByType<PlayerWeapons>();
            if (playerShield != null) so.FindProperty("_playerShield").objectReferenceValue = playerShield;
            if (playerWeapons != null) so.FindProperty("_playerWeapons").objectReferenceValue = playerWeapons;

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[HUDSetup] HUD created in Game scene.");
        }

        private static Text CreateText(Transform parent, string name, Vector2 pos, TextAnchor anchor, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = FontHelper.GetFont();
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, fontSize * 1.6f);

            // アンカーに応じて配置基準を設定
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                    rt.pivot = new Vector2(0, 1);
                    break;
                case TextAnchor.UpperRight:
                    rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(1, 1);
                    break;
                case TextAnchor.UpperCenter:
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
                    rt.pivot = new Vector2(0.5f, 1);
                    break;
                case TextAnchor.LowerLeft:
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
                    rt.pivot = new Vector2(0, 0);
                    break;
            }
            rt.anchoredPosition = pos;

            return text;
        }
    }
}
