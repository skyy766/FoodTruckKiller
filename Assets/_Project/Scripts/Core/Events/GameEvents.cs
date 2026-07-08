using UnityEngine;

namespace FoodTruckKiller.Core.Events
{
    /// <summary>
    /// 静态事件聚合类：运行时统一创建并持有所有 <see cref="GameEvent"/> 实例，
    /// 供跨系统通信使用。
    /// <para>调用 <c>GameEvents.Init()</c> 在场景引导阶段完成实例化（幂等）。</para>
    /// <para>使用约定：</para>
    /// <list type="bullet">
    /// <item>发布：<c>GameEvents.OnXxx.Raise()</c></item>
    /// <item>订阅：<c>GameEvents.OnXxx.Register(Action)</c> / <c>Unregister(Action)</c></item>
    /// </list>
    /// <para>设计动机：沙箱无编辑器，无法通过 Inspector 注入 ScriptableObject 资产，
    /// 故所有事件实例在运行时由 <see cref="ScriptableObject.CreateInstance{T}()"/> 创建。</para>
    /// </summary>
    public static class GameEvents
    {
        // ---- 核心 7 个事件 ----

        /// <summary>订单出餐成功（普通顾客收款触发）。</summary>
        public static GameEvent OnOrderServed;

        /// <summary>任意顾客死亡（暗杀/误杀均触发）。</summary>
        public static GameEvent OnCustomerDied;

        /// <summary>暗杀目标被击杀（仅 Target 类型顾客死亡时触发）。</summary>
        public static GameEvent OnTargetKilled;

        /// <summary>通缉度变化或达到阈值（由 WantedSystem 触发）。</summary>
        public static GameEvent OnWanted;

        /// <summary>尸体被发现（由 CorpseDetectionTag 触发）。</summary>
        public static GameEvent OnCorpseFound;

        /// <summary>瞬时警报值变化（由 AlertSystem 触发）。</summary>
        public static GameEvent OnAlertChanged;

        /// <summary>白天结束（由 DayTimeController 倒计时归零触发，GameManager 监听结算）。</summary>
        public static GameEvent OnDayEnd;

        // ---- 音效细粒度事件（A5 AudioFeedbackBinder 订阅） ----

        /// <summary>新订单进入（顾客点单）。</summary>
        public static GameEvent OnOrderIn;

        /// <summary>出餐动作（与 OnOrderServed 同步或紧随）。</summary>
        public static GameEvent OnServe;

        /// <summary>金币入账反馈（EconomyManager 加钱后触发）。</summary>
        public static GameEvent OnCash;

        /// <summary>切菜音效（CookingStation.Chop 工作位）。</summary>
        public static GameEvent OnChop;

        /// <summary>煎烤滋滋声（CookingStation.Grill 工作位）。</summary>
        public static GameEvent OnSizzle;

        /// <summary>玩家脚步声（PlayerController 移动时触发）。</summary>
        public static GameEvent OnFootstep;

        /// <summary>绞肉机工作声（处理尸体时触发）。</summary>
        public static GameEvent OnGrind;

        /// <summary>顾客尖叫（目击血案/爆炸）。</summary>
        public static GameEvent OnScream;

        /// <summary>爆炸（煤气罐击杀方式触发）。</summary>
        public static GameEvent OnExplosion;

        /// <summary>是否已完成初始化。</summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// 创建所有 <see cref="GameEvent"/> 实例。幂等：重复调用不会重建已有实例。
        /// 应在 SceneBootstrapper.Awake 阶段最先调用。
        /// </summary>
        public static void Init()
        {
            if (IsInitialized) return;

            OnOrderServed = Create();
            OnCustomerDied = Create();
            OnTargetKilled = Create();
            OnWanted = Create();
            OnCorpseFound = Create();
            OnAlertChanged = Create();
            OnDayEnd = Create();

            OnOrderIn = Create();
            OnServe = Create();
            OnCash = Create();
            OnChop = Create();
            OnSizzle = Create();
            OnFootstep = Create();
            OnGrind = Create();
            OnScream = Create();
            OnExplosion = Create();

            IsInitialized = true;
        }

        private static GameEvent Create()
        {
            return ScriptableObject.CreateInstance<GameEvent>();
        }
    }
}
