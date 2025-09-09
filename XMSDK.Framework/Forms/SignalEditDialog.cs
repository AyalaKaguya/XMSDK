using System;
using System.Drawing;
using System.Windows.Forms;
using XMSDK.Framework.Communication.Signal;

namespace XMSDK.Framework.Forms
{
    /// <summary>
    /// 信号编辑对话框，用于编辑信号值。
    /// </summary>
    public partial class SignalEditDialog : Form
    {
        private readonly SignalInfo _signal;

        /// <summary>
        /// 获取编辑后的新值
        /// </summary>
        public object? NewValue { get; private set; }

        public SignalEditDialog(SignalInfo signal)
        {
            _signal = signal ?? throw new ArgumentNullException(nameof(signal));
            InitializeComponent();
            SetupDialog();
        }

        private void SetupDialog()
        {
            // 显示信号信息
            _labelName.Text = $"名称：{_signal.Name}";
            _labelAddress.Text = $"地址：{_signal.Address}";
            _labelType.Text = $"类型：{GetTypeDisplayName(_signal.SignalType)}";

            // 设置当前值
            _textBoxValue.Text = SignalConverterManager.ConvertToString(_signal.CurrentValue, _signal.SignalType, _signal.Format);

            // 提示信息
            SetupInputHints();
        }

        private void SetupInputHints()
        {
            var hintText = _signal.SignalType switch
            {
                var t when t == typeof(bool) => "输入: true/false, 1/0, on/off, 开/关",
                var t when t == typeof(int) => "输入整数值",
                var t when t == typeof(long) => "输入长整数值",
                var t when t == typeof(float) => "输入浮点数值",
                var t when t == typeof(double) => "输入双精度浮点数值",
                var t when t == typeof(byte) => "输入字节值 (0-255)",
                var t when t == typeof(short) => "输入短整数值",
                _ => "输入值"
            };

            _labelHint.Text = hintText;
            _labelHint.Font = new Font(Font.FontFamily, 7.5f);
            _labelHint.ForeColor = Color.Gray;
        }

        private string GetTypeDisplayName(Type type)
        {
            return type switch
            {
                var t when t == typeof(bool) => "布尔型",
                var t when t == typeof(int) => "整数型",
                var t when t == typeof(long) => "长整数型",
                var t when t == typeof(float) => "单精度浮点型",
                var t when t == typeof(double) => "双精度浮点型",
                var t when t == typeof(byte) => "字节型",
                var t when t == typeof(short) => "短整型",
                _ => type.Name
            };
        }

        private void ButtonOK_Click(object? sender, EventArgs e)
        {
            // 验证并转换输入值
            if (SignalConverterManager.TryParseValue(_textBoxValue.Text, _signal.SignalType, out var newValue))
            {
                NewValue = newValue;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show($"输入的值格式不正确，请输入有效的{GetTypeDisplayName(_signal.SignalType)}值。", 
                    "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _textBoxValue.Focus();
                _textBoxValue.SelectAll();
            }
        }
    }
}
