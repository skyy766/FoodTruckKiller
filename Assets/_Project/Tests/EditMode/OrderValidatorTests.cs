using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FoodTruckKiller.Cooking;

namespace FoodTruckKiller.Tests.EditMode
{
    /// <summary>
    /// OrderValidator.Validate() 单元测试。
    /// 实际接口：static bool Validate(List&lt;string&gt; assembled, RecipeData recipe)
    /// 命名空间：FoodTruckKiller.Cooking
    /// 行为：食材列表匹配（数量与内容匹配，顺序无关——内部用 FindIndex 消去匹配项）。
    /// </summary>
    [TestFixture]
    public class OrderValidatorTests
    {
        /// <summary>测试用食谱：经典汉堡 ["Bun", "Meat", "Lettuce"]。</summary>
        private RecipeData _testRecipe;

        [SetUp]
        public void SetUp()
        {
            _testRecipe = ScriptableObject.CreateInstance<RecipeData>();
            _testRecipe.id = "recipe_test";
            _testRecipe.name = "ClassicBurger";
            _testRecipe.ingredients = new List<string> { "Bun", "Meat", "Lettuce" };
            _testRecipe.price = 20;
            _testRecipe.type = RecipeType.Normal;
        }

        [TearDown]
        public void TearDown()
        {
            if (_testRecipe != null) Object.DestroyImmediate(_testRecipe);
        }

        /// <summary>
        /// 正确食材序列（顺序、数量完全匹配）应返回 true。
        /// </summary>
        [Test]
        public void Validate_CorrectSequence_ReturnsTrue()
        {
            var assembled = new List<string> { "Bun", "Meat", "Lettuce" };

            bool result = OrderValidator.Validate(assembled, _testRecipe);

            Assert.IsTrue(result, "完全匹配的食材序列应通过校验");
        }

        /// <summary>
        /// 错误食材序列（包含错误食材）应返回 false。
        /// </summary>
        [Test]
        public void Validate_WrongIngredients_ReturnsFalse()
        {
            var assembled = new List<string> { "Bun", "Fish", "Lettuce" };

            bool result = OrderValidator.Validate(assembled, _testRecipe);

            Assert.IsFalse(result, "包含错误食材的序列不应通过校验");
        }

        /// <summary>
        /// 空序列应返回 false（数量不匹配）。
        /// </summary>
        [Test]
        public void Validate_EmptySequence_ReturnsFalse()
        {
            var assembled = new List<string>();

            bool result = OrderValidator.Validate(assembled, _testRecipe);

            Assert.IsFalse(result, "空序列不应通过校验");
        }

        /// <summary>
        /// 食材相同但顺序不同应返回 true（OrderValidator 顺序无关）。
        /// </summary>
        [Test]
        public void Validate_DifferentOrder_SameIngredients_ReturnsTrue()
        {
            // 食谱要求 ["Bun", "Meat", "Lettuce"]，输入顺序颠倒
            var assembled = new List<string> { "Lettuce", "Meat", "Bun" };

            bool result = OrderValidator.Validate(assembled, _testRecipe);

            Assert.IsTrue(result, "OrderValidator 顺序无关，相同食材不同顺序应通过校验");
        }

        /// <summary>
        /// 食材数量不足应返回 false。
        /// </summary>
        [Test]
        public void Validate_InsufficientIngredients_ReturnsFalse()
        {
            var assembled = new List<string> { "Bun", "Meat" };

            bool result = OrderValidator.Validate(assembled, _testRecipe);

            Assert.IsFalse(result, "食材数量不足不应通过校验");
        }

        /// <summary>
        /// 食材数量多余应返回 false。
        /// </summary>
        [Test]
        public void Validate_ExtraIngredients_ReturnsFalse()
        {
            var assembled = new List<string> { "Bun", "Meat", "Lettuce", "Cheese" };

            bool result = OrderValidator.Validate(assembled, _testRecipe);

            Assert.IsFalse(result, "多余食材不应通过校验");
        }

        /// <summary>
        /// recipe 为 null 应返回 false（防御性编程）。
        /// </summary>
        [Test]
        public void Validate_NullRecipe_ReturnsFalse()
        {
            var assembled = new List<string> { "Bun", "Meat", "Lettuce" };

            bool result = OrderValidator.Validate(assembled, null);

            Assert.IsFalse(result, "recipe 为 null 时应返回 false");
        }

        /// <summary>
        /// assembled 为 null 应返回 false（防御性编程）。
        /// </summary>
        [Test]
        public void Validate_NullAssembled_ReturnsFalse()
        {
            bool result = OrderValidator.Validate(null, _testRecipe);

            Assert.IsFalse(result, "assembled 为 null 时应返回 false");
        }

        /// <summary>
        /// 重复食材场景：食谱含两个相同 id 时，组装也需提供两个，且不能以一个重复 id 误判。
        /// </summary>
        [Test]
        public void Validate_DuplicateIngredients_MatchedByCount()
        {
            // 构造含重复食材的食谱 ["Meat", "Meat", "Bun"]
            _testRecipe.ingredients = new List<string> { "Meat", "Meat", "Bun" };

            // 正确：两个 Meat + 一个 Bun
            var ok = new List<string> { "Meat", "Bun", "Meat" };
            Assert.IsTrue(OrderValidator.Validate(ok, _testRecipe), "含重复食材的食谱按数量匹配应通过");

            // 错误：只给一个 Meat + 两个 Bun（数量看似相等但内容不匹配）
            var bad = new List<string> { "Meat", "Bun", "Bun" };
            Assert.IsFalse(OrderValidator.Validate(bad, _testRecipe), "重复食材数量不匹配应失败");
        }
    }
}
