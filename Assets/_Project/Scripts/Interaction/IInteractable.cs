using FoodTruckKiller.Player;

namespace FoodTruckKiller.Interaction
{
    /// <summary>
    /// 可交互物接口。餐车操作台/食材箱/暗杀目标/诱饵餐放置点等实现此接口。
    /// <para>由 PlayerInteractor 在 OverlapCircle 检测命中后，于按下 Interact 时调用 OnInteract。</para>
    /// </summary>
    public interface IInteractable
    {
        /// <summary>玩家按下交互键时调用。</summary>
        /// <param name="player">发起交互的玩家控制器。</param>
        void OnInteract(PlayerController player);

        /// <summary>返回交互提示文本（如"烹饪""拾取""暗杀"）。</summary>
        string GetPromptName();
    }
}
