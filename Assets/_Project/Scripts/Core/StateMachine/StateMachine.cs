namespace FoodTruckKiller.Core.StateMachine
{
    /// <summary>
    /// 泛型有限状态机。
    /// <para>T 为上下文类型（如 AI 拥有者），由状态机持有并暴露给状态使用。</para>
    /// <para>顾客/警察 AI 通过 StateMachine&lt;T&gt; 管理行为状态切换。</para>
    /// </summary>
    /// <typeparam name="T">上下文/拥有者类型。</typeparam>
    public class StateMachine<T>
    {
        /// <summary>状态机上下文，供状态访问拥有者数据。</summary>
        private readonly T context;

        /// <summary>当前状态。</summary>
        public IState CurrentState { get; private set; }

        /// <summary>上下文实例。</summary>
        public T Context => context;

        /// <param name="context">传入拥有者作为上下文。</param>
        public StateMachine(T context)
        {
            this.context = context;
        }

        /// <summary>
        /// 切换状态：先 OnExit 旧状态，再 OnEnter 新状态。
        /// </summary>
        public void Change(IState newState)
        {
            if (newState == null) return;
            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }

        /// <summary>
        /// 每帧驱动当前状态的 OnUpdate（由拥有者 MonoBehaviour.Update 调用）。
        /// </summary>
        public void OnUpdate()
        {
            CurrentState?.OnUpdate();
        }
    }
}
