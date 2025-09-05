using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using XMSDK.Framework.Forms;

namespace XMSDK.Framework.Demo
{
    partial class LoggerDemoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        // 控件声明
        private XMSDK.Framework.Forms.LoggerList loggerList;
        private Panel buttonPanel;
        private Button btnTrace;
        private Button btnDebug;
        private Button btnInfo;
        private Button btnWarning;
        private Button btnError;
        private Button btnCritical;
        private Button btnCustomColor;
        private Button btnBackgroundColor;
        private Button btnClear;
        private Button btnSetMaxCount;
        private Button btnToggleAutoScroll;
        private Button btnStressTest;
        private Button btnHighSpeedLog;
        private Button btnAutoLog;
        private Timer logTimer;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.loggerList = new XMSDK.Framework.Forms.LoggerList();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.btnTrace = new System.Windows.Forms.Button();
            this.btnDebug = new System.Windows.Forms.Button();
            this.btnInfo = new System.Windows.Forms.Button();
            this.btnWarning = new System.Windows.Forms.Button();
            this.btnError = new System.Windows.Forms.Button();
            this.btnCritical = new System.Windows.Forms.Button();
            this.btnCustomColor = new System.Windows.Forms.Button();
            this.btnBackgroundColor = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnSetMaxCount = new System.Windows.Forms.Button();
            this.btnToggleAutoScroll = new System.Windows.Forms.Button();
            this.btnStressTest = new System.Windows.Forms.Button();
            this.btnHighSpeedLog = new System.Windows.Forms.Button();
            this.btnAutoLog = new System.Windows.Forms.Button();
            this.logTimer = new System.Windows.Forms.Timer(this.components);
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // loggerList
            // 
            this.loggerList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.loggerList.AutoScroll = true;
            this.loggerList.Location = new System.Drawing.Point(10, 55);
            this.loggerList.MaxLogCount = 1000;
            this.loggerList.Name = "loggerList";
            this.loggerList.Size = new System.Drawing.Size(1180, 582);
            this.loggerList.TabIndex = 0;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPanel.Controls.Add(this.btnTrace);
            this.buttonPanel.Controls.Add(this.btnDebug);
            this.buttonPanel.Controls.Add(this.btnInfo);
            this.buttonPanel.Controls.Add(this.btnWarning);
            this.buttonPanel.Controls.Add(this.btnError);
            this.buttonPanel.Controls.Add(this.btnCritical);
            this.buttonPanel.Controls.Add(this.btnCustomColor);
            this.buttonPanel.Controls.Add(this.btnBackgroundColor);
            this.buttonPanel.Controls.Add(this.btnClear);
            this.buttonPanel.Controls.Add(this.btnSetMaxCount);
            this.buttonPanel.Controls.Add(this.btnToggleAutoScroll);
            this.buttonPanel.Controls.Add(this.btnStressTest);
            this.buttonPanel.Controls.Add(this.btnHighSpeedLog);
            this.buttonPanel.Controls.Add(this.btnAutoLog);
            this.buttonPanel.Location = new System.Drawing.Point(10, 9);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(1180, 42);
            this.buttonPanel.TabIndex = 1;
            // 
            // btnTrace
            // 
            this.btnTrace.Location = new System.Drawing.Point(0, 9);
            this.btnTrace.Name = "btnTrace";
            this.btnTrace.Size = new System.Drawing.Size(80, 23);
            this.btnTrace.TabIndex = 0;
            this.btnTrace.Text = "Trace";
            this.btnTrace.UseVisualStyleBackColor = true;
            this.btnTrace.Click += new System.EventHandler(this.BtnTrace_Click);
            // 
            // btnDebug
            // 
            this.btnDebug.Location = new System.Drawing.Point(85, 9);
            this.btnDebug.Name = "btnDebug";
            this.btnDebug.Size = new System.Drawing.Size(80, 23);
            this.btnDebug.TabIndex = 1;
            this.btnDebug.Text = "Debug";
            this.btnDebug.UseVisualStyleBackColor = true;
            this.btnDebug.Click += new System.EventHandler(this.BtnDebug_Click);
            // 
            // btnInfo
            // 
            this.btnInfo.Location = new System.Drawing.Point(170, 9);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(80, 23);
            this.btnInfo.TabIndex = 2;
            this.btnInfo.Text = "Info";
            this.btnInfo.UseVisualStyleBackColor = true;
            this.btnInfo.Click += new System.EventHandler(this.BtnInfo_Click);
            // 
            // btnWarning
            // 
            this.btnWarning.Location = new System.Drawing.Point(255, 9);
            this.btnWarning.Name = "btnWarning";
            this.btnWarning.Size = new System.Drawing.Size(80, 23);
            this.btnWarning.TabIndex = 3;
            this.btnWarning.Text = "Warning";
            this.btnWarning.UseVisualStyleBackColor = true;
            this.btnWarning.Click += new System.EventHandler(this.BtnWarning_Click);
            // 
            // btnError
            // 
            this.btnError.Location = new System.Drawing.Point(340, 9);
            this.btnError.Name = "btnError";
            this.btnError.Size = new System.Drawing.Size(80, 23);
            this.btnError.TabIndex = 4;
            this.btnError.Text = "Error";
            this.btnError.UseVisualStyleBackColor = true;
            this.btnError.Click += new System.EventHandler(this.BtnError_Click);
            // 
            // btnCritical
            // 
            this.btnCritical.Location = new System.Drawing.Point(425, 9);
            this.btnCritical.Name = "btnCritical";
            this.btnCritical.Size = new System.Drawing.Size(80, 23);
            this.btnCritical.TabIndex = 5;
            this.btnCritical.Text = "Critical";
            this.btnCritical.UseVisualStyleBackColor = true;
            this.btnCritical.Click += new System.EventHandler(this.BtnCritical_Click);
            // 
            // btnCustomColor
            // 
            this.btnCustomColor.Location = new System.Drawing.Point(510, 9);
            this.btnCustomColor.Name = "btnCustomColor";
            this.btnCustomColor.Size = new System.Drawing.Size(80, 23);
            this.btnCustomColor.TabIndex = 6;
            this.btnCustomColor.Text = "前景色";
            this.btnCustomColor.UseVisualStyleBackColor = true;
            this.btnCustomColor.Click += new System.EventHandler(this.BtnCustomColor_Click);
            // 
            // btnBackgroundColor
            // 
            this.btnBackgroundColor.Location = new System.Drawing.Point(595, 9);
            this.btnBackgroundColor.Name = "btnBackgroundColor";
            this.btnBackgroundColor.Size = new System.Drawing.Size(80, 23);
            this.btnBackgroundColor.TabIndex = 7;
            this.btnBackgroundColor.Text = "背景色";
            this.btnBackgroundColor.UseVisualStyleBackColor = true;
            this.btnBackgroundColor.Click += new System.EventHandler(this.BtnBackgroundColor_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(680, 9);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(80, 23);
            this.btnClear.TabIndex = 8;
            this.btnClear.Text = "清空";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // btnSetMaxCount
            // 
            this.btnSetMaxCount.Location = new System.Drawing.Point(765, 9);
            this.btnSetMaxCount.Name = "btnSetMaxCount";
            this.btnSetMaxCount.Size = new System.Drawing.Size(80, 23);
            this.btnSetMaxCount.TabIndex = 9;
            this.btnSetMaxCount.Text = "最大条数";
            this.btnSetMaxCount.UseVisualStyleBackColor = true;
            this.btnSetMaxCount.Click += new System.EventHandler(this.BtnSetMaxCount_Click);
            // 
            // btnToggleAutoScroll
            // 
            this.btnToggleAutoScroll.BackColor = System.Drawing.Color.LightGreen;
            this.btnToggleAutoScroll.Location = new System.Drawing.Point(850, 9);
            this.btnToggleAutoScroll.Name = "btnToggleAutoScroll";
            this.btnToggleAutoScroll.Size = new System.Drawing.Size(80, 23);
            this.btnToggleAutoScroll.TabIndex = 10;
            this.btnToggleAutoScroll.Text = "关闭滚动";
            this.btnToggleAutoScroll.UseVisualStyleBackColor = false;
            this.btnToggleAutoScroll.Click += new System.EventHandler(this.BtnToggleAutoScroll_Click);
            // 
            // btnStressTest
            // 
            this.btnStressTest.Location = new System.Drawing.Point(935, 9);
            this.btnStressTest.Name = "btnStressTest";
            this.btnStressTest.Size = new System.Drawing.Size(80, 23);
            this.btnStressTest.TabIndex = 11;
            this.btnStressTest.Text = "性能测试";
            this.btnStressTest.UseVisualStyleBackColor = true;
            this.btnStressTest.Click += new System.EventHandler(this.BtnStressTest_Click);
            // 
            // btnHighSpeedLog
            // 
            this.btnHighSpeedLog.Location = new System.Drawing.Point(1020, 9);
            this.btnHighSpeedLog.Name = "btnHighSpeedLog";
            this.btnHighSpeedLog.Size = new System.Drawing.Size(80, 23);
            this.btnHighSpeedLog.TabIndex = 12;
            this.btnHighSpeedLog.Text = "高速日志";
            this.btnHighSpeedLog.UseVisualStyleBackColor = true;
            this.btnHighSpeedLog.Click += new System.EventHandler(this.BtnHighSpeedLog_Click);
            // 
            // btnAutoLog
            // 
            this.btnAutoLog.Location = new System.Drawing.Point(1105, 9);
            this.btnAutoLog.Name = "btnAutoLog";
            this.btnAutoLog.Size = new System.Drawing.Size(80, 23);
            this.btnAutoLog.TabIndex = 13;
            this.btnAutoLog.Text = "自动日志";
            this.btnAutoLog.UseVisualStyleBackColor = true;
            this.btnAutoLog.Click += new System.EventHandler(this.BtnAutoLog_Click);
            // 
            // logTimer
            // 
            this.logTimer.Interval = 1000;
            this.logTimer.Tick += new System.EventHandler(this.LogTimer_Tick);
            // 
            // LoggerDemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 646);
            this.Controls.Add(this.buttonPanel);
            this.Controls.Add(this.loggerList);
            this.MinimumSize = new System.Drawing.Size(800, 465);
            this.Name = "LoggerDemoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "日志查看器演示程序 V2";
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
