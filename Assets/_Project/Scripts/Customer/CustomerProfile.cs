using UnityEngine;

namespace FoodTruckKiller.Customer
{
    /// <summary>
    /// 顾客类型：普通顾客 / 线人 / 暗杀目标。
    /// </summary>
    public enum CustomerType
    {
        /// <summary>普通顾客。</summary>
        Normal,
        /// <summary>线人（可能向警方提供情报）。</summary>
        Informant,
        /// <summary>暗杀目标。</summary>
        Target
    }

    /// <summary>
    /// 顾客画像 ScriptableObject：定义类型、生成概率、耐心、消费区间。
    /// </summary>
    [CreateAssetMenu(fileName = "Customer_", menuName = "FoodTruckKiller/Customer/Profile")]
    public class CustomerProfile : ScriptableObject
    {
        /// <summary>顾客类型唯一标识（对齐 JSON <c>id</c>，如 "normal"/"informant"/"target"）。</summary>
        public string id;

        /// <summary>顾客类型。</summary>
        public CustomerType type = CustomerType.Normal;

        /// <summary>相对生成权重/概率（0~1）。</summary>
        [Range(0f, 1f)] public float probability = 1f;

        /// <summary>耐心时间（秒），超时离开。</summary>
        public float patienceSec = 30f;

        /// <summary>最低消费金额。</summary>
        public int paymentMin = 10;

        /// <summary>最高消费金额。</summary>
        public int paymentMax = 30;

        /// <summary>移动速度。</summary>
        public float moveSpeed = 2f;

        /// <summary>该顾客类型偏好的食谱 id（普通顾客可为空，目标必填）。</summary>
        public string preferredRecipeId;
    }
}
