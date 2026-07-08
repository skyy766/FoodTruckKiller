using UnityEngine;
using UnityEngine.InputSystem;
using FoodTruckKiller.Assassination;
using FoodTruckKiller.Customer;
using FoodTruckKiller.Interaction;
// 注意: 不直接 using FoodTruckKiller.Corpse, 因为 namespace 和类同名
// 使用类型时全限定或用 alias
using CorpseEntity = FoodTruckKiller.Corpse.Corpse;
using DisposalStation = FoodTruckKiller.Corpse.DisposalStation;

namespace FoodTruckKiller.Player
{
    /// <summary>
    /// 玩家交互检测器：在朝向前方检测可交互物（Physics2D.OverlapCircle），
    /// 并在按下 Interact 时触发 IInteractable.OnInteract。
    /// <para>额外支持攻击输入（F 键）：检测前方目标类型顾客并执行近战击杀。</para>
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

        [Header("攻击")]
        [Tooltip("近战攻击距离（世界单位）")]
        [SerializeField] private float attackRange = 0.8f;
        [Tooltip("近战击杀方式数据（运行时从 KillMethods 获取，id=knife）")]
        public KillMethodData meleeKillMethod;

        private PlayerController controller;
        private CarryController carry;
        private KillExecutor killExecutor;
        private InputAction interactAction;
        private IInteractable currentTarget;

        /// <summary>当前检测到的可交互物（可空）。</summary>
        public IInteractable CurrentTarget => currentTarget;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            carry = GetComponent<CarryController>();
            killExecutor = GetComponent<KillExecutor>();
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
            if (carry == null)
                carry = GetComponent<CarryController>();
            if (killExecutor == null)
                killExecutor = GetComponent<KillExecutor>();
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
                interactAction.performed += OnInteractInput;
        }

        private void OnDisable()
        {
            if (interactAction != null)
                interactAction.performed -= OnInteractInput;
            interactAction?.Disable();
        }

        private void Update()
        {
            currentTarget = DetectInteractable();

            // 兜底：旧版 Input（无 InputActionAsset 时也能用 E 键交互）
            if (interactAction == null && currentTarget != null
                && controller != null && Input.GetKeyDown(KeyCode.E))
            {
                HandleInteract();
            }

            // 攻击输入：F 键执行近战击杀
            if (controller != null && Input.GetKeyDown(KeyCode.F))
            {
                HandleAttack();
            }

            // 放下尸体：Q 键放下当前搬运的尸体
            if (carry != null && carry.IsCarrying && Input.GetKeyDown(KeyCode.Q))
            {
                carry.Drop();
            }
        }

        /// <summary>处理交互（E 键）。</summary>
        private void HandleInteract()
        {
            if (currentTarget == null || controller == null) return;

            // 如果正在搬运尸体且目标是处理站，走 DisposalStation
            if (carry != null && carry.IsCarrying && currentTarget is DisposalStation disposal)
            {
                disposal.TryDispose(carry);
                return;
            }

            // 通用交互（尸体拾取/放下、烹饪台、环境击杀等）
            currentTarget.OnInteract(controller);

            // 如果交互结果是拾起了尸体，同步到 CarryController
            if (currentTarget is CorpseEntity corpse && corpse.IsCarried && carry != null && !carry.IsCarrying)
            {
                carry.PickUp(corpse);
            }
        }

        /// <summary>处理攻击（F 键）：在朝向前方检测顾客并执行近战击杀。
        /// M2 测试阶段：允许击杀任何顾客（不限 Target 类型），方便测试暗杀+尸体流程。</summary>
        private void HandleAttack()
        {
            if (killExecutor == null)
            {
                Debug.LogWarning("[PlayerInteractor] F pressed but KillExecutor is null");
                return;
            }

            // 在攻击范围内查找顾客（不限 Layer，直接 OverlapCircleAll）
            Vector2 origin = GetDetectOrigin();
            var hits = Physics2D.OverlapCircleAll(origin, attackRange);
            Debug.Log($"[PlayerInteractor] F pressed at {origin}, range={attackRange}, hits={hits.Length}");
            foreach (var hit in hits)
            {
                var ai = hit.GetComponent<CustomerAI>();
                if (ai == null) ai = hit.GetComponentInParent<CustomerAI>();
                if (ai == null || ai.IsDead) continue;

                // 击杀任何顾客（M2 测试阶段，不限 Target）
                KillMethodData method = meleeKillMethod;
                if (method == null)
                {
                    var methods = Core.DataLoader.JsonDataLoader.KillMethods;
                    if (methods != null)
                        method = methods.Find(m => m.id == "knife");
                }
                Debug.Log($"[PlayerInteractor] Killing customer {ai.name} type={ai.Profile?.type}");
                killExecutor.Execute(ai, method ?? ScriptableObject.CreateInstance<KillMethodData>());
                return; // 每次攻击只击杀一个
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

        private void OnInteractInput(InputAction.CallbackContext ctx)
        {
            HandleInteract();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetDetectOrigin(), detectRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetDetectOrigin(), attackRange);
        }
    }
}
