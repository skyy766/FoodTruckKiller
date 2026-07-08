using System.Collections.Generic;
using FoodTruckKiller.Assassination;
using FoodTruckKiller.Core.Singleton;
using UnityEngine;

namespace FoodTruckKiller.Detection
{
    /// <summary>
    /// 证据追踪器：单例。记录现场证据（血迹/爆炸痕迹），供检测系统判定。
    /// </summary>
    public class EvidenceTracker : SingletonMono<EvidenceTracker>
    {
        /// <summary>证据条目。</summary>
        public class EvidenceEntry
        {
            /// <summary>位置。</summary>
            public Vector3 position;
            /// <summary>类型。</summary>
            public EvidenceType type;
            /// <summary>生成时间。</summary>
            public float time;
        }

        /// <summary>当前场景所有证据。</summary>
        public List<EvidenceEntry> Entries { get; } = new List<EvidenceEntry>();

        /// <summary>证据最大存活时间（秒）。</summary>
        [SerializeField] private float evidenceLifetime = 300f;

        private void Update()
        {
            // 清理过期证据。
            float now = Time.time;
            Entries.RemoveAll(e => now - e.time > evidenceLifetime);
        }

        /// <summary>
        /// 在指定位置留下一处证据。
        /// </summary>
        public void LeaveEvidence(Vector3 position, EvidenceType type)
        {
            if (type == EvidenceType.None) return;
            Entries.Add(new EvidenceEntry
            {
                position = position,
                type = type,
                time = Time.time
            });
        }

        /// <summary>
        /// 查询指定点附近是否有证据。
        /// </summary>
        /// <param name="point">查询点。</param>
        /// <param name="radius">半径。</param>
        /// <returns>是否发现证据。</returns>
        public bool HasEvidenceNearby(Vector3 point, float radius)
        {
            float sqr = radius * radius;
            foreach (var e in Entries)
            {
                if ((e.position - point).sqrMagnitude <= sqr)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 清除所有证据（玩家使用清洁工具时调用）。
        /// </summary>
        public void ClearAll()
        {
            Entries.Clear();
        }

        /// <summary>
        /// 清除指定点附近的证据。
        /// </summary>
        public void ClearNearby(Vector3 point, float radius)
        {
            float sqr = radius * radius;
            Entries.RemoveAll(e => (e.position - point).sqrMagnitude <= sqr);
        }
    }
}
