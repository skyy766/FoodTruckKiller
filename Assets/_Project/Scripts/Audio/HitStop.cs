using System.Collections;
using UnityEngine;

namespace FoodTruckKiller.Audio
{
    /// <summary>
    /// 命中停顿（Hit Stop）：通过短暂将 Time.timeScale 归零制造打击感，
    /// 默认 0.05s 后恢复原 timeScale。无视 timeScale 影响自身协程（使用 WaitForSecondsRealtime）。
    /// </summary>
    [DisallowMultipleComponent]
    public class HitStop : MonoBehaviour
    {
        [Tooltip("默认停顿时长（秒，真实时间）")]
        [SerializeField] private float defaultDuration = 0.05f;

        private float _originalScale = 1f;
        private Coroutine _running;
        private bool _active = false;

        private void Awake()
        {
            _originalScale = Time.timeScale;
        }

        /// <summary>以默认时长触发命中停顿。</summary>
        public void Trigger()
        {
            Trigger(defaultDuration);
        }

        /// <summary>以指定时长触发命中停顿。多次调用取较长者。</summary>
        public void Trigger(float duration)
        {
            if (duration <= 0f) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(DoHitStop(duration));
        }

        private IEnumerator DoHitStop(float duration)
        {
            _active = true;
            _originalScale = Mathf.Approximately(Time.timeScale, 0f) ? _originalScale : Time.timeScale;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = _originalScale > 0f ? _originalScale : 1f;
            _active = false;
            _running = null;
        }

        private void OnDisable()
        {
            // 防止意外销毁时 timeScale 被卡在 0
            if (_active)
            {
                Time.timeScale = _originalScale > 0f ? _originalScale : 1f;
                _active = false;
                _running = null;
            }
        }
    }
}
