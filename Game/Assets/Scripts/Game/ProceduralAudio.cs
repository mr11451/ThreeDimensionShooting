using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// ランタイムで波形を合成してチャイプーン風の効果音を生成するユーティリティ。
    /// 外部音素材なしで簡易 SE を実現する。
    /// </summary>
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        /// <summary>レーザーショット: 下降する矩形波</summary>
        public static AudioClip LaserShot()
        {
            float duration = 0.15f;
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];
            float startFreq = 1200f, endFreq = 300f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float phase = 2f * Mathf.PI * freq * i / SampleRate;
                data[i] = Mathf.Sign(Mathf.Sin(phase)) * (1f - t) * 0.6f; // 矩形波 + 減衰
            }
            return CreateClip("Shot", data);
        }

        /// <summary>ミサイル発射: ノイズ+上昇音</summary>
        public static AudioClip MissileLaunch()
        {
            float duration = 0.3f;
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];
            var rng = new System.Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(200f, 800f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * i / SampleRate);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.3f;
                data[i] = (tone * 0.5f + noise) * (1f - t) * 0.7f;
            }
            return CreateClip("Missile", data);
        }

        /// <summary>爆発: 減衰するノイズ</summary>
        public static AudioClip Explosion()
        {
            float duration = 0.5f;
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];
            var rng = new System.Random();
            float lastValue = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                // ローパスフィルタ風に平滑化して低音化
                lastValue = Mathf.Lerp(lastValue, noise, 0.15f);
                data[i] = lastValue * Mathf.Pow(1f - t, 2f) * 1.2f;
            }
            return CreateClip("Explosion", data);
        }

        /// <summary>自機撃墜: 低い衝撃音と下降する警告音</summary>
        public static AudioClip PlayerDestroyed()
        {
            float duration = 1.4f;
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];
            var random = new System.Random();
            float filteredNoise = 0f;
            float lowPhase = 0f;
            float highPhase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float normalizedTime = t / duration;
                float noise = (float)(random.NextDouble() * 2d - 1d);
                filteredNoise = Mathf.Lerp(filteredNoise, noise, 0.08f);

                float lowFrequency = Mathf.Lerp(150f, 38f, normalizedTime);
                float highFrequency = Mathf.Lerp(720f, 110f, normalizedTime);
                lowPhase += 2f * Mathf.PI * lowFrequency / SampleRate;
                highPhase += 2f * Mathf.PI * highFrequency / SampleRate;

                float impactEnvelope = Mathf.Exp(-t * 4.5f);
                float alarmEnvelope = Mathf.Sin(Mathf.PI * normalizedTime) * 0.5f + 0.5f;
                float impact = (filteredNoise * 0.55f + Mathf.Sin(lowPhase) * 0.35f) * impactEnvelope;
                float alarm = Mathf.Sin(highPhase) * alarmEnvelope * (1f - normalizedTime) * 0.3f;
                data[i] = (impact + alarm) * 0.8f;
            }

            return CreateClip("PlayerDestroyed", data);
        }

        /// <summary>シールド被弾: 短い金属音</summary>
        public static AudioClip ShieldHit()
        {
            float duration = 0.1f;
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = 2200f + Mathf.Sin(t * 50f) * 300f;
                float phase = 2f * Mathf.PI * freq * i / SampleRate;
                data[i] = Mathf.Sin(phase) * (1f - t) * 0.5f;
            }
            return CreateClip("Hit", data);
        }

        /// <summary>ロックオン: 短い2音</summary>
        public static AudioClip LockOn()
        {
            float duration = 0.12f;
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = (t < 0.5f) ? 880f : 1320f;
                float phase = 2f * Mathf.PI * freq * i / SampleRate;
                data[i] = Mathf.Sin(phase) * 0.4f;
            }
            return CreateClip("LockOn", data);
        }

        /// <summary>
        /// 簡易BGM: シンセ系の短いループフレーズ。ベースライン + アルペジオ風の音型。
        /// レトロなアーケード感を出すため矩形波を使う。
        /// </summary>
        public static AudioClip BgmLoop()
        {
            // Aマイナー系のシンプルなフレーズ(周波数)
            // ベース: A2, C3, E3, G3 / 上音: アルペジオ
            float[] bassNotes = { 110f, 130.81f, 164.81f, 196f }; // A2, C3, E3, G3
            float[] arpNotes = { 220f, 261.63f, 329.63f, 392f, 440f, 523.25f }; // A3..C5

            float beatSec = 0.5f;          // 1拍
            int beatsPerBar = 4;
            int bars = 2;                  // 2小節でループ
            float duration = beatSec * beatsPerBar * bars;
            int samples = (int)(SampleRate * duration);
            var data = new float[samples];

            int samplesPerBeat = (int)(SampleRate * beatSec);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                int beat = i / samplesPerBeat;
                int bar = beat / beatsPerBar;
                int beatInBar = beat % beatsPerBar;
                float tInBeat = (float)(i % samplesPerBeat) / samplesPerBeat;

                // ベース: 1小節ごとに bassNotes を巡回、各拍で持続(減衰付き)
                float bassFreq = bassNotes[(bar * beatsPerBar + 0) % bassNotes.Length];
                if (beatInBar == 0)
                {
                    bassFreq = bassNotes[bar % bassNotes.Length];
                }
                float bassPhase = 2f * Mathf.PI * bassFreq * t;
                float bass = Mathf.Sign(Mathf.Sin(bassPhase)) * 0.18f * (1f - tInBeat * 0.5f);

                // アルペジオ: 各拍を4分割(16分音符相当)で arpNotes を巡回
                int sub = (i * 4) / samplesPerBeat; // 16分音符インデックス
                float arpFreq = arpNotes[(sub + bar * 4) % arpNotes.Length];
                float arpPhase = 2f * Mathf.PI * arpFreq * t;
                float arpEnv = 1f - ((i % (samplesPerBeat / 4)) / (float)(samplesPerBeat / 4));
                float arp = Mathf.Sign(Mathf.Sin(arpPhase)) * 0.08f * arpEnv;

                data[i] = bass + arp;
            }
            return CreateClip("Bgm", data);
        }

        private static AudioClip CreateClip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
