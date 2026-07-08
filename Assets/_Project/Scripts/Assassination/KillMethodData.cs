using UnityEngine;

namespace FoodTruckKiller.Assassination
{
    /// <summary>
    /// 证据类型枚举。
    /// </summary>
    public enum EvidenceType
    {
        /// <summary>无证据。</summary>
        None,
        /// <summary>血迹。</summary>
        Blood,
        /// <summary>爆炸痕迹。</summary>
        Explosion,
        /// <summary>尸体残骸。</summary>
        Remains
    }

    /// <summary>
    /// 击杀方式 ScriptableObject：定义名称、伤害、噪声半径与遗留证据类型。
    /// </summary>
    [CreateAssetMenu(fileName = "KillMethod_", menuName = "FoodTruckKiller/Assassination/KillMethod")]
    public class KillMethodData : ScriptableObject
    {
        /// <summary>击杀方式唯一标识（对齐 JSON <c>id</c>，如 "knife"/"gas_tank"）。</summary>
        public string id;

        /// <summary>击杀方式名称。</summary>
        public string killName;

        /// <summary>伤害值（用于击杀判定）。</summary>
        public float damage = 100f;

        /// <summary>噪声半径（越大越易被听到）。</summary>
        public float noiseRadius = 5f;

        /// <summary>遗留证据类型。</summary>
        public EvidenceType evidenceType = EvidenceType.Blood;

        /// <summary>是否为环境击杀（用于统计）。</summary>
        public bool isEnvironmental = false;
    }
}
