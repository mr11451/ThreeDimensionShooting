using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// 敵・ボス・弾・ミサイル・補給ポッドのプレハブとマテリアルを生成し、
    /// Game シーンの WaveManager にプレハブを設定する。
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
            var wireOrange = CreateWireMaterial("Wireframe_Orange", new Color(1f, 0.5f, 0.1f));
            var wireYellow = CreateWireMaterial("Wireframe_Yellow", new Color(1f, 0.9f, 0.2f));

            var enemyPrefab = CreateEnemyPrefab(wireRed);
            var bossPrefab = CreateBossPrefab(wireRed);
            var bulletPrefab = CreateBulletPrefab(wireGreen);
            var missilePrefab = CreateMissilePrefab(wireCyan);
            var bossMissilePrefab = CreateBossHomingMissilePrefab(wireOrange);
            var missileSupplyPodPrefab = CreateMissileSupplyPodPrefab(wireYellow);
            var enemyBulletPrefab = CreateEnemyBulletPrefab(wireRed);

            AssignEnemyToWaveManager(enemyPrefab, bossPrefab);
            AssignMissileSupplyPodToWaveManager(missileSupplyPodPrefab);
            AssignPlayerWeapons(bulletPrefab, missilePrefab);
            AssignEnemyBullet(enemyPrefab, enemyBulletPrefab);
            AssignBossBullet(bossPrefab, enemyBulletPrefab);
            AssignBossHomingMissile(bossPrefab, bossMissilePrefab);

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

            var go = new GameObject("ChaserEnemy");
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateLowPolySphereMesh();
            go.transform.localScale = new Vector3(1.5f, 1.5f, 2.5f);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;

            go.AddComponent<ChaserEnemy>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBossPrefab(Material mat)
        {
            string path = $"{PrefabDir}/BossEnemy.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("BossEnemy");
            var mesh = CreateLowPolySphereMesh();
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            go.transform.localScale = new Vector3(8f, 8f, 10f);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            CreateBossArmor(go.transform, "LeftArmor", new Vector3(-0.8f, 0f, -0.25f), mesh, mat);
            CreateBossArmor(go.transform, "RightArmor", new Vector3(0.8f, 0f, -0.25f), mesh, mat);

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            go.AddComponent<BossEnemy>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBulletPrefab(Material mat)
        {
            string path = $"{PrefabDir}/Bullet.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("Bullet");
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateLowPolySphereMesh();
            go.transform.localScale = Vector3.one * 0.3f;

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            var collider = go.AddComponent<SphereCollider>();
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

        private static void ApplyLook(string path, Mesh mesh, Material mat, Vector3 scale)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            root.transform.localScale = scale;
            root.GetComponent<MeshFilter>().sharedMesh = mesh;
            root.GetComponent<MeshRenderer>().sharedMaterial = mat;
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static GameObject CreateBossHomingMissilePrefab(Material mat)
        {
            string path = $"{PrefabDir}/BossHomingMissile.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                ApplyLook(path, CreateLowPolySphereMesh(), mat, new Vector3(0.8f, 0.8f, 2f));
                return existing;
            }

            var go = new GameObject("BossHomingMissile");
            go.transform.localScale = new Vector3(0.8f, 0.8f, 2f);
            go.AddComponent<MeshFilter>().sharedMesh = CreateLowPolySphereMesh();
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;

            go.AddComponent<BossHomingMissile>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateMissileSupplyPodPrefab(Material mat)
        {
            string path = $"{PrefabDir}/MissileSupplyPod.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            if (existing != null)
            {
                ApplyLook(path, cubeMesh, mat, Vector3.one * 2.5f);
                return existing;
            }

            var go = new GameObject("MissileSupplyPod");
            go.transform.localScale = Vector3.one * 2.5f;
            go.AddComponent<MeshFilter>().sharedMesh = cubeMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.8f;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            go.AddComponent<MissileSupplyPod>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateEnemyBulletPrefab(Material mat)
        {
            string path = $"{PrefabDir}/EnemyBullet.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("EnemyBullet");
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateLowPolySphereMesh();
            go.transform.localScale = Vector3.one * 0.4f;

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;

            go.AddComponent<EnemyBullet>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateBossArmor(Transform parent, string name, Vector3 position, Mesh mesh, Material mat)
        {
            var armor = new GameObject(name);
            armor.transform.SetParent(parent, false);
            armor.transform.localPosition = position;
            armor.transform.localScale = new Vector3(0.45f, 0.55f, 0.7f);
            armor.AddComponent<MeshFilter>().sharedMesh = mesh;
            armor.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static Mesh CreateLowPolySphereMesh()
        {
            const string meshPath = PrefabDir + "/LowPolySphere.asset";
            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (saved != null) return saved;

            var mesh = BuildLowPolySphereMesh();
            AssetDatabase.CreateAsset(mesh, meshPath);
            return mesh;
        }

        private static Mesh BuildLowPolySphereMesh()
        {
            const int latitudeSegments = 3;
            const int longitudeSegments = 4;

            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();

            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float v = (float)lat / latitudeSegments;
                float theta = v * Mathf.PI;
                float y = Mathf.Cos(theta);
                float radius = Mathf.Sin(theta);

                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    float u = (float)lon / longitudeSegments;
                    float phi = u * Mathf.PI * 2f;
                    float x = Mathf.Cos(phi) * radius;
                    float z = Mathf.Sin(phi) * radius;
                    vertices.Add(new Vector3(x, y, z) * 0.5f);
                }
            }

            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int current = lat * (longitudeSegments + 1) + lon;
                    int next = current + longitudeSegments + 1;

                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);

                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }

            var mesh = new Mesh
            {
                name = "LowPolySphere",
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AssignEnemyToWaveManager(GameObject enemyPrefab, GameObject bossPrefab)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
            var wm = Object.FindFirstObjectByType<WaveManager>();
            if (wm != null)
            {
                var so = new SerializedObject(wm);
                so.FindProperty("_chaserPrefab").objectReferenceValue = enemyPrefab;
                so.FindProperty("_bossPrefab").objectReferenceValue = bossPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void AssignMissileSupplyPodToWaveManager(GameObject missileSupplyPodPrefab)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
            var waveManager = Object.FindFirstObjectByType<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogError("[PrefabSetup] WaveManager not found in Game scene.");
                return;
            }

            var serializedWaveManager = new SerializedObject(waveManager);
            serializedWaveManager.FindProperty("_missileSupplyPodPrefab").objectReferenceValue = missileSupplyPodPrefab;
            serializedWaveManager.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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

        private static void AssignBossBullet(GameObject bossPrefab, GameObject enemyBulletPrefab)
        {
            string path = AssetDatabase.GetAssetPath(bossPrefab);
            var root = PrefabUtility.LoadPrefabContents(path);
            var boss = root.GetComponent<BossEnemy>();
            if (boss != null)
            {
                var so = new SerializedObject(boss);
                so.FindProperty("_bulletPrefab").objectReferenceValue = enemyBulletPrefab;
                so.FindProperty("_hp").intValue = 300;
                so.FindProperty("_scoreValue").intValue = 2500;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void AssignBossHomingMissile(GameObject bossPrefab, GameObject bossMissilePrefab)
        {
            string path = AssetDatabase.GetAssetPath(bossPrefab);
            var root = PrefabUtility.LoadPrefabContents(path);
            var boss = root.GetComponent<BossEnemy>();
            if (boss != null)
            {
                var so = new SerializedObject(boss);
                so.FindProperty("_homingMissilePrefab").objectReferenceValue = bossMissilePrefab;
                so.FindProperty("_homingMissileInterval").floatValue = 60f;
                so.FindProperty("_homingMissilesPerLaunch").intValue = 2;
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
