using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// 敵・弾・ミサイルのプレハブとワイヤーフレームマテリアルを生成し、
    /// Game シーンの WaveManager に敵プレハブを設定する。
    /// メニュー: Tools > ThreeDimension > Setup Prefabs
    /// </summary>
    public static class PrefabSetup
    {
        private const string MatDir = "Assets/Materials";
        private const string PrefabDir = "Assets/Prefabs";

        [MenuItem("Tools/ThreeDimension/Setup Enemy Bullet")]
        public static void SetupEnemyBulletOnly()
        {
            var wireRed = CreateWireMaterial("Wireframe_Red", new Color(1f, 0.2f, 0.3f));
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/ChaserEnemy.prefab");
            if (enemyPrefab == null)
            {
                Debug.LogError("[PrefabSetup] ChaserEnemy prefab not found. Run 'Setup Prefabs' first.");
                return;
            }
            var enemyBulletPrefab = CreateEnemyBulletPrefab(wireRed);
            AssignEnemyBullet(enemyPrefab, enemyBulletPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[PrefabSetup] Enemy bullet assigned.");
        }

        [MenuItem("Tools/ThreeDimension/Setup Player Weapons")]
        public static void SetupPlayerWeaponsOnly()
        {
            var bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Bullet.prefab");
            var missilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Missile.prefab");
            if (bulletPrefab == null || missilePrefab == null)
            {
                Debug.LogError("[PrefabSetup] Bullet/Missile prefab not found. Run 'Setup Prefabs' first.");
                return;
            }
            AssignPlayerWeapons(bulletPrefab, missilePrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[PrefabSetup] Player weapons assigned.");
        }

        [MenuItem("Tools/ThreeDimension/Setup Prefabs")]
        public static void SetupPrefabs()
        {
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets", "Prefabs");

            var wireRed = CreateWireMaterial("Wireframe_Red", new Color(1f, 0.2f, 0.3f));
            var wireGreen = CreateWireMaterial("Wireframe_Green", new Color(0.2f, 1f, 0.6f));
            var wireCyan = CreateWireMaterial("Wireframe_Cyan", new Color(0.2f, 0.9f, 1f));

            var enemyPrefab = CreateEnemyPrefab(wireRed);
            var bulletPrefab = CreateBulletPrefab(wireGreen);
            var missilePrefab = CreateMissilePrefab(wireCyan);
            var enemyBulletPrefab = CreateEnemyBulletPrefab(wireRed);

            AssignEnemyToWaveManager(enemyPrefab);
            AssignPlayerWeapons(bulletPrefab, missilePrefab);
            AssignEnemyBullet(enemyPrefab, enemyBulletPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PrefabSetup] Prefabs and materials created.");
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Material CreateWireMaterial(string name, Color color)
        {
            string path = $"{MatDir}/{name}.mat";
            var shader = Shader.Find("ThreeDimensionShooter/WireframeUnlit");
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                // 既存マテリアルのシェーダーと色を最新に更新
                existing.shader = shader;
                existing.SetColor("_WireColor", color);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var mat = new Material(shader);
            mat.SetColor("_WireColor", color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static GameObject CreateEnemyPrefab(Material mat)
        {
            string path = $"{PrefabDir}/ChaserEnemy.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "ChaserEnemy";
            go.transform.localScale = new Vector3(1.5f, 1.5f, 2.5f);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            var collider = go.GetComponent<Collider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;

            go.AddComponent<ChaserEnemy>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBulletPrefab(Material mat)
        {
            string path = $"{PrefabDir}/Bullet.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Bullet";
            go.transform.localScale = Vector3.one * 0.3f;

            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var collider = go.GetComponent<Collider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;

            go.AddComponent<Bullet>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateMissilePrefab(Material mat)
        {
            string path = $"{PrefabDir}/Missile.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Missile";
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1.2f);

            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var collider = go.GetComponent<Collider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;

            go.AddComponent<Missile>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateEnemyBulletPrefab(Material mat)
        {
            string path = $"{PrefabDir}/EnemyBullet.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "EnemyBullet";
            go.transform.localScale = Vector3.one * 0.4f;

            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var collider = go.GetComponent<Collider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;

            go.AddComponent<EnemyBullet>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void AssignEnemyToWaveManager(GameObject enemyPrefab)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
            var wm = Object.FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                var so = new SerializedObject(wm);
                so.FindProperty("_chaserPrefab").objectReferenceValue = enemyPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void AssignEnemyBullet(GameObject enemyPrefab, GameObject enemyBulletPrefab)
        {
            // ChaserEnemy プレハブの _bulletPrefab に敵弾を設定する
            string path = AssetDatabase.GetAssetPath(enemyPrefab);
            var root = PrefabUtility.LoadPrefabContents(path);
            var chaser = root.GetComponent<ChaserEnemy>();
            if (chaser != null)
            {
                var so = new SerializedObject(chaser);
                so.FindProperty("_bulletPrefab").objectReferenceValue = enemyBulletPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void AssignPlayerWeapons(GameObject bulletPrefab, GameObject missilePrefab)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
            var pw = Object.FindFirstObjectByType<PlayerWeapons>();
            if (pw != null)
            {
                // マズル(発射位置)がなければ作成
                var muzzle = pw.transform.Find("Muzzle");
                if (muzzle == null)
                {
                    var muzzleGo = new GameObject("Muzzle");
                    muzzleGo.transform.SetParent(pw.transform, false);
                    muzzleGo.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                    muzzle = muzzleGo.transform;
                }

                var so = new SerializedObject(pw);
                so.FindProperty("_bulletPrefab").objectReferenceValue = bulletPrefab;
                so.FindProperty("_missilePrefab").objectReferenceValue = missilePrefab;
                so.FindProperty("_muzzle").objectReferenceValue = muzzle;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
