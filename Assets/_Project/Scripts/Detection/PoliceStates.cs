using FoodTruckKiller.Core.StateMachine;
using UnityEngine;

namespace FoodTruckKiller.Detection
{
    /// <summary>
    /// 巡逻状态：沿 waypoint 循环移动。
    /// </summary>
    public class PatrolState : IState
    {
        private readonly PoliceAI _owner;
        private int _index;

        public PatrolState(PoliceAI owner) { _owner = owner; }

        public void OnEnter() { }

        public void OnUpdate()
        {
            // 看到玩家则切换追击。
            if (_owner.vision != null && _owner.vision.CurrentTarget != null)
            {
                _owner.ChangeState(new ChaseState(_owner, _owner.vision.CurrentTarget));
                return;
            }

            if (_owner.patrolWaypoints == null || _owner.patrolWaypoints.Length == 0)
                return;

            Transform wp = _owner.patrolWaypoints[_index];
            if (_owner.HasReached(wp.position, 0.2f))
            {
                _index = (_index + 1) % _owner.patrolWaypoints.Length;
                return;
            }
            _owner.MoveTowards(wp.position);
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 怀疑状态：发现异常但未确认，原地观察。
    /// </summary>
    public class SuspiciousState : IState
    {
        private readonly PoliceAI _owner;
        private float _timer;

        public SuspiciousState(PoliceAI owner, float duration = 3f)
        {
            _owner = owner;
            _timer = duration;
        }

        public void OnEnter() { }

        public void OnUpdate()
        {
            if (_owner.vision != null && _owner.vision.CurrentTarget != null)
            {
                _owner.ChangeState(new ChaseState(_owner, _owner.vision.CurrentTarget));
                return;
            }
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                _owner.ChangeState(new PatrolState(_owner));
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 调查状态：前往可疑点。
    /// </summary>
    public class InvestigateState : IState
    {
        private readonly PoliceAI _owner;
        private readonly Vector3 _point;

        public InvestigateState(PoliceAI owner, Vector3 point)
        {
            _owner = owner;
            _point = point;
        }

        public void OnEnter() { }

        public void OnUpdate()
        {
            if (_owner.vision != null && _owner.vision.CurrentTarget != null)
            {
                _owner.ChangeState(new ChaseState(_owner, _owner.vision.CurrentTarget));
                return;
            }
            if (_owner.HasReached(_point, 0.3f))
            {
                _owner.ChangeState(new SuspiciousState(_owner, 2f));
                return;
            }
            _owner.MoveTowards(_point);
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 追击状态：追击玩家目标。
    /// </summary>
    public class ChaseState : IState
    {
        private readonly PoliceAI _owner;
        private readonly Transform _target;

        public ChaseState(PoliceAI owner, Transform target)
        {
            _owner = owner;
            _target = target;
        }

        public void OnEnter() { }

        public void OnUpdate()
        {
            if (_target == null)
            {
                _owner.ChangeState(new PatrolState(_owner));
                return;
            }

            if (_owner.vision != null && !_owner.vision.CanSee(_target))
            {
                // 失去视野，前往最后位置调查。
                _owner.ChangeState(new InvestigateState(_owner, _target.position));
                return;
            }

            if (_owner.HasReached(_target.position, 0.6f))
            {
                _owner.Arrest();
                return;
            }
            _owner.MoveTowards(_target.position);
        }

        public void OnExit() { }
    }

    /// <summary>
    /// 逮捕状态：触发 GameOver。
    /// </summary>
    public class ArrestState : IState
    {
        private readonly PoliceAI _owner;

        public ArrestState(PoliceAI owner) { _owner = owner; }

        public void OnEnter()
        {
            // 通过 GameManager 单例触发 GameOver（避免直接依赖 UI）。
            // 此处仅停留，实际 GameOver 由上层订阅 OnWanted 或 Arrest 事件处理。
        }

        public void OnUpdate() { }

        public void OnExit() { }
    }
}
