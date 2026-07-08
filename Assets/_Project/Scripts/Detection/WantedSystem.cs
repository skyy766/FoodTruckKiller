using System;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Core.StateMachine;
using UnityEngine;

namespace FoodTruckKiller.Detection
{
    /// <summary>
    /// 通缉状态枚举。
    /// </summary>
    public enum WantedState
    {
        /// <summary>正常营业。</summary>
        Normal,
        /// <summary>被通缉，无法营业。</summary>
        Wanted,
        /// <summary>已洗清（恢复营业）。</summary>
        Cleared
    }

    /// <summary>
    /// 通缉系统：状态机管理 Normal/Wanted/Cleared。
    /// 订阅 OnWanted 切换到通缉态，通缉时无法营业。
    /// </summary>
    public class WantedSystem : SingletonMono<WantedSystem>
    {
        /// <summary>当前通缉状态。</summary>
        public WantedState State { get; private set; } = WantedState.Normal;

        /// <summary>状态变更事件。</summary>
        public event Action<WantedState> OnStateChanged;

        /// <summary>通缉持续时间（秒）后自动进入 Cleared。</summary>
        [SerializeField] private float wantedDuration = 60f;

        /// <summary>通缉剩余时间。</summary>
        public float RemainingTime { get; private set; }

        private void OnEnable()
        {
            // 静态 GameEvents 注入（沙箱无编辑器）。
            if (GameEvents.OnWanted != null) GameEvents.OnWanted.Register(HandleWanted);
        }

        private void OnDisable()
        {
            if (GameEvents.OnWanted != null) GameEvents.OnWanted.Unregister(HandleWanted);
        }

        private void Update()
        {
            if (State == WantedState.Wanted)
            {
                RemainingTime -= Time.deltaTime;
                if (RemainingTime <= 0f)
                    SetState(WantedState.Cleared);
            }
        }

        /// <summary>
        /// 手动进入通缉态。
        /// </summary>
        public void TriggerWanted()
        {
            SetState(WantedState.Wanted);
        }

        /// <summary>
        /// 手动恢复营业。
        /// </summary>
        public void Clear()
        {
            SetState(WantedState.Cleared);
        }

        /// <summary>
        /// 重置为正常态（新一天）。
        /// </summary>
        public void ResetToNormal()
        {
            SetState(WantedState.Normal);
        }

        private void SetState(WantedState newState)
        {
            if (State == newState) return;
            State = newState;
            if (newState == WantedState.Wanted)
                RemainingTime = wantedDuration;
            OnStateChanged?.Invoke(newState);
        }

        private void HandleWanted()
        {
            SetState(WantedState.Wanted);
        }
    }
}
