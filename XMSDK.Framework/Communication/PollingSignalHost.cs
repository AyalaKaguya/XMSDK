using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace XMSDK.Framework.Communication;

/// <summary>
/// 轮询信号宿主：用于集中注册、周期性异步轮询并缓存各类布尔信号的抽象基类。
/// 提供：
/// 1) 显式注册（避免反射与隐藏开销）
/// 2) 周期轮询 / 主动写入统一生命周期管理 (Start/Stop/Restart/Dispose)
/// 3) 并发安全的信号集合 (ConcurrentDictionary)
/// 4) 缓存 + 仅在值首次获取或变化时派发事件 (SignalChanged 与 per-signal 回调)
/// 5) 可自定义写入逻辑（提供 writer 委托时对外写入，否则本地覆盖）
/// 6) 钩子扩展点：OnSignalChanged / OnAfterCycleAsync / OnSignalException / OnLoopException
/// 7) 轮询节奏：PollIntervalMs = -1(不启动) / 0(紧凑循环) / >0(固定毫秒延时)
/// </summary>
/// <remarks>
/// 使用步骤：
/// 1. 继承本类并在构造函数中调用 <see cref="RegisterBool"/> 注册需要轮询的信号。
/// 2. 设置 <see cref="PollIntervalMs"/>（默认为 100ms）。
/// 3. 调用 <see cref="Start"/> 启动后台轮询；必要时用 <see cref="Stop"/> / <see cref="Restart"/> 控制生命周期；或在 <see cref="Dispose"/> 中自动停止。
/// 4. 通过 <see cref="TryGetCachedBool"/> 读取缓存（非阻塞），或 <see cref="SetBoolAsync"/> 主动写入。
/// 线程安全：集合操作为并发安全；单个信号更新为无锁覆盖（最后写入获胜），如需更严格一致性可在派生类中自行加锁。
/// 异常策略：单个信号轮询/写入异常不会终止主循环，仅通过 <see cref="OnSignalException"/> 回调；主循环异常经 <see cref="OnLoopException"/> 记录后继续。
/// 兼容：.NET Framework 4.6.2 / C# 8 语法。
/// </remarks>
public abstract class PollingSignalHost : IDisposable
{
    // 注册的信号集合（地址 -> 入口）
    private readonly ConcurrentDictionary<string, ISignalEntry> _signals = new();
    private readonly object _loopLock = new();
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    /// <summary>
    /// 轮询周期（毫秒）。取值语义：
    /// -1：不启动后台轮询（调用 <see cref="Start"/> 无效果）
    ///  0：紧凑循环（每轮结束后 <see cref="Task.Yield"/>，几乎持续占用一个线程调度机会）
    /// >0：两轮之间固定延迟指定毫秒数。
    /// 默认 100。
    /// </summary>
    public int PollIntervalMs { get; set; } = 100;

    /// <summary>
    /// 任意信号值发生变化时触发的汇总事件。
    /// (address, oldValue, newValue)
    /// 注意：oldValue 在第一次获取前为默认(bool=false)。
    /// </summary>
    public event Action<string, object, object>? SignalChanged;

    #region 注册 / 访问 API

    /// <summary>
    /// 注册一个布尔信号（周期性读取并在变化时触发事件）。
    /// </summary>
    /// <param name="address">信号唯一地址/键（区分大小写，重复注册抛异常）。</param>
    /// <param name="reader">异步读取委托（每轮轮询调用）；不可为 null。</param>
    /// <param name="writer">可选异步写入委托；为 null 时 <see cref="SetBoolAsync"/> 仅更新本地缓存。</param>
    /// <param name="onChanged">信号级别变化回调 (old,new)，早于 <see cref="SignalChanged"/> 执行。</param>
    /// <exception cref="ArgumentNullException">address 或 reader 为空。</exception>
    /// <exception cref="InvalidOperationException">address 已被注册。</exception>
    protected void RegisterBool(
        string address,
        Func<CancellationToken, Task<bool>> reader,
        Func<bool, CancellationToken, Task>? writer = null,
        Action<bool, bool>? onChanged = null)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));
        if (reader == null) throw new ArgumentNullException(nameof(reader));
        var entry = new BoolSignalEntry(address, reader, writer, onChanged, RaiseChangedInternal);
        if (!_signals.TryAdd(address, entry))
            throw new InvalidOperationException("信号已重复注册: " + address);
    }

    /// <summary>
    /// 尝试获取指定地址的布尔信号最新缓存值（非阻塞，不触发外部读取）。
    /// </summary>
    /// <param name="address">信号地址。</param>
    /// <param name="value">输出：若返回 true，则为当前缓存值；否则为默认 false。</param>
    /// <returns>若该信号存在且已经至少成功轮询过一次返回 true；否则 false。</returns>
    public bool TryGetCachedBool(string address, out bool value)
    {
        value = false;
        if (_signals.TryGetValue(address, out var e) && e is BoolSignalEntry { HasValue: true } be)
        {
            value = be.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 主动写入（或本地覆盖）指定布尔信号。
    /// </summary>
    /// <param name="address">信号地址。</param>
    /// <param name="value">目标值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>始终返回 true；若 writer 异常会向上抛出。</returns>
    /// <exception cref="KeyNotFoundException">未注册的信号。</exception>
    /// <exception cref="InvalidOperationException">信号类型不是布尔。</exception>
    public Task<bool> SetBoolAsync(string address, bool value, CancellationToken ct = default)
    {
        if (!_signals.TryGetValue(address, out var e))
            throw new KeyNotFoundException(address);
        if (e is not BoolSignalEntry be)
            throw new InvalidOperationException("信号类型不是布尔: " + address);
        return be.SetAsync(value, ct);
    }

    #endregion

    #region 生命周期

    /// <summary>
    /// 启动后台轮询（若 <see cref="PollIntervalMs"/> == -1 或已在运行则忽略）。
    /// </summary>
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
    /// 停止后台轮询并尝试在 200ms 内等待当前任务结束。
    /// </summary>
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
    /// 重新启动（Stop 后立即 Start）。
    /// </summary>
    public void Restart()
    {
        Stop();
        Start();
    }

    #endregion

    #region 轮询核心

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
                break;
            }
            catch (Exception ex)
            {
                OnLoopException(ex);
            }

            if (PollIntervalMs == 0)
                await Task.Yield();
            else if (PollIntervalMs > 0)
            {
                try
                {
                    await Task.Delay(PollIntervalMs, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
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

    #endregion

    #region 事件派发 & 钩子

    private void RaiseChangedInternal(string address, object oldValue, object newValue)
    {
        var handler = SignalChanged; // 防止竞态
        handler?.Invoke(address, oldValue, newValue);
        OnSignalChanged(address, oldValue, newValue);
    }

    /// <summary>
    /// 当任意信号变化后（在汇总事件触发之后）调用。派生类可重写添加日志、消息推送等。
    /// </summary>
    /// <param name="address">信号地址。</param>
    /// <param name="oldValue">旧值。</param>
    /// <param name="newValue">新值。</param>
    protected virtual void OnSignalChanged(string address, object oldValue, object newValue)
    {
    }

    /// <summary>
    /// 每轮全部信号轮询结束后调用（即使其中有单个信号失败仍会执行）。
    /// 可用于批量聚合、统一上报等。
    /// </summary>
    /// <param name="token">取消令牌。</param>
    /// <returns>异步任务。</returns>
    protected virtual Task OnAfterCycleAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 主循环（整轮）异常回调，默认 Debug 输出后继续运行。
    /// </summary>
    /// <param name="ex">异常实例。</param>
    protected virtual void OnLoopException(Exception ex)
    {
        Debug.WriteLine("[PollingSignalHost] Loop exception: " + ex);
    }

    /// <summary>
    /// 单个信号轮询或写入出现异常时回调，默认仅输出简要信息。
    /// </summary>
    /// <param name="address">信号地址。</param>
    /// <param name="ex">异常实例。</param>
    protected virtual void OnSignalException(string address, Exception ex)
    {
        Debug.WriteLine("[PollingSignalHost] Signal " + address + " exception: " + ex.Message);
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// 释放资源：调用 <see cref="Stop"/> 终止后台轮询。
    /// </summary>
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 内部结构

    private interface ISignalEntry
    {
        Task PollAsync(CancellationToken token);
    }

    private sealed class BoolSignalEntry(
        string address,
        Func<CancellationToken, Task<bool>> reader,
        Func<bool, CancellationToken, Task>? writer,
        Action<bool, bool>? userChanged,
        Action<string, object, object>? raise)
        : ISignalEntry
    {
        public string Address { get; private set; } = address;
        public bool HasValue { get; private set; }
        public bool Value { get; private set; }

        public async Task PollAsync(CancellationToken token)
        {
            var newVal = await reader(token).ConfigureAwait(false);
            if (!HasValue || newVal != Value)
            {
                var old = Value;
                Value = newVal;
                HasValue = true;
                userChanged?.Invoke(old, newVal);
                raise?.Invoke(Address, old, newVal);
            }
        }

        public async Task<bool> SetAsync(bool value, CancellationToken token)
        {
            if (writer == null)
            {
                if (!HasValue || value != Value)
                {
                    var oldLocal = Value;
                    Value = value;
                    HasValue = true;
                    userChanged?.Invoke(oldLocal, value);
                    raise?.Invoke(Address, oldLocal, value);
                }

                return true;
            }

            await writer(value, token).ConfigureAwait(false);
            if (!HasValue || value != Value)
            {
                var old = Value;
                Value = value;
                HasValue = true;
                userChanged?.Invoke(old, value);
                raise?.Invoke(Address, old, value);
            }

            return true;
        }
    }

    #endregion
}