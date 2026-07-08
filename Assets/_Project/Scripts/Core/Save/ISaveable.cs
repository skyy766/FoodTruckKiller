namespace FoodTruckKiller.Core.Save
{
    /// <summary>
    /// 可存档接口。需要持久化的系统实现此接口并向 SaveManager 注册。
    /// <para>存档采用 JSON 序列化：Save 返回 JSON 字符串，Load 接收 JSON 字符串。</para>
    /// </summary>
    public interface ISaveable
    {
        /// <summary>存档唯一标识，作为存档数据键。</summary>
        string GetSaveKey();

        /// <summary>序列化当前状态为 JSON 字符串。</summary>
        string Save();

        /// <summary>从 JSON 字符串反序列化并恢复状态。</summary>
        void Load(string json);
    }
}
