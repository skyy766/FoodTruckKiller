using FoodTruckKiller.Core.Events;
using CorpseEntity = FoodTruckKiller.Corpse.Corpse;
using FoodTruckKiller.Customer;
using FoodTruckKiller.Detection;
using UnityEngine;

namespace FoodTruckKiller.Assassination
{
    /// <summary>
    /// 击杀执行器：按 KillMethodData 执行击杀，生成尸体，触发 GameEvent。
    /// </summary>
    public class KillExecutor : MonoBehaviour
    {
        /// <summary>尸体预制体（沙箱无编辑器时可为空，运行时生成空 GameObject）。</summary>
        [SerializeField] private GameObject corpsePrefab;

        /// <summary>
        /// 执行击杀。
        /// </summary>
        /// <param name="victim">被击杀的顾客 AI。</param>
        /// <param name="method">击杀方式数据。</param>
        public void Execute(CustomerAI victim, KillMethodData method)
        {
            if (victim == null || method == null) return;

            // 标记顾客死亡。
            victim.MarkDead();

            // 生成尸体。
            SpawnCorpse(victim.transform.position, method);

            // 累加警戒值（通过 AlertSystem）。
            if (AlertSystem.Instance != null)
                AlertSystem.Instance.AddAlert(method.noiseRadius * 2f);

            // 留下证据（通过 EvidenceTracker）。
            if (EvidenceTracker.Instance != null)
                EvidenceTracker.Instance.LeaveEvidence(victim.transform.position, method.evidenceType);

            // 触发通用顾客死亡事件（静态 GameEvents）。
            GameEvents.OnCustomerDied?.Raise();

            // 若为目标，触发目标击杀事件。
            if (victim.Profile != null && victim.Profile.type == CustomerType.Target)
            {
                GameEvents.OnTargetKilled?.Raise();
            }
        }

        /// <summary>
        /// 在指定位置生成尸体。
        /// </summary>
        private void SpawnCorpse(Vector3 pos, KillMethodData method)
        {
            GameObject go;
            if (corpsePrefab != null)
            {
                go = Instantiate(corpsePrefab, pos, Quaternion.identity);
            }
            else
            {
                go = new GameObject("Corpse_Runtime");
                go.transform.position = pos;
                go.AddComponent<BoxCollider2D>();
                // M2.5: 加 kinematic Rigidbody2D, 物理引擎知道这是静态物体不会推玩家
                var crb = go.AddComponent<Rigidbody2D>();
                crb.bodyType = RigidbodyType2D.Kinematic;
                crb.gravityScale = 0f;

                // 视觉：尝试加载 corpse.png，失败回退到暗红色块
                var sr = go.AddComponent<SpriteRenderer>();
                var corpseSprite = FoodTruckKiller.GameManager.SceneBootstrapper.LoadSprite("Sprites/Corpse/corpse");
                if (corpseSprite != null)
                {
                    sr.sprite = corpseSprite;
                    sr.color = Color.white;
                    sr.transform.localScale = new Vector3(1.2f, 1.2f, 1f); // 16x16 放大至近 32x32
                }
                else
                {
                    var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Point;
                    tex.SetPixel(0, 0, new Color(0.6f, 0.1f, 0.1f));
                    tex.Apply();
                    sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                    sr.color = Color.white;
                    sr.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                }
                sr.sortingOrder = 1;
            }

            // 添加尸体组件
            var corpse = go.GetComponent<CorpseEntity>() ?? go.AddComponent<CorpseEntity>();
            corpse.Initialize(method.evidenceType);

            // 添加检测标签（供警察/卫生检查员视野检测）
            if (go.GetComponent<Corpse.CorpseDetectionTag>() == null)
                go.AddComponent<Corpse.CorpseDetectionTag>();

            Debug.Log($"[KillExecutor] Spawned corpse at {pos} with evidence={method.evidenceType}");
        }
    }
}
