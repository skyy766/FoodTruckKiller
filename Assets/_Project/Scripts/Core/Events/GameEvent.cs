using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodTruckKiller.Core.Events
{
    /// <summary>
    /// 基于 ScriptableObject 的事件资产。
    /// <para>跨系统通信的核心通道：发布者调用 Raise()，订阅者通过
    /// GameEventListener（Inspector 绑定 UnityEvent）或
    /// Register(Action)/Unregister(Action)（代码监听）接���事件。</para>
    /// <para>禁止跨系统直接 GetComponent，一切解耦通信走 GameEvent。</para>
    /// </summary>
    [CreateAssetMenu(fileName = "GameEvent", menuName = "FoodTruckKiller/Events/GameEvent", order = 0)]
    public class GameEvent : ScriptableObject
    {
        /// <summary>Inspector 绑定型监听者列表（GameEventListener）。</summary>
        private readonly List<GameEventListener> listeners = new List<GameEventListener>();

        /// <summary>代码型监听者（Action）。</summary>
        private event Action onRaised;

        /// <summary>
        /// 注册 GameEventListener（通常由 GameEventListener.OnEnable 调用）。
        /// </summary>
        public void Register(GameEventListener listener)
        {
            if (listener == null) return;
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        /// <summary>
        /// 注销 GameEventListener（通常由 GameEventListener.OnDisable 调用）。
        /// </summary>
        public void Unregister(GameEventListener listener)
        {
            if (listener == null) return;
            listeners.Remove(listener);
        }

        /// <summary>
        /// 注册代码监听者（供除 GameEventListener 外的系统使用）。
        /// </summary>
        public void Register(Action callback)
        {
            if (callback == null) return;
            onRaised += callback;
        }

        /// <summary>
        /// 注销代码监听者。
        /// </summary>
        public void Unregister(Action callback)
        {
            if (callback == null) return;
            onRaised -= callback;
        }

        /// <summary>
        /// 触发事件：逆序遍历 GameEventListener，再调用 Action 监听者。
        /// </summary>
        public void Raise()
        {
            // 逆序遍历，允许监听者在回调中安全注销自身
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                if (i < listeners.Count && listeners[i] != null)
                    listeners[i].OnEventRaised();
            }
            onRaised?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>编辑器辅助：当前监听者数量（调试用）。</summary>
        public int ListenerCount => listeners.Count;
#endif
    }
}
