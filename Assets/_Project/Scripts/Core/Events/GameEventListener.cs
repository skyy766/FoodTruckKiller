using UnityEngine;
using UnityEngine.Events;

namespace FoodTruckKiller.Core.Events
{
    /// <summary>
    /// 事件监听组件：将 GameEvent（ScriptableObject）转发为 UnityEvent。
    /// <para>挂载到需要响应事件的 GameObject 上，在 Inspector 中指定
    /// 监听的 GameEvent 资产与触发的 UnityEvent 回调。</para>
    /// <para>OnEnable 自动注册，OnDisable 自动注销，避免生命周期泄漏。</para>
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("监听的事件资产（ScriptableObject）")]
        [SerializeField] private GameEvent gameEvent;

        [Tooltip("事件触发时调用的响应")]
        [SerializeField] private UnityEvent response;

        /// <summary>当前监听的事件资产。</summary>
        public GameEvent GameEvent
        {
            get => gameEvent;
            set => gameEvent = value;
        }

        private void OnEnable()
        {
            if (gameEvent != null)
                gameEvent.Register(this);
        }

        private void OnDisable()
        {
            if (gameEvent != null)
                gameEvent.Unregister(this);
        }

        /// <summary>
        /// 由 GameEvent.Raise() 调用，触发 Inspector 绑定的 UnityEvent。
        /// </summary>
        public void OnEventRaised()
        {
            response?.Invoke();
        }
    }
}
