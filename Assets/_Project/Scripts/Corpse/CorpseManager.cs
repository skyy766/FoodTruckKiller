using System.Collections.Generic;
using FoodTruckKiller.Core.Singleton;
using UnityEngine;

namespace FoodTruckKiller.Corpse
{
    /// <summary>
    /// 尸体管理器：单例。维护全局尸体列表，提供注册/查询接口。
    /// </summary>
    public class CorpseManager : SingletonMono<CorpseManager>
    {
        /// <summary>当前场景中所有未处理的尸体。</summary>
        public List<Corpse> ActiveCorpses { get; } = new List<Corpse>();

        /// <summary>注册一具尸体。</summary>
        public void Register(Corpse corpse)
        {
            if (corpse == null || ActiveCorpses.Contains(corpse)) return;
            ActiveCorpses.Add(corpse);
        }

        /// <summary>注销一具尸体。</summary>
        public void Unregister(Corpse corpse)
        {
            if (corpse == null) return;
            ActiveCorpses.Remove(corpse);
        }

        /// <summary>
        /// 获取距离指定点最近的可见尸体。
        /// </summary>
        /// <param name="point">查询点。</param>
        /// <param name="maxDistance">最大距离。</param>
        /// <returns>最近的尸体，无则 null。</returns>
        public Corpse GetNearestVisible(Vector3 point, float maxDistance = 5f)
        {
            Corpse best = null;
            float bestSqr = maxDistance * maxDistance;
            foreach (var c in ActiveCorpses)
            {
                if (c == null || c.IsDisposed) continue;
                var tag = c.GetComponent<CorpseDetectionTag>();
                if (tag != null && !tag.IsVisible) continue;
                float sqr = (c.transform.position - point).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = c;
                }
            }
            return best;
        }

        /// <summary>
        /// 场景中是否存在任何尸体。
        /// </summary>
        public bool HasAny => ActiveCorpses.Count > 0;
    }
}
