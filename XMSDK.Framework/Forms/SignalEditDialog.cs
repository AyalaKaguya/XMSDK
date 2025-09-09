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
        private TextBox _textBoxValue;
        private Label _labelName;
        private Label _labelAddress;
        private Label _labelType;
        private Button _buttonOk;
        private Button _buttonCancel;

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

        private void InitializeComponent()
        {
            _labelName = new Label();
            _labelAddress = new Label();
            _labelType = new Label();
            _textBoxValue = new TextBox();
            _buttonOk = new Button();
            _buttonCancel = new Button();

            SuspendLayout();

            // 
            // labelName
            // 
            _labelName.AutoSize = true;
            _labelName.Location = new Point(12, 15);
            _labelName.Name = "_labelName";
            _labelName.Size = new Size(44, 15);
            _labelName.TabIndex = 0;
            _labelName.Text = "名称：";

            // 
            // labelAddress
            // 
            _labelAddress.AutoSize = true;
            _labelAddress.Location = new Point(12, 45);
            _labelAddress.Name = "_labelAddress";
            _labelAddress.Size = new Size(44, 15);
            _labelAddress.TabIndex = 1;
            _labelAddress.Text = "地址：";

            // 
            // labelType
            // 
            _labelType.AutoSize = true;
            _labelType.Location = new Point(12, 75);
            _labelType.Name = "_labelType";
            _labelType.Size = new Size(44, 15);
            _labelType.TabIndex = 2;
            _labelType.Text = "类型：";

            // 
            // textBoxValue
            // 
            _textBoxValue.Location = new Point(80, 105);
            _textBoxValue.Name = "_textBoxValue";
            _textBoxValue.Size = new Size(200, 23);
            _textBoxValue.TabIndex = 3;

            // 
            // buttonOK
            // 
            _buttonOk.DialogResult = DialogResult.OK;
            _buttonOk.Location = new Point(125, 145);
            _buttonOk.Name = "_buttonOk";
            _buttonOk.Size = new Size(75, 23);
            _buttonOk.TabIndex = 4;
            _buttonOk.Text = "确定";
            _buttonOk.UseVisualStyleBackColor = true;
            _buttonOk.Click += ButtonOK_Click;

            // 
            // buttonCancel
            // 
            _buttonCancel.DialogResult = DialogResult.Cancel;
            _buttonCancel.Location = new Point(205, 145);
            _buttonCancel.Name = "_buttonCancel";
            _buttonCancel.Size = new Size(75, 23);
            _buttonCancel.TabIndex = 5;
            _buttonCancel.Text = "取消";
            _buttonCancel.UseVisualStyleBackColor = true;

            // 
            // SignalEditDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(294, 181);
            Controls.Add(_buttonCancel);
            Controls.Add(_buttonOk);
            Controls.Add(_textBoxValue);
            Controls.Add(_labelType);
            Controls.Add(_labelAddress);
            Controls.Add(_labelName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SignalEditDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "编辑信号值";

            ResumeLayout(false);
            PerformLayout();
        }

        private void SetupDialog()
        {
            // 显示信号信息
            _labelName.Text = $"名称：{_signal.Name}";
            _labelAddress.Text = $"地址：{_signal.Address}";
            _labelType.Text = $"类型：{GetTypeDisplayName(_signal.SignalType)}";

            // 设置当前值
            _textBoxValue.Text = SignalConverterManager.ConvertToString(_signal.CurrentValue, _signal.SignalType, _signal.Format);

            // 添加值标签
            var labelValue = new Label
            {
                Text = "值：",
                Location = new Point(12, 108),
                AutoSize = true
            };
            Controls.Add(labelValue);

            // 根据信号类型提供输入提示
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

            // 创建提示标签
            var labelHint = new Label
            {
                Text = hintText,
                Location = new Point(80, 130),
                Size = new Size(200, 15),
                ForeColor = Color.Gray,
                Font = new Font(Font.FontFamily, 7.5f)
            };
            Controls.Add(labelHint);

            // 调整窗体高度
            Height += 15;
            _buttonOk.Location = new Point(_buttonOk.Location.X, _buttonOk.Location.Y + 15);
            _buttonCancel.Location = new Point(_buttonCancel.Location.X, _buttonCancel.Location.Y + 15);
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
                var t when t == typeof(short) => "短整数型",
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
