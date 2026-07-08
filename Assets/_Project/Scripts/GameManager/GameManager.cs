using UnityEngine;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Economy;
using FoodTruckKiller.Cooking;
using FoodTruckKiller.Customer;

namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// 游戏全局管理器（单例）。负责游戏生命周期与全局状态流转，
    /// 持有各子系统引用（DayTimeController/ObjectiveTracker/EconomyManager/CookingController/CustomerSpawner）。
    /// <para>状态机以枚举 GameState 驱动，TransitionTo 负责进入/退出逻辑。</para>
    /// <para>事件订阅通过静态 <see cref="GameEvents"/> 聚合类，沙箱无编辑器亦可工作。</para>
    /// </summary>
    public class GameManager : SingletonMono<GameManager>
    {
        [Header("子系统引用（由 SceneBootstrapper 注入）")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private DayTimeController dayTimeController;
        [SerializeField] private ObjectiveTracker objectiveTracker;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private CustomerSpawner customerSpawner;
        [SerializeField] private CookingController cookingController;

        public LevelManager LevelManager => levelManager;
        public DayTimeController DayTimeController => dayTimeController;
        public ObjectiveTracker ObjectiveTracker => objectiveTracker;
        public EconomyManager EconomyManager => economyManager;
        public CustomerSpawner CustomerSpawner => customerSpawner;
        public CookingController CookingController => cookingController;

        /// <summary>由 SceneBootstrapper 注入子系统引用。</summary>
        public void AssignDayTimeController(DayTimeController value) => dayTimeController = value;

        /// <summary>由 SceneBootstrapper 注入子系统引用。</summary>
        public void AssignObjectiveTracker(ObjectiveTracker value) => objectiveTracker = value;

        /// <summary>由 SceneBootstrapper 注入子系统引用。</summary>
        public void AssignEconomyManager(EconomyManager value) => economyManager = value;

        /// <summary>由 SceneBootstrapper 注入子系统引用。</summary>
        public void AssignCustomerSpawner(CustomerSpawner value) => customerSpawner = value;

        /// <summary>由 SceneBootstrapper 注入子系统引用。</summary>
        public void AssignCookingController(CookingController value) => cookingController = value;

        /// <summary>当前游戏状态。</summary>
        public GameState CurrentState { get; private set; } = GameState.Boot;

        protected override void OnSingletonAwake()
        {
            // 监听白天结束（静态 GameEvents，沙箱无编辑器）。
            if (GameEvents.OnDayEnd != null)
                GameEvents.OnDayEnd.Register(HandleDayEnd);
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                if (dayTimeController != null)
                    dayTimeController.Tick(Time.deltaTime);
            }
        }

        /// <summary>
        /// 切换游戏状态，执行对应进入逻辑。
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            if (newState == CurrentState) return;
            Debug.Log($"[GameManager] {CurrentState} -> {newState}");
            CurrentState = newState;

            switch (newState)
            {
                case GameState.Boot:
                    BootGame();
                    break;
                case GameState.Playing:
                    StartDay();
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
                case GameState.Victory:
                    Time.timeScale = 0f;
                    break;
            }
        }

        private void BootGame()
        {
            Time.timeScale = 1f;
            levelManager?.Init();
            Debug.Log("[GameManager] Boot complete.");
        }

        /// <summary>
        /// 启动游戏：进入 Boot 完成初始化后切到 Playing。
        /// 由 SceneBootstrapper 在所有子系统就绪后调用。
        /// </summary>
        public void StartGame()
        {
            if (CurrentState == GameState.Boot)
                BootGame();
            TransitionTo(GameState.Playing);
        }

        /// <summary>开始一个白天周期（由 TransitionTo(Playing) 触发）。</summary>
        private void StartDay()
        {
            Time.timeScale = 1f;
            dayTimeController?.ApplyConfig();
            objectiveTracker?.ApplyConfig();
            dayTimeController?.StartDay();
            objectiveTracker?.Reset();
            Debug.Log("[GameManager] Day started.");
        }

        /// <summary>
        /// 白天结束结算入口（外部可调用，便于测试/调试）。
        /// </summary>
        public void EndDay()
        {
            HandleDayEnd();
        }

        /// <summary>白天结束结算：根据目标完成情况判定胜负。</summary>
        private void HandleDayEnd()
        {
            Debug.Log("[GameManager] Day end resolved.");
            if (objectiveTracker != null && objectiveTracker.AllTargetsCleared)
                TransitionTo(GameState.Victory);
            else
                TransitionTo(GameState.GameOver);
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            if (GameEvents.OnDayEnd != null)
                GameEvents.OnDayEnd.Unregister(HandleDayEnd);
        }
    }
}
