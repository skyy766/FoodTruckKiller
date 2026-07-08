using UnityEngine;
using FoodTruckKiller.Assassination;
using FoodTruckKiller.Audio;
using FoodTruckKiller.Cooking;
using FoodTruckKiller.Corpse;
using FoodTruckKiller.Core.DataLoader;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Customer;
using FoodTruckKiller.Detection;
using FoodTruckKiller.Economy;
using FoodTruckKiller.Player;

namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// 场景引导器：场景启动时按依赖顺序创建并初始化所有子系统。
    /// <para>挂载在场景中的空 GameObject 上即可自动完成 M1+M2 全部布线，
    /// 无需 Inspector 注入（沙箱无编辑器亦可运行）。</para>
    /// <para>Awake 顺序：</para>
    /// <list type="number">
    /// <item><see cref="GameEvents.Init"/> —— 创建所有事件实例</item>
    /// <item><see cref="JsonDataLoader.LoadAll"/> —— 加载 JSON 数据</item>
    /// <item>AudioManager（单例）</item>
    /// <item>GameManager / EconomyManager（单例）</item>
    /// <item>DayTimeController / ObjectiveTracker</item>
    /// <item>CorpseManager（单例，尸体注册依赖）</item>
    /// <item>AssassinationManager（单例）+ BaitSystem（单例）</item>
    /// <item>CustomerSpawner / CookingController + CookingStation x4</item>
    /// <item>环境击杀对象（煤气罐 + 广告牌）</item>
    /// <item>处理站（绞肉机 + 冰柜 + 垃圾桶）</item>
    /// <item>AudioFeedbackBinder + HitStop</item>
    /// <item>主摄像机 + ScreenShake</item>
    /// <item>Player（含 KillExecutor + CarryController）</item>
    /// </list>
    /// <para>Start 阶段调用 <see cref="GameManager.StartGame"/> 进入 Playing 状态。</para>
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SceneBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            // === 1. 事件聚合（必须最先：其他系统 OnEnable 会注册） ===
            GameEvents.Init();

            // === 2. 数据加载 ===
            JsonDataLoader.LoadAll();

            // === 3. AudioManager（单例） ===
            var audioGo = new GameObject("[AudioManager]");
            audioGo.AddComponent<AudioManager>();

            // === 4. GameManager / EconomyManager（单例） ===
            var gameMgrGo = new GameObject("[GameManager]");
            var gameMgr = gameMgrGo.AddComponent<GameManager>();

            var econGo = new GameObject("[EconomyManager]");
            var econMgr = econGo.AddComponent<EconomyManager>();
            gameMgr.AssignEconomyManager(econMgr);

            // === 5. DayTimeController / ObjectiveTracker ===
            var dayGo = new GameObject("[DayTimeController]");
            var dayCtl = dayGo.AddComponent<DayTimeController>();
            gameMgr.AssignDayTimeController(dayCtl);

            var objGo = new GameObject("[ObjectiveTracker]");
            var objTracker = objGo.AddComponent<ObjectiveTracker>();
            gameMgr.AssignObjectiveTracker(objTracker);

            // === 6. CorpseManager（单例，尸体 Awake 注册依赖） ===
            var corpseMgrGo = new GameObject("[CorpseManager]");
            corpseMgrGo.AddComponent<CorpseManager>();

            // === 7. AssassinationManager + BaitSystem（单例） ===
            var amGo = new GameObject("[AssassinationManager]");
            var am = amGo.AddComponent<AssassinationManager>();
            // 注入 targetPool 从 JSON 数据
            if (JsonDataLoader.Targets != null)
                am.targetPool = new System.Collections.Generic.List<TargetProfile>(JsonDataLoader.Targets);

            var baitGo = new GameObject("[BaitSystem]");
            var bait = baitGo.AddComponent<BaitSystem>();
            // 诱饵投放点 = 暗巷死角（scene_layout.json DarkAlley 区域中心）
            bait.baitDropPoint = new GameObject("BaitDropPoint").transform;
            bait.baitDropPoint.position = new Vector3(0.203125f, 0.984375f, 0f); // DarkAlley 中心

            // === 8. AlertSystem / EvidenceTracker / WantedSystem（M2 警戒系统） ===
            var alertGo = new GameObject("[AlertSystem]");
            alertGo.AddComponent<AlertSystem>();
            var evidenceGo = new GameObject("[EvidenceTracker]");
            evidenceGo.AddComponent<EvidenceTracker>();
            var wantedGo = new GameObject("[WantedSystem]");
            wantedGo.AddComponent<WantedSystem>();

            // === 9. CustomerSpawner / CookingController + CookingStation x4 ===
            var queueGo = new GameObject("[QueueManager]");
            queueGo.AddComponent<QueueManager>();

            var spawnerGo = new GameObject("[CustomerSpawner]");
            var spawner = spawnerGo.AddComponent<CustomerSpawner>();
            if (JsonDataLoader.CustomerProfiles != null)
                spawner.profiles = JsonDataLoader.CustomerProfiles;
            if (JsonDataLoader.Recipes != null)
                spawner.availableRecipes = JsonDataLoader.Recipes;
            gameMgr.AssignCustomerSpawner(spawner);

            var cookGo = new GameObject("[CookingController]");
            var cookCtl = cookGo.AddComponent<CookingController>();
            gameMgr.AssignCookingController(cookCtl);

            CreateCookingStation(CookingWorkstation.Chop, "ChopStation", new Vector3(-2f, -1f, 0f));
            CreateCookingStation(CookingWorkstation.Grill, "GrillStation", new Vector3(0f, -1f, 0f));
            CreateCookingStation(CookingWorkstation.Assemble, "AssembleStation", new Vector3(2f, -1f, 0f));
            CreateCookingStation(CookingWorkstation.Serve, "ServeStation", new Vector3(0f, 1f, 0f));

            // === 10. 环境击杀对象 ===
            CreateEnvironmentKill("GasCanister",
                new Vector3(1.65625f, 0.28125f, 0f),  // scene_layout.json gasCanister
                "gas_tank", 2f, "引爆煤气罐");

            CreateEnvironmentKill("Billboard",
                new Vector3(1.75f, 0.375f, 0f),  // scene_layout.json billboard
                "billboard", 1.5f, "推倒广告牌");

            // === 11. 处理站 ===
            CreateDisposalStation("Grinder", DisposalMethod.Grind,
                new Vector3(0.1875f, 1.0f, 0f), "绞肉机");      // scene_layout grinder
            CreateDisposalStation("Freezer", DisposalMethod.Freeze,
                new Vector3(1.03125f, 0.40625f, 0f), "冰柜");    // scene_layout freezer
            CreateDisposalStation("Dump", DisposalMethod.Dump,
                new Vector3(0.1875f, 1.09375f, 0f), "垃圾桶");   // scene_layout dump

            // === 12. AudioFeedbackBinder + HitStop ===
            var binderGo = new GameObject("[AudioFeedbackBinder]");
            binderGo.AddComponent<AudioFeedbackBinder>();
            binderGo.AddComponent<HitStop>();

            // === 13. 主摄像机 + ScreenShake ===
            var camGo = new GameObject("[MainCamera]");
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.18f, 0.12f, 0.22f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.AddComponent<ScreenShake>();

            // === 14. Player（含 KillExecutor + CarryController） ===
            CreatePlayer(new Vector3(0f, 0f, 0f));
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartGame();
        }

        // ---- 辅助：创建烹饪工作位 ----

        private static CookingStation CreateCookingStation(CookingWorkstation ws, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;
            col.isTrigger = true;
            var station = go.AddComponent<CookingStation>();
            station.workstation = ws;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = ws switch
            {
                CookingWorkstation.Chop => new Color(0.4f, 0.3f, 0.2f),
                CookingWorkstation.Grill => new Color(1.0f, 0.4f, 0.1f),
                CookingWorkstation.Assemble => new Color(0.6f, 0.6f, 0.7f),
                CookingWorkstation.Serve => new Color(0.2f, 0.8f, 0.3f),
                _ => Color.white
            };
            sr.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            sr.sortingOrder = 5;
            return station;
        }

        // ---- 辅助：创建环境击杀对象 ----

        private static EnvironmentKill CreateEnvironmentKill(string name, Vector3 pos,
            string killMethodId, float radius, string prompt)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            var ek = go.AddComponent<EnvironmentKill>();
            ek.effectRadius = radius;

            // 从 JsonDataLoader 获取 KillMethodData
            if (JsonDataLoader.KillMethods != null)
            {
                ek.killMethod = JsonDataLoader.KillMethods.Find(m => m.id == killMethodId);
            }

            // 视觉
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = killMethodId switch
            {
                "gas_tank" => new Color(1f, 0.3f, 0f),     // 橙色煤气罐
                "billboard" => new Color(0.5f, 0.5f, 0.6f), // 灰色广告牌
                _ => Color.gray
            };
            sr.transform.localScale = new Vector3(1f, 1f, 1f);
            sr.sortingOrder = 3;

            Debug.Log($"[SceneBootstrapper] Created environment kill: {name} at {pos} method={killMethodId}");
            return ek;
        }

        // ---- 辅助：创建处理站 ----

        private static DisposalStation CreateDisposalStation(string name, DisposalMethod method,
            Vector3 pos, string label)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            var ds = go.AddComponent<DisposalStation>();
            ds.method = method;
            ds.processDuration = 2f;

            // 视觉
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = method switch
            {
                DisposalMethod.Grind => new Color(0.7f, 0.1f, 0.1f),   // 深红绞肉机
                DisposalMethod.Freeze => new Color(0.2f, 0.6f, 1f),     // 蓝色冰柜
                DisposalMethod.Dump => new Color(0.3f, 0.3f, 0.2f),     // 暗棕垃圾桶
                _ => Color.gray
            };
            sr.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            sr.sortingOrder = 3;

            Debug.Log($"[SceneBootstrapper] Created disposal station: {name} ({label}) at {pos}");
            return ds;
        }

        // ---- 辅助：创建玩家 ----

        private static GameObject CreatePlayer(Vector3 pos)
        {
            var go = new GameObject("[Player]");
            go.transform.position = pos;
            // 顺序：Rigidbody2D → PlayerMotor → PlayerController → KillExecutor → PlayerInteractor → CarryController
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<PlayerMotor>();
            var controller = go.AddComponent<PlayerController>();
            var executor = go.AddComponent<KillExecutor>();
            var interactor = go.AddComponent<PlayerInteractor>();
            go.AddComponent<CarryController>();

            // 绑定近战击杀方式
            if (JsonDataLoader.KillMethods != null)
            {
                var knife = JsonDataLoader.KillMethods.Find(m => m.id == "knife");
                if (knife != null)
                    interactor.meleeKillMethod = knife;
            }

            controller.RebindDependencies();
            interactor.RebindDependencies();
            interactor.ConfigureDetection(~0, 0.6f);

            // 视觉：红色方块（玩家厨师）
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = new Color(0.9f, 0.2f, 0.2f);
            sr.sortingOrder = 20;

            Debug.Log($"[SceneBootstrapper] Created Player at {pos} with KillExecutor + CarryController");
            return go;
        }

        // ---- 工具：创建一个白色 1x1 像素 Sprite（运行时） ----

        private static Sprite _solidSprite;
        private static Sprite MakeSolidSprite()
        {
            if (_solidSprite != null) return _solidSprite;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _solidSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            _solidSprite.name = "GeneratedSolid";
            return _solidSprite;
        }
    }
}
