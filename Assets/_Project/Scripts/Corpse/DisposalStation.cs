using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Interaction;
using FoodTruckKiller.Inventory;
using FoodTruckKiller.Player;
using UnityEngine;
using CorpseEntity = FoodTruckKiller.Corpse.Corpse;

namespace FoodTruckKiller.Corpse
{
    /// <summary>
    /// 尸体处理站：交互后按 DisposalMethod 处理玩家当前搬运的尸体。
    /// 绞肉可产出人肉食材加入背包。
    /// </summary>
    public class DisposalStation : MonoBehaviour, IInteractable
    {
        /// <summary>处理方式。</summary>
        public DisposalMethod method = DisposalMethod.Grind;

        /// <summary>处理时长（秒）。</summary>
        public float processDuration = 1.5f;

        /// <summary>交互提示。</summary>
        [SerializeField] private string promptName = "处理尸体";

        /// <summary>绞肉产出的人肉食材 id。</summary>
        public string humanMeatIngredientId = "human_meat";

        /// <summary>是否正在处理。</summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// 玩家交互。
        /// </summary>
        public void OnInteract(PlayerController player)
        {
            if (IsProcessing) return;
            Corpse target = FindCarriedCorpse(player);
            if (target == null) return;
            ProcessCorpse(target);
        }

        /// <summary>
        /// 由 PlayerInteractor 调用：尝试使用 CarryController 中的尸体进行处理。
        /// </summary>
        public void TryDispose(CarryController carry)
        {
            if (IsProcessing || carry == null || !carry.IsCarrying) return;
            if (carry.CurrentCarryable is CorpseEntity corpse && !corpse.IsDisposed)
            {
                ProcessCorpse(corpse);
            }
        }

        /// <summary>
        /// 返回交互提示。
        /// </summary>
        public string GetPromptName()
        {
            return promptName;
        }

        /// <summary>
        /// 查找玩家正在搬运的尸体（基于 CorpseManager 列表）。
        /// </summary>
        private Corpse FindCarriedCorpse(PlayerController player)
        {
            if (CorpseManager.Instance == null) return null;
            foreach (var c in CorpseManager.Instance.ActiveCorpses)
            {
                if (c != null && c.IsCarried) return c;
            }
            return null;
        }

        /// <summary>
        /// 处理尸体。
        /// </summary>
        private void ProcessCorpse(Corpse corpse)
        {
            IsProcessing = true;
            Invoke(nameof(FinishProcess), processDuration);
            _pending = corpse;
        }

        private Corpse _pending;

        /// <summary>完成处理回调。</summary>
        private void FinishProcess()
        {
            if (_pending == null)
            {
                IsProcessing = false;
                return;
            }

            switch (method)
            {
                case DisposalMethod.Grind:
                    if (InventorySystem.Instance != null)
                        InventorySystem.Instance.Add(humanMeatIngredientId, 2);
                    break;
                case DisposalMethod.Dump:
                    if (Random.value < 0.3f)
                        GameEvents.OnCorpseFound?.Raise();
                    break;
                case DisposalMethod.Freeze:
                    break;
                case DisposalMethod.Dissolve:
                    break;
            }

            _pending.MarkDisposed();
            _pending = null;
            IsProcessing = false;
        }
    }
}
