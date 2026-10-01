using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// Game シーンのカメラに FollowCamera を追加し、Player に追従させる。
    /// メニュー: Tools > ThreeDimension > Setup Follow Camera
    /// </summary>
    public static class CameraSetup
    {
        [MenuItem("Tools/ThreeDimension/Setup Follow Camera")]
        public static void Setup()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);

            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                Debug.LogError("[CameraSetup] Camera not found in Game scene.");
                return;
            }

            // 既存の FollowCamera があれば削除
            var existing = cam.GetComponent<FollowCamera>();
            if (existing != null) Object.DestroyImmediate(existing);

            var follow = cam.gameObject.AddComponent<FollowCamera>();
            var player = Object.FindFirstObjectByType<PlayerShipController>();
            if (player != null)
            {
                var so = new SerializedObject(follow);
                so.FindProperty("_target").objectReferenceValue = player.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[CameraSetup] FollowCamera added to camera.");
        }
    }
}
