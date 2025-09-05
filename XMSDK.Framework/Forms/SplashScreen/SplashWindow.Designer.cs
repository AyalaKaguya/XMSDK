using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace XMSDK.Framework.Forms.SplashScreen
{
    sealed partial class SplashWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        
        // 新增的控件字段声明
        private System.Windows.Forms.Panel overlayPanel;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblAuthor;
        private Label lblCurrentItem;
        private Label lblPercent;
        private ProgressBar progressBar;
        private System.Windows.Forms.Label lblCopyright;
        private ListBox listBoxSteps; // 步骤列表
        private Label lblStepCounter; // (x/n)

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
            this.overlayPanel = new System.Windows.Forms.Panel();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblCurrentItem = new System.Windows.Forms.Label();
            this.lblPercent = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStepCounter = new System.Windows.Forms.Label();
            this.listBoxSteps = new System.Windows.Forms.ListBox();
            this.overlayPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // overlayPanel
            // 
            this.overlayPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.overlayPanel.Controls.Add(this.lblAppTitle);
            this.overlayPanel.Controls.Add(this.lblDescription);
            this.overlayPanel.Controls.Add(this.lblAuthor);
            this.overlayPanel.Controls.Add(this.lblCopyright);
            this.overlayPanel.Controls.Add(this.lblCurrentItem);
            this.overlayPanel.Controls.Add(this.lblPercent);
            this.overlayPanel.Controls.Add(this.progressBar);
            this.overlayPanel.Controls.Add(this.lblStepCounter);
            this.overlayPanel.Controls.Add(this.listBoxSteps);
            this.overlayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overlayPanel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.overlayPanel.Location = new System.Drawing.Point(0, 0);
            this.overlayPanel.Name = "overlayPanel";
            this.overlayPanel.Padding = new System.Windows.Forms.Padding(28, 28, 28, 20);
            this.overlayPanel.Size = new System.Drawing.Size(760, 420);
            this.overlayPanel.TabIndex = 0;
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAppTitle.ForeColor = System.Drawing.Color.White;
            this.lblAppTitle.Location = new System.Drawing.Point(31, 28);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(150, 41);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Text = "应用名称";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDescription.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblDescription.Location = new System.Drawing.Point(35, 80);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(73, 20);
            this.lblDescription.TabIndex = 1;
            this.lblDescription.Text = "应用描述";
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAuthor.ForeColor = System.Drawing.Color.LightGray;
            this.lblAuthor.Location = new System.Drawing.Point(35, 108);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(33, 15);
            this.lblAuthor.TabIndex = 2;
            this.lblAuthor.Text = "作者";
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCopyright.ForeColor = System.Drawing.Color.LightGray;
            this.lblCopyright.Location = new System.Drawing.Point(35, 126);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(58, 13);
            this.lblCopyright.TabIndex = 3;
            this.lblCopyright.Text = "Copyright";
            // 
            // lblCurrentItem
            // 
            this.lblCurrentItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentItem.AutoEllipsis = true;
            this.lblCurrentItem.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCurrentItem.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblCurrentItem.Location = new System.Drawing.Point(40, 581);
            this.lblCurrentItem.Name = "lblCurrentItem";
            this.lblCurrentItem.Size = new System.Drawing.Size(844, 20);
            this.lblCurrentItem.TabIndex = 4;
            this.lblCurrentItem.Text = "准备开始...";
            // 
            // lblPercent
            // 
            this.lblPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPercent.AutoSize = true;
            this.lblPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPercent.ForeColor = System.Drawing.Color.White;
            this.lblPercent.Location = new System.Drawing.Point(884, 581);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(24, 15);
            this.lblPercent.TabIndex = 5;
            this.lblPercent.Text = "0%";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(40, 581);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(844, 20);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 6;
            // 
            // lblStepCounter
            // 
            this.lblStepCounter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStepCounter.AutoSize = true;
            this.lblStepCounter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStepCounter.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblStepCounter.Location = new System.Drawing.Point(38, 581);
            this.lblStepCounter.Name = "lblStepCounter";
            this.lblStepCounter.Size = new System.Drawing.Size(34, 15);
            this.lblStepCounter.TabIndex = 7;
            this.lblStepCounter.Text = "(0/0)";
            // 
            // listBoxSteps
            // 
            this.listBoxSteps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSteps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.listBoxSteps.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxSteps.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBoxSteps.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.listBoxSteps.IntegralHeight = false;
            this.listBoxSteps.ItemHeight = 12;
            this.listBoxSteps.Location = new System.Drawing.Point(844, 30);
            this.listBoxSteps.Name = "listBoxSteps";
            this.listBoxSteps.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listBoxSteps.Size = new System.Drawing.Size(300, 581);
            this.listBoxSteps.TabIndex = 8;
            // 
            // SplashWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(760, 420);
            this.Controls.Add(this.overlayPanel);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(640, 380);
            this.Name = "SplashWindow";
            this.Text = "Splash";
            this.overlayPanel.ResumeLayout(false);
            this.overlayPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}