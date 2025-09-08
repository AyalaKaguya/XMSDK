using System;
using System.Threading.Tasks;

namespace XMSDK.Framework.Communication
{
    public class DataDriver
    {
        public async Task<bool> GetBoolAsync(string address)
        {
            // 模拟异步操作
            await Task.Delay(50);
            // 返回模拟数据
            return DateTime.Now.Second % 2 == 0; // 偶数秒返回true，奇数秒返回false
        }

        public async Task<(bool Success, string ErrorMessage)> SetBoolAsync(string address, bool value)
        {
            // 模拟异步操作
            await Task.Delay(50);
            // 模拟成功设置
            return (true, null);
        }
    }

    public class ExampleOsc: ObservableSignalCollection
    {
        // 方法注解
        [ObservableSignal("D2837")]
        private bool _d2837;

        private readonly DataDriver _someDataDriver; // 数据驱动可以是任何类型
        public ExampleOsc(DataDriver driver = null, int updateTimeout = 100)
        {
            _someDataDriver = driver ?? new DataDriver();
            UpdateTimeout = updateTimeout; // 可在调用 Initialize 前修改
            Initialize(); // 收集并启动轮询
        }
        
        // 自动生成的代码，不可见
        public bool D2837
        {
            get => _d2837;
            set => DataChanged(ref _d2837, value, "D2837");
        }


        
        // 重写的函数，尝试更新一个值的实现
        public override async Task<bool> GetBool(string address)
        {
            return await _someDataDriver.GetBoolAsync(address).ConfigureAwait(false);
        }

        public override async Task<bool> SetBool(string address, bool value)
        {
            var result = await _someDataDriver.SetBoolAsync(address, value).ConfigureAwait(false);
            if (result.Success && address == "D2837")
            {
                // 同步本地缓存，避免等待下一轮轮询
                if (_d2837 != value) D2837 = value;
            }
            return result.Success;
        }

        // 可选扩展：当某个信号更新后可执行附加逻辑
        protected override Task OnSignalUpdatedAsync(string address, object newValue, System.Threading.CancellationToken token)
        {
            // 简单示例：可以在此写日志/触发总线
            System.Diagnostics.Debug.WriteLine($"Signal {address} updated -> {newValue}");
            return Task.CompletedTask;
        }
    }
}