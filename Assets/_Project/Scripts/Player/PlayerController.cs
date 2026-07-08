using UnityEngine;
using UnityEngine.InputSystem;

namespace FoodTruckKiller.Player
{
    /// <summary>
    /// 玩家控制器：处理俯视角 8 方向移动输入，记录朝向，驱动 PlayerMotor。
    /// <para>输入采用 Input System（InputActionAsset），通过 Move action 读取 Vector2。</para>
    /// <para>朝向锁定到 8 方向，供 PlayerInteractor 检测与动画系统使用。</para>
    /// </summary>
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerController : MonoBehaviour
    {
        [Header("输入")]
        [Tooltip("Input System 资产，需包含 Move action（Vector2）")]
        [SerializeField] private InputActionAsset inputAsset;

        [Header("子系统")]
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private CarryController carryController;

        /// <summary>当前移动输入（已 8 方向锁定的原始向量）。</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>当前朝向（8 方向之一，默认向下）。</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        /// <summary>当前玩家状态。</summary>
        public PlayerState State { get; set; } = PlayerState.Idle;

        /// <summary>是否正在移动。</summary>
        public bool IsMoving => MoveInput.sqrMagnitude > 0.01f;

        private InputAction moveAction;

        public PlayerMotor Motor => motor;
        public PlayerInteractor Interactor => interactor;
        public CarryController Carry => carryController;

        private void Awake()
        {
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (interactor == null) interactor = GetComponent<PlayerInteractor>();
            if (carryController == null) carryController = GetComponent<CarryController>();

            if (inputAsset != null)
            {
                moveAction = inputAsset.FindAction("Move", throwIfNotFound: false);
            }
        }

        /// <summary>
        /// 重新解析子系统依赖。由 SceneBootstrapper 在所有组件添加完毕后调用，
        /// 解决 <see cref="AddComponent"/> 顺序导致的 Awake 时机问题。
        /// </summary>
        public void RebindDependencies()
        {
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (interactor == null) interactor = GetComponent<PlayerInteractor>();
            if (carryController == null) carryController = GetComponent<CarryController>();
        }

        private void OnEnable()
        {
            moveAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
        }

        private void Update()
        {
            // 读取移动输入（优先 InputAction，无资产时回退到旧版 Input.GetAxisRaw）
            Vector2 raw = ReadMoveInput();
            MoveInput = raw.magnitude > 0.01f ? SnapToEightDirection(raw) : Vector2.zero;

            // 更新朝向（仅在有输入时）
            if (MoveInput.sqrMagnitude > 0.01f)
            {
                Facing = MoveInput;
                if (State == PlayerState.Idle)
                    State = PlayerState.Move;
            }
            else if (State == PlayerState.Move)
            {
                State = PlayerState.Idle;
            }
        }

        private void FixedUpdate()
        {
            // 物理更新：调用 motor.Tick (MovePosition 需在 FixedUpdate)
            if (motor != null) motor.Tick(MoveInput);
        }

        private Vector2 ReadMoveInput()
        {
            if (moveAction != null)
                return moveAction.ReadValue<Vector2>();
            // 兜底：旧版输入（无 InputActionAsset 时也能用 WASD/方向键）
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            return new Vector2(x, y);
        }

        /// <summary>将任意方向向量吸附到最近的 8 方向。</summary>
        private Vector2 SnapToEightDirection(Vector2 input)
        {
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            float snapped = Mathf.Round(angle / 45f) * 45f;
            float rad = snapped * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        }
    }
}
