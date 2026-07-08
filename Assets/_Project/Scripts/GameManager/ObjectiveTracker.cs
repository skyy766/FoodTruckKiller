using UnityEngine;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.DataLoader;

namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// 暗杀目标进度跟踪器。监听 OnTargetKilled 事件累计完成数，
    /// 供 GameManager 在白天结束时判定胜负。
    /// <para>totalTargets 运行时根据 <see cref="JsonDataLoader.Targets"/> 自动设置。</para>
    /// </summary>
    public class ObjectiveTracker : MonoBehaviour
    {
        [Header("目标配置")]
        [Tooltip("本关需要暗杀的目标总数，运行时被 JsonDataLoader.Targets.Count 覆盖")]
        [SerializeField] private int totalTargets = 1;

        /// <summary>需击杀目标总数。</summary>
        public int TotalTargets => totalTargets;

        /// <summary>已击杀目标数。</summary>
        public int KilledTargets { get; private set; }

        /// <summary>是否已全部清除。</summary>
        public bool AllTargetsCleared => KilledTargets >= totalTargets;

        private void OnEnable()
        {
            // 静态 GameEvents 注入（沙箱无编辑器）。
            if (GameEvents.OnTargetKilled != null)
                GameEvents.OnTargetKilled.Register(HandleTargetKilled);
        }

        private void OnDisable()
        {
            if (GameEvents.OnTargetKilled != null)
                GameEvents.OnTargetKilled.Unregister(HandleTargetKilled);
        }

        /// <summary>根据 JsonDataLoader.Targets 设置目标总数。</summary>
        public void ApplyConfig()
        {
            if (JsonDataLoader.Targets != null && JsonDataLoader.Targets.Count > 0)
                totalTargets = JsonDataLoader.Targets.Count;
        }

        private void HandleTargetKilled()
        {
            KilledTargets++;
            Debug.Log($"[ObjectiveTracker] Target killed: {KilledTargets}/{totalTargets}");
        }

        /// <summary>重置进度（每个白天周期开始时调用）。</summary>
        public void Reset()
        {
            KilledTargets = 0;
        }
    }
}
