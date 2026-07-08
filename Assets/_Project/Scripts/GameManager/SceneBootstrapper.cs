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
    /// <item>tilemap 铺地（按 scene_layout.json）</item>
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
            // 诱饵投放点 = 暗巷死角（场景中点附近）
            bait.baitDropPoint = new GameObject("BaitDropPoint").transform;
            bait.baitDropPoint.position = new Vector3(0.2f, 0.8f, 0f);

            // === 8. AlertSystem / EvidenceTracker / WantedSystem（M2 警戒系统） ===
            var alertGo = new GameObject("[AlertSystem]");
            alertGo.AddComponent<AlertSystem>();
            var evidenceGo = new GameObject("[EvidenceTracker]");
            evidenceGo.AddComponent<EvidenceTracker>();
            var wantedGo = new GameObject("[WantedSystem]");
            wantedGo.AddComponent<WantedSystem>();

            // === 9. CustomerSpawner / CookingController + CookingStation x4 ===
            // QueueManager 放在出餐窗口上方（玩家头顶位置），排队方向朝下
            var queueGo = new GameObject("[QueueManager]");
            var queueMgr = queueGo.AddComponent<QueueManager>();
            queueGo.transform.position = new Vector3(0f, 1.5f, 0f); // 排队起点（窗口上方）

            // CustomerSpawner 放在屏幕左上角，顾客从左侧入场
            var spawnerGo = new GameObject("[CustomerSpawner]");
            spawnerGo.transform.position = new Vector3(-3f, 1.5f, 0f); // 顾客生成点（左侧入场）
            var spawner = spawnerGo.AddComponent<CustomerSpawner>();
            // 设一个离场点（右侧）
            var exitGo = new GameObject("CustomerExitPoint");
            exitGo.transform.position = new Vector3(3f, 1.5f, 0f);
            if (JsonDataLoader.CustomerProfiles != null)
                spawner.profiles = JsonDataLoader.CustomerProfiles;
            if (JsonDataLoader.Recipes != null)
                spawner.availableRecipes = JsonDataLoader.Recipes;
            gameMgr.AssignCustomerSpawner(spawner);

            var cookGo = new GameObject("[CookingController]");
            var cookCtl = cookGo.AddComponent<CookingController>();
            gameMgr.AssignCookingController(cookCtl);

            CreateCookingStation(CookingWorkstation.Chop, "ChopStation", new Vector3(-1.2f, 0.2f, 0f));
            CreateCookingStation(CookingWorkstation.Grill, "GrillStation", new Vector3(0.0f, 0.2f, 0f));
            CreateCookingStation(CookingWorkstation.Assemble, "AssembleStation", new Vector3(1.2f, 0.2f, 0f));
            CreateCookingStation(CookingWorkstation.Serve, "ServeStation", new Vector3(0f, 0.9f, 0f));

            // === 10. 环境击杀对象 ===
            CreateEnvironmentKill("GasCanister",
                new Vector3(1.6f, -0.3f, 0f),
                "gas_tank", 2f, "引爆煤气罐");

            CreateEnvironmentKill("Billboard",
                new Vector3(1.4f, 0.9f, 0f),
                "billboard", 1.5f, "推倒广告牌");

            // === 11. 处理站 ===
            CreateDisposalStation("Grinder", DisposalMethod.Grind,
                new Vector3(-1.0f, -0.7f, 0f), "绞肉机");
            CreateDisposalStation("Freezer", DisposalMethod.Freeze,
                new Vector3(0.5f, -0.8f, 0f), "冰柜");
            CreateDisposalStation("Dump", DisposalMethod.Dump,
                new Vector3(1.1f, -0.7f, 0f), "垃圾桶");

            // === 12. AudioFeedbackBinder + HitStop ===
            var binderGo = new GameObject("[AudioFeedbackBinder]");
            binderGo.AddComponent<AudioFeedbackBinder>();
            binderGo.AddComponent<HitStop>();

            // === 13. 主摄像机 + ScreenShake（统一由本类管理，SceneSetupWizard 不再配置） ===
            var camGo = new GameObject("[MainCamera]");
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 3.5f;            // 拉近视野，看清细节
            cam.backgroundColor = new Color(0.12f, 0.08f, 0.16f); // 暗紫夜色
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.AddComponent<ScreenShake>();

            // === 14. Player（含 KillExecutor + CarryController） ===
            CreatePlayer(new Vector3(0f, 0f, 0f));

            // === 15. tilemap 铺地（M2 视觉增强） ===
            var bgGo = new GameObject("[Background]");
            var builder = bgGo.AddComponent<TilemapBuilder>();
            builder.Build();
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

            // 加载美术：4 个工作位用 cooking_station.png（共用底座）
            var sprite = LoadSprite("Sprites/Props/cooking_station");
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = MakeSolidSprite();
                sr.color = ws switch
                {
                    CookingWorkstation.Chop => new Color(0.4f, 0.3f, 0.2f),
                    CookingWorkstation.Grill => new Color(1.0f, 0.4f, 0.1f),
                    CookingWorkstation.Assemble => new Color(0.6f, 0.6f, 0.7f),
                    CookingWorkstation.Serve => new Color(0.2f, 0.8f, 0.3f),
                    _ => Color.white
                };
            }
            sr.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
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
            string spriteName = killMethodId switch
            {
                "gas_tank"  => "Sprites/Props/gas_canister",
                "billboard" => "Sprites/Props/billboard",
                _ => null
            };
            var sprite = spriteName != null ? LoadSprite(spriteName) : null;
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = MakeSolidSprite();
                sr.color = killMethodId switch
                {
                    "gas_tank" => new Color(1f, 0.3f, 0f),
                    "billboard" => new Color(0.5f, 0.5f, 0.6f),
                    _ => Color.gray
                };
            }
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
            string spriteName = method switch
            {
                DisposalMethod.Grind  => "Sprites/Props/grinder",
                DisposalMethod.Freeze => "Sprites/Props/freezer",
                DisposalMethod.Dump   => "Sprites/Props/trash_bin",
                _ => null
            };
            var sprite = spriteName != null ? LoadSprite(spriteName) : null;
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = MakeSolidSprite();
                sr.color = method switch
                {
                    DisposalMethod.Grind => new Color(0.7f, 0.1f, 0.1f),
                    DisposalMethod.Freeze => new Color(0.2f, 0.6f, 1f),
                    DisposalMethod.Dump => new Color(0.3f, 0.3f, 0.2f),
                    _ => Color.gray
                };
            }
            sr.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            sr.sortingOrder = 3;

            Debug.Log($"[SceneBootstrapper] Created disposal station: {name} ({label}) at {pos}");
            return ds;
        }

        // ---- 辅助：创建玩家 ----

        private static GameObject CreatePlayer(Vector3 pos)
        {
            var             go = new GameObject("[Player]");
            go.transform.position = pos;
            // 顺序：Rigidbody2D → PlayerMotor → PlayerController → KillExecutor → PlayerInteractor → CarryController
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<PlayerMotor>();
            var controller = go.AddComponent<PlayerController>();
            var executor = go.AddComponent<KillExecutor>();
            var interactor = go.AddComponent<PlayerInteractor>();
            var carry = go.AddComponent<CarryController>();
            // M2.5 修复: CarryController 需要 carryAnchor (拖挂尸体的锚点), 没有它 OnPickedUp(null) 直接 return
            var anchorGo = new GameObject("CarryAnchor");
            anchorGo.transform.SetParent(go.transform, false);
            anchorGo.transform.localPosition = new Vector3(0f, -0.3f, -0.1f);
            // 用反射设置 carryAnchor 字段 (它是 [SerializeField] private)
            var field = typeof(CarryController).GetField("carryAnchor",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null) field.SetValue(carry, anchorGo.transform);
            else Debug.LogError("[SceneBootstrapper] 未找到 carryAnchor 字段");

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

            // 视觉：加载 player_idle 美术
            var sr = go.AddComponent<SpriteRenderer>();
            var idle = LoadSprite("Sprites/Characters/Player/player_idle");
            if (idle != null)
            {
                sr.sprite = idle;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = MakeSolidSprite();
                sr.color = new Color(0.9f, 0.2f, 0.2f);
            }
            sr.sortingOrder = 20;

            // 视觉切换组件
            go.AddComponent<PlayerVisualController>();

            Debug.Log($"[SceneBootstrapper] Created Player at {pos} with KillExecutor + CarryController");
            return go;
        }

        // ---- 工具 ----

        /// <summary>从 Resources 加载 Sprite，自动从 Texture2D 创建（不依赖 .meta 的 TextureType 配置）。
        /// 失败时打印详细错误日志。</summary>
        public static Sprite LoadSprite(string path)
        {
            // 优先尝试直接加载 Sprite（如果 .meta 已配 Sprite 导入）
            var direct = Resources.Load<Sprite>(path);
            if (direct != null) return direct;

            // 退化：加载 Texture2D 手动创建 Sprite
            // 这样不依赖 Unity 把 PNG 识别为 sprite
            var tex = Resources.Load<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogError($"[SceneBootstrapper] Resources.Load(\"{path}\") 找不到资源 — 检查路径或后缀");
                return null;
            }
            // PPU=32 与项目约定一致；如果纹理本身 32x32 像素，1 sprite = 1 世界单位
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32f);
            sprite.name = path;
            return sprite;
        }

        private static Sprite _solidSprite;
        public static Sprite MakeSolidSprite()
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
