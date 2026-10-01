using UnityEngine;

namespace ThreeDimensionShooter
{
    /// <summary>
    /// 効果音・BGM を管理する。音素材はランタイムで波形合成(プロシージャル)して生成する。
    /// チャイプーン風の簡易 SE/BGM。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SE Volume")]
        [SerializeField, Range(0f, 1f)] private float _seVolume = 0.5f;

        private AudioSource _seSource;
        private AudioSource _bgmSource;

        // 生成したクリップ
        private AudioClip _shotClip;
        private AudioClip _missileClip;
        private AudioClip _explosionClip;
        private AudioClip _playerDestroyedClip;
        private AudioClip _hitClip;
        private AudioClip _lockOnClip;
        private AudioClip _bgmClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _seSource = gameObject.AddComponent<AudioSource>();
            _seSource.playOnAwake = false;

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.volume = 0.25f;

            GenerateClips();
        }

        private void GenerateClips()
        {
            _shotClip = ProceduralAudio.LaserShot();
            _missileClip = ProceduralAudio.MissileLaunch();
            _explosionClip = ProceduralAudio.Explosion();
            _playerDestroyedClip = ProceduralAudio.PlayerDestroyed();
            _hitClip = ProceduralAudio.ShieldHit();
            _lockOnClip = ProceduralAudio.LockOn();
            _bgmClip = ProceduralAudio.BgmLoop();
        }

        public void PlayShot() => PlaySE(_shotClip);
        public void PlayMissile() => PlaySE(_missileClip);
        public void PlayExplosion() => PlaySE(_explosionClip);
        public void PlayPlayerDestroyed() => PlaySE(_playerDestroyedClip);
        public void PlayHit() => PlaySE(_hitClip);
        public void PlayLockOn() => PlaySE(_lockOnClip);

        private void PlaySE(AudioClip clip)
        {
            if (clip == null) return;
            _seSource.PlayOneShot(clip, _seVolume);
        }

        public void StartBGM()
        {
            if (_bgmClip == null || _bgmSource.isPlaying) return;
            _bgmSource.clip = _bgmClip;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }
    }
}
