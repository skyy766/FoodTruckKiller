using System;
using System.Collections.Generic;
using FoodTruckKiller.Interaction;
using FoodTruckKiller.Player;
using UnityEngine;

namespace FoodTruckKiller.Cooking
{
    /// <summary>
    /// 烹饪工作位类型：切菜 / 煎烤 / 组装 / 出餐。
    /// </summary>
    [Serializable]
    public enum CookingWorkstation
    {
        Chop,
        Grill,
        Assemble,
        Serve
    }

    /// <summary>
    /// 烹饪台：包含多个工作位，玩家交互后根据当前 Controller 状态进入对应工作流。
    /// 实现 IInteractable 供玩家交互。
    /// </summary>
    public class CookingStation : MonoBehaviour, IInteractable
    {
        /// <summary>本台支持的工作位类型。</summary>
        public CookingWorkstation workstation = CookingWorkstation.Assemble;

        /// <summary>工作位锚点（玩家站立位置 / 食材生成点）。</summary>
        public Transform workAnchor;

        /// <summary>单次处理时长（秒）。</summary>
        public float processDuration = 2f;

        /// <summary>当前是否正在处理。</summary>
        public bool IsBusy { get; private set; }

        /// <summary>当前处理的食材 id 队列。</summary>
        private readonly Queue<string> _processingQueue = new Queue<string>();

        /// <summary>处理完成回调。</summary>
        public event Action<CookingWorkstation, string> OnProcessed;

        /// <summary>交互提示名。</summary>
        [SerializeField] private string promptName = "烹饪";

        /// <summary>
        /// 玩家交互入口。
        /// </summary>
        public void OnInteract(PlayerController player)
        {
            if (IsBusy)
                return;

            var controller = CookingController.Instance;
            if (controller != null)
                controller.EnterWorkstation(this);
        }

        /// <summary>
        /// 返回交互提示文本。
        /// </summary>
        public string GetPromptName()
        {
            return promptName;
        }

        /// <summary>
        /// 投入一个食材进入处理队列。
        /// </summary>
        /// <param name="ingredientId">食材 id。</param>
        public void EnqueueIngredient(string ingredientId)
        {
            if (string.IsNullOrEmpty(ingredientId))
                return;
            _processingQueue.Enqueue(ingredientId);
            if (!IsBusy)
                ProcessNext();
        }

        /// <summary>
        /// 处理队列中的下一个食材。
        /// </summary>
        private void ProcessNext()
        {
            if (_processingQueue.Count == 0)
            {
                IsBusy = false;
                return;
            }

            IsBusy = true;
            string current = _processingQueue.Dequeue();
            // 使用协程模拟处理延时；为避免引入 MonoBehaviour 协程样板，使用 Invoke。
            StartCoroutineHelper(current);
        }

        /// <summary>
        /// 简单延时处理包装（基于 MonoBehaviour.Invoke）。
        /// </summary>
        private void StartCoroutineHelper(string ingredientId)
        {
            // 通过 Invoke 延迟调用完成回调。
            Invoke(nameof(FinishProcess), processDuration);
            _currentProcessingId = ingredientId;
        }

        private string _currentProcessingId;

        /// <summary>烹饪动画帧切换。</summary>
        private float _animTimer;
        private bool _animFrame;
        private Sprite _spriteIdle;
        private Sprite _spriteCook1;
        private Sprite _spriteCook2;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
            {
                _spriteIdle  = Resources.Load<Sprite>("Sprites/Props/cooking_station");
                _spriteCook1 = Resources.Load<Sprite>("Sprites/Props/cooking_station_cooking_01");
                _spriteCook2 = Resources.Load<Sprite>("Sprites/Props/cooking_station_cooking_02");
                if (_spriteIdle != null) _sr.sprite = _spriteIdle;
            }
        }

        private void Update()
        {
            if (_sr == null || _spriteCook1 == null || _spriteCook2 == null) return;
            if (IsBusy)
            {
                _animTimer += Time.deltaTime;
                if (_animTimer >= 0.25f)
                {
                    _animTimer = 0f;
                    _animFrame = !_animFrame;
                    _sr.sprite = _animFrame ? _spriteCook1 : _spriteCook2;
                }
            }
            else if (_sr.sprite == _spriteCook1 || _sr.sprite == _spriteCook2)
            {
                // 恢复 idle
                if (_spriteIdle != null) _sr.sprite = _spriteIdle;
                _animTimer = 0f;
            }
        }

        /// <summary>完成处理回调。</summary>
        private void FinishProcess()
        {
            string finished = _currentProcessingId;
            _currentProcessingId = null;
            IsBusy = false;
            OnProcessed?.Invoke(workstation, finished);
            if (_processingQueue.Count > 0)
                ProcessNext();
        }
    }
}
