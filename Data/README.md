# Data 目录说明

本目录存放《餐车杀手 Food Truck Killer》的所有数据配置 JSON。当前阶段（沙箱无 Unity 编辑器）用 JSON 代替 `.asset` 资源，后续接入 Unity 时按下文 §3 转为 ScriptableObject。

## 文件清单

| 文件 | 内容 | 对应 ScriptableObject |
| --- | --- | --- |
| `recipes.json` | 食谱列表 | `RecipeDefSO` |
| `ingredients.json` | 食材列表 | `IngredientDefSO` |
| `customers.json` | 顾客类型 | `CustomerTypeDefSO` |
| `targets.json` | 暗杀目标 | `TargetDefSO` |
| `killmethods.json` | 击杀方式 | `KillMethodDefSO` |
| `gameconfig.json` | 全局配置 | `GameConfigSO`（单例） |

---

## 1. 各 JSON 结构与字段

### 1.1 recipes.json

```jsonc
{
  "recipes": [
    {
      "id": "burger",                // 食谱唯一 id
      "name": "汉堡",                // 显示名
      "ingredients": ["bread","patty","lettuce","bread"], // 按制作顺序的食材 id 数组
      "price": 15,                   // 售价（Bait/HumanMeat 为 0）
      "type": "Normal"               // Normal / Bait / HumanMeat
    }
  ]
}
```

- `ingredients` 顺序即玩家在烹饪台的投放顺序，程序据此判定是否匹配食谱。
- `type=Bait` 的食谱不直接售卖，用于把目标引离队伍；`type=HumanMeat` 仅在玩家选择用人肉处置时触发。

### 1.2 ingredients.json

```jsonc
{
  "ingredients": [
    { "id": "bread", "name": "面包", "isIllegal": false }
  ]
}
```

- `isIllegal=true` 的食材（人肉/毒药）被卫生检查员或警察发现时直接大幅提升通缉度（见 `Design/BalanceDesign.md` §4.3）。
- 进货价 `cost` 当前未入 JSON，平衡假设见 `BalanceDesign.md` §2.1，后续可在此对象追加 `"cost": 1`。

### 1.3 customers.json

```jsonc
{
  "customers": [
    {
      "id": "normal",
      "type": "Normal",              // Normal / Informant / Target
      "probability": 0.85,           // 生成权重（同帧归一化，三者之和应为 1.0）
      "patienceSec": 40,             // 排队耐心，超时离场
      "paymentMin": 10,
      "paymentMax": 20               // 付款随机区间 [min,max]，Informant 固定=min
    }
  ]
}
```

- `Informant` 付款为 `paymentMin`，并在离场时额外提供 1 点 **情报（intel）**——`intel` 为非金钱资源，不在本 JSON 中，由运行时 `InformantState` 累积，用于在 UI 解锁目标弱点提示。
- `Target` 付款为 0；该 slot 在「累计收入 < 50 元」时被运行时重投为 Normal（见 `BalanceDesign.md` §3）。

### 1.4 targets.json

```jsonc
{
  "targets": [
    {
      "id": "t_tony",
      "name": "Tony \"The Tooth\" Marzano",
      "favFood": "burger",           // 偏好的正常食谱 id，目标会点这道菜
      "weakpoint": "左侧假牙松动……", // 文案+机制提示
      "reward": 200,                 // 暗杀成功金钱奖励
      "baitRecipe": "bait_meal"      // 诱饵食谱 id，用此餐可引导目标走向击杀点
    }
  ]
}
```

- 目标的生成位置与击杀点坐标不在本 JSON 中，而由关卡布局决定（见 `Design/LevelDesign_DayLevel01.md` §5.4），程序按 `id` 关联到关卡布点表。

### 1.5 killmethods.json

```jsonc
{
  "killmethods": [
    {
      "id": "knife",
      "name": "近战厨刀",
      "damage": 100,                 // MVP 目标均为 100 血，一击必杀
      "noiseRadius": 2,              // 引起 AI 注意的格数半径，0=无声
      "evidenceType": "Bloodstain"   // Bloodstain / ExplosionTrace / None
    }
  ]
}
```

- `evidenceType` 决定现场遗留证据类型，影响检查员/警察判定（见 `BalanceDesign.md` §4.3）。

### 1.6 gameconfig.json

```jsonc
{
  "dayDurationSec": 480,        // 单关总时长
  "startMoney": 100,            // 初始金钱
  "coverMax": 100,              // 伪装度上限
  "coverRegenPerSec": 0.5,      // 伪装度自然恢复
  "wantedThreshold": 100,       // 通缉度失败阈值
  "alertDecayPerSec": 2,        // 瞬时警报衰减
  "policeVisionRange": 5,       // 警察视野距离（tile）
  "policeVisionAngle": 60,      // 警察视野锥角度（度）
  "inspectorIntervalSec": 120   // 卫生检查员最短造访间隔
}
```

- 字段含义与三层（cover/wanted/alert）数值关系详见 `Design/BalanceDesign.md` §4。

---

## 2. 字段命名约定

- `id` 全局唯一，小写蛇形（`bait_meal`、`t_tony`）。
- `type` / `evidenceType` 等枚举字段使用 PascalCase 字符串，程序侧用 `enum` 映射。
- 布尔字段前缀 `is`（`isIllegal`）。
- 时间字段后缀 `Sec`，速率字段后缀 `PerSec`。

---

## 3. 转换为 Unity ScriptableObject

### 3.1 通用流程

1. 在 Unity 项目 `Assets/_Project/Scripts/Data/` 下为每类数据创建 `ScriptableObject` 类，字段与 JSON 一一对应。
2. 使用 `JsonUtility`（内置，不支持顶层数组）或第三方 `Newtonsoft.Json` 反序列化。
   - 顶层为数组的 JSON（如 `recipes`）需包一层 `{ "recipes": [...] }`（本目录已这样组织）。
3. 编写一个编辑器菜单 `Tools/Data/Import JSON -> SO`，遍历本目录 JSON，为每条记录生成 `.asset` 文件到 `Assets/_Project/ScriptableObjects/<Type>/`。

### 3.2 示例：RecipeDefSO

```csharp
[CreateAssetMenu(menuName = "FTK/Recipe")]
public class RecipeDefSO : ScriptableObject
{
    public string id;
    public string displayName;            // name 与 SO.name 冲突，改名 displayName
    public IngredientDefSO[] ingredients; // 由食材 id 解析为 SO 引用
    public int price;
    public RecipeType type;               // enum { Normal, Bait, HumanMeat }
}
```

> 注意：`ingredients` 在 JSON 中是 id 字符串数组，导入时需建一张 `id -> IngredientDefSO` 的查找表，把字符串替换为 SO 引用，这样运行时无需再查表。`targets.json` 的 `favFood` / `baitRecipe` 同理解析为 `RecipeDefSO` 引用。

### 3.3 编辑器导入器骨架

```csharp
public static class DataImporter
{
    [MenuItem("Tools/Data/Import JSON -> SO")]
    static void ImportAll()
    {
        Import<IngredientList, IngredientDefSO>("Data/ingredients.json", "ingredients");
        Import<RecipeList,    RecipeDefSO>   ("Data/recipes.json",    "recipes");
        // ...其余同理
        AssetDatabase.SaveAssets();
    }

    static void Import<TList, TSO>(string jsonPath, string fieldName)
        where TList : class
        where TSO : ScriptableObject
    {
        var json = File.ReadAllText(Path.Combine(Application.dataPath, jsonPath));
        var list = JsonUtility.FromJson<TList>(json);
        // 反射或手写：遍历 list.fieldName，逐条 CreateInstance<TSO>()、赋值、AssetDatabase.CreateAsset
    }
}
```

### 3.4 GameConfig 单例

`gameconfig.json` 反序列化为单个 `GameConfigSO` 实例，运行时由 `GameManager` 持有引用，作为只读配置注入各系统。

---

## 4. 校验

所有 JSON 已通过 `python3 -c "json.load"` 语法校验。建议在 CI 中加入 `jq empty *.json` 或 Unity 导入器的 `try/catch` 兜底，防止字段缺失导致运行时崩溃。
