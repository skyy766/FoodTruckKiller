using UnityEngine;
using UnityEngine.InputSystem;
using FoodTruckKiller.Interaction;

namespace FoodTruckKiller.Player
{
    /// <summary>
    /// 玩家交互检测器：在朝向前方检测可交互物（Physics2D.OverlapCircle），
    /// 并在按下 Interact 时触发 IInteractable.OnInteract。
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("输入")]
        [Tooltip("Input System 资产，需包含 Interact action（Button）")]
        [SerializeField] private InputActionAsset inputAsset;

        [Header("检测")]
        [Tooltip("朝向锚点（留空则使用 transform + Facing 偏移）")]
        [SerializeField] private Transform facingAnchor;
        [Tooltip("检测半径（世界单位）")]
        [SerializeField] private float detectRadius = 0.5f;
        [Tooltip("可交互物所在 Layer")]
        [SerializeField] private LayerMask interactableMask;

        private PlayerController controller;
        private InputAction interactAction;
        private IInteractable currentTarget;

        /// <summary>当前检测到的可交互物（可空）。</summary>
        public IInteractable CurrentTarget => currentTarget;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            if (inputAsset != null)
                interactAction = inputAsset.FindAction("Interact", throwIfNotFound: false);
        }

        /// <summary>
        /// 重新解析依赖。由 SceneBootstrapper 在所有组件添加完毕后调用，
        /// 解决 <see cref="AddComponent"/> 顺序导致的 Awake 时机问题。
        /// </summary>
        public void RebindDependencies()
        {
            if (controller == null)
                controller = GetComponent<PlayerController>();
        }

        /// <summary>
        /// 运行时配置检测参数（沙箱无编辑器时由 SceneBootstrapper 调用）。
        /// </summary>
        /// <param name="mask">可交互物 LayerMask。</param>
        /// <param name="radius">检测半径（&lt;=0 表示保持默认）。</param>
        public void ConfigureDetection(LayerMask mask, float radius = -1f)
        {
            interactableMask = mask;
            if (radius > 0f) detectRadius = radius;
        }

        private void OnEnable()
        {
            interactAction?.Enable();
            if (interactAction != null)
                interactAction.performed += OnInteract;
        }

        private void OnDisable()
        {
            if (interactAction != null)
                interactAction.performed -= OnInteract;
            interactAction?.Disable();
        }

        private void Update()
        {
            currentTarget = DetectInteractable();

            // 兜底：旧版 Input（无 InputActionAsset 时也能用 E 键交互）
            if (interactAction == null && currentTarget != null
                && controller != null && Input.GetKeyDown(KeyCode.E))
            {
                currentTarget.OnInteract(controller);
            }
        }

        /// <summary>在朝向前方进行 OverlapCircle 检测。</summary>
        private IInteractable DetectInteractable()
        {
            Vector2 origin = GetDetectOrigin();
            Collider2D hit = Physics2D.OverlapCircle(origin, detectRadius, interactableMask);
            if (hit != null)
            {
                // 用非泛型重载以兼容接口类型
                return hit.GetComponentInParent(typeof(IInteractable)) as IInteractable;
            }
            return null;
        }

        private Vector2 GetDetectOrigin()
        {
            if (facingAnchor != null)
                return facingAnchor.position;
            Vector2 facing = controller != null ? controller.Facing : Vector2.down;
            return (Vector2)transform.position + facing * 0.5f;
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (currentTarget != null && controller != null)
            {
                currentTarget.OnInteract(controller);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetDetectOrigin(), detectRadius);
        }
    }
}
