using System.Collections.Generic;

namespace FoodTruckKiller.Cooking
{
    /// <summary>
    /// 订单校验工具：判断玩家组装的食材序列是否符合食谱。
    /// </summary>
    public static class OrderValidator
    {
        /// <summary>
        /// 校验组装食材列表是否匹配食谱（数量与内容匹配，顺序无关）。
        /// </summary>
        /// <param name="assembled">玩家组装的食材 id 列表。</param>
        /// <param name="recipe">目标食谱。</param>
        /// <returns>匹配返回 true，否则 false。</returns>
        public static bool Validate(List<string> assembled, RecipeData recipe)
        {
            if (recipe == null || recipe.ingredients == null || assembled == null)
                return false;

            if (assembled.Count != recipe.ingredients.Count)
                return false;

            // 复制一份用于消去匹配项，避免重复 id 误判。
            List<string> required = new List<string>(recipe.ingredients);
            for (int i = 0; i < assembled.Count; i++)
            {
                string a = assembled[i];
                int idx = required.FindIndex(r => r == a);
                if (idx < 0)
                    return false;
                required.RemoveAt(idx);
            }
            return true;
        }
    }
}
