using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace XMSDK.Framework.Communication.Signal;

/// <summary>
/// 轮询信号宿主 V2：仅保留核心功能，使用泛型支持 bool/short/int/long 等值类型。
/// 不考虑旧版兼容性，API 简洁直接。
/// <para>
/// 该类提供了一个基于轮询的信号监控框架，支持注册多个信号点，定期读取这些信号的值，
/// 并在值发生变化时触发相应的事件。支持同步和异步的读写操作。
/// </para>
/// </summary>
/// <remarks>
/// 使用示例：
/// <code>
/// public class MySignalHost : PollingSignalHost
/// {
///     protected override void Initialize()
///     {
///         RegisterBool("switch1", ReadSwitchAsync, WriteSwitchAsync);
///         RegisterInt("counter", ReadCounterAsync);
///     }
/// }
/// </code>
/// </remarks>
public abstract class PollingSignalHost : IDisposable, IVariableSignalNotifier
{
    private readonly ConcurrentDictionary<string, ISignalEntry> _signals = new();
    private readonly object _loopLock = new();
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    /// <summary>
    /// 轮询周期（毫秒）：-1 不启动；0 紧凑循环；>0 固定延迟（默认 100）。
    /// </summary>
    /// <value>
    /// <list type="bullet">
    /// <item><description>-1: 不启动轮询循环</description></item>
    /// <item><description>0: 紧凑循环，无延迟但会让出执行权</description></item>
    /// <item><description>>0: 固定延迟轮询，单位为毫秒</description></item>
    /// </list>
    /// </value>
    public int PollIntervalMs { get; set; } = 100;

    /// <summary>
    /// 值变化事件：当任何已注册信号的值发生变化时触发。
    /// </summary>
    /// <remarks>
    /// 事件参数说明：
    /// <list type="table">
    /// <item><term>string address</term><description>信号地址</description></item>
    /// <item><term>object oldValue</term><description>旧值，第一次变化时为 default(T)</description></item>
    /// <item><term>object newValue</term><description>新值</description></item>
    /// <item><term>DateTime timestamp</term><description>变化时间戳</description></item>
    /// </list>
    /// </remarks>
    public event Action<string, object, object, DateTime>? SignalValueChanged;

    /// <summary>
    /// 注册一个泛型信号，支持任何值类型。
    /// </summary>
    /// <typeparam name="T">信号值的类型，必须是值类型</typeparam>
    /// <param name="address">信号地址，用于唯一标识信号</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null（只读信号）</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    /// <exception cref="ArgumentNullException">当 address 或 reader 为 null 时抛出</exception>
    /// <exception cref="InvalidOperationException">当地址已被注册时抛出</exception>
    /// <remarks>
    /// 如果 writer 为 null，则该信号为只读信号，调用 SetAsync 时会直接更新内部缓存值。
    /// </remarks>
    protected void Register<T>(
        string address,
        Func<CancellationToken, Task<T>> reader,
        Func<T, CancellationToken, Task>? writer = null,
        Action<T, T>? onChanged = null)
        where T : struct
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));
        if (reader == null) throw new ArgumentNullException(nameof(reader));
        var entry = new SignalEntry<T>(address, reader, writer, onChanged, RaiseValueChanged);
        if (!_signals.TryAdd(address, entry))
            throw new InvalidOperationException("重复注册: " + address);
    }

    /// <summary>
    /// 注册一个 bool 类型的信号。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    protected void RegisterBool(string address, Func<CancellationToken, Task<bool>> reader, Func<bool, CancellationToken, Task>? writer = null, Action<bool, bool>? onChanged = null)
        => Register(address, reader, writer, onChanged);
        
    /// <summary>
    /// 注册一个 byte 类型的信号。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    protected void RegisterByte(string address, Func<CancellationToken, Task<byte>> reader, Func<byte, CancellationToken, Task>? writer = null, Action<byte, byte>? onChanged = null)
        => Register(address, reader, writer, onChanged);
        
    /// <summary>
    /// 注册一个 short 类型的信号。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    protected void RegisterShort(string address, Func<CancellationToken, Task<short>> reader, Func<short, CancellationToken, Task>? writer = null, Action<short, short>? onChanged = null)
        => Register(address, reader, writer, onChanged);
        
    /// <summary>
    /// 注册一个 int 类型的信号。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    protected void RegisterInt(string address, Func<CancellationToken, Task<int>> reader, Func<int, CancellationToken, Task>? writer = null, Action<int, int>? onChanged = null)
        => Register(address, reader, writer, onChanged);
        
    /// <summary>
    /// 注册一个 long 类型的信号。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    protected void RegisterLong(string address, Func<CancellationToken, Task<long>> reader, Func<long, CancellationToken, Task>? writer = null, Action<long, long>? onChanged = null)
        => Register(address, reader, writer, onChanged);
        
    /// <summary>
    /// 注册一个 float 类型的信号。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    protected void RegisterFloat(string address, Func<CancellationToken, Task<float>> reader, Func<float, CancellationToken, Task>? writer = null, Action<float, float>? onChanged = null)
        => Register(address, reader, writer, onChanged);
        
    /// <summary>
    /// 注册一个 double 类型的信号。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取信号值的异步方法</param>
    /// <param name="writer">写入信号值的异步方法，可为 null</param>
    /// <param name="onChanged">值变化时的回调方法，可为 null</param>
    protected void RegisterDouble(string address, Func<CancellationToken, Task<double>> reader, Func<double, CancellationToken, Task>? writer = null, Action<double, double>? onChanged = null)
        => Register(address, reader, writer, onChanged);

    /// <summary>
    /// 尝试获取指定地址的缓存信号值。
    /// </summary>
    /// <typeparam name="T">信号值的类型</typeparam>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获取到的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    /// <remarks>
    /// 该方法不会触发实际的读取操作，只返回最后一次轮询获取的缓存值。
    /// 如果信号尚未被轮询过或类型不匹配，则返回 false。
    /// </remarks>
    public bool TryGet<T>(string address, out T value) where T : struct
    {
        value = default;
        if (_signals.TryGetValue(address, out var e) && e is SignalEntry<T> { HasValue: true } se)
        {
            value = se.Value;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 尝试获取指定地址的 bool 类型缓存值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获缓存的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    public bool TryGetBool(string address, out bool value) => TryGet(address, out value);
    
    /// <summary>
    /// 尝试获取指定地址的 byte 类型缓存值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获取到的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    public bool TryGetByte(string address, out byte value) => TryGet(address, out value);
    
    /// <summary>
    /// 尝试获取指定地址的 short 类型缓存值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获取到的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    public bool TryGetShort(string address, out short value) => TryGet(address, out value);
    
    /// <summary>
    /// 尝试获取指定地址的 int 类型缓存值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获取到的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    public bool TryGetInt(string address, out int value) => TryGet(address, out value);
    
    /// <summary>
    /// 尝试获取指定地址的 long 类型缓存值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获取到的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    public bool TryGetLong(string address, out long value) => TryGet(address, out value);
    
    /// <summary>
    /// 尝试获取指定地址的 float 类型缓存值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获取到的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    public bool TryGetFloat(string address, out float value) => TryGet(address, out value);
    
    /// <summary>
    /// 尝试获取指定地址的 double 类型缓存值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">输出参数，获取到的信号值</param>
    /// <returns>如果成功获取到值则返回 true，否则返回 false</returns>
    public bool TryGetDouble(string address, out double value) => TryGet(address, out value);

    /// <summary>
    /// 异步设置指定地址的信号值。
    /// </summary>
    /// <typeparam name="T">信号值的类型</typeparam>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    /// <exception cref="KeyNotFoundException">当指定地址的信号不存在时抛出</exception>
    /// <exception cref="InvalidOperationException">当信号类型不匹配时抛出</exception>
    /// <remarks>
    /// 如果信号注册时提供了 writer 方法，则会调用该方法执行实际的写入操作。
    /// 如果没有提供 writer 方法，则直接更新内部缓存值。
    /// </remarks>
    public Task<bool> SetAsync<T>(string address, T value, CancellationToken ct = default) where T : struct
    {
        if (!_signals.TryGetValue(address, out var e))
            throw new KeyNotFoundException(address);
        if (e is not SignalEntry<T> se)
            throw new InvalidOperationException("类型不匹配: " + address);
        return se.SetAsync(value, ct);
    }

    /// <summary>
    /// 异步设置 bool 类型信号的值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    public Task<bool> SetBoolAsync(string address, bool value, CancellationToken ct = default) => SetAsync(address, value, ct);
    
    /// <summary>
    /// 异步设置 byte 类型信号的值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    public Task<bool> SetByteAsync(string address, byte value, CancellationToken ct = default) => SetAsync(address, value, ct);
    
    /// <summary>
    /// 异步设置 short 类型信号的值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    public Task<bool> SetShortAsync(string address, short value, CancellationToken ct = default) => SetAsync(address, value, ct);
    
    /// <summary>
    /// 异步设置 int 类型信号的值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    public Task<bool> SetIntAsync(string address, int value, CancellationToken ct = default) => SetAsync(address, value, ct);
    
    /// <summary>
    /// 异步设置 long 类型信号的值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    public Task<bool> SetLongAsync(string address, long value, CancellationToken ct = default) => SetAsync(address, value, ct);
    
    /// <summary>
    /// 异步设置 float 类型信号的值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    public Task<bool> SetFloatAsync(string address, float value, CancellationToken ct = default) => SetAsync(address, value, ct);
    
    /// <summary>
    /// 异步设置 double 类型信号的值。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="value">要设置的值</param>
    /// <param name="ct">取消标记</param>
    /// <returns>设置成功则返回 true</returns>
    public Task<bool> SetDoubleAsync(string address, double value, CancellationToken ct = default) => SetAsync(address, value, ct);

    /// <summary>
    /// 启动轮询循环。
    /// </summary>
    /// <remarks>
    /// 如果 PollIntervalMs 为 -1，则不会启动轮询。
    /// 如果已经有轮询任务在运行且未完成，则不会重复启动。
    /// </remarks>
    public void Start()
    {
        if (PollIntervalMs == -1) return;
        lock (_loopLock)
        {
            if (_loopTask is { IsCompleted: false }) return;
            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
        }
    }

    /// <summary>
    /// 停止轮询循环。
    /// </summary>
    /// <remarks>
    /// 会取消轮询任务并等待最多 200 毫秒让任务正常结束。
    /// </remarks>
    public void Stop()
    {
        lock (_loopLock)
        {
            _cts?.Cancel();
            _loopTask?.Wait(200);
            _cts = null;
            _loopTask = null;
        }
    }

    /// <summary>
    /// 重新启动轮询循环。
    /// </summary>
    /// <remarks>
    /// 等价于先调用 Stop() 再调用 Start()。
    /// </remarks>
    public void Restart()
    {
        Stop();
        Start();
    }

    private async Task LoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && PollIntervalMs != -1)
        {
            try
            {
                await PollOnceAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                OnLoopException(ex);
            }

            if (PollIntervalMs == 0)
            {
                await Task.Yield();
            }
            else if (PollIntervalMs > 0)
            {
                try
                {
                    await Task.Delay(PollIntervalMs, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private async Task PollOnceAsync(CancellationToken token)
    {
        // 快照当前信号集合，避免枚举期间可能的并发修改影响
        var snapshot = _signals.ToArray();

        // 预检查取消
        token.ThrowIfCancellationRequested();

        var tasks = new List<Task>(snapshot.Length);
        foreach (var kv in snapshot)
        {
            var key = kv.Key;
            var entry = kv.Value;
            tasks.Add(PollSignalAsync(key, entry, token));
        }

        // 并发等待：只有取消会冒泡；其它异常在内部被捕获并上报
        await Task.WhenAll(tasks).ConfigureAwait(false);

        await OnAfterCycleAsync(token).ConfigureAwait(false);

        // 本地函数：封装单个信号的轮询与异常处理
        async Task PollSignalAsync(string address, ISignalEntry entry, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await entry.PollAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 继续向上抛出以终止本轮
                throw;
            }
            catch (Exception ex)
            {
                OnSignalException(address, ex);
            }
        }
    }

    /// <summary>
    /// 轮询循环异常处理钩子方法。
    /// </summary>
    /// <param name="ex">发生的异常</param>
    /// <remarks>
    /// 当轮询循环发生异常时会调用此方法。默认实现是输出调试信息。
    /// 派生类可以重写此方法来自定义异常处理逻辑。
    /// </remarks>
    protected virtual void OnLoopException(Exception ex)
    {
        Debug.WriteLine("[PollingSignalHostV2] Loop exception: " + ex);
    }

    /// <summary>
    /// 信号读取异常处理钩子方法。
    /// </summary>
    /// <param name="address">发生异常的信号地址</param>
    /// <param name="ex">发生的异常</param>
    /// <remarks>
    /// 当某个信号在读取过程中发生异常时会调用此方法。默认实现是输出调试信息。
    /// 派生类可以重写此方法来自定义异常处理逻辑。
    /// </remarks>
    protected virtual void OnSignalException(string address, Exception ex)
    {
        Debug.WriteLine("[PollingSignalHostV2] Signal " + address + " exception: " + ex.Message);
    }

    /// <summary>
    /// 轮询周期结束后的钩子方法。
    /// </summary>
    /// <param name="token">取消标记</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 每次完成一轮信号轮询后会调用此方法。默认实现为空。
    /// 派生类可以重写此方法来添加自定义的周期性操作。
    /// </remarks>
    protected virtual Task OnAfterCycleAsync(CancellationToken token) => Task.CompletedTask;

    private void RaiseValueChanged(string address, object oldValue, object newValue)
    {
        SignalValueChanged?.Invoke(address, oldValue, newValue, DateTime.Now);
        OnValueChanged(address, oldValue, newValue);
    }

    /// <summary>
    /// 信号值变化的虚拟钩子方法。
    /// </summary>
    /// <param name="address">信号地址</param>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    /// <remarks>
    /// 当任何信号值发生变化时会调用此方法。默认实现为空。
    /// 派生类可以重写此方法来添加自定义的值变化处理逻���。
    /// </remarks>
    protected virtual void OnValueChanged(string address, object oldValue, object newValue)
    {
    }

    /// <summary>
    /// 释放资源。
    /// </summary>
    /// <remarks>
    /// 会停止轮询循环并释放相关资源。
    /// </remarks>
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 信号条目接口，用于内部管理不同类型的信号。
    /// </summary>
    private interface ISignalEntry
    {
        /// <summary>
        /// 执行一次信号轮询。
        /// </summary>
        /// <param name="token">取消标记</param>
        /// <returns>异步任务</returns>
        Task PollAsync(CancellationToken token);
    }

    /// <summary>
    /// 泛型信号条目实现，封装特定类型信号的读写逻辑。
    /// </summary>
    /// <typeparam name="T">信号值的类型</typeparam>
    /// <param name="address">信号地址</param>
    /// <param name="reader">读取方法</param>
    /// <param name="writer">写入方法</param>
    /// <param name="userChanged">用户变化回调</param>
    /// <param name="raise">值变化事件触发器</param>
    private sealed class SignalEntry<T>(
        string address,
        Func<CancellationToken, Task<T>> reader,
        Func<T, CancellationToken, Task>? writer,
        Action<T, T>? userChanged,
        Action<string, object, object> raise)
        : ISignalEntry
        where T : struct
    {
        private readonly Func<CancellationToken, Task<T>> _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        private readonly Action<string, object, object> _raise = raise ?? throw new ArgumentNullException(nameof(raise));

        /// <summary>
        /// 信号地址。
        /// </summary>
        public string Address { get; } = address;
        
        /// <summary>
        /// 是否已经有缓存值。
        /// </summary>
        public bool HasValue { get; private set; }
        
        /// <summary>
        /// 当前缓存的信号值。
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// 执行一次信号轮询，读取新值并检查是否发生变化。
        /// </summary>
        /// <param name="token">取消标记</param>
        /// <returns>异步任务</returns>
        public async Task PollAsync(CancellationToken token)
        {
            var newVal = await _reader(token).ConfigureAwait(false);
            if (!HasValue || !EqualityComparer<T>.Default.Equals(newVal, Value))
            {
                var old = Value;
                Value = newVal;
                HasValue = true;
                userChanged?.Invoke(old, newVal);
                _raise(Address, old, newVal);
            }
        }

        /// <summary>
        /// 异步设置信号值。
        /// </summary>
        /// <param name="value">要设置的值</param>
        /// <param name="token">取消标记</param>
        /// <returns>设置成功则返回 true</returns>
        public async Task<bool> SetAsync(T value, CancellationToken token)
        {
            if (writer == null)
            {
                if (!HasValue || !EqualityComparer<T>.Default.Equals(value, Value))
                {
                    var oldLocal = Value;
                    Value = value;
                    HasValue = true;
                    userChanged?.Invoke(oldLocal, value);
                    _raise(Address, oldLocal, value);
                }
                return true;
            }

            await writer(value, token).ConfigureAwait(false);
            if (!HasValue || !EqualityComparer<T>.Default.Equals(value, Value))
            {
                var old = Value;
                Value = value;
                HasValue = true;
                userChanged?.Invoke(old, value);
                _raise(Address, old, value);
            }
            return true;
        }
    }
}
