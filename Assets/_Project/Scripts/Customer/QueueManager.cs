using System.Collections.Generic;
using FoodTruckKiller.Core.Singleton;
using UnityEngine;

namespace FoodTruckKiller.Customer
{
    /// <summary>
    /// 排队管理器：管理排队锚点数组，分配与释放位置。
    /// <para>沙箱无编辑器时 <see cref="queueAnchors"/> 可为空，
    /// 此时 <see cref="GetPosition"/> 返回基于自身位置的默认线性排队点。</para>
    /// </summary>
    public class QueueManager : SingletonMono<QueueManager>
    {
        /// <summary>排队锚点数组（0 号为窗口首位）。</summary>
        public Transform[] queueAnchors;

        /// <summary>沙箱默认排队点间距（世界单位）。</summary>
        [SerializeField] private float defaultSlotSpacing = 1f;

        /// <summary>沙箱默认排队方向（顾客面朝窗口的方向）。</summary>
        [SerializeField] private Vector2 defaultLineDirection = Vector2.down;

        /// <summary>已占用的槽位（索引 -> 顾客）。</summary>
        private readonly Dictionary<int, CustomerAI> _occupied = new Dictionary<int, CustomerAI>();

        /// <summary>
        /// 分配一个空闲槽位给顾客。
        /// </summary>
        /// <returns>槽位索引，-1 表示无可用。</returns>
        public int AssignSlot(CustomerAI customer)
        {
            // 沙箱无锚点时使用动态扩展模式：从 0 起递增分配。
            if (queueAnchors == null || queueAnchors.Length == 0)
            {
                int idx = 0;
                while (_occupied.ContainsKey(idx)) idx++;
                _occupied[idx] = customer;
                return idx;
            }

            for (int i = 0; i < queueAnchors.Length; i++)
            {
                if (!_occupied.ContainsKey(i))
                {
                    _occupied[i] = customer;
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 释放指定槽位。
        /// </summary>
        public void ReleaseSlot(int index)
        {
            if (index < 0) return;
            _occupied.Remove(index);
        }

        /// <summary>
        /// 获取指定槽位的世界坐标。
        /// 沙箱无锚点时返回基于自身位置 + 索引 * 间距 的默认点。
        /// </summary>
        public Vector3 GetPosition(int index)
        {
            if (queueAnchors != null && index >= 0 && index < queueAnchors.Length)
                return queueAnchors[index].position;

            // 沙箱默认：自身位置沿排队方向延伸。
            if (index < 0) index = 0;
            Vector3 basePos = transform.position;
            Vector3 dir = ((Vector3)defaultLineDirection).normalized;
            return basePos + dir * (index * defaultSlotSpacing);
        }

        /// <summary>
        /// 当前队列长度。
        /// </summary>
        public int Count => _occupied.Count;
    }
}
