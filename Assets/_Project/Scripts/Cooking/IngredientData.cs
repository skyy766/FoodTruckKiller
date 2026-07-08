using UnityEngine;

namespace FoodTruckKiller.Cooking
{
    /// <summary>
    /// 食材 ScriptableObject：定义食材唯一标识、显示名与是否非法。
    /// <para>非法食材（人肉/毒药）一旦被卫生检查员或警察发现将大幅提升通缉度。</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Ingredient_", menuName = "FoodTruckKiller/Cooking/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        /// <summary>食材唯一标识（对齐 JSON <c>id</c>）。</summary>
        public string id;

        /// <summary>显示名称（对齐 JSON <c>name</c>）。</summary>
        public string displayName;

        /// <summary>是否为非法食材。</summary>
        public bool isIllegal;
    }
}
