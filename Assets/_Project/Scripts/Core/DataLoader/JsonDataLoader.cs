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
}
