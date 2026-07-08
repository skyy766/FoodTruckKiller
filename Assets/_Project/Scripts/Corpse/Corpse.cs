using FoodTruckKiller.Assassination;
using FoodTruckKiller.Interaction;
using FoodTruckKiller.Player;
using UnityEngine;

namespace FoodTruckKiller.Corpse
{
    /// <summary>
    /// 尸体实体：可被搬运、被发现。实现 IInteractable 供玩家拾取，实现 ICarryable 供 CarryController 管理。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Corpse : MonoBehaviour, IInteractable, ICarryable
    {
        /// <summary>遗留证据类型。</summary>
        public EvidenceType Evidence { get; private set; } = EvidenceType.Blood;

        /// <summary>是否正被搬运。</summary>
        public bool IsCarried { get; private set; }

        /// <summary>是否已被处理（消失）。</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>搬运者（玩家）。</summary>
        public Transform Carrier { get; private set; }

        /// <summary>交互提示。</summary>
        [SerializeField] private string promptName = "搬运尸体";

        /// <summary>尸体检测标签（运行时启用/禁用可见性）。</summary>
        private CorpseDetectionTag _detectionTag;

        private void Awake()
        {
            _detectionTag = GetComponent<CorpseDetectionTag>();
            // 自动向全局管理器注册。
            if (CorpseManager.Instance != null)
                CorpseManager.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (CorpseManager.Instance != null)
                CorpseManager.Instance.Unregister(this);
        }

        /// <summary>
        /// 初始化尸体（设置证据类型）。
        /// </summary>
        public void Initialize(EvidenceType evidence)
        {
            Evidence = evidence;
        }

        /// <summary>
        /// 玩家交互：拾起或放下。
        /// </summary>
        public void OnInteract(PlayerController player)
        {
            if (IsDisposed) return;
            if (!IsCarried)
                PickUp(player != null ? player.transform : null);
            else
                Drop();
        }

        /// <summary>
        /// 返回交互提示。
        /// </summary>
        public string GetPromptName()
        {
            return promptName;
        }

        /// <summary>
        /// 拾起尸体。
        /// </summary>
        public void PickUp(Transform carrier)
        {
            if (IsCarried || carrier == null) return;
            Carrier = carrier;
            IsCarried = true;
            transform.SetParent(carrier);
            transform.localPosition = Vector3.zero;
            // 搬运时不可被视野检测发现（折叠到玩家身上）。
            if (_detectionTag != null) _detectionTag.SetVisible(false);
        }

        /// <summary>
        /// 放下尸体。
        /// </summary>
        public void Drop()
        {
            if (!IsCarried) return;
            transform.SetParent(null);
            IsCarried = false;
            Carrier = null;
            if (_detectionTag != null) _detectionTag.SetVisible(true);
        }

        /// <summary>
        /// 标记为已处理（处理站调用）。
        /// </summary>
        public void MarkDisposed()
        {
            IsDisposed = true;
            if (CorpseManager.Instance != null)
                CorpseManager.Instance.Unregister(this);
            Destroy(gameObject);
        }

        // ---- ICarryable 实现 ----

        /// <summary>
        /// 被 CarryController 拾起时调用。
        /// </summary>
        void ICarryable.OnPickedUp(Transform anchor)
        {
            PickUp(anchor);
        }

        /// <summary>
        /// 被 CarryController 放下时调用。
        /// </summary>
        void ICarryable.OnDropped()
        {
            Drop();
        }
    }
}
