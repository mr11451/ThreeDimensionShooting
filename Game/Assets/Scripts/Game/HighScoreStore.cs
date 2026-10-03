using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ローカル保存されたハイスコア情報を管理する。
    /// 3Dシューティングのスコアランキングは最大 10 件まで保持する。
    /// </summary>
    public static class HighScoreStore
    {
        private const string SaveKey = "ThreeDimension.HighScores";
        private const int MaxEntries = 10;

        [Serializable]
        private sealed class HighScoreList
        {
            public int[] scores = Array.Empty<int>();
        }

        public static int BestScore => GetTopScores().DefaultIfEmpty(0).First();

        public static int[] GetTopScores()
        {
            var json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<int>();
            }

            try
            {
                var data = JsonUtility.FromJson<HighScoreList>(json);
                return data != null && data.scores != null && data.scores.Length > 0
                    ? data.scores
                    : Array.Empty<int>();
            }
            catch
            {
                return Array.Empty<int>();
            }
        }

        public static int UpdateHighScore(int score)
        {
            var normalized = Mathf.Max(0, score);
            var scores = new List<int>(GetTopScores());
            scores.Add(normalized);
            scores.Sort((left, right) => right.CompareTo(left));

            var trimmed = scores.Take(MaxEntries).ToArray();
            var payload = JsonUtility.ToJson(new HighScoreList { scores = trimmed });
            PlayerPrefs.SetString(SaveKey, payload);
            PlayerPrefs.Save();

            return trimmed.Length > 0 ? trimmed[0] : 0;
        }
    }
}
