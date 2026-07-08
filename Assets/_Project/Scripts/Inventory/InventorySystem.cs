using System.Collections.Generic;
using FoodTruckKiller.Core.Singleton;
using UnityEngine;

namespace FoodTruckKiller.Inventory
{
    /// <summary>
    /// 简易物品栏：管理食材/诱饵餐/人肉等物品的添加、移除、查询。
    /// </summary>
    public class InventorySystem : SingletonMono<InventorySystem>
    {
        /// <summary>物品 id -> 数量。</summary>
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();

        /// <summary>容量上限（-1 表示无限）。</summary>
        [SerializeField] private int capacity = -1;

        /// <summary>
        /// 添加物品。
        /// </summary>
        /// <param name="itemId">物品 id。</param>
        /// <param name="count">数量。</param>
        public void Add(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0) return;
            // 容量检查（capacity = -1 表示无限）
            if (capacity >= 0)
            {
                int total = 0;
                foreach (var v in _items.Values) total += v;
                if (total + count > capacity) return;
            }
            if (!_items.ContainsKey(itemId)) _items[itemId] = 0;
            _items[itemId] += count;
        }

        /// <summary>
        /// 移除物品，数量不足返回 false。
        /// </summary>
        public bool Remove(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0) return false;
            if (!_items.ContainsKey(itemId) || _items[itemId] < count)
                return false;
            _items[itemId] -= count;
            if (_items[itemId] <= 0)
                _items.Remove(itemId);
            return true;
        }

        /// <summary>
        /// 是否拥有指定物品（至少 1 个）。
        /// </summary>
        public bool Has(string itemId)
        {
            return _items.ContainsKey(itemId) && _items[itemId] > 0;
        }

        /// <summary>
        /// 获取指定物品数量。
        /// </summary>
        public int GetCount(string itemId)
        {
            return _items.TryGetValue(itemId, out int v) ? v : 0;
        }

        /// <summary>
        /// 获取所有物品 id 快照。
        /// </summary>
        public List<string> GetAllItemIds()
        {
            return new List<string>(_items.Keys);
        }

        /// <summary>
        /// 清空物品栏。
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }
    }
}
