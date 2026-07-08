using FoodTruckKiller.Core.StateMachine;
using FoodTruckKiller.Cooking;
using UnityEngine;

namespace FoodTruckKiller.Customer
{
    /// <summary>
    /// 顾客 AI：持有状态机与画像，驱动排队、点单、等待、进食、离开、逃跑、死亡等行为。
    /// <para>视觉由 <see cref="CustomerVisualController"/> 子组件负责（按 variant + 移动速度切 walk 帧）。</para>
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CustomerAI : MonoBehaviour
    {
        /// <summary>顾客画像。</summary>
        public CustomerProfile Profile { get; private set; }

        /// <summary>当前订单。</summary>
        public Order CurrentOrder { get; private set; }

        /// <summary>所在排队索引（-1 表示未排队）。</summary>
        public int QueueIndex { get; set; } = -1;

        /// <summary>状态机实例。</summary>
        public StateMachine<CustomerAI> StateMachine { get; private set; }

        /// <summary>移动速度（运行时取自 Profile）。</summary>
        public float MoveSpeed => Profile != null ? Profile.moveSpeed : 2f;

        /// <summary>顾客是否已被击杀。</summary>
        public bool IsDead { get; private set; }

        /// <summary>顾客是否正在被诱饵吸引。</summary>
        public bool IsBaited { get; set; }

        /// <summary>诱饵目标点（由 BaitSystem 设置）。</summary>
        public Vector3 BaitPoint { get; set; }

        /// <summary>顾客头顶气泡。</summary>
        private OrderBubble _bubble;

        /// <summary>刚体引用。</summary>
        private Rigidbody2D _rb;

        /// <summary>当前速度（供视觉组件判定是否在走）。</summary>
        public Vector2 CurrentVelocity { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            StateMachine = new StateMachine<CustomerAI>(this);
        }

        /// <summary>
        /// 初始化顾客画像与订单。
        /// </summary>
        /// <param name="profile">画像。</param>
        /// <param name="order">订单。</param>
        public void Initialize(CustomerProfile profile, Order order)
        {
            Profile = profile;
            CurrentOrder = order;
            _bubble = GetComponentInChildren<OrderBubble>();
            if (_bubble != null && order != null)
                _bubble.Show(order.Recipe);
        }

        /// <summary>
        /// 由状态机调用，分配/重置当前订单（用于 OrderingState 兜底创建订单）。
        /// </summary>
        public void AssignOrder(Order order)
        {
            CurrentOrder = order;
        }

        /// <summary>
        /// 切换到指定状态。
        /// </summary>
        public void ChangeState(IState newState)
        {
            StateMachine.Change(newState);
        }

        /// <summary>
        /// 标记为已死亡，切换到 DeadState。
        /// </summary>
        public void MarkDead()
        {
            IsDead = true;
            StateMachine.Change(new DeadState(this));
        }

        private void Update()
        {
            if (Profile == null) return;
            StateMachine.OnUpdate();
        }

        /// <summary>
        /// 向目标点移动（由状态调用）。
        /// </summary>
        /// <param name="target">目标点。</param>
        public void MoveTowards(Vector3 target)
        {
            if (_rb == null) return;
            Vector2 dir = (target - transform.position).normalized;
            Vector2 next = _rb.position + dir * MoveSpeed * Time.deltaTime;
            _rb.MovePosition(next);
            CurrentVelocity = dir * MoveSpeed;
        }

        /// <summary>
        /// 是否到达指定点。
        /// </summary>
        public bool HasReached(Vector3 target, float threshold = 0.1f)
        {
            return Vector3.Distance(transform.position, target) <= threshold;
        }
    }
}
