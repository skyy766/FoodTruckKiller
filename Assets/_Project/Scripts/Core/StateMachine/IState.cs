namespace FoodTruckKiller.Core.StateMachine
{
    /// <summary>
    /// 状态机状态接口。所有状态（玩家/AI/顾客/警察）需实现此接口。
    /// <para>OnEnter：进入状态时调用（设置初始动画/参数）。</para>
    /// <para>OnUpdate：每帧调用（驱动行为逻辑）。</para>
    /// <para>OnExit：离开状态时调用（清理/重置）。</para>
    /// </summary>
    public interface IState
    {
        /// <summary>进入状态。</summary>
        void OnEnter();

        /// <summary>每帧更新。</summary>
        void OnUpdate();

        /// <summary>离开状态。</summary>
        void OnExit();
    }
}
