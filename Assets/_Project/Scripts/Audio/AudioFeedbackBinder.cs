using UnityEngine;
using FoodTruckKiller.Core.Events;

namespace FoodTruckKiller.Audio
{
    /// <summary>
    /// 将 GameEvent 绑定到音效播放的胶水层。
    /// 在 OnEnable 注册 Core.Events.GameEvents 上的事件，在 OnDisable 取消注册。
    /// 依赖 A1 在 FoodTruckKiller.Core.Events 中提供 GameEvent（Register/Unregister/Raise）
    /// 以及静态 GameEvents 聚合类（含 OnCustomerDied、OnWanted 等具名字段）。
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioFeedbackBinder : MonoBehaviour
    {
        [Header("Optional Feedback Tuning")]
        [SerializeField] private bool enableKillShake = true;
        [SerializeField] private float killShakeAmplitude = 0.18f;
        [SerializeField] private float killShakeDuration = 0.25f;

        private AudioManager _audio;

        private void OnEnable()
        {
            // 轻量防御：若 GameEvents 尚未由 A1 实现，避免硬编译失败——
            // 这里直接引用静态字段；若 A1 命名不同，仅需调整本文件即可。
            BindAll();
        }

        private void OnDisable()
        {
            UnbindAll();
        }

        private void EnsureAudio()
        {
            if (_audio == null) _audio = AudioManager.Instance;
        }

        private void BindAll()
        {
            // 经营反馈
            SafeRegister(GameEvents.OnOrderIn, OnOrderIn);
            SafeRegister(GameEvents.OnServe, OnServe);
            SafeRegister(GameEvents.OnCash, OnCash);
            SafeRegister(GameEvents.OnChop, OnChop);
            SafeRegister(GameEvents.OnSizzle, OnSizzle);
            SafeRegister(GameEvents.OnFootstep, OnFootstep);
            SafeRegister(GameEvents.OnGrind, OnGrind);

            // 暴力/危机反馈
            SafeRegister(GameEvents.OnCustomerDied, OnCustomerDied);
            SafeRegister(GameEvents.OnScream, OnScream);
            SafeRegister(GameEvents.OnExplosion, OnExplosion);
            SafeRegister(GameEvents.OnWanted, OnWanted);
        }

        private void UnbindAll()
        {
            SafeUnregister(GameEvents.OnOrderIn, OnOrderIn);
            SafeUnregister(GameEvents.OnServe, OnServe);
            SafeUnregister(GameEvents.OnCash, OnCash);
            SafeUnregister(GameEvents.OnChop, OnChop);
            SafeUnregister(GameEvents.OnSizzle, OnSizzle);
            SafeUnregister(GameEvents.OnFootstep, OnFootstep);
            SafeUnregister(GameEvents.OnGrind, OnGrind);

            SafeUnregister(GameEvents.OnCustomerDied, OnCustomerDied);
            SafeUnregister(GameEvents.OnScream, OnScream);
            SafeUnregister(GameEvents.OnExplosion, OnExplosion);
            SafeUnregister(GameEvents.OnWanted, OnWanted);
        }

        // ---- 事件处理：调用 AudioManager.PlaySFX ----

        private void OnOrderIn() { EnsureAudio(); _audio?.PlaySFX("order_in"); }
        private void OnServe()    { EnsureAudio(); _audio?.PlaySFX("serve"); }
        private void OnCash()     { EnsureAudio(); _audio?.PlaySFX("cash"); }
        private void OnChop()     { EnsureAudio(); _audio?.PlaySFX("chop"); }
        private void OnSizzle()   { EnsureAudio(); _audio?.PlaySFX("sizzle"); }
        private void OnFootstep() { EnsureAudio(); _audio?.PlaySFX("footstep"); }
        private void OnGrind()    { EnsureAudio(); _audio?.PlaySFX("grind"); }

        private void OnCustomerDied()
        {
            EnsureAudio();
            _audio?.PlaySFX("kill");
            if (enableKillShake && Camera.main != null)
            {
                var shaker = Camera.main.GetComponent<ScreenShake>();
                shaker?.Shake(killShakeAmplitude, killShakeDuration);
            }
            var hitStop = GetComponent<HitStop>();
            hitStop?.Trigger(0.05f);
        }

        private void OnScream()    { EnsureAudio(); _audio?.PlaySFX("scream"); }
        private void OnExplosion() { EnsureAudio(); _audio?.PlaySFX("explosion"); }
        private void OnWanted()    { EnsureAudio(); _audio?.PlaySFX("wanted_warning"); _audio?.PlaySFX("alarm"); }

        // ---- 安全注册辅助（GameEvent 缺失时只告警不崩）----
        private static void SafeRegister(GameEvent evt, System.Action handler)
        {
            if (evt == null || handler == null) return;
            evt.Register(handler);
        }

        private static void SafeUnregister(GameEvent evt, System.Action handler)
        {
            if (evt == null || handler == null) return;
            // A1 的 GameEvent 已提供 Unregister(Action)，无需反射 hack。
            evt.Unregister(handler);
        }
    }
}
