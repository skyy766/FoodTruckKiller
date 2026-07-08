using System;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Corpse;
using FoodTruckKiller.Economy;
using FoodTruckKiller.Inventory;
using UnityEngine;

namespace FoodTruckKiller.Detection
{
    /// <summary>
    /// 卫生检查员：定时随机检查餐车。
    /// 若发现尸体或人肉食材则触发检查失败（直接调用 CoverSystem.ApplyInspectFailPenalty）。
    /// </summary>
    public class HealthInspector : SingletonMono<HealthInspector>
    {
        /// <summary>检查间隔（秒）。</summary>
        [SerializeField] private float inspectInterval = 120f;

        /// <summary>检查半径（覆盖餐车范围）。</summary>
        [SerializeField] private float inspectRadius = 5f;

        /// <summary>检查中心点（餐车位置）。</summary>
        public Transform inspectCenter;

        /// <summary>人肉食材 id（用于背包检查）。</summary>
        public string humanMeatId = "human_meat";

        private float _timer;

        private void OnEnable()
        {
            // 静态 GameEvents 注入（沙箱无编辑器）。
            if (GameEvents.OnDayEnd != null) GameEvents.OnDayEnd.Register(HandleDayEnd);
        }

        private void OnDisable()
        {
            if (GameEvents.OnDayEnd != null) GameEvents.OnDayEnd.Unregister(HandleDayEnd);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = inspectInterval;
                PerformInspection();
            }
        }

        /// <summary>
        /// 执行一次检查。
        /// </summary>
        public void PerformInspection()
        {
            Vector3 center = inspectCenter != null ? inspectCenter.position : transform.position;

            // 检查视野内尸体。
            if (CorpseManager.Instance != null)
            {
                var corpse = CorpseManager.Instance.GetNearestVisible(center, inspectRadius);
                if (corpse != null)
                {
                    // 尸体被发现：触发全局 OnCorpseFound 事件（CoverSystem 等订阅）。
                    GameEvents.OnCorpseFound?.Raise();
                    FailInspection();
                    return;
                }
            }

            // 检查背包内人肉食材。
            if (InventorySystem.Instance != null && InventorySystem.Instance.Has(humanMeatId))
            {
                FailInspection();
                return;
            }

            // 检查通过，无动作。
        }

        /// <summary>
        /// 触发检查失败：直接调用 CoverSystem 扣减伪装度。
        /// <para>OnInspectFail 不在 GameEvents 16 个静态事件之列，故走直接方法调用。</para>
        /// </summary>
        private void FailInspection()
        {
            if (CoverSystem.Instance != null)
                CoverSystem.Instance.ApplyInspectFailPenalty();
        }

        private void HandleDayEnd()
        {
            _timer = inspectInterval;
        }
    }
}
