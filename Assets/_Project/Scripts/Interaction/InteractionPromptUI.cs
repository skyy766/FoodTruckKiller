using UnityEngine;
using UnityEngine.UI;
using FoodTruckKiller.Player;

namespace FoodTruckKiller.Interaction
{
    /// <summary>
    /// 交互提示 UI：当玩家朝向可交互物时显示"按 E 交互"提示。
    /// <para>由 PlayerInteractor 或外部系统调用 Show/Hide。</para>
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Tooltip("提示根节点（显示/隐藏整体）")]
        [SerializeField] private GameObject promptRoot;

        [Tooltip("提示文本组件")]
        [SerializeField] private Text promptText;

        [Tooltip("交互按键名")]
        [SerializeField] private string keyName = "E";

        private void Awake()
        {
            Hide();
        }

        /// <summary>显示提示，promptName 来自 IInteractable.GetPromptName()。</summary>
        public void Show(string promptName)
        {
            if (promptRoot != null) promptRoot.SetActive(true);
            if (promptText != null)
                promptText.text = $"按 {keyName} {promptName}";
        }

        /// <summary>隐藏提示。</summary>
        public void Hide()
        {
            if (promptRoot != null) promptRoot.SetActive(false);
        }
    }
}
