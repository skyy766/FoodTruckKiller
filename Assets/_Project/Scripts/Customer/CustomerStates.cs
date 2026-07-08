using FoodTruckKiller.Core.StateMachine;
using FoodTruckKiller.Cooking;
using UnityEngine;

namespace FoodTruckKiller.Customer
{
    /// <summary>
    /// 排队状态：前往队列分配的位置。
    /// </summary>
    public class QueuingState : IState
    {
        /// <summary>状态机持有者。</summary>
        private readonly CustomerAI _owner;

        /// <summary>构造。</summary>
        public QueuingState(CustomerAI owner) { _owner = owner; }

        /// <summary>进入状态。</summary>
        public void OnEnter() { }

        /// <summary>每帧更新：朝队列点移动。</summary>
        public void OnUpdate()
        {
            Vector3 target = QueueManager.Instance != null
                ? QueueManager.Instance.GetPosition(_owner.QueueIndex)
                : _owner.transform.position;

            if (_owner.HasReached(target))
            {
                _owner.ChangeState(new OrderingState(_owner));
                return;
            }
            _owner.MoveTowards(target);
        }

        /// <summary>离开状态。</summary>
        public void OnExit() { }
    }

    /// <summary>
    /// 点单状态：到达窗口后创建订单并进入等待。
    /// </summary>
    public class OrderingState : IState
    {
        private readonly CustomerAI _owner;

        public OrderingState(CustomerAI owner) { _owner = owner; }

        public void OnEnter()
        {
            // 订单已在 Initialize 时创建，这里仅兜底：若顾客无订单则随机生成。
            if (_owner.CurrentOrder == null && _owner.Profile != null)
            {
                RecipeData randomRecipe = CustomerSpawner.Instance != null
                    ? CustomerSpawner.Instance.GetRandomRecipe()
                    : null;
                if (randomRecipe != null)
                    _owner.AssignOrder(new Order(randomRecipe));
            }
        }

        public void OnUpdate()
        {
            // 缺少画像无法计算耐心，直接离开。
            if (_owner.Profile == null)
            {
                _owner.ChangeState(new LeavingState(_owner));
                return;
            }
            float patience = _owner.Profile.patienceSec > 0f ? _owner.Profile.patienceSec : 30f;
            _owner.ChangeState(new WaitingState(_owner, Time.time + patience));
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 等待状态：在窗口等待出餐，超时则离开。
    /// <para>进入时尝试将自身订单推送给 <see cref="CookingController"/> 作为当前待处理订单；
    /// 控制器空闲（无订单或已出餐）时接受推送，否则保持等待并在每帧重试。</para>
    /// </summary>
    public class WaitingState : IState
    {
        private readonly CustomerAI _owner;
        private readonly float _deadline;

        public WaitingState(CustomerAI owner, float deadline)
        {
            _owner = owner;
            _deadline = deadline;
        }

        public void OnEnter()
        {
            TryPushOrderToController();
        }

        public void OnUpdate()
        {
            // 被诱饵吸引时切换到诱饵移动逻辑（用 Fleeing 复用，前往 BaitPoint）。
            if (_owner.IsBaited)
            {
                _owner.ChangeState(new FleeingState(_owner, _owner.BaitPoint));
                return;
            }

            // 收到餐：订单已 Served → 进入进食。
            if (_owner.CurrentOrder != null && _owner.CurrentOrder.State == OrderState.Served)
            {
                _owner.ChangeState(new EatingState(_owner));
                return;
            }

            // 控制器空闲时重试推送（前一位顾客的订单刚出餐后接管）。
            TryPushOrderToController();

            // 超时离开。
            if (Time.time >= _deadline)
            {
                _owner.ChangeState(new LeavingState(_owner));
            }
        }

        public void OnExit() { }

        /// <summary>
        /// 当烹饪控制器空闲时把自身订单推送为当前订单。
        /// </summary>
        private void TryPushOrderToController()
        {
            var controller = CookingController.Instance;
            if (controller == null || _owner.CurrentOrder == null) return;
            var current = controller.CurrentOrder;
            // 仅当控制器无订单或上一单已出餐时推送，避免覆盖他人进行中的订单。
            if (current == null || current.State == OrderState.Served)
            {
                controller.SetCurrentOrder(_owner.CurrentOrder);
            }
        }
    }

    /// <summary>
    /// 进食状态：短暂进食后离开。
    /// </summary>
    public class EatingState : IState
    {
        private readonly CustomerAI _owner;
        private float _timer;

        public EatingState(CustomerAI owner)
        {
            _owner = owner;
            _timer = 5f;
        }

        public void OnEnter() { }

        public void OnUpdate()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                _owner.ChangeState(new LeavingState(_owner));
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 离开状态：前往离场点后销毁。
    /// </summary>
    public class LeavingState : IState
    {
        private readonly CustomerAI _owner;
        private readonly Vector3 _exitPoint;

        public LeavingState(CustomerAI owner)
        {
            _owner = owner;
            _exitPoint = CustomerSpawner.Instance != null
                ? CustomerSpawner.Instance.ExitPoint
                : owner.transform.position + Vector3.right * 10f;
        }

        public void OnEnter() { }

        public void OnUpdate()
        {
            if (_owner.HasReached(_exitPoint, 0.5f))
            {
                if (CustomerSpawner.Instance != null)
                    CustomerSpawner.Instance.NotifyCustomerLeft(_owner);
                Object.Destroy(_owner.gameObject);
                return;
            }
            _owner.MoveTowards(_exitPoint);
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 逃跑/前往指定点状态（被诱饵或受惊时使用）。
    /// </summary>
    public class FleeingState : IState
    {
        private readonly CustomerAI _owner;
        private readonly Vector3 _target;

        public FleeingState(CustomerAI owner, Vector3 target)
        {
            _owner = owner;
            _target = target;
        }

        public void OnEnter() { }

        public void OnUpdate()
        {
            if (_owner.HasReached(_target, 0.2f))
            {
                // 到达诱饵点后保持等待击杀窗口（这里直接停留，由 KillExecutor 处理）。
                return;
            }
            _owner.MoveTowards(_target);
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 死亡状态：停止一切行为，等待尸体系统接管。
    /// </summary>
    public class DeadState : IState
    {
        private readonly CustomerAI _owner;

        public DeadState(CustomerAI owner) { _owner = owner; }

        public void OnEnter()
        {
            // 禁用移动等组件。
            if (_owner == null) return;
            var rb = _owner.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
        }

        public void OnUpdate() { }

        public void OnExit() { }
    }
}
