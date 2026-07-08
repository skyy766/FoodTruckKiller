using UnityEngine;

namespace FoodTruckKiller.Corpse
{
    /// <summary>
    /// 尸体检测标签：标记尸体是否在视野内可见。
    /// 由 VisionSensor 或其他检测系统通过 SetVisible 切换状态。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CorpseDetectionTag : MonoBehaviour
    {
        /// <summary>当前是否处于可被发现状态。</summary>
        public bool IsVisible { get; private set; } = true;

        /// <summary>最近一次进入视野的时间。</summary>
        public float LastSpottedTime { get; private set; } = -1f;

        /// <summary>
        /// 设置可见性。
        /// </summary>
        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            if (visible)
                LastSpottedTime = Time.time;
        }

        /// <summary>
        /// 由检测系统在视野命中时调用。
        /// </summary>
        public void NotifySpotted()
        {
            LastSpottedTime = Time.time;
            IsVisible = true;
        }
    }
}
