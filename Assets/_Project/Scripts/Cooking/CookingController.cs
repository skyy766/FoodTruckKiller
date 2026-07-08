using System;
using System.Collections.Generic;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Player;
using UnityEngine;

namespace FoodTruckKiller.Cooking
{
    /// <summary>
    /// 烹饪控制器：协调玩家当前烹饪操作，组合食材序列并交付订单。
    /// 作为单例存在，便于烹饪台与工作位共享状态。
    /// </summary>
    public class CookingController : SingletonMono<CookingController>
    {
        /// <summary>当前烹饪状态。</summary>
        public CookingState State { get; private set; } = CookingState.Idle;

        /// <summary>当前正在处理的订单。</summary>
        public Order CurrentOrder { get; private set; }

        /// <summary>正在组装的食材序列。</summary>
        public List<string> Assembling { get; } = new List<string>();

        /// <summary>当前所在工作位。</summary>
        public CookingStation ActiveStation { get; private set; }

        /// <summary>订单完成事件（参数为完成的订单）。</summary>
        public event Action<Order> OnOrderReady;

        /// <summary>状态变更事件。</summary>
        public event Action<CookingState> OnStateChanged;

        /// <summary>
        /// 设置当前待处理订单。
        /// </summary>
        /// <param name="order">订单实例。</param>
        public void SetCurrentOrder(Order order)
        {
            CurrentOrder = order;
            Assembling.Clear();
            SetState(CookingState.Idle);
            // 新订单进入 → 触发音效事件
            if (order != null && GameEvents.OnOrderIn != null)
                GameEvents.OnOrderIn.Raise();
        }

        /// <summary>
        /// 进入指定烹饪工作位。
        /// </summary>
        /// <param name="station">工作位。</param>
        public void EnterWorkstation(CookingStation station)
        {
            ActiveStation = station;
            switch (station.workstation)
            {
                case CookingWorkstation.Chop:
                    SetState(CookingState.Chopping);
                    GameEvents.OnChop?.Raise();
                    break;
                case CookingWorkstation.Grill:
                    SetState(CookingState.Grilling);
                    GameEvents.OnSizzle?.Raise();
                    break;
                case CookingWorkstation.Assemble:
                    SetState(CookingState.Assembling);
                    // M1 联调：食材拾取 UI 未实装，这里自动从当前订单食谱填充组装序列并校验。
                    // 玩家走到组装台按交互即完成组装；正式版需替换为玩家手动投入食材。
                    AutoFillAssemblingFromRecipe();
                    TryAssemble();
                    break;
                case CookingWorkstation.Serve:
                    SetState(CookingState.Serving);
                    TryServe();
                    break;
            }
        }

        /// <summary>
        /// M1 联调辅助：从当前订单食谱自动填充组装序列。
        /// 正式版应移除，改为由玩家手动投入食材到组装台。
        /// </summary>
        private void AutoFillAssemblingFromRecipe()
        {
            Assembling.Clear();
            if (CurrentOrder == null || CurrentOrder.Recipe == null) return;
            var ingredients = CurrentOrder.Recipe.ingredients;
            if (ingredients == null) return;
            for (int i = 0; i < ingredients.Count; i++)
                Assembling.Add(ingredients[i]);
        }

        /// <summary>
        /// 添加一个食材到组装序列（玩家手动投入组装台时调用）。
        /// </summary>
        /// <param name="ingredientId">食材 id。</param>
        public void AddIngredient(string ingredientId)
        {
            if (string.IsNullOrEmpty(ingredientId))
                return;
            Assembling.Add(ingredientId);
        }

        /// <summary>
        /// 尝试完成组装并校验订单。
        /// </summary>
        public void TryAssemble()
        {
            if (CurrentOrder == null || CurrentOrder.Recipe == null)
                return;

            if (OrderValidator.Validate(Assembling, CurrentOrder.Recipe))
            {
                CurrentOrder.MarkReady();
                OnOrderReady?.Invoke(CurrentOrder);
                SetState(CookingState.Idle);
            }
            else
            {
                // 组装失败：清空当前序列并回到空闲态。
                Assembling.Clear();
                SetState(CookingState.Idle);
            }
        }

        /// <summary>
        /// 出餐：将已 Ready 的订单交付。成功出餐触发 OnServe / OnOrderServed 事件，
        /// 供 EconomyManager 加钱与 AudioFeedbackBinder 播放音效。
        /// </summary>
        private void TryServe()
        {
            if (CurrentOrder == null)
            {
                SetState(CookingState.Idle);
                return;
            }

            if (CurrentOrder.State == OrderState.Ready)
            {
                CurrentOrder.MarkServed();
                // 触发出餐音效与全局订单完成事件
                GameEvents.OnServe?.Raise();
                GameEvents.OnOrderServed?.Raise();
            }
            SetState(CookingState.Idle);
        }

        /// <summary>
        /// 切换状态并触发事件。
        /// </summary>
        private void SetState(CookingState newState)
        {
            if (State == newState)
                return;
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
