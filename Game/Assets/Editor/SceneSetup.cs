using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ThreeDimensionShooter;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// Title / Game / GameOver の3シーンを生成し、EditorBuildSettings に登録する。
    /// メニュー: Tools > ThreeDimension > Setup Scenes
    /// </summary>
    public static class SceneSetup
    {
        private const string SceneDir = "Assets/Scenes";

        [MenuItem("Tools/ThreeDimension/Setup Scenes")]
        public static void SetupScenes()
        {
            EnsureFolder("Assets", "Scenes");

            var titlePath = $"{SceneDir}/Title.unity";
            var gamePath = $"{SceneDir}/Game.unity";
            var gameOverPath = $"{SceneDir}/GameOver.unity";

            CreateTitleScene(titlePath);
            CreateGameScene(gamePath);
            CreateGameOverScene(gameOverPath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(titlePath, true),
                new EditorBuildSettingsScene(gamePath, true),
                new EditorBuildSettingsScene(gameOverPath, true),
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneSetup] Scenes created and registered in EditorBuildSettings.");
        }

        [MenuItem("Tools/ThreeDimension/Setup Navigation Backdrop")]
        public static void SetupNavigationBackdrop()
        {
            var scene = EditorSceneManager.OpenScene($"{SceneDir}/Game.unity", OpenSceneMode.Single);
            var existing = Object.FindFirstObjectByType<ThreeDimensionShooter.NavigationBackdrop>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var backdrop = new GameObject("NavigationBackdrop");
            var backdropComponent = backdrop.AddComponent<ThreeDimensionShooter.NavigationBackdrop>();
            var backdropSerialized = new SerializedObject(backdropComponent);
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Wireframe_Cyan.mat");
            backdropSerialized.FindProperty("_starMaterial").objectReferenceValue = material;
            backdropSerialized.FindProperty("_landmarkMaterial").objectReferenceValue = material;
            backdropSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] Navigation backdrop added to Game scene.");
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Scene NewEmptyScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.SaveScene(scene, path);
        }

        private static Camera CreateCamera(Vector3 pos, Color bg)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = pos;
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            go.AddComponent<AudioListener>();
            return cam;
        }

        private static void CreateDirectionalLight()
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.font = FontHelper.GetFont();
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(800f, fontSize * 2f);
            return text;
        }

        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            return canvas;
        }

        // ---------------- Title ----------------
        private static void CreateTitleScene(string path)
        {
            var scene = NewEmptyScene();
            CreateCamera(new Vector3(0, 0, -10), new Color(0.02f, 0.02f, 0.08f));

            var canvas = CreateCanvas("Canvas");
            var title = CreateText(canvas.transform, "TitleText", "3D SHOOTING", 96, new Vector2(0, 150), TextAnchor.MiddleCenter);
            title.color = new Color(0.3f, 1f, 0.7f); // ネオングリーン
            var prompt = CreateText(canvas.transform, "PromptText", "Press Start / Space", 36, new Vector2(0, -120), TextAnchor.MiddleCenter);
            prompt.color = new Color(0.9f, 0.9f, 1f);

            var ctrl = new GameObject("TitleController");
            ctrl.AddComponent<TitleController>();

            SaveScene(scene, path);
        }

        // ---------------- Game ----------------
        private static void CreateGameScene(string path)
        {
            var scene = NewEmptyScene();
            CreateCamera(new Vector3(0, 2, -8), Color.black);
            CreateDirectionalLight();

            // Player
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            var rb = player.AddComponent<Rigidbody>();
            rb.useGravity = false;
            player.AddComponent<PlayerShipController>();
            player.AddComponent<PlayerShield>();
            player.AddComponent<PlayerWeapons>();
            var col = player.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 2f;

            var backdrop = new GameObject("NavigationBackdrop");
            backdrop.AddComponent<ThreeDimensionShooter.NavigationBackdrop>();

            // GameManager
            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();

            // WaveManager
            var wm = new GameObject("WaveManager");
            wm.AddComponent<WaveManager>();

            SaveScene(scene, path);
        }

        // ---------------- GameOver ----------------
        private static void CreateGameOverScene(string path)
        {
            var scene = NewEmptyScene();
            CreateCamera(new Vector3(0, 0, -10), new Color(0.05f, 0f, 0f));

            var canvas = CreateCanvas("Canvas");
            CreateText(canvas.transform, "GameOverText", "GAME OVER", 96, new Vector2(0, 150), TextAnchor.MiddleCenter);

            var scoreGo = new GameObject("ScoreText", typeof(RectTransform));
            scoreGo.transform.SetParent(canvas.transform, false);
            var scoreText = scoreGo.AddComponent<Text>();
            scoreText.fontSize = 40;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.color = Color.white;
            scoreText.font = FontHelper.GetFont();
            var srt = scoreGo.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(0, -20);
            srt.sizeDelta = new Vector2(900, 80);
            var scoreView = scoreGo.AddComponent<GameOverScoreView>();

            CreateText(canvas.transform, "PromptText", "Start: Retry / Back: Title", 28, new Vector2(0, -160), TextAnchor.MiddleCenter);

            var ctrl = new GameObject("GameOverController");
            var gc = ctrl.AddComponent<GameOverController>();
            // 参照を設定
            var so = new SerializedObject(gc);
            so.FindProperty("_scoreView").objectReferenceValue = scoreView;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(scene, path);
        }
    }
}
