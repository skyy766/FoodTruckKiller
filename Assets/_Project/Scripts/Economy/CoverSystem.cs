using System;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using UnityEngine;

namespace FoodTruckKiller.Economy
{
    /// <summary>
    /// 伪装度系统：餐车伪装度 0~100，正常经营上升，异常行为下降。
    /// 订阅相关 GameEvent 自动调整。
    /// </summary>
    public class CoverSystem : SingletonMono<CoverSystem>
    {
        /// <summary>当前伪装度（0~100）。</summary>
        public float Cover { get; private set; } = 100f;

        /// <summary>正常营业时每秒上升速率。</summary>
        [SerializeField] private float recoverPerSec = 0.5f;

        /// <summary>异常事件扣减量配置。</summary>
        [SerializeField] private float corpseFoundPenalty = 40f;
        [SerializeField] private float customerDiedPenalty = 10f;
        [SerializeField] private float inspectFailPenalty = 30f;

        /// <summary>伪装度变化事件（参数为新值）。</summary>
        public event Action<float> OnCoverChanged;

        private void OnEnable()
        {
            // 静态 GameEvents 注入（沙箱无编辑器，不依赖 Inspector 字段）。
            if (GameEvents.OnOrderServed != null) GameEvents.OnOrderServed.Register(HandleOrderServed);
            if (GameEvents.OnCustomerDied != null) GameEvents.OnCustomerDied.Register(HandleCustomerDied);
            if (GameEvents.OnCorpseFound != null) GameEvents.OnCorpseFound.Register(HandleCorpseFound);
        }

        private void OnDisable()
        {
            if (GameEvents.OnOrderServed != null) GameEvents.OnOrderServed.Unregister(HandleOrderServed);
            if (GameEvents.OnCustomerDied != null) GameEvents.OnCustomerDied.Unregister(HandleCustomerDied);
            if (GameEvents.OnCorpseFound != null) GameEvents.OnCorpseFound.Unregister(HandleCorpseFound);
        }

        private void Update()
        {
            // 正常营业缓慢回升。
            if (Cover < 100f)
            {
                Cover = Mathf.Min(100f, Cover + recoverPerSec * Time.deltaTime);
                OnCoverChanged?.Invoke(Cover);
            }
        }

        /// <summary>
        /// 手动扣减伪装度（用于自定义异常事件）。
        /// </summary>
        public void ReduceCover(float amount)
        {
            Cover = Mathf.Max(0f, Cover - amount);
            OnCoverChanged?.Invoke(Cover);
        }

        private void HandleOrderServed()
        {
            // 正常出餐小额回升。
            Cover = Mathf.Min(100f, Cover + 1f);
            OnCoverChanged?.Invoke(Cover);
        }

        private void HandleCustomerDied()
        {
            ReduceCover(customerDiedPenalty);
        }

        private void HandleCorpseFound()
        {
            ReduceCover(corpseFoundPenalty);
        }

        /// <summary>
        /// 卫生检查失败时由 <see cref="Detection.HealthInspector"/> 直接调用。
        /// <para>OnInspectFail 不在 GameEvents 16 个静态事件之列，故走直接方法调用，
        /// 这同样是跨系统通信的合法手段（非 [SerializeField] GameEvent）。</para>
        /// </summary>
        public void ApplyInspectFailPenalty()
        {
            ReduceCover(inspectFailPenalty);
        }
    }
}
