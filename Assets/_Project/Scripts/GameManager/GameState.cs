namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// 游戏全局状态枚举。由 GameManager 持有并驱动流转。
    /// </summary>
    public enum GameState
    {
        /// <summary>启动/初始化阶段（加载配置、初始化子系统）。</summary>
        Boot,

        /// <summary>白天经营+暗杀进行中。</summary>
        Playing,

        /// <summary>暂停（菜单/对话）。</summary>
        Paused,

        /// <summary>失败（被通缉满/被发现/超时未达成）。</summary>
        GameOver,

        /// <summary>胜利（达成暗杀目标并成功撤离）。</summary>
        Victory
    }
}
