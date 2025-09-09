using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace XMSDK.Framework.Demo
{
    partial class SignalSystemDemoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.components = new Container();
            this.gbControls = new GroupBox();
            this.btnToggleSignal = new Button();
            this.btnRefresh = new Button();
            this.lblStatus = new Label();
            this.gbSignalList = new GroupBox();
            this.signalList = new XMSDK.Framework.Forms.SignalList();
            this.gbControls.SuspendLayout();
            this.gbSignalList.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // gbControls
            // 
            this.gbControls.Controls.Add(this.btnToggleSignal);
            this.gbControls.Controls.Add(this.btnRefresh);
            this.gbControls.Controls.Add(this.lblStatus);
            this.gbControls.Location = new Point(10, 10);
            this.gbControls.Name = "gbControls";
            this.gbControls.Size = new Size(760, 80);
            this.gbControls.TabIndex = 0;
            this.gbControls.TabStop = false;
            this.gbControls.Text = "控制面板";
            this.gbControls.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right));
            
            // 
            // btnToggleSignal
            // 
            this.btnToggleSignal.Location = new Point(10, 25);
            this.btnToggleSignal.Name = "btnToggleSignal";
            this.btnToggleSignal.Size = new Size(120, 30);
            this.btnToggleSignal.TabIndex = 0;
            this.btnToggleSignal.Text = "切换控制信号";
            this.btnToggleSignal.UseVisualStyleBackColor = true;
            this.btnToggleSignal.Click += new System.EventHandler(this.BtnToggleSignal_Click);
            
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new Point(140, 25);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(100, 30);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "手动刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = false;
            this.lblStatus.ForeColor = Color.Green;
            this.lblStatus.Location = new Point(250, 30);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(300, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "状态: 准备就绪";
            
            // 
            // gbSignalList
            // 
            this.gbSignalList.Controls.Add(this.signalList);
            this.gbSignalList.Location = new Point(10, 100);
            this.gbSignalList.Name = "gbSignalList";
            this.gbSignalList.Size = new Size(760, 460);
            this.gbSignalList.TabIndex = 1;
            this.gbSignalList.TabStop = false;
            this.gbSignalList.Text = "信号列表 - 实时监控";
            this.gbSignalList.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right));
            
            // 
            // signalList
            // 
            this.signalList.Location = new Point(10, 25);
            this.signalList.Name = "signalList";
            this.signalList.Size = new Size(740, 425);
            this.signalList.TabIndex = 0;
            this.signalList.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right));
            
            // 
            // SignalSystemDemoForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 561);
            this.Controls.Add(this.gbControls);
            this.Controls.Add(this.gbSignalList);
            this.MinimumSize = new Size(600, 400);
            this.Name = "SignalSystemDemoForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "XMSDK 信号系统演示";
            this.Load += new System.EventHandler(this.SignalSystemDemoForm_Load);
            this.FormClosed += new FormClosedEventHandler(this.SignalSystemDemoForm_FormClosed);
            
            this.gbControls.ResumeLayout(false);
            this.gbSignalList.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private XMSDK.Framework.Forms.SignalList signalList;
        private Button btnToggleSignal;
        private Button btnRefresh;
        private Label lblStatus;
        private GroupBox gbControls;
        private GroupBox gbSignalList;
    }
}
