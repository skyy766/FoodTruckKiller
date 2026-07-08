namespace FoodTruckKiller.Cooking
{
    /// <summary>
    /// 烹饪操作状态枚举：空闲 / 切菜 / 煎烤 / 组装 / 出餐。
    /// </summary>
    public enum CookingState
    {
        /// <summary>空闲态，可接受新操作。</summary>
        Idle,
        /// <summary>切菜中。</summary>
        Chopping,
        /// <summary>煎烤中。</summary>
        Grilling,
        /// <summary>组装中。</summary>
        Assembling,
        /// <summary>出餐中。</summary>
        Serving
    }
}
