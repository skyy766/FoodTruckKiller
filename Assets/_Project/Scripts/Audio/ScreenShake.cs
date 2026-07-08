using System.Collections;
using UnityEngine;

namespace FoodTruckKiller.Audio
{
    /// <summary>
    /// 屏幕震动：协程驱动的 transform 抖动，幅度/时长可配。
    /// 通常挂在主摄像机上，由 AudioManager.PlaySFXWithFeedback 或其他系统调用 Shake()。
    /// </summary>
    [DisallowMultipleComponent]
    public class ScreenShake : MonoBehaviour
    {
        [Header("Defaults")]
        [Tooltip("默认震幅（屏幕单位/相对偏移）")]
        [SerializeField] private float defaultAmplitude = 0.15f;
        [Tooltip("默认震动时长（秒）")]
        [SerializeField] private float defaultDuration = 0.2f;
        [Tooltip("震动频率（Hz）")]
        [SerializeField] private float frequency = 25f;
        [Tooltip("衰减曲线指数（越大衰减越快）")]
        [SerializeField] private float decay = 4f;
        [Tooltip("是否作用于本地位置（true=本地，false=世界）")]
        [SerializeField] private bool useLocalPosition = true;

        private Vector3 _origin;
        private Coroutine _running;

        private void Awake()
        {
            _origin = useLocalPosition ? transform.localPosition : transform.position;
        }

        /// <summary>以默认参数震动。</summary>
        public void Shake()
        {
            Shake(defaultAmplitude, defaultDuration);
        }

        /// <summary>以指定幅度/时长震动。多次调用会叠加（新调用接管）。</summary>
        public void Shake(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(DoShake(amplitude, duration));
        }

        private IEnumerator DoShake(float amplitude, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - (t / duration);                  // 0..1 线性递减
                float env = Mathf.Pow(k, decay);                // 指数衰减
                float amp = amplitude * env;

                // 用 Perlin/正弦混合得到不规则抖动，避免纯周期感
                float x = (Mathf.PerlinNoise(t * frequency, 0f) - 0.5f) * 2f * amp;
                float y = (Mathf.PerlinNoise(0f, t * frequency) - 0.5f) * 2f * amp;
                float z = (Mathf.PerlinNoise(t * frequency, t * frequency) - 0.5f) * 2f * amp * 0.5f;

                if (useLocalPosition)
                    transform.localPosition = _origin + new Vector3(x, y, z);
                else
                    transform.position = _origin + new Vector3(x, y, z);

                yield return null;
            }

            // 复位
            if (useLocalPosition) transform.localPosition = _origin;
            else transform.position = _origin;
            _running = null;
        }

        /// <summary>外部修改原点（如摄像机跟随逻辑更新后）。</summary>
        public void SetOrigin(Vector3 newOrigin)
        {
            _origin = newOrigin;
        }
    }
}
