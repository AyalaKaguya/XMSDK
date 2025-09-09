using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using XMSDK.Framework.Communication;
using XMSDK.Framework.Communication.Signal;

namespace XMSDK.Framework.Demo
{
    /// <summary>
    /// 增强版信号演示类，包含多种信号类型
    /// </summary>
    [ObservableSignalCollection(Description = "演示系统信号集合", Group = "Demo")]
    public class EnhancedSignalDemo : PollingSignalHost
    {
        private readonly EnhancedMockDataDriver _driver;
        private volatile bool _d2837;
        private volatile bool _m100;
        private volatile int _counter1;
        private volatile float _temperature;
        private double _pressure;
        private volatile byte _status;
        private Timer? _simulationTimer;

        public EnhancedSignalDemo(EnhancedMockDataDriver driver, int pollIntervalMs = 800)
        {
            _driver = driver;
            PollIntervalMs = pollIntervalMs;

            RegisterSignals();
            StartSimulation();
            Start(); // 启动轮询
        }

        private void RegisterSignals()
        {
            // 注册布尔信号 - 控制信号
            RegisterBool(
                address: "D2837",
                reader: _ => _driver.GetBoolAsync("D2837"),
                writer: async (val, _) =>
                {
                    var rst = await _driver.SetBoolAsync("D2837", val);
                    if (!rst.Success)
                        throw new InvalidOperationException(rst.ErrorMessage ?? "写入失败");
                },
                onChanged: (_, newV) => _d2837 = newV
            );

            // 注册布尔信号 - 状态信号（只读）
            RegisterBool(
                address: "M100",
                reader: _ => _driver.GetBoolAsync("M100"),
                writer: null, // 只读信号
                onChanged: (_, newV) => _m100 = newV
            );

            // 注册整数信号 - 计数器
            RegisterInt(
                address: "Counter1",
                reader: _ => _driver.GetIntAsync("Counter1"),
                writer: async (val, _) =>
                {
                    var rst = await _driver.SetIntAsync("Counter1", val);
                    if (!rst.Success)
                        throw new InvalidOperationException(rst.ErrorMessage ?? "写入失败");
                },
                onChanged: (_, newV) => _counter1 = newV
            );

            // 注册浮点信号 - 温度
            RegisterFloat(
                address: "Temperature",
                reader: _ => _driver.GetFloatAsync("Temperature"),
                writer: async (val, _) =>
                {
                    var rst = await _driver.SetFloatAsync("Temperature", val);
                    if (!rst.Success)
                        throw new InvalidOperationException(rst.ErrorMessage ?? "写入失败");
                },
                onChanged: (_, newV) => _temperature = newV
            );

            // 注册双精度信号 - 压力
            RegisterDouble(
                address: "Pressure",
                reader: _ => _driver.GetDoubleAsync("Pressure"),
                writer: async (val, _) =>
                {
                    var rst = await _driver.SetDoubleAsync("Pressure", val);
                    if (!rst.Success)
                        throw new InvalidOperationException(rst.ErrorMessage ?? "写入失败");
                },
                onChanged: (_, newV) => _pressure = newV
            );

            // 注册字节信号 - 状态码
            RegisterByte(
                address: "Status",
                reader: _ => _driver.GetByteAsync("Status"),
                writer: async (val, _) =>
                {
                    var rst = await _driver.SetByteAsync("Status", val);
                    if (!rst.Success)
                        throw new InvalidOperationException(rst.ErrorMessage ?? "写入失败");
                },
                onChanged: (_, newV) => _status = newV
            );
        }

        private void StartSimulation()
        {
            // 启动模拟定时器，每3秒随机改变一些信号值
            _simulationTimer = new Timer(SimulationCallback, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
        }

        private void SimulationCallback(object state)
        {
            try
            {
                _driver.SimulateRandomChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"模拟数据变化时发生异常: {ex.Message}");
            }
        }

        #region 公开的信号属性

        /// <summary>
        /// 控制信号 D2837
        /// </summary>
        [ObservableSignal(Name = "控制信号", Address = "D2837", Type = typeof(bool),
            Description = "主控制开关信号", Group = "控制", Unit = "", Format = "ON_OFF")]
        public bool ControlSignal
        {
            get => _d2837;
            set => SetBoolAsync("D2837", value);
        }

        /// <summary>
        /// 状态信号 M100（只读）
        /// </summary>
        [ObservableSignal(Name = "系统状态", Address = "M100", Type = typeof(bool),
            Description = "系统运行状态指示", Group = "状态", IsReadOnly = true, Format = "启停")]
        public bool SystemStatus => _m100;

        /// <summary>
        /// 计数器信号
        /// </summary>
        [ObservableSignal(Name = "计数器", Address = "Counter1", Type = typeof(int),
            Description = "生产计数器", Group = "数据", Unit = "个", MinValue = 0, MaxValue = 999)]
        public int Counter
        {
            get => _counter1;
            set => SetIntAsync("Counter1", value).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 温度信号
        /// </summary>
        [ObservableSignal(Name = "温度", Address = "Temperature", Type = typeof(float),
            Description = "环境温度监测", Group = "传感器", Unit = "℃", Format = "F1",
            MinValue = -20.0f, MaxValue = 80.0f)]
        public float Temperature
        {
            get => _temperature;
            set => SetFloatAsync("Temperature", value).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 压力信号
        /// </summary>
        [ObservableSignal(Name = "压力", Address = "Pressure", Type = typeof(double),
            Description = "系统压力监测", Group = "传感器", Unit = "bar", Format = "F3",
            MinValue = 0.0, MaxValue = 10.0)]
        public double Pressure
        {
            get => _pressure;
            set => SetDoubleAsync("Pressure", value).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 状态码信号
        /// </summary>
        [ObservableSignal(Name = "状态码", Address = "Status", Type = typeof(byte),
            Description = "设备状态编码", Group = "状态", Unit = "",
            MinValue = (byte)0, MaxValue = (byte)255)]
        public byte StatusCode
        {
            get => _status;
            set => SetByteAsync("Status", value).GetAwaiter().GetResult();
        }

        #endregion

        protected override void OnValueChanged(string address, object oldValue, object newValue)
        {
            // 记录信号变化
            Debug.WriteLine($"[演示系统] {address}: {oldValue} -> {newValue} @ {DateTime.Now:HH:mm:ss.fff}");
        }

        protected new void Dispose()
        {
            _simulationTimer?.Dispose();
            base.Dispose();
        }
    }
}