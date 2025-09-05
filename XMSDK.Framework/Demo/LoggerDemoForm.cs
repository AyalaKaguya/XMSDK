using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XMSDK.Framework.Logger;

namespace XMSDK.Framework.Demo
{
    public partial class LoggerDemoForm : Form
    {
        private ILogger<LoggerDemoForm> _logger;
        private readonly Random _random = new Random();

        public LoggerDemoForm()
        {
            InitializeComponent();
            SetupLogging();
        }

        private void SetupLogging()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.AddListView(loggerList)
                       .SetMinimumLevel(LogLevel.Trace);
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = serviceProvider.GetRequiredService<ILogger<LoggerDemoForm>>();

            // 添加欢迎日志
            _logger.LogInformation("欢迎使用日志查看器演示程序V2！");
            _logger.LogInformation("功能特性:");
            _logger.LogInformation("1. 自动滚动开关（默认开启）");
            _logger.LogInformation("2. 支持前景色和背景色配置");
            _logger.LogInformation("3. 右键菜单中的自动滚动选项");
            _logger.LogInformation("4. 性能优化，支持大量日志流畅显示");
            _logger.LogInformation("5. 点击按钮测试不同级别的日志");
        }

        #region 事件处理方法

        private void BtnTrace_Click(object sender, EventArgs e)
        {
            _logger.LogTrace("这是跟踪日志消息");
        }

        private void BtnDebug_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("这是调试日志消息");
        }

        private void BtnInfo_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("这是信息日志消息");
        }

        private void BtnWarning_Click(object sender, EventArgs e)
        {
            _logger.LogWarning("这是警告日志消息");
        }

        private void BtnError_Click(object sender, EventArgs e)
        {
            _logger.LogError("这是错误日志消息");
        }

        private void BtnCritical_Click(object sender, EventArgs e)
        {
            _logger.LogCritical("这是严重错误日志消息");
        }

        private void BtnCustomColor_Click(object sender, EventArgs e)
        {
            // 演示前景色配置
            loggerList.SetLogLevelColor(LogLevel.Information, Color.DarkBlue);
            loggerList.SetLogLevelColor(LogLevel.Warning, Color.Purple);
            loggerList.SetLogLevelColor(LogLevel.Error, Color.DarkRed);
            loggerList.SetLogLevelColor(LogLevel.Critical, Color.Magenta);
            
            _logger.LogInformation("已更新前景色配置");
            _logger.LogWarning("警告日志现在是紫色");
            _logger.LogError("错误日志现在是深红色");
            _logger.LogCritical("严重日志现在是洋红色");
        }

        private void BtnBackgroundColor_Click(object sender, EventArgs e)
        {
            // 演示背景色配置
            loggerList.SetLogLevelColor(LogLevel.Information, Color.DarkBlue, Color.LightCyan);
            loggerList.SetLogLevelColor(LogLevel.Warning, Color.DarkOrange, Color.LightYellow);
            loggerList.SetLogLevelColor(LogLevel.Error, Color.White, Color.LightCoral);
            loggerList.SetLogLevelColor(LogLevel.Critical, Color.White, Color.Red);
            
            _logger.LogInformation("已更新背景色配置 - 信息日志有浅青色背景");
            _logger.LogWarning("警告日志有浅黄色背景");
            _logger.LogError("错误日志有浅珊瑚色背景");
            _logger.LogCritical("严重日志有红色背景");
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            loggerList.Clear();
        }

        private void BtnSetMaxCount_Click(object sender, EventArgs e)
        {
            // 使用简单的InputBox替代
            using (var inputForm = new Form())
            {
                inputForm.Text = "设置最大日志条数";
                inputForm.Size = new Size(360, 180);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                
                var label = new Label() { Text = "请输入最大日志条数:", Location = new Point(10, 15), Size = new Size(200, 20) };
                var textBox = new TextBox() { Text = loggerList.MaxLogCount.ToString(), Location = new Point(10, 40), Size = new Size(200, 20) };
                var btnOk = new Button() { Text = "确定", Location = new Point(50, 70), Size = new Size(75, 23), DialogResult = DialogResult.OK };
                var btnCancel = new Button() { Text = "取消", Location = new Point(135, 70), Size = new Size(75, 23), DialogResult = DialogResult.Cancel };
                
                inputForm.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
                inputForm.AcceptButton = btnOk;
                inputForm.CancelButton = btnCancel;
                
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    if (int.TryParse(textBox.Text, out int maxCount) && maxCount > 0)
                    {
                        loggerList.MaxLogCount = maxCount;
                        _logger.LogInformation($"最大日志条数已设置为: {maxCount}");
                    }
                }
            }
        }

        private void BtnToggleAutoScroll_Click(object sender, EventArgs e)
        {
            loggerList.AutoScroll = !loggerList.AutoScroll;
            UpdateAutoScrollButtonText();
            
            _logger.LogInformation($"自动滚动已{(loggerList.AutoScroll ? "开启" : "关闭")}");
            
            if (!loggerList.AutoScroll)
            {
                _logger.LogInformation("现在新日志不会自动滚动到底部");
                _logger.LogInformation("您可以手动滚动查看历史日志");
            }
        }

        private void BtnStressTest_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "这将快速生成1000条日志来测试性能，继续吗？", 
                "性能测试", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result == DialogResult.Yes)
            {
                var startTime = DateTime.Now;
                _logger.LogInformation($"开始性能测试 - {startTime:HH:mm:ss.fff}");
                
                // 生成1000条不同级别的日志
                for (int i = 1; i <= 1000; i++)
                {
                    var logLevel = (LogLevel)(i % 6 + 1); // 循环使用不同级别
                    _logger.Log(logLevel, $"性能测试日志 #{i:D4} - 测试ListView优化效果");
                }
                
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                _logger.LogInformation($"性能测试完成 - {endTime:HH:mm:ss.fff} (耗时: {duration.TotalMilliseconds:F0}ms)");
            }
        }

        private void BtnHighSpeedLog_Click(object sender, EventArgs e)
        {
            if (logTimer.Interval == 1000)
            {
                // 切换到高速模式 - 每100ms一条日志
                logTimer.Interval = 100;
                btnHighSpeedLog.Text = "正常速度";
                btnHighSpeedLog.BackColor = Color.Orange;
                _logger.LogWarning("已切换到高速日志模式 - 每100ms一条日志");
            }
            else
            {
                // 切换回正常模式
                logTimer.Interval = 1000;
                btnHighSpeedLog.Text = "高速日志";
                btnHighSpeedLog.BackColor = SystemColors.Control;
                _logger.LogInformation("已切换回正常日志模式 - 每秒一条日志");
            }
        }

        private void BtnAutoLog_Click(object sender, EventArgs e)
        {
            if (logTimer.Enabled)
            {
                logTimer.Stop();
                btnAutoLog.Text = "自动日志";
                _logger.LogInformation("自动日志已停止");
            }
            else
            {
                logTimer.Start();
                btnAutoLog.Text = "停止自动日志";
                _logger.LogInformation("自动日志已启动");
            }
        }

        private void LogTimer_Tick(object sender, EventArgs e)
        {
            var logLevels = new[] 
            { 
                LogLevel.Trace, LogLevel.Debug, LogLevel.Information, 
                LogLevel.Warning, LogLevel.Error, LogLevel.Critical 
            };
            
            var messages = new[]
            {
                "系统运行正常",
                "处理用户请求",
                "数据库连接成功",
                "网络延迟较高",
                "文件读取失败",
                "系统内存不足"
            };

            var logLevel = logLevels[_random.Next(logLevels.Length)];
            var message = messages[_random.Next(messages.Length)];
            
            _logger.Log(logLevel, $"{message} (自动生成 {DateTime.Now:HH:mm:ss})");
        }

        #endregion

        private void UpdateAutoScrollButtonText()
        {
            btnToggleAutoScroll.Text = loggerList.AutoScroll ? "关闭滚动" : "开启滚动";
            btnToggleAutoScroll.BackColor = loggerList.AutoScroll ? Color.LightGreen : Color.LightCoral;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                logTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}