using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace XMSDK.Framework.Communication;

public abstract class DataDriver
{
    private readonly Dictionary<string, dynamic> _dic = new();
        
    public async Task<bool> GetBoolAsync(string address)
    {
        await Task.Delay(50).ConfigureAwait(false);
        return _dic[address];
    }

    public async Task<(bool Success, string? ErrorMessage)> SetBoolAsync(string address, bool value)
    {
        await Task.Delay(50).ConfigureAwait(false);
        _dic[address] = value;
        return (true, null);
    }
}

/// <summary>
/// 使用新的 PollingSignalHost 的示例：显式注册信号而非反射。
/// </summary>
public class ExampleOsc : PollingSignalHost
{
    private volatile bool _d2837; // 本地缓存（回调中更新）

    public ExampleOsc(DataDriver driver, int pollIntervalMs = 100)
    {
        PollIntervalMs = pollIntervalMs;

        // 注册一个布尔信号 D2837
        RegisterBool(
            address: "D2837",
            reader: _ => driver.GetBoolAsync("D2837"),
            writer: async (val, _) =>
            {
                var rst = await driver.SetBoolAsync("D2837", val).ConfigureAwait(false);
                if (!rst.Success)
                    throw new InvalidOperationException(rst.ErrorMessage ?? "写入失败");
            },
            onChanged: (_, newV) => _d2837 = newV // 用户层缓存更新（避免每次 TryGet）
        );

        Start(); // 启动轮询
    }

    /// <summary>
    /// 暴露属性（读取本地缓存；设置同步调用底层写入——若需不阻塞可提供 SetD2837Async）
    /// </summary>
    public bool D2837
    {
        get => _d2837;
        set => SetBoolAsync("D2837", value).GetAwaiter().GetResult();
    }

    // 可选：也可覆盖生命周期钩子
    protected override void OnSignalChanged(string address, object oldValue, object newValue)
    {
        // 这里可以放日志或总线事件
        Debug.WriteLine($"[{nameof(ExampleOsc)}] {address} {oldValue} -> {newValue}");
    }
}