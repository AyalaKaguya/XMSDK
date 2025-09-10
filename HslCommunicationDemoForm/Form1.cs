using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using XMSDK.Framework.Communication.Signal;

namespace HslCommunicationDemoForm
{
    public partial class Form1 : Form
    {
        private MockMelsecMcNet mmc;
        private CancellationTokenSource _cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            mmc = new MockMelsecMcNet();
            signalList1.SetSignalSource(mmc);

            _cancellationTokenSource = new CancellationTokenSource();

            // 订阅信号变化事件
            if (mmc is IVariableSignalNotifier notifier)
            {
                notifier.SignalValueChanged += OnSignalValueChanged;
            }
        }

        #region 相机信号处理逻辑

        private void OnSignalValueChanged(string address, object oldVal, object newVal, DateTime timestamp)
        {
            Task.Run(() => ProcessSignalChange(address, oldVal, newVal, timestamp), _cancellationTokenSource.Token);
        }

        private async Task ProcessSignalChange(string address, object oldVal, object newVal, DateTime timestamp)
        {
            try
            {
                // 根据信号地址处理不同的任务
                switch (address)
                {
                    case "M3260":
                        if (newVal is bool triggerValue && triggerValue)
                        {
                            await HandleCamera1Trigger();
                        }
                        break;
                    case "M3261":
                        if (newVal is bool triggerValue2 && triggerValue2)
                        {
                            await HandleCamera2Trigger();
                        }
                        break;
                    case "M3262":
                        if (newVal is bool triggerValue3 && triggerValue3)
                        {
                            await HandleCamera3Trigger();
                        }
                        break;
                    case "M3263":
                        if (newVal is bool triggerValue4 && triggerValue4)
                        {
                            await HandleCamera4Trigger();
                        }
                        break;
                    case "M3264":
                        if (newVal is bool triggerValue5 && triggerValue5)
                        {
                            await HandleCamera5Trigger();
                        }
                        break;
                    case "D720":
                        if (newVal is short resetValue && mmc.M3276)
                        {
                            Trace.WriteLine($"产品已切换到{resetValue}");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // 记录异常或处理错误
                Trace.WriteLine($"处理信号变化时发生错误: {ex.Message}");
            }
        }

        private async Task HandleCamera1Trigger()
        {
            // 相机1触发信号的处理示例
            Trace.WriteLine("检测到相机1触发信号，开始处理...");
            mmc.M3260 = false;

            // 模拟相机处理时间
            await Task.Delay(1000);
            // 设置处理结果
            mmc.D703 = 1; // 设置相机1结果为OK
            mmc.M3216 = true;
            Trace.WriteLine("相机1处理完成");

            // 延迟后重置完成信号
            await Task.Delay(500);
            mmc.D703 = 0;
            mmc.M3216 = false;
        }

        private async Task HandleCamera2Trigger()
        {
            // 相机2触发信号的处理示例
            Trace.WriteLine("检测到相机2触发信号，开始处理...");
            mmc.M3261 = false;

            // 模拟相机处理时间
            await Task.Delay(1000);
            // 设置处理结果
            mmc.D704 = 1; // 设置相机2结果为OK
            mmc.M3217 = true;
            Trace.WriteLine("相机2处理完成");

            // 延迟后重置完成信号
            await Task.Delay(500);
            mmc.D704 = 0;
            mmc.M3217 = false;
        }

        private async Task HandleCamera3Trigger()
        {
            // 相机3触发信号的处理示例
            Trace.WriteLine("检测到相机3触发信号，开始处理...");
            mmc.M3262 = false;

            // 模拟相机处理时间
            await Task.Delay(1000);
            // 设置处理结果
            mmc.D705 = 1; // 设置相机3结果为OK
            mmc.M3218 = true;
            Trace.WriteLine("相机3处理完成");

            // 延迟后重置完成信号
            await Task.Delay(500);
            mmc.D705 = 0;
            mmc.M3218 = false;
        }

        private async Task HandleCamera4Trigger()
        {
            // 相机4触发信号的处理示例
            Trace.WriteLine("检测到相机4触发信号，开始处理...");
            mmc.M3263 = false;

            // 模拟相机处理时间
            await Task.Delay(1000);
            // 设置处理结果
            mmc.D706 = 1; // 设置相机4结果为OK
            mmc.M3219 = true;
            Trace.WriteLine("相机4处理完成");

            // 延迟后重置完成信号
            await Task.Delay(500);
            mmc.D706 = 0;
            mmc.M3219 = false;
        }

        private async Task HandleCamera5Trigger()
        {
            // 相机5触发信号的处理示例
            Trace.WriteLine("检测到相机5触发信号，开始处理...");
            mmc.M3264 = false;

            // 模拟相机处理时间
            await Task.Delay(1000);
            // 设置处理结果
            mmc.D707 = 1; // 设置相机5结果为OK
            mmc.M3220 = true;
            Trace.WriteLine("相机5处理完成");

            // 延迟后重置完成信号
            await Task.Delay(500);
            mmc.D707 = 0;
            mmc.M3220 = false;
        }
        
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 取消后台任务
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                // 取消事件订阅
                if (mmc is IVariableSignalNotifier notifier)
                {
                    notifier.SignalValueChanged -= OnSignalValueChanged;
                }

                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}