using System;
using System.Collections.Generic;
using UnityEngine;
using FoodTruckKiller.Cooking;
using FoodTruckKiller.Customer;
using FoodTruckKiller.Assassination;

namespace FoodTruckKiller.Core.DataLoader
{
    /// <summary>
    /// JSON 数据加载器：从 <c>Resources/Data/*.json</c> 加载并解析为 SO/POCO 实例。
    /// <para>使用 <see cref="Resources.Load{T}"/> 加载 <c>TextAsset</c>，
    /// 用 <see cref="JsonUtility"/> 解析。由于 JsonUtility 不支持顶层数组与字符串枚举，
    /// 这里使用内部 POCO 包装类（含 List 字段与 string 类型字段）做中转，
    /// 再用 <see cref="ScriptableObject.CreateInstance{T}()"/> 创建 SO 并手动填充字段。</para>
    /// <para>加载完成后通过静态字段提供全局访问。</para>
    /// </summary>
    public static class JsonDataLoader
    {
        // ---- 加载结果（全局访问） ----

        /// <summary>食谱列表。</summary>
        public static List<RecipeData> Recipes { get; private set; }

        /// <summary>顾客画像列表。</summary>
        public static List<CustomerProfile> CustomerProfiles { get; private set; }

        /// <summary>暗杀目标画像列表。</summary>
        public static List<TargetProfile> Targets { get; private set; }

        /// <summary>击杀方式列表。</summary>
        public static List<KillMethodData> KillMethods { get; private set; }

        /// <summary>食材列表。</summary>
        public static List<IngredientData> Ingredients { get; private set; }

        /// <summary>场景布局数据（M2 视觉）。</summary>
        public static SceneLayoutData SceneLayout { get; private set; }

        /// <summary>全局配置。</summary>
        public static GameConfig Config { get; private set; }

        /// <summary>是否已加载。</summary>
        public static bool IsLoaded { get; private set; }

        /// <summary>
        /// 一次性加载所有数据。幂等：已加载则直接返回。
        /// 应在 SceneBootstrapper 中于 GameEvents.Init 之后、各系统初始化之前调用。
        /// </summary>
        public static void LoadAll()
        {
            if (IsLoaded) return;

            Config = LoadConfig();
            Recipes = LoadRecipes();
            CustomerProfiles = LoadCustomers();
            Targets = LoadTargets();
            KillMethods = LoadKillMethods();
            Ingredients = LoadIngredients();
            SceneLayout = LoadSceneLayout();

            IsLoaded = true;

            Debug.Log($"[JsonDataLoader] LoadAll complete: " +
                      $"recipes={Recipes.Count}, customers={CustomerProfiles.Count}, " +
                      $"targets={Targets.Count}, killMethods={KillMethods.Count}, " +
                      $"ingredients={Ingredients.Count}.");
        }

        // ---- 各数据加载 ----

        private static GameConfig LoadConfig()
        {
            TextAsset ta = Resources.Load<TextAsset>("Data/gameconfig");
            if (ta == null)
            {
                Debug.LogWarning("[JsonDataLoader] gameconfig.json not found, using defaults.");
                return new GameConfig();
            }
            var cfg = JsonUtility.FromJson<GameConfig>(ta.text);
            return cfg ?? new GameConfig();
        }

        private static List<RecipeData> LoadRecipes()
        {
            TextAsset ta = Resources.Load<TextAsset>("Data/recipes");
            var result = new List<RecipeData>();
            if (ta == null)
            {
                Debug.LogWarning("[JsonDataLoader] recipes.json not found.");
                return result;
            }

            var wrapper = JsonUtility.FromJson<RecipeListWrapper>(ta.text);
            if (wrapper?.recipes == null) return result;

            foreach (var p in wrapper.recipes)
            {
                var so = ScriptableObject.CreateInstance<RecipeData>();
                so.id = p.id;
                so.name = p.name;
                so.ingredients = p.ingredients ?? new List<string>();
                so.price = p.price;
                so.type = ParseEnum<RecipeType>(p.type, RecipeType.Normal);
                so.cookDuration = p.cookDuration > 0f ? p.cookDuration : 5f;
                result.Add(so);
            }
            return result;
        }

        private static List<CustomerProfile> LoadCustomers()
        {
            TextAsset ta = Resources.Load<TextAsset>("Data/customers");
            var result = new List<CustomerProfile>();
            if (ta == null)
            {
                Debug.LogWarning("[JsonDataLoader] customers.json not found.");
                return result;
            }

            var wrapper = JsonUtility.FromJson<CustomerListWrapper>(ta.text);
            if (wrapper?.customers == null) return result;

            foreach (var p in wrapper.customers)
            {
                var so = ScriptableObject.CreateInstance<CustomerProfile>();
                so.id = p.id;
                so.type = ParseEnum<CustomerType>(p.type, CustomerType.Normal);
                so.probability = p.probability;
                so.patienceSec = p.patienceSec;
                so.paymentMin = p.paymentMin;
                so.paymentMax = p.paymentMax;
                so.preferredRecipeId = p.preferredRecipeId;
                result.Add(so);
            }
            return result;
        }

        private static List<TargetProfile> LoadTargets()
        {
            TextAsset ta = Resources.Load<TextAsset>("Data/targets");
            var result = new List<TargetProfile>();
            if (ta == null)
            {
                Debug.LogWarning("[JsonDataLoader] targets.json not found.");
                return result;
            }

            var wrapper = JsonUtility.FromJson<TargetListWrapper>(ta.text);
            if (wrapper?.targets == null) return result;

            foreach (var p in wrapper.targets)
            {
                var so = ScriptableObject.CreateInstance<TargetProfile>();
                so.id = p.id;
                so.targetName = p.name;
                so.favRecipe = p.favFood;
                so.baitRecipe = p.baitRecipe;
                so.weakpoint = p.weakpoint;
                so.reward = p.reward;
                result.Add(so);
            }
            return result;
        }

        private static List<KillMethodData> LoadKillMethods()
        {
            TextAsset ta = Resources.Load<TextAsset>("Data/killmethods");
            var result = new List<KillMethodData>();
            if (ta == null)
            {
                Debug.LogWarning("[JsonDataLoader] killmethods.json not found.");
                return result;
            }

            var wrapper = JsonUtility.FromJson<KillMethodListWrapper>(ta.text);
            if (wrapper?.killmethods == null) return result;

            foreach (var p in wrapper.killmethods)
            {
                var so = ScriptableObject.CreateInstance<KillMethodData>();
                so.id = p.id;
                so.killName = p.name;
                so.damage = p.damage;
                so.noiseRadius = p.noiseRadius;
                so.evidenceType = MapEvidenceType(p.evidenceType);
                result.Add(so);
            }
            return result;
        }

        private static List<IngredientData> LoadIngredients()
        {
            TextAsset ta = Resources.Load<TextAsset>("Data/ingredients");
            var result = new List<IngredientData>();
            if (ta == null)
            {
                Debug.LogWarning("[JsonDataLoader] ingredients.json not found.");
                return result;
            }

            var wrapper = JsonUtility.FromJson<IngredientListWrapper>(ta.text);
            if (wrapper?.ingredients == null) return result;

            foreach (var p in wrapper.ingredients)
            {
                var so = ScriptableObject.CreateInstance<IngredientData>();
                so.id = p.id;
                so.displayName = p.name;
                so.isIllegal = p.isIllegal;
                result.Add(so);
            }
            return result;
        }

        /// <summary>
        /// 加载场景布局 JSON：tileSize=0.03125，地图 60x40 tile = 1.875x1.25 世界单位。
        /// 加载时把坐标**放大 32 倍**（×32）转换为与 PPU=32 一致的"1 sprite = 1 单位"世界坐标。
        /// </summary>
        private static SceneLayoutData LoadSceneLayout()
        {
            TextAsset ta = Resources.Load<TextAsset>("Data/scene_layout");
            if (ta == null)
            {
                Debug.LogWarning("[JsonDataLoader] scene_layout.json not found, using defaults.");
                return ScriptableObject.CreateInstance<SceneLayoutData>();
            }

            var raw = JsonUtility.FromJson<SceneLayoutRaw>(ta.text);
            var so = ScriptableObject.CreateInstance<SceneLayoutData>();
            if (raw == null) return so;

            // scene_layout.json 的 tileSize=0.03125 是"60 像素 PPU 误用"。
            // 本项目 PPU=32、1 sprite=1 单位，此处**保持原 JSON 数值**不放大，
            // TilemapBuilder 会按 1.875×1.25 世界范围贴小背景块（详见该类实现）。
            so.mapWidthUnits  = raw.map.worldWidth;
            so.mapHeightUnits = raw.map.worldHeight;
            so.playerStart    = new Vector2(raw.playerStart.x, raw.playerStart.y);
            so.cookingStation = new Vector2(raw.cookingStation.x, raw.cookingStation.y);
            so.queuePoints = new List<Vector2>();
            foreach (var p in raw.queuePoints) so.queuePoints.Add(new Vector2(p.x, p.y));
            so.customerSpawnPoints = new List<Vector2>();
            foreach (var p in raw.customerSpawnPoints) so.customerSpawnPoints.Add(new Vector2(p.x, p.y));
            so.customerExitPoint = new Vector2(raw.customerExitPoint.x, raw.customerExitPoint.y);
            so.policePatrolWaypoints = new List<Vector2>();
            foreach (var p in raw.policePatrolWaypoints) so.policePatrolWaypoints.Add(new Vector2(p.x, p.y));
            so.disposalGrinder  = new Vector2(raw.disposalPoints.grinder.x,  raw.disposalPoints.grinder.y);
            so.disposalFreezer  = new Vector2(raw.disposalPoints.freezer.x,  raw.disposalPoints.freezer.y);
            so.disposalDump     = new Vector2(raw.disposalPoints.dump.x,     raw.disposalPoints.dump.y);
            so.envKillGasCanister = new Vector2(raw.environmentKillObjects.gasCanister.x, raw.environmentKillObjects.gasCanister.y);
            so.envKillBillboard   = new Vector2(raw.environmentKillObjects.billboard.x,   raw.environmentKillObjects.billboard.y);
            so.inspectorEntry   = new Vector2(raw.inspectorEntry.x, raw.inspectorEntry.y);
            so.regions = new List<SceneRegion>();
            foreach (var r in raw.regions)
            {
                so.regions.Add(new SceneRegion
                {
                    name = r.name,
                    tileType = r.tileType,
                    x = r.x, y = r.y, w = r.w, h = r.h,
                });
            }
            return so;
        }

        // ---- 辅助 ----

        /// <summary>
        /// 将字符串安全解析为枚举，失败时返回默认值。
        /// </summary>
        private static T ParseEnum<T>(string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            if (Enum.TryParse(value, true, out T result)) return result;
            return defaultValue;
        }

        /// <summary>
        /// JSON 中 evidenceType 取值为 None/Bloodstain/ExplosionTrace，
        /// 需映射到 <see cref="EvidenceType"/> 枚举。
        /// </summary>
        private static EvidenceType MapEvidenceType(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return EvidenceType.None;
            switch (raw)
            {
                case "None": return EvidenceType.None;
                case "Bloodstain": return EvidenceType.Blood;
                case "ExplosionTrace": return EvidenceType.Explosion;
                default:
                    if (Enum.TryParse(raw, true, out EvidenceType e)) return e;
                    return EvidenceType.None;
            }
        }

        // ---- POCO 包装类（对齐 JSON 结构） ----

        [Serializable]
        private class RecipeListWrapper { public List<RecipePoco> recipes; }

        [Serializable]
        private class RecipePoco
        {
            public string id;
            public string name;
            public List<string> ingredients;
            public int price;
            public string type;
            public float cookDuration;
        }

        [Serializable]
        private class CustomerListWrapper { public List<CustomerPoco> customers; }

        [Serializable]
        private class CustomerPoco
        {
            public string id;
            public string type;
            public float probability;
            public float patienceSec;
            public int paymentMin;
            public int paymentMax;
            public string preferredRecipeId;
        }

        [Serializable]
        private class TargetListWrapper { public List<TargetPoco> targets; }

        [Serializable]
        private class TargetPoco
        {
            public string id;
            public string name;
            public string favFood;
            public string weakpoint;
            public int reward;
            public string baitRecipe;
        }

        [Serializable]
        private class KillMethodListWrapper { public List<KillMethodPoco> killmethods; }

        [Serializable]
        private class KillMethodPoco
        {
            public string id;
            public string name;
            public float damage;
            public float noiseRadius;
            public string evidenceType;
        }

        [Serializable]
        private class IngredientListWrapper { public List<IngredientPoco> ingredients; }

        [Serializable]
        private class IngredientPoco
        {
            public string id;
            public string name;
            public bool isIllegal;
        }
    }

    // ========== 场景布局（M2 视觉）==========

    /// <summary>JSON 原始结构（与 scene_layout.json 一一对应）。</summary>
    [Serializable]
    public class SceneLayoutRaw
    {
        public SceneMapRaw map;
        public SceneVec2Raw playerStart;
        public SceneVec2Raw cookingStation;
        public List<SceneVec2Raw> queuePoints = new List<SceneVec2Raw>();
        public List<SceneVec2Raw> customerSpawnPoints = new List<SceneVec2Raw>();
        public SceneVec2Raw customerExitPoint;
        public List<SceneVec2Raw> policePatrolWaypoints = new List<SceneVec2Raw>();
        public SceneDisposalRaw disposalPoints;
        public SceneEnvKillRaw environmentKillObjects;
        public SceneVec2Raw inspectorEntry;
        public List<SceneRegionRaw> regions = new List<SceneRegionRaw>();
    }

    [Serializable] public class SceneMapRaw { public float worldWidth; public float worldHeight; }
    [Serializable] public class SceneVec2Raw { public float x; public float y; }
    [Serializable] public class SceneDisposalRaw { public SceneVec2Raw grinder; public SceneVec2Raw freezer; public SceneVec2Raw dump; }
    [Serializable] public class SceneEnvKillRaw { public SceneVec2Raw gasCanister; public SceneVec2Raw billboard; }
    [Serializable] public class SceneRegionRaw { public string name; public string tileType; public float x; public float y; public float w; public float h; }

    /// <summary>
    /// 场景布局数据：从 <c>scene_layout.json</c> 反序列化为 ScriptableObject。
    /// 包含地图尺寸 / 玩家起点 / 烹饪台 / 队列点 / 出生点 / 离场点 / 警察巡逻点 / 处置点 / 环境击杀点 / 区域。
    /// </summary>
    public class SceneLayoutData : ScriptableObject
    {
        public float mapWidthUnits = 1.875f;
        public float mapHeightUnits = 1.25f;
        public Vector2 playerStart = new Vector2(0.9375f, 0.40625f);
        public Vector2 cookingStation = new Vector2(0.875f, 0.40625f);
        public List<Vector2> queuePoints = new List<Vector2>();
        public List<Vector2> customerSpawnPoints = new List<Vector2>();
        public Vector2 customerExitPoint = new Vector2(0.9375f, 0.5f);
        public List<Vector2> policePatrolWaypoints = new List<Vector2>();
        public Vector2 disposalGrinder = new Vector2(0.1875f, 1.0f);
        public Vector2 disposalFreezer = new Vector2(1.03125f, 0.40625f);
        public Vector2 disposalDump = new Vector2(0.1875f, 1.09375f);
        public Vector2 envKillGasCanister = new Vector2(1.65625f, 0.28125f);
        public Vector2 envKillBillboard = new Vector2(1.75f, 0.375f);
        public Vector2 inspectorEntry = new Vector2(0.9375f, 0.0f);
        public List<SceneRegion> regions = new List<SceneRegion>();
    }

    /// <summary>区域定义（用于 tilemap 铺地）。</summary>
    [Serializable]
    public class SceneRegion
    {
        public string name;
        public string tileType;
        public float x, y, w, h;
    }
}
