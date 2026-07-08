using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Interaction;
using FoodTruckKiller.Player;
using UnityEngine;

namespace FoodTruckKiller.Assassination
{
    /// <summary>
    /// 环境击杀触发器：如煤气罐爆炸、广告牌掉落。
    /// 玩家交互后触发环境击杀（对附近目标造成伤害并生成证据）。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnvironmentKill : MonoBehaviour, IInteractable
    {
        /// <summary>击杀方式数据（环境类）。</summary>
        public KillMethodData killMethod;

        /// <summary>影响半径。</summary>
        public float effectRadius = 2f;

        /// <summary>爆炸/触发特效预制体。</summary>
        public GameObject effectPrefab;

        /// <summary>触发后是否一次性销毁。</summary>
        public bool oneShot = true;

        /// <summary>交互提示。</summary>
        [SerializeField] private string promptName = "触发环境击杀";

        /// <summary>击杀执行器引用（场景中放置）。</summary>
        [SerializeField] private KillExecutor killExecutor;

        /// <summary>已被触发。</summary>
        public bool Triggered { get; private set; }

        /// <summary>
        /// 玩家交互触发环境击杀。
        /// </summary>
        public void OnInteract(PlayerController player)
        {
            if (Triggered && oneShot) return;
            Triggered = true;

            // 优先使用 effectPrefab，否则按击杀方式自建 FX
            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                SpawnAutoFX();
            }

            // 运行时动态获取玩家身上的 KillExecutor
            KillExecutor executor = killExecutor;
            if (executor == null && player != null)
                executor = player.GetComponent<KillExecutor>();

            // 查找半径内的目标顾客并执行击杀。
            var hits = Physics2D.OverlapCircleAll(transform.position, effectRadius);
            foreach (var hit in hits)
            {
                var ai = hit.GetComponent<FoodTruckKiller.Customer.CustomerAI>();
                if (ai != null && !ai.IsDead && executor != null)
                    executor.Execute(ai, killMethod);
            }

            // 触发爆炸事件（音效+震屏）
            GameEvents.OnExplosion?.Raise();

            // 触发自身警戒（爆炸噪声）。
            if (Detection.AlertSystem.Instance != null && killMethod != null)
                Detection.AlertSystem.Instance.AddAlert(killMethod.noiseRadius * 3f);

            if (oneShot)
                Destroy(gameObject);
        }

        /// <summary>
        /// 返回交互提示。
        /// </summary>
        public string GetPromptName()
        {
            return promptName;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, effectRadius);
        }

        /// <summary>
        /// 自建临时 FX：按击杀方式选 explosion/smoke sprite，缩放由小到大，0.5s 后销毁。
        /// </summary>
        private void SpawnAutoFX()
        {
            bool isExplosion = killMethod != null && killMethod.id == "gas_tank";
            string spriteName = isExplosion ? "Sprites/FX/explosion" : "Sprites/FX/smoke";
            var sprite = FoodTruckKiller.GameManager.SceneBootstrapper.LoadSprite(spriteName);
            if (sprite == null) return;

            var fxGo = new GameObject(isExplosion ? "FX_Explosion" : "FX_Smoke");
            fxGo.transform.position = transform.position;
            var sr = fxGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 30;
            sr.color = Color.white;
            fxGo.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

            // 简易缩放动画
            var fader = fxGo.AddComponent<SimpleFXFader>();
            fader.duration = 0.6f;
            fader.endScale = isExplosion ? 2.5f : 1.8f;
        }
    }
}
