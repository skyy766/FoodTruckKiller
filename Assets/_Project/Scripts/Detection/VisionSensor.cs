using UnityEngine;

namespace FoodTruckKiller.Detection
{
    /// <summary>
    /// 扇形视野传感器：基于 Physics2D.OverlapCircle + 角度判断 + 射线遮挡。
    /// 暴露 CanSee(Transform) 接口供 AI 使用。
    /// </summary>
    public class VisionSensor : MonoBehaviour
    {
        /// <summary>视野中心朝向（通常为 transform.up）。</summary>
        public Vector2 facing => transform.up;

        /// <summary>视野半径。</summary>
        public float viewRadius = 6f;

        /// <summary>视野半角（度）。</summary>
        [Range(1f, 180f)] public float viewHalfAngle = 45f;

        /// <summary>视线遮挡层（障碍物）。</summary>
        public LayerMask obstructionMask;

        /// <summary>目标层（玩家/尸体）。</summary>
        public LayerMask targetMask;

        /// <summary>当前可见目标（最近一个）。</summary>
        public Transform CurrentTarget { get; private set; }

        private void Update()
        {
            CurrentTarget = ScanForTarget();
        }

        /// <summary>
        /// 判断是否能看见指定目标。
        /// </summary>
        public bool CanSee(Transform target)
        {
            if (target == null) return false;
            Vector2 origin = transform.position;
            Vector2 toTarget = (Vector2)target.position - origin;

            if (toTarget.sqrMagnitude > viewRadius * viewRadius)
                return false;

            float angle = Vector2.Angle(facing, toTarget.normalized);
            if (angle > viewHalfAngle)
                return false;

            // 射线遮挡检测。
            float dist = toTarget.magnitude;
            RaycastHit2D hit = Physics2D.Raycast(origin, toTarget.normalized, dist, obstructionMask | targetMask);
            if (hit.collider == null) return false;
            // 命中目标层且为目标本身。
            return hit.transform == target;
        }

        /// <summary>
        /// 扫描视野内的目标。
        /// </summary>
        private Transform ScanForTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);
            Transform best = null;
            float bestSqr = float.MaxValue;
            foreach (var h in hits)
            {
                if (h == null) continue;
                if (CanSee(h.transform))
                {
                    float sqr = (h.transform.position - transform.position).sqrMagnitude;
                    if (sqr < bestSqr)
                    {
                        bestSqr = sqr;
                        best = h.transform;
                    }
                }
            }
            return best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewRadius);
            Vector3 a = Quaternion.Euler(0, 0, viewHalfAngle) * facing * viewRadius;
            Vector3 b = Quaternion.Euler(0, 0, -viewHalfAngle) * facing * viewRadius;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + a);
            Gizmos.DrawLine(transform.position, transform.position + b);
        }
    }
}
