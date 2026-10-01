using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// Game シーンの HUD にレーダー・照準・ターゲットインジケーターを追加する。
    /// メニュー: Tools > ThreeDimension > Setup Radar & Indicators
    /// </summary>
    public static class RadarSetup
    {
        [MenuItem("Tools/ThreeDimension/Setup Radar & Indicators")]
        public static void Setup()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
            var hud = Object.FindFirstObjectByType<HUD>();
            if (hud == null)
            {
                Debug.LogError("[RadarSetup] HUD not found. Run 'Setup HUD' first.");
                return;
            }
            var canvas = hud.GetComponent<Canvas>();

            // 既存のレーダー/インジケーターを削除
            var oldRadar = Object.FindFirstObjectByType<Radar>();
            if (oldRadar != null) Object.DestroyImmediate(oldRadar.gameObject);
            var oldInd = Object.FindFirstObjectByType<TargetIndicators>();
            if (oldInd != null) Object.DestroyImmediate(oldInd.gameObject);

            SetupRadar(canvas.transform);
            SetupReticle(canvas.transform);
            SetupIndicators(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[RadarSetup] Radar, reticle, indicators added.");
        }

        // ---------------- Radar ----------------
        private static void SetupRadar(Transform canvas)
        {
            // レーダー背景(右下)
            var bgGo = new GameObject("Radar", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvas, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = bgRt.anchorMax = new Vector2(1, 0);
            bgRt.pivot = new Vector2(1, 0);
            bgRt.anchoredPosition = new Vector2(-30, 30);
            bgRt.sizeDelta = new Vector2(200, 200);
            // 背景 Image(スプライトは Radar.Awake でランタイム生成)
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.color = Color.clear;

            var surfaceGo = new GameObject("CircularSurface", typeof(RectTransform), typeof(CircularRadarGraphic));
            surfaceGo.transform.SetParent(bgGo.transform, false);
            surfaceGo.transform.SetAsFirstSibling();
            var surfaceRt = surfaceGo.GetComponent<RectTransform>();
            surfaceRt.anchorMin = Vector2.zero;
            surfaceRt.anchorMax = Vector2.one;
            surfaceRt.offsetMin = Vector2.zero;
            surfaceRt.offsetMax = Vector2.zero;
            var surface = surfaceGo.GetComponent<CircularRadarGraphic>();
            surface.color = new Color(0.015f, 0.06f, 0.09f, 0.92f);
            surface.raycastTarget = false;

            // 中心点(自機)
            var centerGo = new GameObject("Center", typeof(RectTransform), typeof(Image));
            centerGo.transform.SetParent(bgGo.transform, false);
            var cRt = centerGo.GetComponent<RectTransform>();
            cRt.anchorMin = cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.sizeDelta = new Vector2(6, 6);
            cRt.anchoredPosition = Vector2.zero;
            centerGo.GetComponent<Image>().color = new Color(0.3f, 1f, 0.6f);

            // ブリップコンテナ
            var blipContainer = new GameObject("Blips", typeof(RectTransform));
            blipContainer.transform.SetParent(bgGo.transform, false);
            var bcRt = blipContainer.GetComponent<RectTransform>();
            bcRt.anchorMin = Vector2.zero;
            bcRt.anchorMax = Vector2.one;
            bcRt.offsetMin = bcRt.offsetMax = Vector2.zero;

            // ブリッププレハブ(スプライトは Radar.Awake でランタイム生成)
            var blipPrefabGo = new GameObject("BlipPrefab", typeof(RectTransform), typeof(Image));
            blipPrefabGo.transform.SetParent(bgGo.transform, false);
            var bpRt = blipPrefabGo.GetComponent<RectTransform>();
            bpRt.sizeDelta = new Vector2(6, 6);
            blipPrefabGo.SetActive(false); // プレハブは非表示

            var radar = bgGo.AddComponent<Radar>();
            var so = new SerializedObject(radar);
            so.FindProperty("_blipContainer").objectReferenceValue = bcRt;
            so.FindProperty("_blipPrefab").objectReferenceValue = blipPrefabGo.GetComponent<Image>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------- Reticle ----------------
        private static void SetupReticle(Transform canvas)
        {
            var go = new GameObject("Reticle", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(24, 24);
            go.AddComponent<Reticle>(); // スプライトは Reticle.Awake でランタイム生成
        }

        // ---------------- Indicators ----------------
        private static void SetupIndicators(Transform canvas)
        {
            var container = new GameObject("Indicators", typeof(RectTransform));
            container.transform.SetParent(canvas, false);
            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = crt.offsetMax = Vector2.zero;

            // 画面内マーカー(ダイヤ形)
            var markerPrefab = new GameObject("MarkerPrefab", typeof(RectTransform), typeof(Image));
            markerPrefab.transform.SetParent(container.transform, false);
            var mRt = markerPrefab.GetComponent<RectTransform>();
            mRt.sizeDelta = new Vector2(18, 18);
            mRt.rotation = Quaternion.Euler(0, 0, 45);
            markerPrefab.GetComponent<Image>().color = new Color(1f, 0.4f, 0.4f, 0.8f);
            markerPrefab.SetActive(false); // スプライトは TargetIndicators.Awake でランタイム生成

            // 画面外矢印
            var arrowPrefab = new GameObject("ArrowPrefab", typeof(RectTransform), typeof(Image));
            arrowPrefab.transform.SetParent(container.transform, false);
            var aRt = arrowPrefab.GetComponent<RectTransform>();
            aRt.sizeDelta = new Vector2(28, 28);
            arrowPrefab.GetComponent<Image>().color = new Color(1f, 0.6f, 0.2f, 0.9f);
            arrowPrefab.SetActive(false); // スプライトは TargetIndicators.Awake でランタイム生成

            var ti = container.AddComponent<TargetIndicators>();
            var so = new SerializedObject(ti);
            so.FindProperty("_container").objectReferenceValue = crt;
            so.FindProperty("_markerPrefab").objectReferenceValue = markerPrefab.GetComponent<Image>();
            so.FindProperty("_arrowPrefab").objectReferenceValue = arrowPrefab.GetComponent<Image>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------- Sprite generation ----------------
        private static Sprite CreateCircleSprite(int size, bool outlineOnly)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            float r = c - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    bool on = outlineOnly ? Mathf.Abs(d - r) < 1.5f : d <= r;
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateCrosshairSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            int th = 2;
            int gap = size / 5;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool h = Mathf.Abs(y - c) < th && (x < c - gap || x > c + gap);
                    bool v = Mathf.Abs(x - c) < th && (y < c - gap || y > c + gap);
                    tex.SetPixel(x, y, (h || v) ? Color.white : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateDiamondSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool on = Mathf.Abs(x - c) + Mathf.Abs(y - c) <= c - 1f;
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateArrowSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // 上向き三角形
                    float relY = y / (float)size;
                    float halfW = relY * c;
                    bool on = Mathf.Abs(x - c) <= halfW && y > size * 0.15f;
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
