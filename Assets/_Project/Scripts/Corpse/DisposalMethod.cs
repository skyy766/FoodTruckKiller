namespace FoodTruckKiller.Corpse
{
    /// <summary>
    /// 尸体处理方式枚举。
    /// </summary>
    public enum DisposalMethod
    {
        /// <summary>绞肉（可产出人肉食材）。</summary>
        Grind,
        /// <summary>抛尸（丢弃至指定点，可能被发现）。</summary>
        Dump,
        /// <summary>冷冻（延长被发现前的腐败时间）。</summary>
        Freeze,
        /// <summary>溶解（化学处理，无残留）。</summary>
        Dissolve
    }
}
