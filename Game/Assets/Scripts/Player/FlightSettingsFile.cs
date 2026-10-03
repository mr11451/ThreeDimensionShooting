using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 飛行(慣性)パラメーターを外部テキストファイルから読み込む。
    /// 場所: ビルド後は ThreeDimensionShooter_Data/StreamingAssets/FlightSettings.txt
    ///       エディター上は Assets/StreamingAssets/FlightSettings.txt
    /// 書式: 「キー = 値」を1行ずつ。「#」以降はコメント。未記載・不正な値は既定値のまま。
    /// </summary>
    public static class FlightSettingsFile
    {
        public const string FileName = "FlightSettings.txt";

        public static string FilePath => Path.Combine(Application.streamingAssetsPath, FileName);

        public static Dictionary<string, float> Load()
        {
            var values = new Dictionary<string, float>();

            try
            {
                if (!File.Exists(FilePath)) return values;

                foreach (var rawLine in File.ReadAllLines(FilePath))
                {
                    int commentIndex = rawLine.IndexOf('#');
                    string line = (commentIndex >= 0 ? rawLine.Substring(0, commentIndex) : rawLine).Trim();
                    if (line.Length == 0) continue;

                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    string key = line.Substring(0, eq).Trim();
                    string text = line.Substring(eq + 1).Trim();
                    if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    {
                        values[key] = value;
                    }
                    else
                    {
                        Debug.LogWarning($"[FlightSettings] 数値として読めません: {rawLine}");
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[FlightSettings] 読み込み失敗: {e.Message}");
            }

            return values;
        }
    }
}
