using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Customer;
using FoodTruckKiller.Inventory;
using UnityEngine;

namespace FoodTruckKiller.Assassination
{
    /// <summary>
    /// 诱饵系统：投放诱饵餐以吸引目标到死角。
    /// 目标 AI 切换到 Baited 状态前往诱饵点。
    /// </summary>
    public class BaitSystem : SingletonMono<BaitSystem>
    {
        /// <summary>诱饵投放点（死角）。</summary>
        public Transform baitDropPoint;

        /// <summary>诱饵餐食材 id（用于从背包消耗）。</summary>
        public string baitIngredientId = "bait_meal";

        /// <summary>投放诱饵（消耗背包内的诱饵餐）。</summary>
        /// <returns>是否投放成功。</returns>
        public bool DropBait()
        {
            var inv = InventorySystem.Instance;
            if (inv == null || !inv.Has(baitIngredientId))
                return false;

            inv.Remove(baitIngredientId);
            // 通知当前目标前往诱饵点。
            AttractActiveTarget();
            return true;
        }

        /// <summary>
        /// 让当前激活的目标顾客前往诱饵点。
        /// </summary>
        private void AttractActiveTarget()
        {
            if (baitDropPoint == null) return;
            // 遍历在场顾客寻找目标类型。
            var spawner = CustomerSpawner.Instance;
            if (spawner == null) return;

            // 通过画像类型为 Target 的顾客视为目标。
            foreach (var ai in FindActiveTargets())
            {
                ai.IsBaited = true;
                ai.BaitPoint = baitDropPoint.position;
            }
        }

        /// <summary>
        /// 查找所有目标类型顾客（占位实现，避免跨系统依赖）。
        /// </summary>
        private System.Collections.Generic.List<CustomerAI> FindActiveTargets()
        {
            var result = new System.Collections.Generic.List<CustomerAI>();
            var all = FindObjectsOfType<CustomerAI>();
            foreach (var ai in all)
            {
                if (ai.Profile != null && ai.Profile.type == CustomerType.Target)
                    result.Add(ai);
            }
            return result;
        }
    }
}
