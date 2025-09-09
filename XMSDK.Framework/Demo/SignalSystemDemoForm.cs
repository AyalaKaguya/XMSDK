using System;
using System.Drawing;
using System.Windows.Forms;
using XMSDK.Framework.Forms;

namespace XMSDK.Framework.Demo
{
    /// <summary>
    /// 信号系统演示窗体
    /// </summary>
    public partial class SignalSystemDemoForm : Form
    {
        private EnhancedSignalDemo _enhancedSignalDemo;
        private EnhancedMockDataDriver _dataDriver;

        public SignalSystemDemoForm()
        {
            InitializeComponent();
            InitializeDemo();
        }

        private void InitializeDemo()
        {
            try
            {
                // 创建增强版数据驱动
                _dataDriver = new EnhancedMockDataDriver();

                // 创建增强版信号宿主
                _enhancedSignalDemo = new EnhancedSignalDemo(_dataDriver, 1000); // 1秒轮询间隔

                // 设置信号源
                signalList.SetSignalSource(_enhancedSignalDemo);

                // 配置显示列 - 展示所有ObservableSignal特性
                signalList.VisibleColumns = SignalList.DisplayColumns.Default |
                                            SignalList.DisplayColumns.Description |
                                            SignalList.DisplayColumns.Unit |
                                            SignalList.DisplayColumns.Format |
                                            SignalList.DisplayColumns.ReadOnlyStatus;

                // 监听信号变化事件
                _enhancedSignalDemo.SignalValueChanged += OnSignalValueChanged;

                lblStatus.Text = "状态: 演示系统已启动 - 显示完整的信号特性描述";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"状态: 初始化失败 - {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"初始化演示失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SignalSystemDemoForm_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "状态: 演示运行中，信号自动轮询和刷新中...";

            // 显示使用说明
            var instructions =
                "演示说明：\n" +
                "• 控制信号：可读写的布尔信号\n" +
                "• 系统状态：只读的布尔信号\n" +
                "• 计数器：可读写的整数信号\n" +
                "• 温度：可读写的浮点信号，带单位显示\n" +
                "• 压力：可读写的双精度信号\n" +
                "• 状态码：可读写的字节信号\n\n" +
                "可以双击信号行进行编辑，系统会自动验证输入格式。\n" +
                "信号值会定期自动变化以模拟真实环境。";

            MessageBox.Show(instructions, "演示说明", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnToggleSignal_Click(object sender, EventArgs e)
        {
            try
            {
                // 切换控制信号的值
                bool currentValue = _enhancedSignalDemo.ControlSignal;
                _enhancedSignalDemo.ControlSignal = !currentValue;

                lblStatus.Text = $"状态: 控制信号已切换为 {(!currentValue ? "开启" : "关闭")}";
                lblStatus.ForeColor = Color.Blue;

                // 立即刷新显示
                signalList.RefreshSignalValues();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"状态: 切换信号失败 - {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"切换信号失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                signalList.RefreshSignalValues();
                lblStatus.Text = "状态: 手动刷新完成";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"状态: 刷新失败 - {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void OnSignalValueChanged(string address, object oldValue, object newValue, DateTime timestamp)
        {
            // 在UI线程上更新状态
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, object, object, DateTime>(OnSignalValueChanged), address, oldValue, newValue,
                    timestamp);
                return;
            }

            lblStatus.Text = $"状态: 信号 {address} 从 {oldValue} 变更为 {newValue} @ {timestamp:HH:mm:ss}";
            lblStatus.ForeColor = Color.Purple;
        }

        private void SignalSystemDemoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 清理资源
            try
            {
                _enhancedSignalDemo?.Dispose();
                signalList?.Dispose();
            }
            catch (Exception ex)
            {
                // 忽略清理时的异常
                System.Diagnostics.Debug.WriteLine($"清理资源时发生异常: {ex.Message}");
            }
        }
    }
}