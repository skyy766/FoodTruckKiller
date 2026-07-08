using System;
using FoodTruckKiller.Cooking;

namespace FoodTruckKiller.Economy
{
    /// <summary>
    /// 交易类型枚举。
    /// </summary>
    [Serializable]
    public enum TransactionType
    {
        /// <summary>收入：出餐收款。</summary>
        Income,
        /// <summary>支出：购买食材/设备。</summary>
        Expense,
        /// <summary>任务奖励：完成暗杀。</summary>
        MissionReward,
        /// <summary>罚款：被检查/通缉。</summary>
        Fine
    }

    /// <summary>
    /// 交易事件结构体：记录一笔金钱变动。
    /// </summary>
    [Serializable]
    public struct TransactionEvent
    {
        /// <summary>金额（正数为入账，负数为出账）。</summary>
        public int amount;

        /// <summary>交易类型。</summary>
        public TransactionType type;

        /// <summary>关联订单（可空）。</summary>
        public Order order;

        /// <summary>时间戳（Time.time）。</summary>
        public float timestamp;

        /// <summary>构造一笔交易。</summary>
        public TransactionEvent(int amount, TransactionType type, Order order = null)
        {
            this.amount = amount;
            this.type = type;
            this.order = order;
            this.timestamp = UnityEngine.Time.time;
        }
    }
}
