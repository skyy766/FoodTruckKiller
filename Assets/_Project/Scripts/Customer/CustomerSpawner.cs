using System.Collections.Generic;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Cooking;
using UnityEngine;

namespace FoodTruckKiller.Customer
{
    /// <summary>
    /// 顾客生成器：按概率与时间间隔生成不同类型顾客。
    /// <para>沙箱无编辑器时，<see cref="customerPrefab"/> / <see cref="spawnPoint"/> / <see cref="exitTransform"/>
    /// 可为空，运行时使用默认位置与运行时构造的顾客 GameObject（含 Rigidbody2D + CustomerAI）。</para>
    /// <para>订阅 <see cref="GameEvents.OnDayEnd"/> 自动重置当日计数。</para>
    /// </summary>
    public class CustomerSpawner : SingletonMono<CustomerSpawner>
    {
        /// <summary>可生成的顾客画像列表（由 SceneBootstrapper 从 JsonDataLoader.CustomerProfiles 注入）。</summary>
        public List<CustomerProfile> profiles = new List<CustomerProfile>();

        /// <summary>可选食谱列表（普通顾客随机挑选，由 SceneBootstrapper 从 JsonDataLoader.Recipes 注入）。</summary>
        public List<RecipeData> availableRecipes = new List<RecipeData>();

        /// <summary>顾客预制体（须挂载 CustomerAI；沙箱可为空，运行时构造）。</summary>
        public GameObject customerPrefab;

        /// <summary>生成点（可为空，回退到自身位置）。</summary>
        public Transform spawnPoint;

        /// <summary>离场点（可为空，回退到自身位置 + 右侧偏移）。</summary>
        public Transform exitTransform;

        /// <summary>生成间隔（秒）。</summary>
        public float spawnInterval = 8f;

        /// <summary>单日最大生成数。</summary>
        public int maxDailySpawn = 20;

        /// <summary>离场点（暴露给状态机）。</summary>
        public Vector3 ExitPoint => exitTransform != null
            ? exitTransform.position
            : transform.position + Vector3.right * 8f;

        /// <summary>已生成数量。</summary>
        public int SpawnedCount { get; private set; }

        /// <summary>当前在场顾客。</summary>
        private readonly List<CustomerAI> _activeCustomers = new List<CustomerAI>();

        private float _timer;

        private void OnEnable()
        {
            // 静态 GameEvents 注入（沙箱无编辑器）。
            if (GameEvents.OnDayEnd != null) GameEvents.OnDayEnd.Register(HandleDayEnd);
        }

        private void OnDisable()
        {
            if (GameEvents.OnDayEnd != null) GameEvents.OnDayEnd.Unregister(HandleDayEnd);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f && SpawnedCount < maxDailySpawn)
            {
                _timer = spawnInterval;
                TrySpawn();
            }
        }

        /// <summary>
        /// 按概率挑选画像并生成顾客。
        /// </summary>
        private void TrySpawn()
        {
            if (profiles == null || profiles.Count == 0) return;

            CustomerProfile picked = PickProfile();
            if (picked == null) return;

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            CustomerAI ai = CreateCustomer(pos);
            if (ai == null) return;

            RecipeData recipe = GetRecipeFor(picked);
            Order order = recipe != null ? new Order(recipe) : null;
            ai.Initialize(picked, order);

            // 分配队列位置。
            if (QueueManager.Instance != null)
            {
                int idx = QueueManager.Instance.AssignSlot(ai);
                ai.QueueIndex = idx;
            }

            ai.ChangeState(new QueuingState(ai));
            _activeCustomers.Add(ai);
            SpawnedCount++;
        }

        /// <summary>
        /// 创建顾客 GameObject：优先用预制体，沙箱无编辑器时运行时构造。
        /// </summary>
        private CustomerAI CreateCustomer(Vector3 pos)
        {
            GameObject go;
            if (customerPrefab != null)
            {
                go = Instantiate(customerPrefab, pos, Quaternion.identity);
                return go.GetComponent<CustomerAI>();
            }

            // 沙箱运行时构造：CustomerAI 标注 [RequireComponent(typeof(Rigidbody2D))]，
            // 添加 CustomerAI 时 Unity 会自动补齐 Rigidbody2D。
            go = new GameObject("Customer_Runtime");
            go.transform.position = pos;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            int variant = (SpawnedCount % 3) + 1;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.sprite = LoadCustomerSprite(variant - 1);
            if (sr.sprite == null)
            {
                // fallback：sprite 加载失败时用运行时生成方块（变体颜色）
                sr.sprite = MakeFallbackSprite();
                Color[] cols = {
                    new Color(0.29f, 0.56f, 0.85f),  // 蓝
                    new Color(0.36f, 0.72f, 0.36f),  // 绿
                    new Color(0.94f, 0.68f, 0.31f),  // 黄
                };
                sr.color = cols[Mathf.Abs(SpawnedCount) % 3];
            }

            // 视觉控制器（负责 walk 帧切换）
            var visual = go.AddComponent<CustomerVisualController>();
            visual.SetVariant(variant);

            // 头顶 OrderBubble 子对象
            CreateOrderBubble(go.transform);

            return go.AddComponent<CustomerAI>();
        }

        /// <summary>
        /// 在顾客头顶创建一个简单的 OrderBubble 子物体（带 order_bubble.png 背景）。
        /// </summary>
        private static void CreateOrderBubble(Transform parent)
        {
            var bubbleGo = new GameObject("OrderBubble");
            bubbleGo.transform.SetParent(parent, false);
            bubbleGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var sr = bubbleGo.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/UI/order_bubble");
            sr.sortingOrder = 12;
            sr.color = Color.white;
            if (sr.sprite == null)
            {
                // fallback：纯白方块
                sr.sprite = MakeFallbackSprite();
                sr.color = new Color(1f, 1f, 1f, 0.85f);
            }

            var bubble = bubbleGo.AddComponent<OrderBubble>();
            bubble.bubbleRoot = bubbleGo;
            bubbleGo.SetActive(false);
        }

        private static Sprite _fallbackSprite;
        private static Sprite MakeFallbackSprite()
        {
            if (_fallbackSprite != null) return _fallbackSprite;
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            _fallbackSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 32f);
            _fallbackSprite.name = "CustomerFallback";
            return _fallbackSprite;
        }

        /// <summary>
        /// 从 Resources 加载顾客像素 sprite（3 种变体，PPU=32）。
        /// </summary>
        private Sprite LoadCustomerSprite(int variant)
        {
            string path = $"Sprites/Customers/customer_0{Mathf.Clamp(variant, 0, 2) + 1}";
            return Resources.Load<Sprite>(path);
        }

        /// <summary>
        /// 按概率权重挑选画像。
        /// </summary>
        private CustomerProfile PickProfile()
        {
            float total = 0f;
            foreach (var p in profiles) total += Mathf.Max(0f, p.probability);
            if (total <= 0f) return null;

            float r = Random.value * total;
            float acc = 0f;
            foreach (var p in profiles)
            {
                acc += Mathf.Max(0f, p.probability);
                if (r <= acc) return p;
            }
            return profiles[profiles.Count - 1];
        }

        /// <summary>
        /// 根据画像偏好挑选食谱。
        /// </summary>
        private RecipeData GetRecipeFor(CustomerProfile profile)
        {
            if (profile != null && !string.IsNullOrEmpty(profile.preferredRecipeId))
            {
                foreach (var r in availableRecipes)
                    if (r != null && r.id == profile.preferredRecipeId) return r;
            }
            return GetRandomRecipe();
        }

        /// <summary>
        /// 随机返回一个食谱（供状态机使用）。
        /// </summary>
        public RecipeData GetRandomRecipe()
        {
            if (availableRecipes == null || availableRecipes.Count == 0) return null;
            return availableRecipes[Random.Range(0, availableRecipes.Count)];
        }

        /// <summary>
        /// 顾客离开时由状态机回调，清理列表。
        /// </summary>
        public void NotifyCustomerLeft(CustomerAI ai)
        {
            if (ai == null) return;
            if (QueueManager.Instance != null && ai.QueueIndex >= 0)
                QueueManager.Instance.ReleaseSlot(ai.QueueIndex);
            _activeCustomers.Remove(ai);
        }

        /// <summary>
        /// 重置当日计数（订阅 OnDayEnd 自动调用）。
        /// </summary>
        public void ResetDaily()
        {
            SpawnedCount = 0;
            _timer = 0f;
        }

        private void HandleDayEnd()
        {
            ResetDaily();
        }
    }
}
