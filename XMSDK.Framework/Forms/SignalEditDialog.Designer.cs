// filepath: c:\Users\ayala\RiderProjects\XMSDK\XMSDK.Framework\Forms\SignalEditDialog.Designer.cs
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace XMSDK.Framework.Forms
{
    partial class SignalEditDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        private TextBox _textBoxValue = null!;
        private Label _labelName = null!;
        private Label _labelAddress = null!;
        private Label _labelType = null;
        private Label _labelValue = null!;
        private Label _labelHint = null!;
        private System.Windows.Forms.Button _buttonOk;
        private Button _buttonCancel = null!;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._labelName = new System.Windows.Forms.Label();
            this._labelAddress = new System.Windows.Forms.Label();
            this._labelType = new System.Windows.Forms.Label();
            this._labelValue = new System.Windows.Forms.Label();
            this._textBoxValue = new System.Windows.Forms.TextBox();
            this._labelHint = new System.Windows.Forms.Label();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _labelName
            // 
            this._labelName.AutoSize = true;
            this._labelName.Location = new System.Drawing.Point(12, 15);
            this._labelName.Name = "_labelName";
            this._labelName.Size = new System.Drawing.Size(41, 12);
            this._labelName.TabIndex = 9;
            this._labelName.Text = "名称：";
            // 
            // _labelAddress
            // 
            this._labelAddress.AutoSize = true;
            this._labelAddress.Location = new System.Drawing.Point(12, 45);
            this._labelAddress.Name = "_labelAddress";
            this._labelAddress.Size = new System.Drawing.Size(41, 12);
            this._labelAddress.TabIndex = 8;
            this._labelAddress.Text = "地址：";
            // 
            // _labelType
            // 
            this._labelType.AutoSize = true;
            this._labelType.Location = new System.Drawing.Point(12, 75);
            this._labelType.Name = "_labelType";
            this._labelType.Size = new System.Drawing.Size(41, 12);
            this._labelType.TabIndex = 7;
            this._labelType.Text = "类型：";
            // 
            // _labelValue
            // 
            this._labelValue.AutoSize = true;
            this._labelValue.Location = new System.Drawing.Point(12, 108);
            this._labelValue.Name = "_labelValue";
            this._labelValue.Size = new System.Drawing.Size(29, 12);
            this._labelValue.TabIndex = 3;
            this._labelValue.Text = "值：";
            // 
            // _textBoxValue
            // 
            this._textBoxValue.Location = new System.Drawing.Point(80, 105);
            this._textBoxValue.Name = "_textBoxValue";
            this._textBoxValue.Size = new System.Drawing.Size(200, 21);
            this._textBoxValue.TabIndex = 4;
            // 
            // _labelHint
            // 
            this._labelHint.AutoSize = true;
            this._labelHint.ForeColor = System.Drawing.Color.Gray;
            this._labelHint.Location = new System.Drawing.Point(80, 130);
            this._labelHint.Name = "_labelHint";
            this._labelHint.Size = new System.Drawing.Size(0, 12);
            this._labelHint.TabIndex = 5;
            // 
            // _buttonOk
            // 
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.Location = new System.Drawing.Point(125, 155);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 6;
            this._buttonOk.Text = "确定";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(205, 155);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 7;
            this._buttonCancel.Text = "取消";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // SignalEditDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(294, 191);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._textBoxValue);
            this.Controls.Add(this._labelHint);
            this.Controls.Add(this._labelType);
            this.Controls.Add(this._labelValue);
            this.Controls.Add(this._labelAddress);
            this.Controls.Add(this._labelName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimizeBox = false;
            this.Name = "SignalEditDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
