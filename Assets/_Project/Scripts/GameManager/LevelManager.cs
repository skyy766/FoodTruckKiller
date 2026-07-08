using UnityEngine;
using UnityEngine.SceneManagement;

namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// 关卡管理器：负责关卡（白天场景）的加载与重置。
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Tooltip("白天经营关卡场景名")]
        [SerializeField] private string dayLevelSceneName = "DayLevel_01";

        /// <summary>初始化（由 GameManager.Boot 调用）。</summary>
        public void Init()
        {
            Debug.Log("[LevelManager] Init.");
        }

        /// <summary>加载指定名称的场景。</summary>
        public void LoadLevel(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>重新加载当前激活场景。</summary>
        public void ReloadCurrentLevel()
        {
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        /// <summary>加载默认白天关卡。</summary>
        public void LoadDayLevel()
        {
            if (!string.IsNullOrEmpty(dayLevelSceneName))
                LoadLevel(dayLevelSceneName);
        }
    }
}
