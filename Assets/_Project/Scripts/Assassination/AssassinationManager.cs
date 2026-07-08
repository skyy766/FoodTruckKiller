using System;
using System.Collections.Generic;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Economy;
using UnityEngine;

namespace FoodTruckKiller.Assassination
{
    /// <summary>
    /// 暗杀任务状态。
    /// </summary>
    public enum AssassinationStatus
    {
        /// <summary>未接单。</summary>
        None,
        /// <summary>已接单目标激活中。</summary>
        Active,
        /// <summary>已完成击杀。</summary>
        Completed,
        /// <summary>失败（目标逃离/超时）。</summary>
        Failed
    }

    /// <summary>
    /// 暗杀管理器：单例。负责接单、目标激活、结算。
    /// 订阅 OnTargetKilled 触发奖励发放。
    /// </summary>
    public class AssassinationManager : SingletonMono<AssassinationManager>
    {
        /// <summary>当前目标画像。</summary>
        public TargetProfile ActiveTarget { get; private set; }

        /// <summary>当前任务状态。</summary>
        public AssassinationStatus Status { get; private set; } = AssassinationStatus.None;

        /// <summary>可用目标列表（任务池）。</summary>
        public List<TargetProfile> targetPool = new List<TargetProfile>();

        /// <summary>任务状态变更事件。</summary>
        public event Action<AssassinationStatus, TargetProfile> OnStatusChanged;

        private void OnEnable()
        {
            // 静态 GameEvents 注入（沙箱无编辑器）。
            if (GameEvents.OnTargetKilled != null) GameEvents.OnTargetKilled.Register(HandleTargetKilled);
        }

        private void OnDisable()
        {
            if (GameEvents.OnTargetKilled != null) GameEvents.OnTargetKilled.Unregister(HandleTargetKilled);
        }

        /// <summary>
        /// 接单：从任务池挑选目标并激活。
        /// </summary>
        /// <param name="target">指定目标，为空则从池中随机。</param>
        public bool AcceptContract(TargetProfile target = null)
        {
            if (Status == AssassinationStatus.Active) return false;

            if (target == null && targetPool != null && targetPool.Count > 0)
                target = targetPool[UnityEngine.Random.Range(0, targetPool.Count)];

            if (target == null) return false;

            ActiveTarget = target;
            Status = AssassinationStatus.Active;
            OnStatusChanged?.Invoke(Status, ActiveTarget);
            return true;
        }

        /// <summary>
        /// 结算任务（成功）。
        /// </summary>
        public void SettleSuccess()
        {
            if (Status != AssassinationStatus.Active) return;
            int reward = ActiveTarget != null ? ActiveTarget.reward : 0;
            if (reward > 0 && EconomyManager.Instance != null)
                EconomyManager.Instance.AddMoney(reward, TransactionType.MissionReward);
            Status = AssassinationStatus.Completed;
            OnStatusChanged?.Invoke(Status, ActiveTarget);
        }

        /// <summary>
        /// 任务失败。
        /// </summary>
        public void SettleFailure()
        {
            if (Status != AssassinationStatus.Active) return;
            Status = AssassinationStatus.Failed;
            OnStatusChanged?.Invoke(Status, ActiveTarget);
        }

        /// <summary>
        /// 重置到未接单状态。
        /// </summary>
        public void ResetContract()
        {
            Status = AssassinationStatus.None;
            ActiveTarget = null;
            OnStatusChanged?.Invoke(Status, ActiveTarget);
        }

        /// <summary>
        /// OnTargetKilled 处理：结算奖励。
        /// </summary>
        private void HandleTargetKilled()
        {
            if (Status == AssassinationStatus.Active)
                SettleSuccess();
        }
    }
}
