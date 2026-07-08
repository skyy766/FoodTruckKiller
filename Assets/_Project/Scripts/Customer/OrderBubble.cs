using FoodTruckKiller.Cooking;
using UnityEngine;

namespace FoodTruckKiller.Customer
{
    /// <summary>
    /// 顾客头顶订单气泡：显示订单食谱图标/名称。
    /// </summary>
    public class OrderBubble : MonoBehaviour
    {
        /// <summary>气泡根节点（含 SpriteRenderer 或 UI）。</summary>
        public GameObject bubbleRoot;

        /// <summary>显示食谱名称的 TextMesh（可选）。</summary>
        public TextMesh label;

        /// <summary>当前显示的食谱。</summary>
        public RecipeData Current { get; private set; }

        private void Awake()
        {
            if (bubbleRoot != null) bubbleRoot.SetActive(false);
        }

        /// <summary>
        /// 显示指定食谱的气泡。
        /// </summary>
        public void Show(RecipeData recipe)
        {
            Current = recipe;
            if (bubbleRoot != null) bubbleRoot.SetActive(true);
            if (label != null && recipe != null) label.text = recipe.name;
        }

        /// <summary>
        /// 隐藏气泡。
        /// </summary>
        public void Hide()
        {
            if (bubbleRoot != null) bubbleRoot.SetActive(false);
        }
    }
}
