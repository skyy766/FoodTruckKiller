using UnityEngine;

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

        /// <summary>拾取一个可携带物品。</summary>
        public void PickUp(ICarryable carryable)
        {
            if (carryable == null || IsCarrying) return;
            CurrentCarryable = carryable;
            CurrentCarryable.OnPickedUp(carryAnchor);
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
