using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ThreeDimensionShooter.EditorTools
{
    /// <summary>
    /// Windows 向けスタンドアロンビルドを実行する。
    /// メニュー: Tools > ThreeDimension > Build Windows / Build And Run Windows
    /// コマンドライン: Unity.exe -batchmode -quit -projectPath <path> -executeMethod ThreeDimensionShooter.EditorTools.BuildPlayer.BuildWindows
    /// </summary>
    public static class BuildPlayer
    {
        private const string OutputDir = "Builds/Windows";
        private const string ExeName = "ThreeDimensionShooter.exe";

        [MenuItem("Tools/ThreeDimension/Build Windows")]
        public static void BuildWindows()
        {
            BuildWindows(BuildOptions.None);
        }

        [MenuItem("Tools/ThreeDimension/Build And Run Windows")]
        public static void BuildAndRunWindows()
        {
            BuildWindows(BuildOptions.AutoRunPlayer);
        }

        private static void BuildWindows(BuildOptions buildOptions)
        {
            // EditorBuildSettings に登録されたシーンを使用
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled) scenes.Add(s.path);
            }

            if (scenes.Count == 0)
            {
                Debug.LogError("[Build] No scenes in EditorBuildSettings.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = $"{OutputDir}/{ExeName}",
                target = BuildTarget.StandaloneWindows64,
                options = buildOptions,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] Succeeded: {summary.totalSize / (1024 * 1024)} MB -> {options.locationPathName}");
            }
            else
            {
                Debug.LogError($"[Build] Failed: {summary.result}");
            }
        }
    }
}
