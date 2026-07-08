using System.Collections.Generic;
using UnityEngine;
using FoodTruckKiller.Core.Singleton;

namespace FoodTruckKiller.Audio
{
    /// <summary>
    /// 音频管理器：单例，持有 AudioSource 对象池，提供按名播放 SFX / BGM 的统一接口。
    /// <para>沙箱无编辑器：所有音效通过 <see cref="Resources.LoadAll{T}(string)"/> 从
    /// <c>Assets/Resources/Audio/SFX/</c> 与 <c>Assets/Resources/Audio/Music/</c> 预加载到字典。</para>
    /// 依赖 A1 提供的 SingletonMono&lt;T&gt; 基类（FoodTruckKiller.Core.Singleton）。
    /// </summary>
    [RequireComponent(typeof(AudioListener))]
    public class AudioManager : SingletonMono<AudioManager>
    {
        [Header("Pool")]
        [Tooltip("SFX 对象池大小")]
        [SerializeField] private int poolSize = 8;
        [Tooltip("BGM 专用 AudioSource（留空则运行时自动创建）")]
        [SerializeField] private AudioSource musicSource = null;

        [Header("Defaults")]
        [SerializeField] private float defaultSfxVolume = 1f;
        [SerializeField] private float defaultMusicVolume = 0.6f;

        // 资源路径（Resources.LoadAll 的相对路径，不含扩展名）
        private const string SfxResourcesPath = "Audio/SFX";
        private const string MusicResourcesPath = "Audio/Music";

        private readonly List<AudioSource> _pool = new List<AudioSource>();
        private readonly Dictionary<string, AudioClip> _sfxMap = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, AudioClip> _musicMap = new Dictionary<string, AudioClip>();
        private int _poolCursor = 0;

        /// <summary>
        /// SingletonMono 首次实例化时调用一次。预加载所有 SFX/BGM 并初始化对象池。
        /// </summary>
        protected override void OnSingletonAwake()
        {
            PreloadAll();
            InitPool();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = defaultMusicVolume;
            }
        }

        private void PreloadAll()
        {
            _sfxMap.Clear();
            _musicMap.Clear();

            var sfxClips = Resources.LoadAll<AudioClip>(SfxResourcesPath);
            foreach (var clip in sfxClips)
            {
                if (clip == null) continue;
                // Resources.LoadAll 返回的 name 不含路径与扩展名，直接作为键
                _sfxMap[clip.name] = clip;
            }

            var musicClips = Resources.LoadAll<AudioClip>(MusicResourcesPath);
            foreach (var clip in musicClips)
            {
                if (clip == null) continue;
                _musicMap[clip.name] = clip;
            }

            Debug.Log($"[AudioManager] Preloaded SFX={_sfxMap.Count}, Music={_musicMap.Count}");
        }

        private void InitPool()
        {
            for (int i = _pool.Count; i < poolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                _pool.Add(src);
            }
        }

        /// <summary>
        /// 运行时注册/覆盖一个 SFX 片段（供其他模块代码注入或 Resources 预加载未覆盖的情况使用）。
        /// </summary>
        public void RegisterClip(string clipName, AudioClip clip)
        {
            if (string.IsNullOrEmpty(clipName) || clip == null) return;
            _sfxMap[clipName] = clip;
        }

        /// <summary>
        /// 按 clipName 播放 SFX。先查预加载字典，未命中时尝试 Resources.Load 兜底。
        /// </summary>
        /// <param name="clipName">片段名（不含扩展名，如 "kill"）</param>
        public void PlaySFX(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return;

            if (!_sfxMap.TryGetValue(clipName, out var clip) || clip == null)
            {
                clip = Resources.Load<AudioClip>(SfxResourcesPath + "/" + clipName);
                if (clip != null) _sfxMap[clipName] = clip;
            }
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX not found: {clipName}");
                return;
            }

            PlayClipOnPool(clip, defaultSfxVolume);
        }

        /// <summary>
        /// 播放 SFX 并显式指定音量。
        /// </summary>
        public void PlaySFX(string clipName, float volume)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            if (!_sfxMap.TryGetValue(clipName, out var clip) || clip == null)
            {
                clip = Resources.Load<AudioClip>(SfxResourcesPath + "/" + clipName);
                if (clip != null) _sfxMap[clipName] = clip;
            }
            if (clip == null) return;
            PlayClipOnPool(clip, Mathf.Clamp01(volume));
        }

        private void PlayClipOnPool(AudioClip clip, float volume)
        {
            if (_pool.Count == 0) InitPool();
            // 轮询取一个未在播放（或最早开始）的源
            AudioSource src = null;
            for (int i = 0; i < _pool.Count; i++)
            {
                var candidate = _pool[(_poolCursor + i) % _pool.Count];
                if (!candidate.isPlaying) { src = candidate; break; }
            }
            if (src == null) src = _pool[_poolCursor % _pool.Count];
            _poolCursor = (_poolCursor + 1) % _pool.Count;

            src.clip = clip;
            src.volume = volume;
            src.pitch = 1f;
            src.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// 播放 BGM 循环。同名片段会平滑切换。
        /// </summary>
        public void PlayMusic(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.volume = defaultMusicVolume;
            }

            if (!_musicMap.TryGetValue(clipName, out var clip) || clip == null)
            {
                clip = Resources.Load<AudioClip>(MusicResourcesPath + "/" + clipName);
                if (clip != null) _musicMap[clipName] = clip;
            }
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] Music not found: {clipName}");
                return;
            }
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = defaultMusicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
                musicSource.Stop();
        }

        public void SetMusicVolume(float v)
        {
            if (musicSource != null) musicSource.volume = Mathf.Clamp01(v);
        }

        /// <summary>触发命中停顿 + 屏幕震动等组合反馈的便捷入口（可选挂载）。</summary>
        public void PlaySFXWithFeedback(string clipName, float shakeAmp = 0f, float shakeDur = 0f)
        {
            PlaySFX(clipName);
            if (shakeDur > 0f && Camera.main != null)
            {
                var shaker = Camera.main.GetComponent<ScreenShake>();
                if (shaker != null) shaker.Shake(shakeAmp, shakeDur);
            }
        }
    }
}
