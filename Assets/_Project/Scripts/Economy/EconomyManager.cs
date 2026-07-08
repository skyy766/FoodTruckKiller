using System;
using System.Collections.Generic;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Core.DataLoader;
using FoodTruckKiller.Cooking;
using UnityEngine;

namespace FoodTruckKiller.Economy
{
    /// <summary>
    /// 经济管理器：全局金钱管理，订阅 OnOrderServed 加钱。
    /// </summary>
    public class EconomyManager : SingletonMono<EconomyManager>
    {
        /// <summary>当前金钱。</summary>
        public int Money { get; private set; }

        /// <summary>起始金钱（运行时由 GameConfig.startMoney 覆盖）。</summary>
        [SerializeField] private int startMoney = 100;

        /// <summary>金钱变更事件（参数为变更后余额）。</summary>
        public event Action<int> OnMoneyChanged;

        /// <summary>交易记录。</summary>
        public List<TransactionEvent> Transactions { get; } = new List<TransactionEvent>();

        private void OnEnable()
        {
            // 静态 GameEvents 注入（沙箱无编辑器，不依赖 Inspector 字段）。
            if (GameEvents.OnOrderServed != null) GameEvents.OnOrderServed.Register(HandleOrderServed);
            if (GameEvents.OnTargetKilled != null) GameEvents.OnTargetKilled.Register(HandleTargetKilled);
        }

        private void OnDisable()
        {
            if (GameEvents.OnOrderServed != null) GameEvents.OnOrderServed.Unregister(HandleOrderServed);
            if (GameEvents.OnTargetKilled != null) GameEvents.OnTargetKilled.Unregister(HandleTargetKilled);
        }

        protected override void OnSingletonAwake()
        {
            // 优先从 JsonDataLoader.Config 取起始金钱，回退到 Inspector 默认。
            int start = (JsonDataLoader.Config != null) ? JsonDataLoader.Config.startMoney : startMoney;
            Money = start;
        }

        /// <summary>
        /// 增加金钱。
        /// </summary>
        public void AddMoney(int amount, TransactionType type, Order order = null)
        {
            if (amount == 0) return;
            Money += amount;
            var tx = new TransactionEvent(amount, type, order);
            Transactions.Add(tx);
            OnMoneyChanged?.Invoke(Money);
            // 收入类交易触发金币音效（音效反馈环）
            if (amount > 0 && GameEvents.OnCash != null)
                GameEvents.OnCash.Raise();
        }

        /// <summary>
        /// 消耗金钱，余额不足返回 false。
        /// </summary>
        public bool SpendMoney(int amount, TransactionType type)
        {
            if (Money < amount) return false;
            Money -= amount;
            var tx = new TransactionEvent(-amount, type, null);
            Transactions.Add(tx);
            OnMoneyChanged?.Invoke(Money);
            return true;
        }

        /// <summary>
        /// OnOrderServed 处理：按订单食谱售价加钱。
        /// 由于 GameEvent 不带参数，这里通过 CookingController.CurrentOrder 推断。
        /// </summary>
        private void HandleOrderServed()
        {
            var controller = CookingController.Instance;
            if (controller != null && controller.CurrentOrder != null
                && controller.CurrentOrder.State == OrderState.Served)
            {
                int price = controller.CurrentOrder.Recipe != null ? controller.CurrentOrder.Recipe.price : 0;
                if (price > 0)
                    AddMoney(price, TransactionType.Income, controller.CurrentOrder);
            }
        }

        /// <summary>
        /// OnTargetKilled 处理：任务奖励加钱（具体金额由 AssassinationManager 决策，这里仅占位）。
        /// </summary>
        private void HandleTargetKilled()
        {
            // 奖励由 AssassinationManager 直接调用 AddMoney(MissionReward)，此处仅作占位。
        }
    }
}
