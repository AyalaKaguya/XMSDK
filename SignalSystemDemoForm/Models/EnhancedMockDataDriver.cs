using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalSystemDemoForm.Models
{
    /// <summary>
    /// 增强版数据驱动，支持多种信号类型的演示
    /// </summary>
    public class EnhancedMockDataDriver
    {
        private readonly Dictionary<string, object> _signalValues = new();
        private readonly Random _random = new();

        public EnhancedMockDataDriver()
        {
            // 初始化一些演示数据
            _signalValues["D2837"] = false;
            _signalValues["M100"] = true;
            _signalValues["Counter1"] = 42;
            _signalValues["Temperature"] = 23.5f;
            _signalValues["Pressure"] = 1.013;
            _signalValues["Status"] = (byte)1;
        }

        // 扩展支持更多数据类型
        public async Task<int> GetIntAsync(string address)
        {
            await Task.Delay(30);
            return _signalValues.TryGetValue(address, out var value) ? (int)value : 0;
        }

        public async Task<(bool Success, string? ErrorMessage)> SetIntAsync(string address, int value)
        {
            await Task.Delay(30);
            _signalValues[address] = value;
            return (true, null);
        }

        public async Task<float> GetFloatAsync(string address)
        {
            await Task.Delay(30);
            return _signalValues.TryGetValue(address, out var value) ? (float)value : 0f;
        }

        public async Task<(bool Success, string? ErrorMessage)> SetFloatAsync(string address, float value)
        {
            await Task.Delay(30);
            _signalValues[address] = value;
            return (true, null);
        }

        public async Task<double> GetDoubleAsync(string address)
        {
            await Task.Delay(30);
            return _signalValues.TryGetValue(address, out var value) ? (double)value : 0.0;
        }

        public async Task<(bool Success, string? ErrorMessage)> SetDoubleAsync(string address, double value)
        {
            await Task.Delay(30);
            _signalValues[address] = value;
            return (true, null);
        }

        public async Task<byte> GetByteAsync(string address)
        {
            await Task.Delay(30);
            return _signalValues.TryGetValue(address, out var value) ? (byte)value : (byte)0;
        }

        public async Task<(bool Success, string? ErrorMessage)> SetByteAsync(string address, byte value)
        {
            await Task.Delay(30);
            _signalValues[address] = value;
            return (true, null);
        }
        
        public async Task<bool> GetBoolAsync(string address)
        {
            await Task.Delay(30);
            return _signalValues.TryGetValue(address, out var value) && (bool)value;
        }

        public async Task<(bool Success, string? ErrorMessage)> SetBoolAsync(string address, bool value)
        {
            await Task.Delay(30);
            _signalValues[address] = value;
            return (true, null);
        }

        /// <summary>
        /// 模拟信号值的随机变化
        /// </summary>
        public void SimulateRandomChanges()
        {
            // 随机改变计数器
            if (_random.NextDouble() < 0.3)
            {
                _signalValues["Counter1"] = _random.Next(0, 100);
            }

            // 随机改变温度
            if (_random.NextDouble() < 0.4)
            {
                _signalValues["Temperature"] = (float)(_random.NextDouble() * 40 + 10); // 10-50度
            }

            // 随机改变压力
            if (_random.NextDouble() < 0.2)
            {
                _signalValues["Pressure"] = _random.NextDouble() * 2 + 0.5; // 0.5-2.5
            }

            // 随机改变状态
            if (_random.NextDouble() < 0.1)
            {
                _signalValues["Status"] = (byte)_random.Next(0, 4);
            }
        }
    }
}
