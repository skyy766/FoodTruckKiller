using UnityEngine;

namespace FoodTruckKiller.Player
{
    /// <summary>
    /// 玩家位移组件：以 velocity 方式驱动 Rigidbody2D。
    /// <para>由 PlayerController 每帧调用 Tick(moveInput)。</para>
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMotor : MonoBehaviour
    {
        [Tooltip("移动速度（单位/秒），像素游戏建议 3~5")]
        [SerializeField] private float moveSpeed = 4f;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            // 俯视角像素游戏：重力归零，避免下落
            rb.gravityScale = 0f;
        }

        /// <summary>每帧根据输入设置刚体速度。</summary>
        public void Tick(Vector2 moveInput)
        {
            rb.velocity = moveInput * moveSpeed;
        }

        /// <summary>立即停止移动。</summary>
        public void Stop()
        {
            rb.velocity = Vector2.zero;
        }
    }
}
