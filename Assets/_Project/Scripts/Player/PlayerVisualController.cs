using UnityEngine;
using FoodTruckKiller.GameManager;

namespace FoodTruckKiller.Player
{
    /// <summary>
    /// 玩家视觉控制器：按 <see cref="PlayerController.Facing"/> 和 <see cref="PlayerController.State"/>
    /// 切换 SpriteRenderer 的 sprite。4 方向 walk + idle + attack + carry。
    /// <para>所有 sprite 从 <c>Resources/Sprites/Characters/Player/</c> 加载，
    /// 失败时回退到默认色块（<see cref="SceneBootstrapper.MakeSolidSprite"/>）。</para>
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerVisualController : MonoBehaviour
    {
        [Header("资源路径前缀")]
        [SerializeField] private string basePath = "Sprites/Characters/Player/";

        [Header("帧切换")]
        [Tooltip("walk 动画切换周期（秒）")]
        [SerializeField] private float walkFrameInterval = 0.15f;

        private PlayerController _controller;
        private SpriteRenderer _sr;

        // 预加载缓存
        private Sprite _idle;
        private Sprite _attack;
        private Sprite _carry;
        private Sprite _walkDown1, _walkDown2;
        private Sprite _walkUp1,   _walkUp2;
        private Sprite _walkLeft1, _walkLeft2;
        private Sprite _walkRight1, _walkRight2;

        private float _walkTimer;
        private int _walkFrame; // 0/1 交替
        private Vector2 _lastFacing = Vector2.down;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _sr = GetComponent<SpriteRenderer>();
            LoadSprites();
            ApplyIdle();
        }

        private void LoadSprites()
        {
            _idle      = Load("player_idle");
            _attack    = Load("player_attack");
            _carry     = Load("player_carry");
            _walkDown1 = Load("player_walk_down_01");
            _walkDown2 = Load("player_walk_down_02");
            _walkUp1   = Load("player_walk_up_01");
            _walkUp2   = Load("player_walk_up_02");
            _walkLeft1 = Load("player_walk_left_01");
            _walkLeft2 = Load("player_walk_left_02");
            _walkRight1= Load("player_walk_right_01");
            _walkRight2= Load("player_walk_right_02");
        }

        private Sprite Load(string name)
        {
            return FoodTruckKiller.GameManager.SceneBootstrapper.LoadSprite(basePath + name);
        }

        private void Update()
        {
            if (_sr == null || _controller == null) return;

            // 攻击 / 搬运 状态优先
            if (_controller.State == PlayerState.Kill && _attack != null)
            {
                _sr.sprite = _attack;
                return;
            }
            if (_controller.State == PlayerState.Carry && _carry != null)
            {
                _sr.sprite = _carry;
                return;
            }
            if (_controller.State == PlayerState.Cook)
            {
                // 烹饪时显示 carry/attack 之外的固定姿态，回退到 idle
                _sr.sprite = _idle != null ? _idle : _sr.sprite;
                return;
            }

            // 行走 / 站立：根据朝向和行走状态切 sprite
            bool isMoving = _controller.IsMoving;
            if (isMoving)
            {
                _walkTimer += Time.deltaTime;
                if (_walkTimer >= walkFrameInterval)
                {
                    _walkTimer = 0f;
                    _walkFrame = 1 - _walkFrame; // 0/1 切换
                }
                _sr.sprite = GetWalkSprite(_controller.Facing, _walkFrame);
            }
            else
            {
                _walkTimer = 0f;
                _walkFrame = 0;
                _sr.sprite = _idle != null ? _idle : _sr.sprite;
            }

            _lastFacing = _controller.Facing;
        }

        private Sprite GetWalkSprite(Vector2 facing, int frame)
        {
            // 量化到 4 方向：优先水平、否则垂直
            bool horizontal = Mathf.Abs(facing.x) >= Mathf.Abs(facing.y);
            if (horizontal)
            {
                bool right = facing.x >= 0;
                if (right) return frame == 0 ? _walkRight1 : _walkRight2;
                else       return frame == 0 ? _walkLeft1  : _walkLeft2;
            }
            else
            {
                bool up = facing.y >= 0;
                if (up) return frame == 0 ? _walkUp1   : _walkUp2;
                else   return frame == 0 ? _walkDown1 : _walkDown2;
            }
        }

        private void ApplyIdle()
        {
            if (_sr != null && _idle != null) _sr.sprite = _idle;
        }
    }
}
