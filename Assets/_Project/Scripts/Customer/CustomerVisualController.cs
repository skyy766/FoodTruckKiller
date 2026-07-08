using UnityEngine;
using FoodTruckKiller.GameManager;

namespace FoodTruckKiller.Customer
{
    /// <summary>
    /// 顾客视觉控制器：按 <see cref="CustomerAI.CurrentVelocity"/> 切换 walk 帧，
    /// 站立即静止。资源路径 <c>Sprites/Customers/customer_0{variant}{_walk_NN}</c>。
    /// </summary>
    [RequireComponent(typeof(CustomerAI))]
    public class CustomerVisualController : MonoBehaviour
    {
        [Tooltip("顾客 variant 1~3（对应 customer_01/02/03）")]
        [SerializeField] private int variant = 1;

        [Tooltip("walk 帧切换周期（秒）")]
        [SerializeField] private float walkFrameInterval = 0.15f;

        [Tooltip("行走判定速度阈值")]
        [SerializeField] private float moveThreshold = 0.05f;

        private CustomerAI _ai;
        private SpriteRenderer _sr;

        private Sprite _idle;
        private Sprite[] _walkFrames = new Sprite[4];

        private float _walkTimer;
        private int _walkFrame; // 0~3

        /// <summary>设置 variant（1~3），运行时换皮。</summary>
        public void SetVariant(int v)
        {
            variant = Mathf.Clamp(v, 1, 3);
            Reload();
        }

        private void Awake()
        {
            _ai = GetComponent<CustomerAI>();
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            Reload();
        }

        private void Reload()
        {
            string basePath = $"Sprites/Customers/customer_{variant:00}";
            _idle = Resources.Load<Sprite>(basePath);
            for (int i = 0; i < 4; i++)
                _walkFrames[i] = Resources.Load<Sprite>($"{basePath}_walk_{i + 1:00}");

            if (_sr != null && _idle != null)
            {
                _sr.sprite = _idle;
                _sr.color = Color.white;
            }
        }

        private void Update()
        {
            if (_sr == null || _ai == null) return;

            Vector2 v = _ai.CurrentVelocity;
            bool moving = v.sqrMagnitude > moveThreshold * moveThreshold;

            if (moving)
            {
                _walkTimer += Time.deltaTime;
                if (_walkTimer >= walkFrameInterval)
                {
                    _walkTimer = 0f;
                    _walkFrame = (_walkFrame + 1) % 4;
                }
                if (_walkFrames[_walkFrame] != null)
                    _sr.sprite = _walkFrames[_walkFrame];
            }
            else
            {
                _walkTimer = 0f;
                _walkFrame = 0;
                if (_idle != null) _sr.sprite = _idle;
            }
        }
    }
}
