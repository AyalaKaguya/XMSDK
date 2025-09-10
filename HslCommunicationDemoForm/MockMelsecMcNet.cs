using XMSDK.Framework.Communication.Signal;
using HslCommunication.Profinet.Melsec;

namespace HslCommunicationDemoForm
{
    [ObservableSignalCollection(Description = "三菱MC协议模拟驱动", Group = "Demo")]
    public class MockMelsecMcNet : PollingSignalHost
    {
        public MockMelsecMcNet(string ip = "127.0.0.1", int port = 6000)
        {
            var plc1 = new MelsecMcNet();
            plc1.NetworkNumber = 0;
            plc1.NetworkStationNumber = 0;
            plc1.TargetIOStation = 1023;
            plc1.EnableWriteBitToWordRegister = false;
            plc1.ByteTransform.IsStringReverseByteWord = false;
            plc1.CommunicationPipe = new HslCommunication.Core.Pipe.PipeTcpNet(ip, port)
            {
                ConnectTimeOut = 5000, // 连接超时时间，单位毫秒
                ReceiveTimeOut = 10000, // 接收设备数据反馈的超时时间
                SleepTime = 0,
                SocketKeepAliveTime = -1,
                IsPersistentConnection = true,
            };

            // 注册布尔类型信号
            RegisterBool(
                address: "M3260",
                reader: async ct => (await plc1.ReadBoolAsync("M3260")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3260", val),
                onChanged: (_, newV) => _m3260 = newV
            );

            RegisterBool(
                address: "M3261",
                reader: async ct => (await plc1.ReadBoolAsync("M3261")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3261", val),
                onChanged: (_, newV) => _m3261 = newV
            );

            RegisterBool(
                address: "M3262",
                reader: async ct => (await plc1.ReadBoolAsync("M3262")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3262", val),
                onChanged: (_, newV) => _m3262 = newV
            );

            RegisterBool(
                address: "M3263",
                reader: async ct => (await plc1.ReadBoolAsync("M3263")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3263", val),
                onChanged: (_, newV) => _m3263 = newV
            );

            RegisterBool(
                address: "M3264",
                reader: async ct => (await plc1.ReadBoolAsync("M3264")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3264", val),
                onChanged: (_, newV) => _m3264 = newV
            );

            RegisterBool(
                address: "M3216",
                reader: async ct => (await plc1.ReadBoolAsync("M3216")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3216", val),
                onChanged: (_, newV) => _m3216 = newV
            );

            RegisterBool(
                address: "M3217",
                reader: async ct => (await plc1.ReadBoolAsync("M3217")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3217", val),
                onChanged: (_, newV) => _m3217 = newV
            );

            RegisterBool(
                address: "M3218",
                reader: async ct => (await plc1.ReadBoolAsync("M3218")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3218", val),
                onChanged: (_, newV) => _m3218 = newV
            );

            RegisterBool(
                address: "M3219",
                reader: async ct => (await plc1.ReadBoolAsync("M3219")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3219", val),
                onChanged: (_, newV) => _m3219 = newV
            );

            RegisterBool(
                address: "M3220",
                reader: async ct => (await plc1.ReadBoolAsync("M3220")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3220", val),
                onChanged: (_, newV) => _m3220 = newV
            );

            RegisterBool(
                address: "M3276",
                reader: async ct => (await plc1.ReadBoolAsync("M3276")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("M3276", val),
                onChanged: (_, newV) => _m3276 = newV
            );

            // 注册短整型信号
            RegisterShort(
                address: "D703",
                reader: async ct => (await plc1.ReadInt16Async("D703")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("D703", val),
                onChanged: (_, newV) => _d703 = newV
            );

            RegisterShort(
                address: "D704",
                reader: async ct => (await plc1.ReadInt16Async("D704")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("D704", val),
                onChanged: (_, newV) => _d704 = newV
            );

            RegisterShort(
                address: "D705",
                reader: async ct => (await plc1.ReadInt16Async("D705")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("D705", val),
                onChanged: (_, newV) => _d705 = newV
            );

            RegisterShort(
                address: "D706",
                reader: async ct => (await plc1.ReadInt16Async("D706")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("D706", val),
                onChanged: (_, newV) => _d706 = newV
            );

            RegisterShort(
                address: "D707",
                reader: async ct => (await plc1.ReadInt16Async("D707")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("D707", val),
                onChanged: (_, newV) => _d707 = newV
            );

            RegisterShort(
                address: "D720",
                reader: async ct => (await plc1.ReadInt16Async("D720")).Content,
                writer: async (val, ct) => await plc1.WriteAsync("D720", val),
                onChanged: (_, newV) => _d720 = newV
            );

            Start();
        }

        // 布尔类型私有字段
        private bool _m3260;
        private bool _m3261;
        private bool _m3262;
        private bool _m3263;
        private bool _m3264;
        private bool _m3216;
        private bool _m3217;
        private bool _m3218;
        private bool _m3219;
        private bool _m3220;
        private bool _m3276;

        // 短整型私有字段
        private short _d703;
        private short _d704;
        private short _d705;
        private short _d706;
        private short _d707;
        private short _d720;

        // 相机触发信号属性
        [ObservableSignal(Address = "M3260", Description = "相机1触发", IsReadOnly = true, Group = "触发信号")]
        public bool M3260
        {
            get => _m3260;
            set => SetBoolAsync("M3260", value).Wait();
        }

        [ObservableSignal(Address = "M3261", Description = "相机2触发", IsReadOnly = true, Group = "触发信号")]
        public bool M3261
        {
            get => _m3261;
            set => SetBoolAsync("M3261", value).Wait();
        }

        [ObservableSignal(Address = "M3262", Description = "相机3触发", IsReadOnly = true, Group = "触发信号")]
        public bool M3262
        {
            get => _m3262;
            set => SetBoolAsync("M3262", value).Wait();
        }

        [ObservableSignal(Address = "M3263", Description = "相机4触发", IsReadOnly = true, Group = "触发信号")]
        public bool M3263
        {
            get => _m3263;
            set => SetBoolAsync("M3263", value).Wait();
        }

        [ObservableSignal(Address = "M3264", Description = "相机5触发", IsReadOnly = true, Group = "触发信号")]
        public bool M3264
        {
            get => _m3264;
            set => SetBoolAsync("M3264", value).Wait();
        }

        // 相机完成信号属性
        [ObservableSignal(Address = "M3216", Description = "相机1完成", IsReadOnly = false, Group = "完成信号")]
        public bool M3216
        {
            get => _m3216;
            set => SetBoolAsync("M3216", value).Wait();
        }

        [ObservableSignal(Address = "M3217", Description = "相机2完成", IsReadOnly = false, Group = "完成信号")]
        public bool M3217
        {
            get => _m3217;
            set => SetBoolAsync("M3217", value).Wait();
        }

        [ObservableSignal(Address = "M3218", Description = "相机3完成", IsReadOnly = false, Group = "完成信号")]
        public bool M3218
        {
            get => _m3218;
            set => SetBoolAsync("M3218", value).Wait();
        }

        [ObservableSignal(Address = "M3219", Description = "相机4完成", IsReadOnly = false, Group = "完成信号")]
        public bool M3219
        {
            get => _m3219;
            set => SetBoolAsync("M3219", value).Wait();
        }

        [ObservableSignal(Address = "M3220", Description = "相机5完成", IsReadOnly = false, Group = "完成信号")]
        public bool M3220
        {
            get => _m3220;
            set => SetBoolAsync("M3220", value).Wait();
        }

        // 其他布尔信号
        [ObservableSignal(Address = "M3276", Description = "产品切换", IsReadOnly = false, Group = "控制信号")]
        public bool M3276
        {
            get => _m3276;
            set => SetBoolAsync("M3276", value).Wait();
        }

        // 相机结果信号属性
        [ObservableSignal(Address = "D703", Description = "相机1结果", IsReadOnly = false, Group = "结果数据")]
        public short D703
        {
            get => _d703;
            set => SetShortAsync("D703", value).Wait();
        }

        [ObservableSignal(Address = "D704", Description = "相机2结果", IsReadOnly = false, Group = "结果数据")]
        public short D704
        {
            get => _d704;
            set => SetShortAsync("D704", value).Wait();
        }

        [ObservableSignal(Address = "D705", Description = "相机3结果", IsReadOnly = false, Group = "结果数据")]
        public short D705
        {
            get => _d705;
            set => SetShortAsync("D705", value).Wait();
        }

        [ObservableSignal(Address = "D706", Description = "相机4结果", IsReadOnly = false, Group = "结果数据")]
        public short D706
        {
            get => _d706;
            set => SetShortAsync("D706", value).Wait();
        }

        [ObservableSignal(Address = "D707", Description = "相机5结果", IsReadOnly = false, Group = "结果数据")]
        public short D707
        {
            get => _d707;
            set => SetShortAsync("D707", value).Wait();
        }

        [ObservableSignal(Address = "D720", Description = "产品型号", IsReadOnly = false, Group = "控制信号")]
        public short D720
        {
            get => _d720;
            set => SetShortAsync("D720", value).Wait();
        }
    }
}