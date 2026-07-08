using UnityEngine;

namespace FoodTruckKiller.Core.Singleton
{
    /// <summary>
    /// 泛型单例基类。全局管理器（GameManager/SaveManager/UIManager 等）继承此类。
    /// <para>首次访问时若实例不存在，自动查找场景中已存在的实例。</para>
    /// <para>挂载在场景中的实例会被 DontDestroyOnLoad 保护，重复实例自动销毁。</para>
    /// </summary>
    /// <typeparam name="T">单例具体类型。</typeparam>
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static bool isQuitting;

        /// <summary>全局单例实例。</summary>
        public static T Instance
        {
            get
            {
                if (isQuitting) return instance;
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (instance != this as T)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>子类重写以执行初始化逻辑（仅首个实例调用一次）。</summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }
    }
}
