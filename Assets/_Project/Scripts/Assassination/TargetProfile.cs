using UnityEngine;

namespace FoodTruckKiller.Assassination
{
    /// <summary>
    /// 暗杀目标画像 ScriptableObject：定义目标偏好食谱、弱点、奖励与诱饵食谱。
    /// </summary>
    [CreateAssetMenu(fileName = "Target_", menuName = "FoodTruckKiller/Assassination/Target")]
    public class TargetProfile : ScriptableObject
    {
        /// <summary>目标唯一标识（对齐 JSON <c>id</c>，如 "t_tony"）。</summary>
        public string id;

        /// <summary>目标名称。</summary>
        public string targetName;

        /// <summary>目标偏好的食谱 id（普通餐）。</summary>
        public string favRecipe;

        /// <summary>诱饵食谱 id（用于吸引目标）。</summary>
        public string baitRecipe;

        /// <summary>弱点描述（例如「煤气罐旁」「夜晚落单」）。</summary>
        public string weakpoint;

        /// <summary>击杀奖励金额。</summary>
        public int reward = 500;

        /// <summary>任务描述。</summary>
        [TextArea] public string description;
    }
}
