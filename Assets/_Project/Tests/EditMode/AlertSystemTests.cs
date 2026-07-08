using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using FoodTruckKiller.Core.Events;
using FoodTruckKiller.Core.Singleton;
using FoodTruckKiller.Detection;

namespace FoodTruckKiller.Tests.EditMode
{
    /// <summary>
    /// AlertSystem 警戒值系统单元测试。
    /// 实际接口：
    ///   - AlertSystem : SingletonMono&lt;AlertSystem&gt;（需 GameObject + AddComponent）
    ///   - float Alert { get; private set; } （0~100，上限 100）
    ///   - bool WantedRaised { get; private set; }
    ///   - void AddAlert(float amount)  （amount &lt;= 0 忽略）
    ///   - void ClearWanted()           （重置 Alert=0、WantedRaised=false）
    ///   - event Action&lt;float&gt; OnAlertChangedEvent
    ///   - [SerializeField] private float wantedThreshold = 60f
    ///   - [SerializeField] private float decayPerSec = 2f
    ///   - Update() 每帧自然衰减（EditMode 不会被 Unity 自动调用，用反射驱动）
    /// 命名空间：FoodTruckKiller.Detection
    /// </summary>
    [TestFixture]
    public class AlertSystemTests
    {
        private AlertSystem _alertSystem;
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            // 确保静态 GameEvents 已初始化（AddAlert 内部会 Raise OnWanted / OnAlertChanged）。
            GameEvents.Init();

            // 重置单例静态实例，避免上一个测试残留。
            ResetSingleton<AlertSystem>();

            _host = new GameObject("AlertSystem_Test");
            _alertSystem = _host.AddComponent<AlertSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            ResetSingleton<AlertSystem>();
        }

        /// <summary>
        /// 单次 AddAlert 低于阈值时不应触发通缉。
        /// </summary>
        [Test]
        public void AddAlert_BelowThreshold_DoesNotTriggerWanted()
        {
            SetThreshold(100f);

            _alertSystem.AddAlert(50f);

            Assert.IsFalse(_alertSystem.WantedRaised, "警戒值低于阈值不应触发通缉");
            Assert.AreEqual(50f, _alertSystem.Alert, "警戒值应为 50");
        }

        /// <summary>
        /// 多次 AddAlert 累积超过阈值应触发通缉。
        /// </summary>
        [Test]
        public void AddAlert_AccumulatedExceedsThreshold_TriggersWanted()
        {
            SetThreshold(100f);

            _alertSystem.AddAlert(60f);
            _alertSystem.AddAlert(60f);

            Assert.IsTrue(_alertSystem.WantedRaised, "累积警戒值超过阈值应触发通缉");
            Assert.GreaterOrEqual(_alertSystem.Alert, 100f);
            // 上限夹紧到 100
            Assert.AreEqual(100f, _alertSystem.Alert, "警戒值上限应为 100");
        }

        /// <summary>
        /// 警戒值等于阈值（边界值）应触发通缉（&gt;= 判定）。
        /// </summary>
        [Test]
        public void AddAlert_ExactlyAtThreshold_TriggersWanted()
        {
            SetThreshold(60f);

            _alertSystem.AddAlert(60f);

            Assert.IsTrue(_alertSystem.WantedRaised, "警戒值等于阈值应触发通缉（>= 判定）");
            Assert.AreEqual(60f, _alertSystem.Alert);
        }

        /// <summary>
        /// 警戒值低于阈值 1 点（边界值）不应触发通缉。
        /// </summary>
        [Test]
        public void AddAlert_OneBelowThreshold_DoesNotTriggerWanted()
        {
            SetThreshold(60f);

            _alertSystem.AddAlert(59f);

            Assert.IsFalse(_alertSystem.WantedRaised, "警戒值低于阈值 1 点不应触发通缉");
            Assert.AreEqual(59f, _alertSystem.Alert);
        }

        /// <summary>
        /// AddAlert 传入负值或零应被忽略（防御性编程：amount &lt;= 0 直接 return）。
        /// </summary>
        [Test]
        public void AddAlert_NegativeOrZero_Ignored()
        {
            _alertSystem.AddAlert(-20f);
            _alertSystem.AddAlert(0f);

            Assert.AreEqual(0f, _alertSystem.Alert, "负值或零不应改变警戒值");
            Assert.IsFalse(_alertSystem.WantedRaised);
        }

        /// <summary>
        /// 警戒值不应超过上限 100，防止溢出。
        /// </summary>
        [Test]
        public void AddAlert_DoesNotExceedMaxAlert()
        {
            _alertSystem.AddAlert(999f);

            Assert.LessOrEqual(_alertSystem.Alert, 100f, "警戒值不应超过上限 100");
            Assert.AreEqual(100f, _alertSystem.Alert);
        }

        /// <summary>
        /// 警戒值变化应触发 OnAlertChangedEvent 事件，参数为新值。
        /// </summary>
        [Test]
        public void AddAlert_RaisesOnAlertChangedEvent()
        {
            float captured = -1f;
            int callCount = 0;
            _alertSystem.OnAlertChangedEvent += v =>
            {
                captured = v;
                callCount++;
            };

            _alertSystem.AddAlert(30f);

            Assert.GreaterOrEqual(callCount, 1, "AddAlert 应触发 OnAlertChangedEvent");
            Assert.AreEqual(30f, captured, "事件参数应为最新警戒值");
        }

        /// <summary>
        /// ClearWanted 应重置警戒值与通缉标志。
        /// </summary>
        [Test]
        public void ClearWanted_ResetsAlertAndWantedFlag()
        {
            SetThreshold(50f);
            _alertSystem.AddAlert(80f);
            Assert.IsTrue(_alertSystem.WantedRaised);

            _alertSystem.ClearWanted();

            Assert.AreEqual(0f, _alertSystem.Alert, "ClearWanted 后警戒值应归零");
            Assert.IsFalse(_alertSystem.WantedRaised, "ClearWanted 后通缉标志应清除");
        }

        /// <summary>
        /// 触发通缉时应 Raise GameEvents.OnWanted（通过监听代码回调验证）。
        /// </summary>
        [Test]
        public void AddAlert_AboveThreshold_RaisesGlobalOnWantedEvent()
        {
            SetThreshold(50f);
            bool onWantedFired = false;
            Action handler = () => onWantedFired = true;
            GameEvents.OnWanted.Register(handler);

            try
            {
                _alertSystem.AddAlert(60f);
                Assert.IsTrue(onWantedFired, "超过阈值应触发 GameEvents.OnWanted");
            }
            finally
            {
                GameEvents.OnWanted.Unregister(handler);
            }
        }

        /// <summary>
        /// 衰减验证：通过反射调用 Update，由于 EditMode 下 Time.deltaTime 通常为 0，
        /// 衰减量为 0；此处主要验证 Update 不会错误增加警戒值，且 Alert 不超过初始值。
        /// 严格的衰减量验证交由 PlayMode BusinessLoopTests 进行。
        /// </summary>
        [Test]
        public void Alert_Update_DoesNotIncreaseAlert()
        {
            SetThreshold(100f);
            SetDecayPerSec(10f);
            _alertSystem.AddAlert(40f);
            float before = _alertSystem.Alert;

            // 反射调用私有 Update（EditMode 无帧循环，Time.deltaTime = 0）
            InvokeUpdate();

            Assert.LessOrEqual(_alertSystem.Alert, before, "Update 不应增加警戒值");
            Assert.GreaterOrEqual(_alertSystem.Alert, 0f, "警戒值不应为负");
        }

        // ---- 反射辅助 ----

        private void SetThreshold(float value)
        {
            var fi = typeof(AlertSystem).GetField("wantedThreshold",
                BindingFlags.NonPublic | BindingFlags.Instance);
            fi?.SetValue(_alertSystem, value);
        }

        private void SetDecayPerSec(float value)
        {
            var fi = typeof(AlertSystem).GetField("decayPerSec",
                BindingFlags.NonPublic | BindingFlags.Instance);
            fi?.SetValue(_alertSystem, value);
        }

        private void InvokeUpdate()
        {
            var mi = typeof(AlertSystem).GetMethod("Update",
                BindingFlags.NonPublic | BindingFlags.Instance);
            mi?.Invoke(_alertSystem, null);
        }

        private static void ResetSingleton<T>() where T : MonoBehaviour
        {
            var fi = typeof(SingletonMono<T>).GetField("instance",
                BindingFlags.NonPublic | BindingFlags.Static);
            fi?.SetValue(null, null);
        }
    }
}
