using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FoodTruckKiller.Cooking;

namespace FoodTruckKiller.Tests.EditMode
{
    /// <summary>
    /// RecipeData 数据匹配单元测试。
    /// 实际接口（ScriptableObject）：
    ///   - string id
    ///   - string name
    ///   - List&lt;string&gt; ingredients
    ///   - int price
    ///   - RecipeType type   （Normal / Bait / HumanMeat）
    ///   - float cookDuration = 5f
    ///   - bool Validate()   （id 非空且 ingredients 非空）
    /// 命名空间：FoodTruckKiller.Cooking
    /// </summary>
    [TestFixture]
    public class RecipeDataTests
    {
        /// <summary>测试食谱 A：经典汉堡 ["Bun", "Meat", "Lettuce"]。</summary>
        private RecipeData _burgerRecipe;

        /// <summary>测试食谱 B：鱼柳堡 ["Bun", "Fish", "Cheese"]。</summary>
        private RecipeData _fishBurgerRecipe;

        /// <summary>预期汉堡食材列表。</summary>
        private readonly List<string> _expectedBurger = new List<string> { "Bun", "Meat", "Lettuce" };

        /// <summary>预期鱼柳堡食材列表。</summary>
        private readonly List<string> _expectedFishBurger = new List<string> { "Bun", "Fish", "Cheese" };

        [SetUp]
        public void SetUp()
        {
            _burgerRecipe = ScriptableObject.CreateInstance<RecipeData>();
            _burgerRecipe.id = "recipe_burger";
            _burgerRecipe.name = "ClassicBurger";
            _burgerRecipe.ingredients = new List<string> { "Bun", "Meat", "Lettuce" };
            _burgerRecipe.price = 20;
            _burgerRecipe.type = RecipeType.Normal;
            _burgerRecipe.cookDuration = 5f;

            _fishBurgerRecipe = ScriptableObject.CreateInstance<RecipeData>();
            _fishBurgerRecipe.id = "recipe_fish";
            _fishBurgerRecipe.name = "FishBurger";
            _fishBurgerRecipe.ingredients = new List<string> { "Bun", "Fish", "Cheese" };
            _fishBurgerRecipe.price = 25;
            _fishBurgerRecipe.type = RecipeType.Normal;
            _fishBurgerRecipe.cookDuration = 6f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_burgerRecipe != null) Object.DestroyImmediate(_burgerRecipe);
            if (_fishBurgerRecipe != null) Object.DestroyImmediate(_fishBurgerRecipe);
        }

        /// <summary>
        /// 汉堡食谱食材列表应与预期完全匹配。
        /// </summary>
        [Test]
        public void BurgerRecipe_Ingredients_MatchExpected()
        {
            CollectionAssert.AreEqual(_expectedBurger, _burgerRecipe.ingredients,
                "汉堡食材列表应与预期完全匹配");
        }

        /// <summary>
        /// 鱼柳堡食谱食材列表应与预期完全匹配。
        /// </summary>
        [Test]
        public void FishBurgerRecipe_Ingredients_MatchExpected()
        {
            CollectionAssert.AreEqual(_expectedFishBurger, _fishBurgerRecipe.ingredients,
                "鱼柳堡食材列表应与预期完全匹配");
        }

        /// <summary>
        /// 两个不同食谱的食材列表不应相同。
        /// </summary>
        [Test]
        public void DifferentRecipes_HaveDifferentIngredients()
        {
            CollectionAssert.AreNotEqual(_burgerRecipe.ingredients, _fishBurgerRecipe.ingredients,
                "不同食谱的食材列表应不同");
        }

        /// <summary>
        /// 食谱食材列表不应为空。
        /// </summary>
        [Test]
        public void Recipe_Ingredients_NotEmpty()
        {
            Assert.IsNotEmpty(_burgerRecipe.ingredients, "汉堡食谱食材列表不应为空");
            Assert.IsNotEmpty(_fishBurgerRecipe.ingredients, "鱼柳堡食谱食材列表不应为空");
        }

        /// <summary>
        /// 食谱食材列表不应包含重复项。
        /// </summary>
        [Test]
        public void Recipe_Ingredients_NoDuplicates()
        {
            var distinct = new HashSet<string>(_burgerRecipe.ingredients);

            Assert.AreEqual(_burgerRecipe.ingredients.Count, distinct.Count,
                "食材列表不应包含重复项");
        }

        /// <summary>
        /// 食谱 id 与 name 应被正确赋值。
        /// </summary>
        [Test]
        public void Recipe_IdAndName_AreCorrect()
        {
            Assert.AreEqual("recipe_burger", _burgerRecipe.id);
            Assert.AreEqual("ClassicBurger", _burgerRecipe.name);
            Assert.AreEqual("recipe_fish", _fishBurgerRecipe.id);
            Assert.AreEqual("FishBurger", _fishBurgerRecipe.name);
        }

        /// <summary>
        /// 食谱应能正确识别包含某食材。
        /// </summary>
        [Test]
        public void Recipe_ContainsIngredient_ReturnsTrue()
        {
            bool containsMeat = _burgerRecipe.ingredients.Contains("Meat");

            Assert.IsTrue(containsMeat, "汉堡食谱应包含 Meat");
        }

        /// <summary>
        /// 食谱价格与类型字段应被正确赋值。
        /// </summary>
        [Test]
        public void Recipe_PriceAndType_AreCorrect()
        {
            Assert.AreEqual(20, _burgerRecipe.price);
            Assert.AreEqual(RecipeType.Normal, _burgerRecipe.type);
        }

        /// <summary>
        /// 完整食谱（id 非空、ingredients 非空）Validate() 应返回 true。
        /// </summary>
        [Test]
        public void Validate_CompleteRecipe_ReturnsTrue()
        {
            Assert.IsTrue(_burgerRecipe.Validate(), "完整食谱应通过校验");
        }

        /// <summary>
        /// 空 id 的食谱 Validate() 应返回 false。
        /// </summary>
        [Test]
        public void Validate_EmptyId_ReturnsFalse()
        {
            _burgerRecipe.id = "";

            Assert.IsFalse(_burgerRecipe.Validate(), "空 id 不应通过校验");
        }

        /// <summary>
        /// 空 ingredients 列表的食谱 Validate() 应返回 false。
        /// </summary>
        [Test]
        public void Validate_EmptyIngredients_ReturnsFalse()
        {
            _burgerRecipe.ingredients = new List<string>();

            Assert.IsFalse(_burgerRecipe.Validate(), "空食材列表不应通过校验");
        }
    }
}
