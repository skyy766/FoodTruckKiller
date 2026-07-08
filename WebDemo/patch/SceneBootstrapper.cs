using UnityEngine;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.DataLoader;
using FoodTruckKiller.Audio;
using FoodTruckKiller.Economy;
using FoodTruckKiller.Cooking;
using FoodTruckKiller.Customer;
using FoodTruckKiller.Player;

namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// 场景引导器：场景启动时按依赖顺序创建并初始化所有子系统。
    /// <para>挂载在场景中的空 GameObject 上即可自动完成 M1 经营循环的全部布线，
    /// 无需 Inspector 注入（沙箱无编辑器亦可运行）。</para>
    /// <para>Awake 顺序：</para>
    /// <list type="number">
    /// <item><see cref="GameEvents.Init"/> —— 创建所有事件实例（其他系统 OnEnable 注册依赖于此）</item>
    /// <item><see cref="JsonDataLoader.LoadAll"/> —— 加载 JSON 数据（GameManager/EconomyManager 依赖配置）</item>
    /// <item>AudioManager（单例，AudioFeedbackBinder 依赖）</item>
    /// <item>GameManager / EconomyManager（单例，注册 OnDayEnd/OnOrderServed）</item>
    /// <item>DayTimeController / ObjectiveTracker（注册/触发事件）</item>
    /// <item>CustomerSpawner / CookingController（单例）+ CookingStation × 4</item>
    /// <item>AudioFeedbackBinder（依赖 AudioManager + GameEvents）</item>
    /// <item>Player（PlayerController + PlayerMotor + PlayerInteractor + CarryController）</item>
    /// </list>
    /// <para>Start 阶段调用 <see cref="GameManager.StartGame"/> 进入 Playing 状态。</para>
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SceneBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            // 1. 事件聚合（必须最先：其他系统 OnEnable 会注册）
            GameEvents.Init();

            // 2. 数据加载（EconomyManager/ObjectiveTracker 依赖配置）
            JsonDataLoader.LoadAll();

            // 3. AudioManager（单例，AudioFeedbackBinder 依赖）
            var audioGo = new GameObject("[AudioManager]");
            audioGo.AddComponent<AudioManager>();

            // 4. GameManager / EconomyManager（单例）
            var gameMgrGo = new GameObject("[GameManager]");
            var gameMgr = gameMgrGo.AddComponent<GameManager>();

            var econGo = new GameObject("[EconomyManager]");
            var econMgr = econGo.AddComponent<EconomyManager>();
            gameMgr.AssignEconomyManager(econMgr);

            // 5. DayTimeController / ObjectiveTracker
            var dayGo = new GameObject("[DayTimeController]");
            var dayCtl = dayGo.AddComponent<DayTimeController>();
            gameMgr.AssignDayTimeController(dayCtl);

            var objGo = new GameObject("[ObjectiveTracker]");
            var objTracker = objGo.AddComponent<ObjectiveTracker>();
            gameMgr.AssignObjectiveTracker(objTracker);

            // 6. CustomerSpawner / CookingController + CookingStation × 4
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

            // 7. AudioFeedbackBinder（依赖 AudioManager + GameEvents）
            var binderGo = new GameObject("[AudioFeedbackBinder]");
            binderGo.AddComponent<AudioFeedbackBinder>();
            binderGo.AddComponent<HitStop>();

            // 主摄像机 + ScreenShake（AudioFeedbackBinder.OnCustomerDied 需要 Camera.main）
            var camGo = new GameObject("[MainCamera]");
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.18f, 0.12f, 0.22f); // 暗紫底色
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.AddComponent<ScreenShake>();

            // 8. Player
            CreatePlayer(new Vector3(0f, 0f, 0f));
        }

        private void Start()
        {
            // 所有子系统已就绪，进入 Playing 状态。
            if (GameManager.Instance != null)
                GameManager.Instance.StartGame();
        }

        // ---- 辅助：创建烹饪工作位 ----

        private static CookingStation CreateCookingStation(CookingWorkstation ws, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            // 触发器碰撞体，使 PlayerInteractor.OverlapCircle 能检测到
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;
            col.isTrigger = true;
            var station = go.AddComponent<CookingStation>();
            station.workstation = ws;
            // 视觉：方块+颜色（按工作站类型区分）
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = ws switch
            {
                CookingWorkstation.Chop => new Color(0.4f, 0.3f, 0.2f),    // 棕色切菜板
                CookingWorkstation.Grill => new Color(1.0f, 0.4f, 0.1f),   // 橙色煎烤
                CookingWorkstation.Assemble => new Color(0.6f, 0.6f, 0.7f),// 银灰组装台
                CookingWorkstation.Serve => new Color(0.2f, 0.8f, 0.3f),   // 绿色出餐窗
                _ => Color.white
            };
            sr.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            sr.sortingOrder = 5;
            return station;
        }

        // ---- 辅助：创建玩家 ----

        private static GameObject CreatePlayer(Vector3 pos)
        {
            var go = new GameObject("[Player]");
            go.transform.position = pos;
            // 顺序：Rigidbody2D → PlayerMotor → PlayerController → PlayerInteractor → CarryController
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<PlayerMotor>();
            var controller = go.AddComponent<PlayerController>();
            var interactor = go.AddComponent<PlayerInteractor>();
            go.AddComponent<CarryController>();
            controller.RebindDependencies();
            interactor.RebindDependencies();
            interactor.ConfigureDetection(~0, 0.6f);
            // 视觉：红色方块（玩家厨师）
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = new Color(0.9f, 0.2f, 0.2f);
            sr.sortingOrder = 20;
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
