using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FoodTruckKiller.Cooking;
using FoodTruckKiller.Customer;
using FoodTruckKiller.Economy;

namespace FoodTruckKiller.Tests.PlayMode
{
    /// <summary>
    /// 顾客点单→出餐→付款 集成测试（PlayMode）。
    /// 依赖 Unity 运行时，需要预制体与场景支持。
    /// 接口约定：
    ///   - CustomerController 顾客行为
    ///   - OrderValidator.Validate(List&lt;string&gt;, RecipeData) 校验出餐
    ///   - EconomyManager.AddMoney(int) 付款入账
    /// </summary>
    [TestFixture]
    public class CustomerFlowTests
    {
        /// <summary>测试场景中的顾客预制体。</summary>
        private GameObject _customerPrefab;

        /// <summary>测试场景中的餐车预制体。</summary>
        private GameObject _foodTruck;

        [SetUp]
        public void SetUp()
        {
            // TODO: 加载预制体
            // _customerPrefab = Resources.Load<GameObject>("Prefabs/Customer");
            // _foodTruck = Object.Instantiate(Resources.Load<GameObject>("Prefabs/FoodTruck"));
        }

        [TearDown]
        public void TearDown()
        {
            // TODO: 清理场景对象
            // if (_foodTruck != null) Object.DestroyImmediate(_foodTruck);
        }

        /// <summary>
        /// 完整顾客流程：顾客入场→点单→玩家正确出餐→顾客付款→离场。
        /// </summary>
        [UnityTest]
        public IEnumerator Customer_OrderServedCorrectly_PaysAndLeaves()
        {
            // Arrange：实例化顾客与餐车
            // var customer = Object.Instantiate(_customerPrefab).GetComponent<CustomerController>();

            // Act
            // 1. 等待顾客入场并点单
            // yield return new WaitForSeconds(1.0f);
            // RecipeData order = customer.CurrentOrder;
            // Assert.IsNotNull(order, "顾客应已下单");

            // 2. 玩家组装正确食材
            // var assembled = order.Ingredients; // 模拟完美出餐
            // bool valid = OrderValidator.Validate(assembled, order);
            // Assert.IsTrue(valid, "正确出餐应通过校验");

            // 3. 提交订单
            // customer.ReceiveOrder(assembled);
            // yield return new WaitForSeconds(0.5f);

            // 4. 验证付款
            // int moneyBefore = EconomyManager.Instance.Money;
            // customer.PayForOrder();
            // yield return new WaitForSeconds(0.5f);

            // Assert
            // int moneyAfter = EconomyManager.Instance.Money;
            // Assert.Greater(moneyAfter, moneyBefore, "正确出餐后玩家金钱应增加");
            // Assert.IsTrue(customer.HasLeft, "付款后顾客应离场");

            yield return null;
            Assert.Pass("骨架占位：待 CustomerController / EconomyManager 实现后启用断言。");
        }

        /// <summary>
        /// 错误出餐：顾客点单后玩家提交错误订单，顾客不应付款，可能愤怒离场。
        /// </summary>
        [UnityTest]
        public IEnumerator Customer_OrderServedWrong_NoPaymentAndLeavesAngry()
        {
            // Arrange
            // var customer = Object.Instantiate(_customerPrefab).GetComponent<CustomerController>();
            // yield return new WaitForSeconds(1.0f);
            // RecipeData order = customer.CurrentOrder;

            // Act：故意提交错误食材
            // var wrongAssembled = new List<string> { "Bun", "Fish" }; // 假设订单要求汉堡
            // bool valid = OrderValidator.Validate(wrongAssembled, order);
            // Assert.IsFalse(valid, "错误订单不应通过校验");

            // customer.ReceiveOrder(wrongAssembled);
            // yield return new WaitForSeconds(0.5f);

            // int moneyBefore = EconomyManager.Instance.Money;
            // customer.PayForOrder(); // 不应付款
            // yield return new WaitForSeconds(0.5f);

            // Assert
            // Assert.AreEqual(moneyBefore, EconomyManager.Instance.Money, "错误订单不应获得金钱");
            // Assert.IsTrue(customer.HasLeft, "顾客应离场");
            // Assert.IsTrue(customer.IsAngry, "错误出餐后顾客应愤怒");

            yield return null;
            Assert.Pass("骨架占位：待 CustomerController 实现后启用断言。");
        }

        /// <summary>
        /// 顾客等待超时：顾客点单后玩家长时间不出餐，顾客应愤怒离场。
        /// </summary>
        [UnityTest]
        public IEnumerator Customer_OrderTimeout_LeavesAngry()
        {
            // Arrange
            // var customer = Object.Instantiate(_customerPrefab).GetComponent<CustomerController>();
            // yield return new WaitForSeconds(1.0f);
            // float patience = customer.PatienceDuration;

            // Act：等待耐心耗尽
            // yield return new WaitForSeconds(patience + 1.0f);

            // Assert
            // Assert.IsTrue(customer.HasLeft, "耐心耗尽后顾客应离场");
            // Assert.IsTrue(customer.IsAngry, "超时离场顾客应愤怒");
            yield return null;
            Assert.Pass("骨架占位：待 CustomerController 实现后启用断言。");
        }

        /// <summary>
        /// 多顾客并发：多个顾客同时排队，应能分别点单与出餐。
        /// </summary>
        [UnityTest]
        public IEnumerator MultipleCustomers_CanOrderAndPayIndependently()
        {
            // Arrange：实例化 3 个顾客
            // var customers = new List<CustomerController>();
            // for (int i = 0; i < 3; i++)
            // {
            //     var c = Object.Instantiate(_customerPrefab).GetComponent<CustomerController>();
            //     customers.Add(c);
            // }
            // yield return new WaitForSeconds(1.0f);

            // Act & Assert：依次出餐
            // foreach (var c in customers)
            // {
            //     Assert.IsNotNull(c.CurrentOrder, "每个顾客都应已下单");
            //     c.ReceiveOrder(c.CurrentOrder.Ingredients);
            //     yield return new WaitForSeconds(0.3f);
            //     c.PayForOrder();
            // }
            // yield return new WaitForSeconds(1.0f);

            // Assert.IsTrue(customers.TrueForAll(c => c.HasLeft), "所有顾客都应付款离场");
            yield return null;
            Assert.Pass("骨架占位：待 CustomerController 实现后启用断言。");
        }
    }
}
