using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace XMSDK.Framework.Communication
{
    /// <summary>
    /// 标记一个后备字段(通常对应公开属性)为可自动同步的信号。参数为信号地址(比如 PLC/设备点位)。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ObservableSignalAttribute : Attribute
    {
        public string Address { get; }
        public ObservableSignalAttribute(string address)
        {
            Address = address;
        }
    }

    public class ObservableSignalCollection : IDisposable
    {
        /// <summary>
        /// 设定自动更新器的延迟，-1为不更新，0表示尽可能快循环，>0 为毫秒周期。
        /// </summary>
        protected int UpdateTimeout { get; set; } = 100;

        private readonly List<SignalEntry> _signals = new List<SignalEntry>();
        private CancellationTokenSource _cts;
        private Task _loopTask;
        private volatile bool _initialized;
        private readonly object _initLock = new object();

        /// <summary>
        /// 信号变更事件 (address, oldValue, newValue)
        /// </summary>
        public event Action<string, object, object> SignalChanged;

        private class SignalEntry
        {
            public string Address;            // 地址
            public FieldInfo BackingField;    // 后备字段
            public PropertyInfo Property;     // 公开属性 (可为空, 但优先使用属性设置器以复用 DataChanged 逻辑)
            public Type FieldType;            // 字段类型
        }

        /// <summary>
        /// 由派生类在构造函数最后调用，或在完成所有配置(例如修改 UpdateTimeout)后调用。
        /// </summary>
        protected void Initialize()
        {
            if (_initialized) return;
            lock (_initLock)
            {
                if (_initialized) return;

                CollectSignals();
                _initialized = true;
                if (UpdateTimeout != -1)
                {
                    StartLoop();
                }
            }
        }

        private void CollectSignals()
        {
            _signals.Clear();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var field in GetType().GetFields(flags))
            {
                var attr = field.GetCustomAttribute<ObservableSignalAttribute>(true);
                if (attr == null) continue;
                // 寻找同名(去掉前导下划线 & 首字母大写)属性
                PropertyInfo prop = null;
                var baseName = field.Name.TrimStart('_');
                // 常见模式: _d2837 -> D2837
                var candidate = char.ToUpperInvariant(baseName[0]) + baseName.Substring(1);
                prop = GetType().GetProperty(candidate, flags & ~BindingFlags.NonPublic | BindingFlags.Public);
                _signals.Add(new SignalEntry
                {
                    Address = attr.Address,
                    BackingField = field,
                    Property = prop,
                    FieldType = field.FieldType
                });
            }
        }

        private void StartLoop()
        {
            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => UpdateTask(_cts.Token));
        }

        private async Task UpdateTask(CancellationToken token)
        {
            // 每轮: 并发读所有信号(按类型分类)，再写回差异
            while (!token.IsCancellationRequested && UpdateTimeout != -1)
            {
                try
                {
                    var tasks = new List<Task>();
                    foreach (var s in _signals)
                    {
                        if (s.FieldType == typeof(bool))
                        {
                            tasks.Add(UpdateBoolSignalAsync(s, token));
                        }
                        // 未来扩展: int/float/string 等
                    }
                    if (tasks.Count > 0)
                        await Task.WhenAll(tasks).ConfigureAwait(false);

                    await OnAfterUpdateCycleAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnUpdateLoopException(ex);
                }

                if (UpdateTimeout == 0)
                {
                    await Task.Yield();
                }
                else if (UpdateTimeout > 0)
                {
                    try { await Task.Delay(UpdateTimeout, token).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                }
            }
        }

        private async Task UpdateBoolSignalAsync(SignalEntry entry, CancellationToken token)
        {
            var newVal = await GetBool(entry.Address).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            var current = (bool)entry.BackingField.GetValue(this);
            if (current != newVal)
            {
                // 优先使用属性设置器，确保 DataChanged 逻辑执行
                if (entry.Property != null && entry.Property.CanWrite)
                {
                    entry.Property.SetValue(this, newVal, null);
                }
                else
                {
                    var old = current;
                    entry.BackingField.SetValue(this, newVal);
                    RaiseSignalChanged(entry.Address, old, newVal);
                }
                await OnSignalUpdatedAsync(entry.Address, newVal, token).ConfigureAwait(false);
            }
        }

        protected virtual Task OnSignalUpdatedAsync(string address, object newValue, CancellationToken token)
        {
            return Task.CompletedTask; // 派生类可重写
        }

        protected virtual Task OnAfterUpdateCycleAsync(CancellationToken token)
        {
            return Task.CompletedTask; // 派生类可重写
        }

        protected virtual void OnUpdateLoopException(Exception ex)
        {
            // 可加入日志框架，这里简单忽略或Console输出
            System.Diagnostics.Debug.WriteLine($"[ObservableSignalCollection] Loop exception: {ex}");
        }

        private void RaiseSignalChanged(string address, object oldVal, object newVal)
        {
            SignalChanged?.Invoke(address, oldVal, newVal);
        }

        /// <summary>
        /// 统一的变更封装：只有在值不同才更新并触发事件。
        /// </summary>
        protected T DataChanged<T>(ref T sig, T value, string address = null)
        {
            // 值类型无需 null 检查；引用类型直接比较引用或 Equals
            if (EqualityComparer<T>.Default.Equals(sig, value)) return sig;
            var old = sig;
            sig = value;
            if (address != null)
            {
                RaiseSignalChanged(address, old, value);
            }
            return value;
        }

        // 原接口: 派生类实现实际读写逻辑(比如通过驱动/通信层)
        public virtual Task<bool> GetBool(string address)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> SetBool(string address, bool value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 主动刷新所有已登记信号(同步触发一轮)，不依赖定时器。可供外部调用。
        /// </summary>
        public Task ManualRefreshAsync(CancellationToken token = default)
        {
            return UpdateTask(token); // 利用同一逻辑
        }

        /// <summary>
        /// 停止自动轮询(若需要临时暂停)。
        /// </summary>
        public void StopAutoUpdate()
        {
            _cts?.Cancel();
            try { _loopTask?.Wait(200); } catch { /* ignore */ }
            _cts = null;
            _loopTask = null;
        }

        /// <summary>
        /// 重新启动(如修改 UpdateTimeout 后)。
        /// </summary>
        public void RestartAutoUpdate()
        {
            StopAutoUpdate();
            if (UpdateTimeout != -1)
            {
                StartLoop();
            }
        }

        public void Dispose()
        {
            StopAutoUpdate();
            GC.SuppressFinalize(this);
        }
    }
}