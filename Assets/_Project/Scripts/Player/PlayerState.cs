namespace FoodTruckKiller.Player
{
    /// <summary>
    /// 玩家行为状态枚举。
    /// </summary>
    public enum PlayerState
    {
        /// <summary>站立/空闲。</summary>
        Idle,

        /// <summary>移动中。</summary>
        Move,

        /// <summary>烹饪中（无法移动）。</summary>
        Cook,

        /// <summary>执行暗杀动作。</summary>
        Kill,

        /// <summary>携带物品（尸体/食材/诱饵餐）。</summary>
        Carry
    }
}
