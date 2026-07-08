#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace FoodTruckKiller.EditorTools
{
    /// <summary>
    /// 一键搭建白天关卡场景。
    /// 用法：菜单 FoodTruckKiller > 搭建白天关卡场景
    /// </summary>
    public static class SceneSetupWizard
    {
        [MenuItem("FoodTruckKiller/搭建白天关卡场景")]
        public static void SetupDayLevel()
        {
            // 创建空场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- Main Camera ----
            // 注意：摄像机由 SceneBootstrapper 统一创建并管理（size 4.5、position (0,0,-10)），
            // 此处不再创建，避免双相机冲突。
            Debug.Log("[餐车杀手] Camera 由 SceneBootstrapper 创建，跳过。");

            // ---- Directional Light (2D 场景可选) ----
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<UnityEngine.Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;

            // ---- SceneBootstrapper（运行时自动初始化所有系统）----
            var bootGo = new GameObject("[SceneBootstrapper]");
            bootGo.AddComponent<GameManager.SceneBootstrapper>();

            // ---- 保存场景 ----
            string sceneDir = "Assets/_Project/Scenes";
            Directory.CreateDirectory(sceneDir);
            string scenePath = Path.Combine(sceneDir, "DayLevel_01.unity");
            EditorSceneManager.SaveScene(scene, scenePath);

            // 加入 Build Settings
            var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!buildScenes.Exists(s => s.path == scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            Debug.Log("[餐车杀手] ✅ 白天关卡场景搭建完成！路径：Assets/_Project/Scenes/DayLevel_01.unity\n按 ▶ Play 即可运行。");
            EditorUtility.DisplayDialog("餐车杀手", "白天关卡场景搭建完成！\n\n场景已保存到 Assets/_Project/Scenes/DayLevel_01.unity\n\n按 ▶ Play 运行。", "好的");
        }

        [MenuItem("FoodTruckKiller/检查/编译状态与说明")]
        public static void ShowHelp()
        {
            Debug.Log(
                "[餐车杀手] 运行说明：\n" +
                "1. 确认 Package Manager 中已安装 Input System\n" +
                "2. 菜单 FoodTruckKiller > 搭建白天关卡场景\n" +
                "3. 按 ▶ Play 运行\n" +
                "4. WASD 移动，走到烹饪台按 E 交互\n" +
                "5. 经营循环：顾客排队 → 烹饪出餐 → 赚钱\n\n" +
                "如有编译错误请检查：\n" +
                "- Unity 版本是否 6000.0 LTS\n" +
                "- URP 2D / Input System 包是否已安装\n" +
                "- Assets/_Project 和 Assets/Resources 是否完整复制"
            );
        }

        /// <summary>用反射尝试添加组件，类型不存在则跳过（不报错）</summary>
        private static void TryAddComponent(GameObject go, string typeName)
        {
            var type = System.Type.GetType(typeName);
            if (type != null)
            {
                go.AddComponent(type);
            }
            else
            {
                Debug.LogWarning($"[餐车杀手] 未找到类型 {typeName}，已跳过。可能需要手动添加组件。");
            }
        }
    }
}
#endif
