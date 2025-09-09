using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace XMSDK.Framework.Communication;

/// <summary>
/// 轮询信号宿主 V2：仅保留核心功能，使用泛型支持 bool/short/int/long 等值类型。
/// 不考虑旧版兼容性，API 简洁直接。
/// </summary>
public abstract class PollingSignalHost : IDisposable
{
    private readonly ConcurrentDictionary<string, ISignalEntry> _signals = new();
    private readonly object _loopLock = new();
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    /// <summary>
    /// 轮询周期（毫秒）：-1 不启动；0 紧凑循环；>0 固定延迟（默认 100）。
    /// </summary>
    public int PollIntervalMs { get; set; } = 100;

    /// <summary>
    /// 值变化事件：(address, oldValue, newValue)。第一次变化 old 为 default(T)。
    /// </summary>
    public event Action<string, object, object>? ValueChanged;

    // 注册
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

    // 便捷重载
    protected void RegisterBool(string address, Func<CancellationToken, Task<bool>> reader, Func<bool, CancellationToken, Task>? writer = null, Action<bool, bool>? onChanged = null)
        => Register(address, reader, writer, onChanged);
    protected void RegisterByte(string address, Func<CancellationToken, Task<byte>> reader, Func<byte, CancellationToken, Task>? writer = null, Action<byte, byte>? onChanged = null)
        => Register(address, reader, writer, onChanged);
    protected void RegisterShort(string address, Func<CancellationToken, Task<short>> reader, Func<short, CancellationToken, Task>? writer = null, Action<short, short>? onChanged = null)
        => Register(address, reader, writer, onChanged);
    protected void RegisterInt(string address, Func<CancellationToken, Task<int>> reader, Func<int, CancellationToken, Task>? writer = null, Action<int, int>? onChanged = null)
        => Register(address, reader, writer, onChanged);
    protected void RegisterLong(string address, Func<CancellationToken, Task<long>> reader, Func<long, CancellationToken, Task>? writer = null, Action<long, long>? onChanged = null)
        => Register(address, reader, writer, onChanged);
    protected void RegisterFloat(string address, Func<CancellationToken, Task<float>> reader, Func<float, CancellationToken, Task>? writer = null, Action<float, float>? onChanged = null)
        => Register(address, reader, writer, onChanged);
    protected void RegisterDouble(string address, Func<CancellationToken, Task<double>> reader, Func<double, CancellationToken, Task>? writer = null, Action<double, double>? onChanged = null)
        => Register(address, reader, writer, onChanged);

    // 缓存读取
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

    public bool TryGetBool(string address, out bool value) => TryGet(address, out value);
    public bool TryGetByte(string address, out byte value) => TryGet(address, out value);
    public bool TryGetShort(string address, out short value) => TryGet(address, out value);
    public bool TryGetInt(string address, out int value) => TryGet(address, out value);
    public bool TryGetLong(string address, out long value) => TryGet(address, out value);
    public bool TryGetFloat(string address, out float value) => TryGet(address, out value);
    public bool TryGetDouble(string address, out double value) => TryGet(address, out value);

    // 主动写入
    public Task<bool> SetAsync<T>(string address, T value, CancellationToken ct = default) where T : struct
    {
        if (!_signals.TryGetValue(address, out var e))
            throw new KeyNotFoundException(address);
        if (e is not SignalEntry<T> se)
            throw new InvalidOperationException("类型不匹配: " + address);
        return se.SetAsync(value, ct);
    }

    public Task<bool> SetBoolAsync(string address, bool value, CancellationToken ct = default) => SetAsync(address, value, ct);
    public Task<bool> SetByteAsync(string address, byte value, CancellationToken ct = default) => SetAsync(address, value, ct);
    public Task<bool> SetShortAsync(string address, short value, CancellationToken ct = default) => SetAsync(address, value, ct);
    public Task<bool> SetIntAsync(string address, int value, CancellationToken ct = default) => SetAsync(address, value, ct);
    public Task<bool> SetLongAsync(string address, long value, CancellationToken ct = default) => SetAsync(address, value, ct);
    public Task<bool> SetFloatAsync(string address, float value, CancellationToken ct = default) => SetAsync(address, value, ct);
    public Task<bool> SetDoubleAsync(string address, double value, CancellationToken ct = default) => SetAsync(address, value, ct);

    // 生命周期
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
        foreach (var kv in _signals)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                await kv.Value.PollAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnSignalException(kv.Key, ex);
            }
        }

        await OnAfterCycleAsync(token).ConfigureAwait(false);
    }

    // 钩子
    protected virtual void OnLoopException(Exception ex)
    {
        Debug.WriteLine("[PollingSignalHostV2] Loop exception: " + ex);
    }

    protected virtual void OnSignalException(string address, Exception ex)
    {
        Debug.WriteLine("[PollingSignalHostV2] Signal " + address + " exception: " + ex.Message);
    }

    protected virtual Task OnAfterCycleAsync(CancellationToken token) => Task.CompletedTask;

    private void RaiseValueChanged(string address, object oldValue, object newValue)
    {
        ValueChanged?.Invoke(address, oldValue, newValue);
        OnValueChanged(address, oldValue, newValue);
    }

    protected virtual void OnValueChanged(string address, object oldValue, object newValue)
    {
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    // 内部结构
    private interface ISignalEntry
    {
        Task PollAsync(CancellationToken token);
    }

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

        public string Address { get; } = address;
        public bool HasValue { get; private set; }
        public T Value { get; private set; }

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
