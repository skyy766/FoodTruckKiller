# 餐车杀手 Food Truck Killer — 测试用例清单

> 文档版本：v1.0
> 维护：QA/测试工程师 A6
> 适用范围：游戏核心玩法覆盖（经营、击杀、尸体处理、检测、结算）
> 优先级定义：P0（阻塞）/ P1（关键）/ P2（重要）/ P3（一般）

---

## 用例总览

| 用例 ID | 模块 | 用例标题 | 优先级 |
|---------|------|----------|--------|
| TC-001 | 经营 | 正常出餐获得金钱 | P0 |
| TC-002 | 经营 | 错误订单无奖励 | P0 |
| TC-003 | 击杀 | 诱饵餐吸引目标到死角 | P0 |
| TC-004 | 击杀 | 近战击杀执行 | P0 |
| TC-005 | 击杀 | 煤气罐环境击杀 | P1 |
| TC-006 | 击杀 | 广告牌环境击杀 | P1 |
| TC-007 | 尸体处理 | 尸体绞肉处理（产出人肉食材） | P0 |
| TC-008 | 尸体处理 | 尸体丢弃处理 | P1 |
| TC-009 | 尸体处理 | 尸体冷库暂存 | P1 |
| TC-010 | 检测 | 警察视野内发现尸体触发通缉 | P0 |
| TC-011 | 检测 | 卫生检查员发现人肉食材判定失败 | P0 |
| TC-012 | 检测 | 通缉后无法营业 | P1 |
| TC-013 | 结算 | 时间到结算胜利（达成目标） | P0 |
| TC-014 | 结算 | 时间到结算失败（未达成） | P0 |
| TC-015 | 伪装 | 伪装度耗尽后果 | P1 |
| TC-016 | 经营 | 顾客生成（按概率生成不同类型顾客） | P1 |
| TC-017 | 经营 | 顾客点单推送订单到烹饪控制器 | P0 |
| TC-018 | 经营 | 正确出餐触发加钱（OnOrderServed 链路） | P0 |
| TC-019 | 经营 | 错误订单不出餐不加钱 | P0 |
| TC-020 | 经营 | 顾客耐心超时离开 | P1 |

---

## TC-001 正常出餐获得金钱

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-001 |
| 模块 | 经营 / Cooking |
| 优先级 | P0 |
| 前置条件 | 1. 进入经营场景（白天）。 2. 餐车食材库存充足（含 Bun、Meat、Lettuce）。 3. EconomyManager.Money 初始值已知（记为 M0）。 4. 至少一名顾客入队点单。 |
| 操作步骤 | 1. 等待顾客点单，记录订单食谱（假设为经典汉堡 ["Bun","Meat","Lettuce"]）。 2. 玩家从库存取出对应食材，按顺序组装到砧板/出餐台。 3. 调用 OrderValidator.Validate(assembled, recipe) 校验。 4. 提交出餐给顾客。 5. 等待顾客接收并付款。 |
| 预期结果 | 1. OrderValidator.Validate 返回 true。 2. 顾客状态变为"已出餐"，播放进食动画。 3. EconomyManager.AddMoney(订单金额) 被调用，Money 增加订单奖励值。 4. 顾客付款后离场，队列前移。 5. 无警戒值增加。 |
| 备注 | 覆盖 EditMode OrderValidatorTests + PlayMode CustomerFlowTests.Customer_OrderServedCorrectly_PaysAndLeaves |

---

## TC-002 错误订单无奖励

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-002 |
| 模块 | 经营 / Cooking |
| 优先级 | P0 |
| 前置条件 | 1. 进入经营场景。 2. 库存含 Bun、Fish 等食材。 3. EconomyManager.Money = M0。 4. 顾客点单为汉堡，但玩家故意组装错误序列。 |
| 操作步骤 | 1. 顾客点单汉堡 ["Bun","Meat","Lettuce"]。 2. 玩家组装错误食材 ["Bun","Fish"]（缺 Lettuce、错用 Fish）。 3. 调用 OrderValidator.Validate 校验。 4. 提交出餐给顾客。 |
| 预期结果 | 1. OrderValidator.Validate 返回 false。 2. 顾客拒绝接收订单。 3. EconomyManager.Money 不变（M0）。 4. 顾客状态变为"愤怒"，可能离场或降低满意度。 5. 无奖励、无警戒值变化。 |
| 备注 | 覆盖 EditMode OrderValidatorTests.Validate_WrongIngredients_ReturnsFalse + PlayMode CustomerFlowTests.Customer_OrderServedWrong_NoPaymentAndLeavesAngry |

---

## TC-003 诱饵餐吸引目标到死角

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-003 |
| 模块 | 击杀 / Assassination |
| 优先级 | P0 |
| 前置条件 | 1. 进入击杀阶段，目标 NPC 已在场景中巡逻。 2. 场景存在死角区域（如小巷、餐车后方）。 3. 玩家持有诱饵餐道具。 4. 目标对诱饵餐有偏好（目标配置中 BaitAffinity = true）。 |
| 操作步骤 | 1. 玩家移动到死角附近。 2. 在死角位置投放诱饵餐（BaitSystem.PlaceBait(position)）。 3. 等待目标 AI 检测到诱饵。 4. 观察目标移动路径。 |
| 预期结果 | 1. 目标 AI 检测到诱饵后，移动目标切换为诱饵位置。 2. 目标沿最短路径移动到死角。 3. 目标到达后停留在诱饵位置进食（状态 = Eating）。 4. 期间目标视野朝向死角外，玩家可从背后接近。 5. 无其他 NPC 目击（死角无路人）。 |
| 备注 | 覆盖 PlayMode AssassinationFlowTests.Bait_MeleeKill_SpawnsCorpse 前置部分 |

---

## TC-004 近战击杀执行

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-004 |
| 模块 | 击杀 / Assassination |
| 优先级 | P0 |
| 前置条件 | 1. TC-003 已完成，目标在死角进食。 2. 玩家位于目标攻击范围内（距离 < 1.5m）。 3. 玩家持有近战武器（如菜刀）。 4. 场景无目击者。 |
| 操作步骤 | 1. 玩家按下攻击键。 2. KillExecutor.ExecuteMelee(target) 被调用。 3. 播放击杀动画。 4. 等待动画结束。 |
| 预期结果 | 1. 目标状态变为 IsDead = true。 2. 目标 GameObject 销毁或转为死亡状态。 3. CorpseSpawner 在目标位置生成 Corpse 实体。 4. Corpse 位置 = 目标死亡位置。 5. 由于无目击者，AlertSystem.CurrentAlert 不变。 6. 击杀计数 +1，任务目标更新。 |
| 备注 | 覆盖 PlayMode AssassinationFlowTests.Bait_MeleeKill_SpawnsCorpse |

---

## TC-005 煤气罐环境击杀

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-005 |
| 模块 | 击杀 / Assassination（环境击杀） |
| 优先级 | P1 |
| 前置条件 | 1. 场景中存在可互动煤气罐道具。 2. 玩家已投放诱饵吸引目标接近煤气罐（距离 < 2m）。 3. 玩家持有远程点火方式（如打火机/枪）。 4. 煤气罐爆炸半径 = 3m。 |
| 操作步骤 | 1. 等待目标进入爆炸范围。 2. 玩家对煤气罐执行点火交互（GasCanister.Ignite()）。 3. 等待爆炸动画与伤害结算。 |
| 预期结果 | 1. 煤气罐播放爆炸特效，对范围内所有实体造成致命伤害。 2. 目标 IsDead = true。 3. CorpseSpawner 在目标位置生成尸体。 4. 爆炸可能产生噪音，警戒值小幅增加（如 +20）。 5. 若玩家在爆炸范围内，玩家也会受伤/死亡（注意站位）。 6. 击杀计数 +1。 |
| 备注 | 覆盖 PlayMode AssassinationFlowTests.Bait_GasCanisterKill_SpawnsCorpse |

---

## TC-006 广告牌环境击杀

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-006 |
| 模块 | 击杀 / Assassination（环境击杀） |
| 优先级 | P1 |
| 前置条件 | 1. 场景中存在可坠落广告牌道具（位于高位）。 2. 玩家已投放诱饵引导目标经过广告牌下方。 3. 广告牌下方为必经路径。 4. 玩家可触发广告牌坠落（如剪断绳索/推倒支撑）。 |
| 操作步骤 | 1. 等待目标移动到广告牌正下方（x 坐标对齐）。 2. 玩家触发广告牌坠落交互（Billboard.TriggerCollapse()）。 3. 等待坠落动画与伤害结算。 |
| 预期结果 | 1. 广告牌从高位坠落，播放坠落动画。 2. 广告牌击中目标，目标 IsDead = true。 3. CorpseSpawner 在目标位置生成尸体。 4. 此击杀方式伪装为意外事故，警戒值不增加（或仅微量增加）。 5. 击杀计数 +1。 |
| 备注 | 覆盖 PlayMode AssassinationFlowTests.Bait_BillboardKill_SpawnsCorpse |

---

## TC-007 尸体绞肉处理（产出人肉食材）

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-007 |
| 模块 | 尸体处理 / Corpse |
| 优先级 | P0 |
| 前置条件 | 1. 场景中存在一具未被发现且未处理的尸体。 2. 餐车配备绞肉机设备。 3. 玩家可搬运尸体。 4. 库存中人肉食材数量 = 0。 |
| 操作步骤 | 1. 玩家接近尸体，按交互键搬运。 2. 搬运尸体到绞肉机前。 3. 对绞肉机执行投料交互（CorpseProcessor.Process(corpse, GrindMode)）。 4. 等待处理动画完成。 |
| 预期结果 | 1. 尸体从场景中移除（Corpse 销毁）。 2. InventoryManager 增加 N 个人肉食材（如 HumanMeat × 3）。 3. 绞肉机状态变为"已污染"（卫生检查风险）。 4. 不增加警戒值（前提：未被目击）。 5. 人肉食材可用于制作特殊食谱（高风险高回报）。 |
| 备注 | 涉及卫生检查 TC-011 的前置链路 |

---

## TC-008 尸体丢弃处理

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-008 |
| 模块 | 尸体处理 / Corpse |
| 优先级 | P1 |
| 前置条件 | 1. 场景中存在一具尸体。 2. 场景存在丢弃点（如垃圾桶、深井、河边）。 3. 玩家可搬运尸体。 |
| 操作步骤 | 1. 玩家搬运尸体到丢弃点。 2. 对丢弃点执行投放交互（CorpseDisposal.Dispose(corpse)）。 3. 等待投放动画完成。 |
| 预期结果 | 1. 尸体从场景中移除。 2. 不产出任何食材。 3. 丢弃点状态记录（避免短时间多次丢弃被察觉）。 4. 不增加警戒值（前提：未被目击）。 5. 场景尸体计数 -1。 |
| 备注 | 风险较低但无收益的处理方式 |

---

## TC-009 尸体冷库暂存

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-009 |
| 模块 | 尸体处理 / Corpse |
| 优先级 | P1 |
| 前置条件 | 1. 场景中存在一具尸体。 2. 餐车配备冷库（可暂存尸体）。 3. 冷库当前未满（容量未达上限）。 4. 玩家可搬运尸体。 |
| 操作步骤 | 1. 玩家搬运尸体到冷库前。 2. 对冷库执行存放交互（ColdStorage.Store(corpse)）。 3. 等待存放动画完成。 |
| 预期结果 | 1. 尸体从场景移入冷库（场景中不可见，冷库计数 +1）。 2. 尸体保持待处理状态，可后续取出绞肉或丢弃。 3. 冷库内尸体不会被警察视野检测（除非警察检查冷库）。 4. 不增加警戒值。 5. 冷库接近满时给出提示。 |
| 备注 | 临时中转方案，需后续处理（否则冷库满后无法再存） |

---

## TC-010 警察视野内发现尸体触发通缉

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-010 |
| 模块 | 检测 / Detection |
| 优先级 | P0 |
| 前置条件 | 1. 场景中存在一具未处理尸体（位于警察巡逻路径上）。 2. 警察 NPC 在巡逻，视野锥朝向尸体方向。 3. 警察视野范围 = 5m，视角 = 90°。 4. AlertSystem.WantedThreshold = 100，CurrentAlert = 0。 |
| 操作步骤 | 1. 警察巡逻到视野覆盖尸体的位置。 2. 警察视野检测命中尸体。 3. CorpseController.IsDiscovered = true。 4. AlertSystem.AddAlert(警戒增量) 被调用。 5. 等待警戒值累积超阈值。 |
| 预期结果 | 1. 尸体被标记为已发现（IsDiscovered = true）。 2. AlertSystem.CurrentAlert 增加（单次发现增量 +X）。 3. 若 CurrentAlert >= WantedThreshold，IsWanted = true（触发通缉）。 4. 通缉后 UI 显示通缉等级，警察进入追捕模式。 5. 触发 TC-012 通缉后无法营业。 |
| 备注 | 覆盖 EditMode AlertSystemTests + PlayMode DetectionFlowTests.Corpse_InPoliceVision_TriggersWanted |

---

## TC-011 卫生检查员发现人肉食材判定失败

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-011 |
| 模块 | 检测 / Detection |
| 优先级 | P0 |
| 前置条件 | 1. 卫生检查员 NPC 到访（定期事件）。 2. 餐车库存中含人肉食材（来自 TC-007 绞肉）。 3. 检查员会检查库存与设备。 4. BusinessManager.IsOpen = true。 |
| 操作步骤 | 1. 检查员入场，开始检查流程（HealthInspectorController.StartInspection()）。 2. 检查员巡视库存区域。 3. 检测到人肉食材（InventoryManager.Contains("HumanMeat")）。 4. 检查员判定。 |
| 预期结果 | 1. 检查员状态变为 InspectionFailed = true。 2. 触发卫生检查失败事件（营业许可证吊销）。 3. BusinessManager.IsOpen 强制设为 false（无法继续营业）。 4. 触发警戒值大幅增加（如 +80）或直接通缉。 5. UI 显示"卫生检查失败"警告。 6. 可能直接判定关卡失败（取决于关卡配置）。 |
| 备注 | 覆盖 PlayMode DetectionFlowTests.HealthInspector_FindsHumanMeat_FailsInspection |

---

## TC-012 通缉后无法营业

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-012 |
| 模块 | 检测 / Detection |
| 优先级 | P1 |
| 前置条件 | 1. AlertSystem.IsWanted = true（已触发通缉，如来自 TC-010）。 2. BusinessManager.IsOpen = true（营业中）。 3. 队列中有等待的顾客。 |
| 操作步骤 | 1. 通缉状态触发。 2. BusinessManager 监听到通缉事件。 3. 强制关闭营业。 4. 观察顾客队列。 |
| 预期结果 | 1. BusinessManager.IsOpen = false。 2. 顾客队列清空或全部离场（不下单、不付款）。 3. 新顾客不再生成。 4. 经营 UI 禁用（无法出餐、无法接单）。 5. 玩家只能选择逃脱/贿赂/重启关卡。 6. 警戒值衰减至阈值以下后，IsWanted 解除（如配置允许）。 |
| 备注 | 覆盖 PlayMode DetectionFlowTests.WantedState_BusinessDisabled |

---

## TC-013 时间到结算胜利（达成目标）

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-013 |
| 模块 | 结算 / GameManager |
| 优先级 | P0 |
| 前置条件 | 1. 关卡时间设置（如 DayLevel_01 总时长 = 8 分钟）。 2. 关卡目标：击杀指定目标 × 1 + 累计金钱 >= 500。 3. 当前已击杀目标 1 个，金钱 = 600。 4. 警戒值未触发通缉（IsWanted = false）。 5. 剩余时间 = 0 秒。 |
| 操作步骤 | 1. 时间倒计时归零。 2. GameManager 触发结算流程（GameManager.OnDayEnd()）。 3. 评估胜利条件。 |
| 预期结果 | 1. 击杀目标数 1 >= 1（满足）。 2. 金钱 600 >= 500（满足）。 3. 未通缉（满足）。 4. 结算界面显示"营业成功 / 任务完成"。 5. 显示统计：击杀数、金钱、剩余警戒、伪装度。 6. 解锁下一关或显示通关画面。 7. 存档更新。 |
| 备注 | 关键胜利路径，必须通过 |

---

## TC-014 时间到结算失败（未达成）

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-014 |
| 模块 | 结算 / GameManager |
| 优先级 | P0 |
| 前置条件 | 1. 关卡时长已耗尽。 2. 胜利条件之一未满足（如击杀目标数 0 < 1，或金钱 300 < 500，或 IsWanted = true）。 |
| 操作步骤 | 1. 时间倒计时归零。 2. GameManager 触发结算流程。 3. 评估胜利条件。 |
| 预期结果 | 1. 至少一项胜利条件未满足。 2. 结算界面显示"任务失败"。 3. 显示未达成项（如"目标未击杀"或"金钱不足"或"被通缉"）。 4. 提供重试 / 读档选项。 5. 不解锁下一关。 |
| 备注 | 关键失败路径，需明确反馈失败原因 |

---

## TC-015 伪装度耗尽后果

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-015 |
| 模块 | 伪装 / Economy（CoverSystem） |
| 优先级 | P1 |
| 前置条件 | 1. CoverSystem.Cover = 100（满值）。 2. 玩家进行多次可见行为（如在视野内搬运尸体、留下血迹）。 3. 每次可见行为降低 CoverSystem.Cover。 |
| 操作步骤 | 1. 玩家执行可见行为 1：在路人视野内搬运尸体（ReduceCover(40)）。 2. 玩家执行可见行为 2：留下血迹未清理（ReduceCover(40)）。 3. 玩家执行可见行为 3：在视野内挥舞武器（ReduceCover(30)）。 4. CoverSystem.Cover 降至 0。 |
| 预期结果 | 1. Cover 从 100 → 60 → 20 → 0（夹紧）。 2. Cover = 0 时 IsCompromised = true（玩家身份暴露）。 3. 触发警戒值大幅增加（如直接 +100 触发通缉）。 4. 警察立即接到报警并赶赴现场。 5. 路人 NPC 恐慌逃跑。 6. 进入 TC-012 通缉后无法营业状态。 7. 玩家可通过清理血迹/换装恢复部分 Cover（RestoreCover）。 |
| 备注 | 覆盖 EditMode EconomyTests.Cover_Zero_IsCompromisedTrue |

---

## TC-016 顾客生成（按概率生成不同类型顾客）

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-016 |
| 模块 | 经营 / Customer |
| 优先级 | P1 |
| 前置条件 | 1. CustomerSpawner.profiles 已由 SceneBootstrapper 从 JsonDataLoader.CustomerProfiles 注入。 2. availableRecipes 已注入。 3. spawnInterval 与 maxDailySpawn 配置就绪。 4. QueueManager 单例存在。 |
| 操作步骤 | 1. CustomerSpawner.Update 计时器到达 spawnInterval。 2. TrySpawn() 按概率权重 PickProfile() 挑选画像。 3. CreateCustomer() 生成顾客 GameObject（沙箱无预制体时运行时构造 Rigidbody2D + CustomerAI）。 4. GetRecipeFor(profile) 按偏好挑选食谱，构造 Order。 5. ai.Initialize(profile, order)。 6. QueueManager.AssignSlot(ai) 分配队列位置。 7. ai.ChangeState(new QueuingState)。 |
| 预期结果 | 1. SpawnedCount +1。 2. 顾客 GameObject 在场景中存在，挂载 CustomerAI + Rigidbody2D�� 3. 顾客 CurrentOrder 非 null（除非 availableRecipes 为空）。 4. QueueIndex >= 0。 5. 顾客状态机进入 QueuingState。 6. 达到 maxDailySpawn 后停止生成。 7. OnDayEnd 触发后 ResetDaily 重置计数。 |
| 备注 | 生成器主体由 A2 实现；本用例验证生成结果��非概率分布（概率分布需大量样本统计测试） |

---

## TC-017 顾客点单推送订单到烹饪控制器

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-017 |
| 模块 | 经营 / Customer + Cooking |
| 优先级 | P0 |
| 前置条件 | 1. 顾客已到达窗口（QueuingState → OrderingState → WaitingState）。 2. 顾客 CurrentOrder 已创建。 3. CookingController 单例存在且 CurrentOrder 为 null 或上一单已 Served。 |
| 操作步骤 | 1. WaitingState.OnEnter 调用 TryPushOrderToController()。 2. 检测 CookingController.CurrentOrder == null 或 .State == Served。 3. controller.SetCurrentOrder(_owner.CurrentOrder)。 4. SetCurrentOrder 内触发 GameEvents.OnOrderIn.Raise()。 |
| 预期结果 | 1. CookingController.CurrentOrder == 顾客订单。 2. Assembling 列表被清空。 3. State 切到 Idle。 4. GameEvents.OnOrderIn 被触发（可被 AudioFeedbackBinder 订阅播点单音效）。 5. 控制器忙碌时（已有进行中订单）不覆盖，顾客保持等待并在每帧重试。 |
| 备注 | 覆盖 PlayMode BusinessLoopTests.OrderServedCorrectly_AddsMoney 前置部分（SetCurrentOrder 调用） |

---

## TC-018 正确出餐触发加钱（OnOrderServed 链路）

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-018 |
| 模块 | 经营 / Cooking + Economy |
| 优先级 | P0 |
| 前置条件 | 1. CookingController.CurrentOrder 已设置，State = Pending。 2. EconomyManager 单例已订阅 GameEvents.OnOrderServed。 3. EconomyManager.Money = M0（默认 100）。 4. 订单食谱 price = 20。 |
| 操作步骤 | 1. 玩家进入组装台（EnterWorkstation(Assemble)）。 2. AutoFillAssemblingFromRecipe 从食谱填充 Assembling。 3. TryAssemble 调用 OrderValidator.Validate → true → CurrentOrder.MarkReady()。 4. 玩家进入出餐台（EnterWorkstation(Serve)）。 5. TryServe 检测 State == Ready → CurrentOrder.MarkServed()。 6. GameEvents.OnServe.Raise() + GameEvents.OnOrderServed.Raise()。 7. EconomyManager.HandleOrderServed 读取 CookingController.Instance.CurrentOrder，确认 State == Served，调用 AddMoney(price, Income, order)。 |
| 预期结果 | 1. CurrentOrder.State == Served。 2. EconomyManager.Money == M0 + 20。 3. Transactions 新增一笔：amount=20, type=Income, order=当前订单。 4. GameEvents.OnCash 被 Raise（AddMoney 内部触发金币音效）。 5. CoverSystem.HandleOrderServed 触发 Cover 小额回升 +1。 |
| 备注 | 覆盖 PlayMode BusinessLoopTests.OrderServedCorrectly_AddsMoney |

---

## TC-019 错误订单不出餐不加钱

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-019 |
| 模块 | 经营 / Cooking + Economy |
| 优先级 | P0 |
| 前置条件 | 1. CookingController.CurrentOrder 已设置，State = Pending。 2. EconomyManager.Money = M0。 3. 食谱要求 ["Bun","Meat","Lettuce"]。 |
| 操作步骤 | 1. 玩家手动投入错误食材（AddIngredient("Bun"), AddIngredient("Fish"), AddIngredient("Lettuce")）。 2. TryAssemble 调用 OrderValidator.Validate → false（Fish 不在食谱）。 3. Assembling 被清空，State 回到 Idle，CurrentOrder.State 仍为 Pending。 4. 玩家进入出餐台（EnterWorkstation(Serve)）。 5. TryServe 检测 State != Ready → 不出餐，不触发 OnOrderServed。 |
| 预期结果 | 1. CurrentOrder.State 仍为 Pending（未 Ready、未 Served）。 2. EconomyManager.Money == M0（不变）。 3. Transactions 不新增。 4. GameEvents.OnOrderServed 未被触发。 5. GameEvents.OnCash 未被触发。 |
| 备注 | 覆盖 PlayMode BusinessLoopTests.WrongOrder_NoMoneyAdded + EditMode OrderValidatorTests.Validate_WrongIngredients_ReturnsFalse |

---

## TC-020 顾客耐心超时离开

| 字段 | 内容 |
|------|------|
| 用例 ID | TC-020 |
| 模块 | 经营 / Customer |
| 优先级 | P1 |
| 前置条件 | 1. 顾客已进入 WaitingState，deadline = Time.time + profile.patienceSec。 2. 玩家长时间不出餐。 3. 顾客未被诱饵吸引（IsBaited = false）。 4. CurrentOrder.State != Served。 |
| 操作步骤 | 1. WaitingState.OnUpdate 每帧检查 Time.time >= _deadline。 2. 超时后 _owner.ChangeState(new LeavingState(_owner))。 3. LeavingState.OnUpdate 朝 ExitPoint 移动。 4. 到达 ExitPoint 后 CustomerSpawner.NotifyCustomerLeft(ai) + Object.Destroy(gameObject)。 |
| 预期结果 | 1. 顾客状态机从 WaitingState 切到 LeavingState。 2. QueueManager 释放该顾客的 QueueIndex。 3. CustomerSpawner._activeCustomers 移除该顾客。 4. 顾客 GameObject 销毁。 5. EconomyManager.Money 不变（未出餐）。 6. 不触发 OnOrderServed。 |
| 备注 | 耐心超时纯状态机行为，由 CustomerStates.WaitingState 实现；本用例对应 PlayMode 集成测试需配合时间快进验证 |

---

## 附录：测试用例与自动化测试映射

| 测试用例 | 对应自动化测试 | 类型 |
|----------|----------------|------|
| TC-001 | OrderValidatorTests.Validate_CorrectSequence_ReturnsTrue<br>BusinessLoopTests.OrderServedCorrectly_AddsMoney | EditMode + PlayMode |
| TC-002 | OrderValidatorTests.Validate_WrongIngredients_ReturnsFalse<br>BusinessLoopTests.WrongOrder_NoMoneyAdded | EditMode + PlayMode |
| TC-003 | AssassinationFlowTests.Bait_MeleeKill_SpawnsCorpse（前置） | PlayMode |
| TC-004 | AssassinationFlowTests.Bait_MeleeKill_SpawnsCorpse | PlayMode |
| TC-005 | AssassinationFlowTests.Bait_GasCanisterKill_SpawnsCorpse | PlayMode |
| TC-006 | AssassinationFlowTests.Bait_BillboardKill_SpawnsCorpse | PlayMode |
| TC-007 | （待补 CorpseProcessorTests） | EditMode |
| TC-008 | （待补 CorpseDisposalTests） | EditMode |
| TC-009 | （待补 ColdStorageTests） | EditMode |
| TC-010 | AlertSystemTests.AddAlert_AccumulatedExceedsThreshold_TriggersWanted<br>AlertSystemTests.AddAlert_AboveThreshold_RaisesGlobalOnWantedEvent<br>DetectionFlowTests.Corpse_InPoliceVision_TriggersWanted | EditMode + PlayMode |
| TC-011 | DetectionFlowTests.HealthInspector_FindsHumanMeat_FailsInspection | PlayMode |
| TC-012 | DetectionFlowTests.WantedState_BusinessDisabled | PlayMode |
| TC-013 | BusinessLoopTests.DayTimeEnd_TriggersSettlement_VictoryWhenObjectiveCleared | PlayMode |
| TC-014 | BusinessLoopTests.DayTimeEnd_TriggersSettlement_GameOverWhenObjectiveNotCleared | PlayMode |
| TC-015 | EconomyTests.ReduceCover_DoesNotGoBelowZero<br>EconomyTests.ApplyInspectFailPenalty_ReducesCover | EditMode |
| TC-016 | （待补 CustomerSpawnerTests，主体由 A2 CustomerSpawner 实现） | PlayMode |
| TC-017 | BusinessLoopTests.OrderServedCorrectly_AddsMoney（SetCurrentOrder 推送） | PlayMode |
| TC-018 | BusinessLoopTests.OrderServedCorrectly_AddsMoney<br>BusinessLoopTests.MultipleOrders_ServedSequentially_AccumulatesMoney | PlayMode |
| TC-019 | BusinessLoopTests.WrongOrder_NoMoneyAdded | PlayMode |
| TC-020 | （待补 CustomerPatienceTests，需配合时间快进验证 WaitingState 超时） | PlayMode |

---

## 修订记录

| 日期 | 版本 | 作者 | 变更 |
|------|------|------|------|
| 2026-07-07 | v1.0 | A6 qa-tester | 初始版本，15 个核心用例 |
| 2026-07-07 | v1.1 | A6 qa-tester | M1 经营循环：新增 TC-016~TC-020（顾客生成、点单推送、出餐加钱、错误订单、耐心超时）；更新自动化映射表，对齐 EditMode 测试到实际接口（OrderValidator/AlertSystem/EconomyManager/CoverSystem/RecipeData），新增 PlayMode BusinessLoopTests 覆盖经营结算流程 |
