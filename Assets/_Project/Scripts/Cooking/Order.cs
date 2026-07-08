namespace FoodTruckKiller.Cooking
{
    /// <summary>
    /// 顾客订单状态：等待制作 / 制作完成 / 已出餐。
    /// </summary>
    public enum OrderState
    {
        /// <summary>已下单等待制作。</summary>
        Pending,
        /// <summary>制作完成等待出餐。</summary>
        Ready,
        /// <summary>已交付顾客。</summary>
        Served
    }

    /// <summary>
    /// 顾客订单：持有一份食谱与当前状态，由烹饪系统驱动状态流转。
    /// </summary>
    public class Order
    {
        /// <summary>关联的食谱数据。</summary>
        public RecipeData Recipe { get; private set; }

        /// <summary>当前订单状态。</summary>
        public OrderState State { get; set; }

        /// <summary>下单时间戳（Time.time）。</summary>
        public float CreatedTime { get; private set; }

        /// <summary>构造一个新订单。</summary>
        /// <param name="recipe">关联食谱。</param>
        public Order(RecipeData recipe)
        {
            Recipe = recipe;
            State = OrderState.Pending;
            CreatedTime = UnityEngine.Time.time;
        }

        /// <summary>标记订单为已完成制作。</summary>
        public void MarkReady()
        {
            if (State == OrderState.Pending)
                State = OrderState.Ready;
        }

        /// <summary>标记订单为已出餐。</summary>
        public void MarkServed()
        {
            State = OrderState.Served;
        }
    }
}
