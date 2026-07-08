using UnityEngine;
using CorpseEntity = FoodTruckKiller.Corpse.Corpse;

namespace FoodTruckKiller.Player
{
    /// <summary>
    /// 可携带物品接口。尸体/食材/诱饵餐等实现此接口以支持拾取与放下。
    /// </summary>
    public interface ICarryable
    {
        /// <summary>被拾起时调用，参数为携带锚点 Transform。</summary>
        void OnPickedUp(Transform anchor);

        /// <summary>被放下时调用。</summary>
        void OnDropped();
    }

    /// <summary>
    /// 携带控制器：管理玩家当前携带的物品（尸体/食材/诱饵餐），
    /// 提供拾取（PickUp）与放下（Drop）能力。
    /// </summary>
    public class CarryController : MonoBehaviour
    {
        [Tooltip("携带物品挂载的锚点（玩家身体前方/头顶）")]
        [SerializeField] private Transform carryAnchor;

        /// <summary>当前携带的物品。</summary>
        public ICarryable CurrentCarryable { get; private set; }

        /// <summary>是否正携带物品。</summary>
        public bool IsCarrying => CurrentCarryable != null;

        /// <summary>拾取一个可携带物品。
        /// 原子操作: 先调 OnPickedUp 让物品自己处理 parent/状态, 仅当成功才设 CurrentCarryable。
        /// 如果物品拒绝拾取 (返回 false 或保持 IsCarried=false) 则放弃。</summary>
        public void PickUp(ICarryable carryable)
        {
            if (carryable == null || IsCarrying) return;
            bool wasCarried = IsCarriedBy(carryable);
            carryable.OnPickedUp(carryAnchor);
            // 验证物品真的被拾起了 (通过反射或约定接口)
            if (IsCarriedBy(carryable))
            {
                CurrentCarryable = carryable;
            }
            else
            {
                Debug.LogWarning($"[CarryController] 拾取失败, 物品未绑定 Carrier. wasCarried={wasCarried}");
            }
        }

        /// <summary>通过反射检查某个 ICarryable 是否被某物 carrier 持有 (Corpse.IsCarried)。</summary>
        private bool IsCarriedBy(ICarryable carryable)
        {
            if (carryable is CorpseEntity c) return c.IsCarried;
            // 其它实现: 假设 OnPickedUp 后 IsCarrying 由它自己管
            return true;
        }

        /// <summary>放下当前携带物品。</summary>
        public void Drop()
        {
            if (!IsCarrying) return;
            CurrentCarryable.OnDropped();
            CurrentCarryable = null;
        }
    }
}
