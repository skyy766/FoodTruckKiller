using System;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using UnityEngine;

namespace FoodTruckKiller.Detection
{
    /// <summary>
    /// 警戒系统：单例。维护全局警戒值 0~100，超阈值触发 OnWanted。
    /// 每帧衰减。
    /// </summary>
    public class AlertSystem : SingletonMono<AlertSystem>
    {
        /// <summary>当前警戒值（0~100）。</summary>
        public float Alert { get; private set; }

        /// <summary>警戒阈值，超过则触发通缉。</summary>
        [SerializeField] private float wantedThreshold = 60f;

        /// <summary>每秒自然衰减速率。</summary>
        [SerializeField] private float decayPerSec = 2f;

        /// <summary>警戒值变化事件。</summary>
        public event Action<float> OnAlertChangedEvent;

        /// <summary>是否已触发通缉。</summary>
        public bool WantedRaised { get; private set; }

        private void Update()
        {
            if (Alert > 0f)
            {
                Alert = Mathf.Max(0f, Alert - decayPerSec * Time.deltaTime);
                NotifyChanged();
            }
        }

        /// <summary>
        /// 增加警戒值。
        /// </summary>
        public void AddAlert(float amount)
        {
            if (amount <= 0f) return;
            Alert = Mathf.Min(100f, Alert + amount);
            NotifyChanged();
            if (!WantedRaised && Alert >= wantedThreshold)
            {
                WantedRaised = true;
                // 静态 GameEvents 注入（沙箱无编辑器）。
                GameEvents.OnWanted?.Raise();
            }
        }

        /// <summary>
        /// 手动清除通缉状态（例如任务结束）。
        /// </summary>
        public void ClearWanted()
        {
            Alert = 0f;
            WantedRaised = false;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            OnAlertChangedEvent?.Invoke(Alert);
            // 静态 GameEvents 注入（沙箱无编辑器）。
            GameEvents.OnAlertChanged?.Raise();
        }
    }
}
