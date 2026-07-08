using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodTruckKiller.Cooking
{
    /// <summary>
    /// 食谱类型枚举：普通餐 / 诱饵餐 / 人肉餐。
    /// </summary>
    public enum RecipeType
    {
        /// <summary>普通售卖餐品。</summary>
        Normal,
        /// <summary>用于诱杀目标的诱饵餐。</summary>
        Bait,
        /// <summary>含人肉的非法餐品。</summary>
        HumanMeat
    }

    /// <summary>
    /// 食谱 ScriptableObject：定义一份餐品所需的食材、售价与类型。
    /// </summary>
    [CreateAssetMenu(fileName = "Recipe_", menuName = "FoodTruckKiller/Cooking/Recipe")]
    public class RecipeData : ScriptableObject
    {
        /// <summary>食谱唯一标识。</summary>
        public string id;

        /// <summary>食谱显示名称。</summary>
        public new string name;

        /// <summary>所需食材 id 列表（顺序无关或按组装规则匹配）。</summary>
        public List<string> ingredients = new List<string>();

        /// <summary>售价。</summary>
        public int price;

        /// <summary>食谱类型。</summary>
        public RecipeType type = RecipeType.Normal;

        /// <summary>预期烹饪总时长（秒），用于烹饪台计时。</summary>
        public float cookDuration = 5f;

        /// <summary>校验自身字段完整性（编辑器/运行时皆可调用）。</summary>
        public virtual bool Validate()
        {
            return !string.IsNullOrEmpty(id) && ingredients != null && ingredients.Count > 0;
        }
    }
}
