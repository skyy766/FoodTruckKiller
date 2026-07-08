using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Cooking;
using FoodTruckKiller.Economy;

namespace FoodTruckKiller.Tests.EditMode
{
    /// <summary>
    /// EconomyManager 与 CoverSystem 经济/伪装系统单元测试。
    /// 实际接口：
    ///   - EconomyManager : SingletonMono&lt;EconomyManager&gt;
    ///       - int Money { get; private set; }  （初始 = JsonDataLoader.Config.startMoney 或 startMoney=100）
    ///       - event Action&lt;int&gt; OnMoneyChanged
    ///       - List&lt;TransactionEvent&gt; Transactions
    ///       - void AddMoney(int amount, TransactionType type, Order order = null)
    ///       - bool SpendMoney(int amount, TransactionType type)
    ///   - CoverSystem : SingletonMono&lt;CoverSystem&gt;
    ///       - float Cover { get; private set; } = 100f  （0~100）
    ///       - event Action&lt;float&gt; OnCoverChanged
    ///       - void ReduceCover(float amount)
    ///       - void ApplyInspectFailPenalty()
    ///   - TransactionType { Income, Expense, MissionReward, Fine }
    /// 命名空间：FoodTruckKiller.Economy
    /// </summary>
    [TestFixture]
    public class EconomyTests
    {
        private EconomyManager _economy;
        private CoverSystem _cover;
        private GameObject _economyHost;
        private GameObject _coverHost;

        [SetUp]
        public void SetUp()
        {
            GameEvents.Init();

            ResetSingleton<EconomyManager>();
            ResetSingleton<CoverSystem>();

            _economyHost = new GameObject("EconomyManager_Test");
            _economy = _economyHost.AddComponent<EconomyManager>();

            _coverHost = new GameObject("CoverSystem_Test");
            _cover = _coverHost.AddComponent<CoverSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_economyHost != null) Object.DestroyImmediate(_economyHost);
            if (_coverHost != null) Object.DestroyImmediate(_coverHost);
            ResetSingleton<EconomyManager>();
            ResetSingleton<CoverSystem>();
        }

        #region EconomyManager 金钱增减

        /// <summary>
        /// 初始金钱应为默认起始值 100（EditMode 未加载 JsonDataLoader.Config，回退到 startMoney=100）。
        /// </summary>
        [Test]
        public void Money_InitialValue_IsDefaultStartMoney()
        {
            Assert.AreEqual(100, _economy.Money, "初始金钱应为默认 startMoney=100");
        }

        /// <summary>
        /// AddMoney 正值应正确增加金钱，并记录交易。
        /// </summary>
        [Test]
        public void AddMoney_PositiveAmount_IncreasesMoney()
        {
            _economy.AddMoney(100, TransactionType.Income);

            Assert.AreEqual(200, _economy.Money, "100 起始 + 100 收入应为 200");
            Assert.AreEqual(1, _economy.Transactions.Count, "应记录一笔交易");
            Assert.AreEqual(100, _economy.Transactions[0].amount);
            Assert.AreEqual(TransactionType.Income, _economy.Transactions[0].type);
        }

        /// <summary>
        /// 多次 AddMoney 应正确累积。
        /// </summary>
        [Test]
        public void AddMoney_MultipleTimes_AccumulatesCorrectly()
        {
            _economy.AddMoney(50, TransactionType.Income);
            _economy.AddMoney(30, TransactionType.Income);
            _economy.AddMoney(20, TransactionType.Income);

            Assert.AreEqual(200, _economy.Money, "100 + 50 + 30 + 20 应为 200");
            Assert.AreEqual(3, _economy.Transactions.Count);
        }

        /// <summary>
        /// SpendMoney 在金钱充足时应成功扣减并返回 true。
        /// </summary>
        [Test]
        public void Spend_EnoughMoney_DeductsAndReturnsTrue()
        {
            bool success = _economy.SpendMoney(40, TransactionType.Expense);

            Assert.IsTrue(success, "金钱充足时应返回 true");
            Assert.AreEqual(60, _economy.Money, "100 - 40 应剩余 60");
        }

        /// <summary>
        /// SpendMoney 在金钱不足时应失败并返回 false，金钱不变。
        /// </summary>
        [Test]
        public void Spend_InsufficientMoney_ReturnsFalseAndDoesNotChangeMoney()
        {
            bool success = _economy.SpendMoney(150, TransactionType.Expense);

            Assert.IsFalse(success, "金钱不足时应返回 false");
            Assert.AreEqual(100, _economy.Money, "金钱不足时金额不应改变");
        }

        /// <summary>
        /// SpendMoney 等于当前金钱（边界值）应成功。
        /// </summary>
        [Test]
        public void Spend_ExactlyEqual_CurrentMoney_Succeeds()
        {
            bool success = _economy.SpendMoney(100, TransactionType.Expense);

            Assert.IsTrue(success, "消费金额等于余额应成功");
            Assert.AreEqual(0, _economy.Money, "消费后余额应为 0");
        }

        /// <summary>
        /// AddMoney 传入 0 应被忽略（防御性编程：amount == 0 直接 return）。
        /// </summary>
        [Test]
        public void AddMoney_Zero_Ignored()
        {
            _economy.AddMoney(0, TransactionType.Income);

            Assert.AreEqual(100, _economy.Money, "零值不应改变金钱");
            Assert.AreEqual(0, _economy.Transactions.Count, "零值不应记录交易");
        }

        /// <summary>
        /// SpendMoney 失败时不记录交易。
        /// </summary>
        [Test]
        public void Spend_InsufficientMoney_DoesNotRecordTransaction()
        {
            int txBefore = _economy.Transactions.Count;

            bool success = _economy.SpendMoney(999, TransactionType.Expense);

            Assert.IsFalse(success);
            Assert.AreEqual(txBefore, _economy.Transactions.Count, "失败的消费不应记录交易");
        }

        /// <summary>
        /// 金钱变更应触发 OnMoneyChanged 事件，参数为新余额。
        /// </summary>
        [Test]
        public void AddMoney_RaisesOnMoneyChangedEvent()
        {
            int captured = -1;
            _economy.OnMoneyChanged += v => captured = v;

            _economy.AddMoney(50, TransactionType.Income);

            Assert.AreEqual(150, captured, "事件参数应为变更后余额");
        }

        /// <summary>
        /// AddMoney 可携带 Order 关联，交易记录中应保留引用。
        /// </summary>
        [Test]
        public void AddMoney_WithOrder_RecordsOrderReference()
        {
            var recipe = ScriptableObject.CreateInstance<RecipeData>();
            recipe.id = "r1";
            recipe.ingredients = new List<string> { "Bun" };
            recipe.price = 15;
            var order = new Order(recipe);

            try
            {
                _economy.AddMoney(15, TransactionType.Income, order);

                Assert.AreSame(order, _economy.Transactions[0].order, "交易记录应保留 Order 引用");
            }
            finally
            {
                Object.DestroyImmediate(recipe);
            }
        }

        #endregion

        #region CoverSystem 伪装度变化

        /// <summary>
        /// 初始伪装度应为 100（满值）。
        /// </summary>
        [Test]
        public void Cover_InitialValue_IsFull()
        {
            Assert.AreEqual(100f, _cover.Cover, "初始伪装度应为 100");
        }

        /// <summary>
        /// ReduceCover 正值应正确降低伪装度。
        /// </summary>
        [Test]
        public void ReduceCover_PositiveAmount_DecreasesCover()
        {
            _cover.ReduceCover(30f);

            Assert.AreEqual(70f, _cover.Cover, "降低 30 后伪装度应为 70");
        }

        /// <summary>
        /// 伪装度不应低于 0（下限夹紧）。
        /// </summary>
        [Test]
        public void ReduceCover_DoesNotGoBelowZero()
        {
            _cover.ReduceCover(150f); // 100-150=-50，应夹紧到 0

            Assert.GreaterOrEqual(_cover.Cover, 0f, "伪装度不应低于 0");
            Assert.AreEqual(0f, _cover.Cover, "伪装度应为 0");
        }

        /// <summary>
        /// 多次 ReduceCover 应正确累积。
        /// </summary>
        [Test]
        public void ReduceCover_MultipleTimes_Accumulates()
        {
            _cover.ReduceCover(40f);
            _cover.ReduceCover(30f);

            Assert.AreEqual(30f, _cover.Cover, "100 - 40 - 30 应为 30");
        }

        /// <summary>
        /// ApplyInspectFailPenalty 应按配置扣减伪装度（默认 inspectFailPenalty=30）。
        /// </summary>
        [Test]
        public void ApplyInspectFailPenalty_ReducesCover()
        {
            _cover.ApplyInspectFailPenalty();

            Assert.Less(_cover.Cover, 100f, "卫生检查失败应降低伪装度");
            Assert.AreEqual(70f, _cover.Cover, "默认 inspectFailPenalty=30，应剩余 70");
        }

        /// <summary>
        /// 伪装度变化应触发 OnCoverChanged 事件。
        /// </summary>
        [Test]
        public void ReduceCover_RaisesOnCoverChangedEvent()
        {
            float captured = -1f;
            _cover.OnCoverChanged += v => captured = v;

            _cover.ReduceCover(25f);

            Assert.AreEqual(75f, captured, "事件参数应为变更后伪装度");
        }

        #endregion

        // ---- 反射辅助 ----

        private static void ResetSingleton<T>() where T : MonoBehaviour
        {
            var fi = typeof(SingletonMono<T>).GetField("instance",
                BindingFlags.NonPublic | BindingFlags.Static);
            fi?.SetValue(null, null);
        }
    }
}
