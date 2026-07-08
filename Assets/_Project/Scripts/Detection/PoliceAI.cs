using FoodTruckKiller.Core.StateMachine;
using UnityEngine;

namespace FoodTruckKiller.Detection
{
    /// <summary>
    /// 警察 AI：FSM 持有者，挂载在警察预制体上。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PoliceAI : MonoBehaviour
    {
        /// <summary>视野传感器。</summary>
        public VisionSensor vision;

        /// <summary>移动速度。</summary>
        public float moveSpeed = 3f;

        /// <summary>巡逻 waypoint 数组。</summary>
        public Transform[] patrolWaypoints;

        /// <summary>状态机。</summary>
        public StateMachine<PoliceAI> StateMachine { get; private set; }

        /// <summary>当前可疑点（由 InvestateState 前往）。</summary>
        public Vector3 SuspiciousPoint { get; set; }

        /// <summary>是否已逮捕玩家。</summary>
        public bool HasArrested { get; private set; }

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            StateMachine = new StateMachine<PoliceAI>(this);
            if (vision == null) vision = GetComponent<VisionSensor>();
        }

        private void Start()
        {
            StateMachine.Change(new PatrolState(this));
        }

        private void Update()
        {
            StateMachine.OnUpdate();
        }

        /// <summary>
        /// 朝目标移动。
        /// </summary>
        public void MoveTowards(Vector3 target)
        {
            if (_rb == null) return;
            Vector2 dir = ((Vector2)target - _rb.position).normalized;
            _rb.MovePosition(_rb.position + dir * moveSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 是否到达。
        /// </summary>
        public bool HasReached(Vector3 target, float threshold = 0.1f)
        {
            return Vector3.Distance(transform.position, target) <= threshold;
        }

        /// <summary>
        /// 切换状态。
        /// </summary>
        public void ChangeState(IState state)
        {
            StateMachine.Change(state);
        }

        /// <summary>
        /// 触发逮捕。
        /// </summary>
        public void Arrest()
        {
            HasArrested = true;
            StateMachine.Change(new ArrestState(this));
        }
    }
}
