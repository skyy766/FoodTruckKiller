using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Cooking;
using FoodTruckKiller.Economy;
using FoodTruckKiller.GameManager;

namespace FoodTruckKiller.Tests.PlayMode
{
    /// <summary>
    /// M1 经营循环集成测试（PlayMode）。
    /// 通过 GameEvents 静态事件聚合类与各 SingletonMono 子系统协作，
    /// 验证 顾客点单 → 出餐 → 加钱 的完整事件链路与结算流程。
    /// 实际接口：
    ///   - GameEvents.OnOrderServed / OnOrderIn / OnDayEnd（GameEvent.Raise / Register(Action)）
    ///   - CookingController.SetCurrentOrder / EnterWorkstation / TryAssemble / Assembling
    ///   - EconomyManager.Money / Transactions / AddMoney(int, TransactionType, Order)
    ///   - DayTimeController.Tick / StartDay
    ///   - GameManager.StartGame / EndDay / CurrentState
    /// </summary>
    [TestFixture]
    public class BusinessLoopTests
    {
        private readonly List<GameObject> _createdGos = new List<GameObject>();
        private readonly List<RecipeData> _createdRecipes = new List<RecipeData>();

        [SetUp]
        public void SetUp()
        {
            GameEvents.Init();
            _createdGos.Clear();
            _createdRecipes.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var recipe in _createdRecipes)
            {
                if (recipe != null) Object.DestroyImmediate(recipe);
            }
            foreach (var go in _createdGos)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _createdGos.Clear();
            _createdRecipes.Clear();

            ResetSingleton<EconomyManager>();
            ResetSingleton<CookingController>();
            ResetSingleton<GameManager>();

            // GameManager 未重写 OnDisable 反订阅 OnDayEnd（仅在 OnApplicationQuit 中反订阅），
            // 测试中不会触发 OnApplicationQuit，故用反射清除 OnDayEnd 订阅者，避免跨测试残留。
            ClearEventSubscribers(GameEvents.OnDayEnd);

            Time.timeScale = 1f;
        }

        /// <summary>
        /// 顾客点单 → 玩家正确出餐 → EconomyManager 加钱 完整流程。
        /// 链路：SetCurrentOrder(OnOrderIn) → EnterWorkstation(Assemble)(AutoFill+TryAssemble→MarkReady)
        ///       → EnterWorkstation(Serve)(TryServe→MarkServed→Raise OnOrderServed)
        ///       → EconomyManager.HandleOrderServed → AddMoney(price, Income, order)
        /// </summary>
        [UnityTest]
        public IEnumerator OrderServedCorrectly_AddsMoney()
        {
            // Arrange
            var economy = CreateSingleton<EconomyManager>();
            var controller = CreateSingleton<CookingController>();
            int moneyBefore = economy.Money;

            var recipe = CreateRecipe("r_burger", 20);
            var order = new Order(recipe);

            // Act：模拟顾客点单推送订单到烹饪控制器
            controller.SetCurrentOrder(order);
            Assert.AreEqual(OrderState.Pending, order.State);

            // 玩家进入组装台：M1 联调版自动从食谱填充食材并校验 → MarkReady
            var assembleStation = CreateCookingStation(CookingWorkstation.Assemble);
            controller.EnterWorkstation(assembleStation);
            Assert.AreEqual(OrderState.Ready, order.State, "正确组装后订单应 Ready");

            // 玩家进入出餐台：TryServe → MarkServed → Raise OnOrderServed → EconomyManager 加钱
            var serveStation = CreateCookingStation(CookingWorkstation.Serve);
            controller.EnterWorkstation(serveStation);

            // 等一帧让事件回调完成
            yield return null;

            // Assert
            Assert.AreEqual(OrderState.Served, order.State, "订单应已出餐");
            Assert.AreEqual(moneyBefore + 20, economy.Money, "应按食谱售价加钱");
            Assert.AreEqual(1, economy.Transactions.Count, "应记录一笔收入交易");
            Assert.AreEqual(TransactionType.Income, economy.Transactions[0].type);
            Assert.AreSame(order, economy.Transactions[0].order, "交易应关联订单");
        }

        /// <summary>
        /// 错误订单不出餐、不加钱。
        /// 组装错误食材 → TryAssemble 失败 → State 仍 Pending → 出餐台 TryServe 不触发 OnOrderServed。
        /// </summary>
        [UnityTest]
        public IEnumerator WrongOrder_NoMoneyAdded()
        {
            // Arrange
            var economy = CreateSingleton<EconomyManager>();
            var controller = CreateSingleton<CookingController>();
            int moneyBefore = economy.Money;

            var recipe = CreateRecipe("r_burger", 20);
            var order = new Order(recipe);
            controller.SetCurrentOrder(order);

            // Act：玩家组装错误食材（食谱要求 Bun/Meat/Lettuce，这里给 Bun/Fish/Lettuce）
            controller.Assembling.Clear();
            controller.AddIngredient("Bun");
            controller.AddIngredient("Fish");
            controller.AddIngredient("Lettuce");
            controller.TryAssemble();

            // 订单未 Ready，尝试出餐不应触发 OnOrderServed
            var serveStation = CreateCookingStation(CookingWorkstation.Serve);
            controller.EnterWorkstation(serveStation);

            yield return null;

            // Assert
            Assert.AreNotEqual(OrderState.Served, order.State, "错误订单不应进入 Served");
            Assert.AreNotEqual(OrderState.Ready, order.State, "错误组装不应进入 Ready");
            Assert.AreEqual(moneyBefore, economy.Money, "错误订单不应加钱");
            Assert.AreEqual(0, economy.Transactions.Count, "不应记录任何交易");
        }

        /// <summary>
        /// 多顾客连续服务：两单依次出餐，金钱应累计。
        /// 验证 CookingController 单例在不同订单间正确切换，EconomyManager 累计加钱。
        /// </summary>
        [UnityTest]
        public IEnumerator MultipleOrders_ServedSequentially_AccumulatesMoney()
        {
            // Arrange
            var economy = CreateSingleton<EconomyManager>();
            var controller = CreateSingleton<CookingController>();
            int moneyBefore = economy.Money;

            var r1 = CreateRecipe("r1", 20);
            var r2 = CreateRecipe("r2", 25);

            // Act：第一单
            var o1 = new Order(r1);
            controller.SetCurrentOrder(o1);
            controller.EnterWorkstation(CreateCookingStation(CookingWorkstation.Assemble));
            controller.EnterWorkstation(CreateCookingStation(CookingWorkstation.Serve));

            // 第二单（覆盖 CurrentOrder，模拟下一位顾客点单推送）
            var o2 = new Order(r2);
            controller.SetCurrentOrder(o2);
            controller.EnterWorkstation(CreateCookingStation(CookingWorkstation.Assemble));
            controller.EnterWorkstation(CreateCookingStation(CookingWorkstation.Serve));

            yield return null;

            // Assert
            Assert.AreEqual(OrderState.Served, o1.State, "第一单应已出餐");
            Assert.AreEqual(OrderState.Served, o2.State, "第二单应已出餐");
            Assert.AreEqual(moneyBefore + 20 + 25, economy.Money, "两单应累计加钱 20+25");
            Assert.AreEqual(2, economy.Transactions.Count, "应记录两笔交易");
        }

        /// <summary>
        /// 时间到结算：DayTimeController.Tick 推进时间至归零 → Raise OnDayEnd
        /// → GameManager.HandleDayEnd → 未达成暗杀目标 → GameState.GameOver。
        /// </summary>
        [UnityTest]
        public IEnumerator DayTimeEnd_TriggersSettlement_GameOverWhenObjectiveNotCleared()
        {
            // Arrange
            var gameManager = CreateSingleton<GameManager>();

            var dayGo = new GameObject("DayTime_Test");
            _createdGos.Add(dayGo);
            var dayTime = dayGo.AddComponent<DayTimeController>();
            SetPrivateField(dayTime, "dayDuration", 0.5f);

            var objGo = new GameObject("Objective_Test");
            _createdGos.Add(objGo);
            var tracker = objGo.AddComponent<ObjectiveTracker>();

            gameManager.AssignDayTimeController(dayTime);
            gameManager.AssignObjectiveTracker(tracker);

            // Act：进入 Playing（StartDay 会调用 dayTime.StartDay 设置 RemainingTime=0.5）
            gameManager.StartGame();
            Assert.AreEqual(GameState.Playing, gameManager.CurrentState, "应进入 Playing 状态");

            // 推进时间超过剩余时间 → 触发 OnDayEnd → GameManager 结算
            dayTime.Tick(1f);

            yield return null;

            // Assert：未击杀任何目标（KilledTargets=0 < totalTargets=1）应判失败
            Assert.AreEqual(GameState.GameOver, gameManager.CurrentState,
                "未达成目标时时间到应进入 GameOver");
        }

        /// <summary>
        /// 时间到结算（胜利路径）：已击杀全部目标 → Raise OnTargetKilled → KilledTargets 达标
        /// → 时间到 OnDayEnd → GameManager 判定 Victory。
        /// </summary>
        [UnityTest]
        public IEnumerator DayTimeEnd_TriggersSettlement_VictoryWhenObjectiveCleared()
        {
            // Arrange
            var gameManager = CreateSingleton<GameManager>();

            var dayGo = new GameObject("DayTime_Test");
            _createdGos.Add(dayGo);
            var dayTime = dayGo.AddComponent<DayTimeController>();
            SetPrivateField(dayTime, "dayDuration", 0.5f);

            var objGo = new GameObject("Objective_Test");
            _createdGos.Add(objGo);
            var tracker = objGo.AddComponent<ObjectiveTracker>();
            // 默认 totalTargets=1，击杀 1 个即达标

            gameManager.AssignDayTimeController(dayTime);
            gameManager.AssignObjectiveTracker(tracker);

            // Act：进入 Playing（StartDay 内会 Reset 进度，需在 StartGame 之后再触发击杀）
            gameManager.StartGame();
            Assert.AreEqual(GameState.Playing, gameManager.CurrentState);

            // 模拟暗杀目标被击杀（ObjectiveTracker 监听 OnTargetKilled，StartDay.Reset 之后触发）
            GameEvents.OnTargetKilled.Raise();
            Assert.AreEqual(1, tracker.KilledTargets, "应记录 1 个目标被击杀");
            Assert.IsTrue(tracker.AllTargetsCleared, "应已全部清除");

            // 时间到
            dayTime.Tick(1f);

            yield return null;

            // Assert
            Assert.AreEqual(GameState.Victory, gameManager.CurrentState,
                "达成全部目标且时间到应进入 Victory");
        }

        // ---- 辅助方法 ----

        private T CreateSingleton<T>() where T : MonoBehaviour
        {
            ResetSingleton<T>();
            var go = new GameObject(typeof(T).Name + "_Test");
            _createdGos.Add(go);
            return go.AddComponent<T>();
        }

        private CookingStation CreateCookingStation(CookingWorkstation ws)
        {
            var go = new GameObject("Station_" + ws);
            _createdGos.Add(go);
            var station = go.AddComponent<CookingStation>();
            station.workstation = ws;
            return station;
        }

        private RecipeData CreateRecipe(string id, int price)
        {
            var r = ScriptableObject.CreateInstance<RecipeData>();
            r.id = id;
            r.name = id;
            r.ingredients = new List<string> { "Bun", "Meat", "Lettuce" };
            r.price = price;
            r.type = RecipeType.Normal;
            r.cookDuration = 5f;
            _createdRecipes.Add(r);
            return r;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var fi = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            fi?.SetValue(target, value);
        }

        private static void ResetSingleton<T>() where T : MonoBehaviour
        {
            var fi = typeof(SingletonMono<T>).GetField("instance",
                BindingFlags.NonPublic | BindingFlags.Static);
            fi?.SetValue(null, null);
        }

        /// <summary>
        /// 反射清除 GameEvent 的 onRaised 委托列表（用于 GameManager 等未在 OnDisable 反订阅的场景）。
        /// 遍历所有 Action 类型私有字段以兼容不同编译器生成的事件 backing field 命名。
        /// </summary>
        private static void ClearEventSubscribers(GameEvent evt)
        {
            if (evt == null) return;
            var fields = typeof(GameEvent).GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fi in fields)
            {
                if (fi.FieldType == typeof(Action) || fi.FieldType == typeof(System.MulticastDelegate))
                    fi.SetValue(evt, null);
            }
        }
    }
}
