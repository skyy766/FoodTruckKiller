using UnityEngine;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.DataLoader;

namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// 白天倒计时控制器。默认 480 秒（8 分钟），时间到触发 OnDayEnd 事件。
    /// <para>由 GameManager 在 Playing 状态下每帧驱动 Tick(deltaTime)。</para>
    /// <para>dayDuration 运行时从 <see cref="JsonDataLoader.Config"/> 取值覆盖。</para>
    /// </summary>
    public class DayTimeController : MonoBehaviour
    {
        [Header("配置")]
        [Tooltip("白天总时长（秒），运行时被 GameConfig.dayDurationSec 覆盖")]
        [SerializeField] private float dayDuration = 480f;

        /// <summary>剩余时间（秒）。</summary>
        public float RemainingTime { get; private set; }

        /// <summary>白天总时长。</summary>
        public float TotalDuration => dayDuration;

        /// <summary>白天进度 0~1（0=开始，1=结束）。</summary>
        public float NormalizedProgress =>
            dayDuration > 0f ? 1f - (RemainingTime / dayDuration) : 0f;

        /// <summary>应用 GameConfig 覆盖默认时长。</summary>
        public void ApplyConfig()
        {
            if (JsonDataLoader.Config != null && JsonDataLoader.Config.dayDurationSec > 0)
                dayDuration = JsonDataLoader.Config.dayDurationSec;
        }

        /// <summary>开始一个新的白天周期。</summary>
        public void StartDay()
        {
            RemainingTime = dayDuration;
        }

        /// <summary>每帧推进时间，归零时触发 OnDayEnd。</summary>
        public void Tick(float deltaTime)
        {
            if (RemainingTime <= 0f) return;
            RemainingTime -= deltaTime;
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                OnDayEnd();
            }
        }

        private void OnDayEnd()
        {
            Debug.Log("[DayTimeController] Day ended (time up).");
            // 静态 GameEvents 注入（沙箱无编辑器）。
            if (GameEvents.OnDayEnd != null)
                GameEvents.OnDayEnd.Raise();
        }
    }
}
