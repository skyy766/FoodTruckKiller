using System;

namespace FoodTruckKiller.Core.DataLoader
{
    /// <summary>
    /// 全局游戏配置（POCO）。字段对齐 <c>Data/gameconfig.json</c>。
    /// 由 <see cref="JsonDataLoader"/> 通过 <c>Resources.Load&lt;TextAsset&gt;</c> 加载并用
    /// <c>JsonUtility.FromJson</c> 解析。
    /// </summary>
    [Serializable]
    public class GameConfig
    {
        /// <summary>单关卡（一个白天）总时长（秒）。</summary>
        public int dayDurationSec = 480;

        /// <summary>起始金钱。</summary>
        public int startMoney = 100;

        /// <summary>伪装度上限。</summary>
        public float coverMax = 100f;

        /// <summary>伪装度每秒自然恢复值。</summary>
        public float coverRegenPerSec = 0.5f;

        /// <summary>通缉度阈值，达到即触发任务失败/警力集结。</summary>
        public float wantedThreshold = 100f;

        /// <summary>瞬时警报值每秒衰减值。</summary>
        public float alertDecayPerSec = 2f;

        /// <summary>警察视野距离（格数）。</summary>
        public float policeVisionRange = 5f;

        /// <summary>警察视野锥角度。</summary>
        public float policeVisionAngle = 60f;

        /// <summary>卫生检查员每次造访的最短间隔（秒）。</summary>
        public float inspectorIntervalSec = 120f;
    }
}
