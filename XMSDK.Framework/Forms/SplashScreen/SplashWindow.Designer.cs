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
        
        // 设计器生成的控件字段声明
        private System.Windows.Forms.Panel overlayPanel;
        private System.Windows.Forms.Label lblAppTitle;
        private Label lblDescription;
        private Label lblAuthor;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblItemDesc;
        private System.Windows.Forms.Label lblItemDetail;

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
            this.lblItemDetail = new System.Windows.Forms.Label();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.lblItemDesc = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.overlayPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // overlayPanel
            // 
            this.overlayPanel.Controls.Add(this.lblItemDetail);
            this.overlayPanel.Controls.Add(this.lblAppTitle);
            this.overlayPanel.Controls.Add(this.lblItemDesc);
            this.overlayPanel.Controls.Add(this.lblDescription);
            this.overlayPanel.Controls.Add(this.lblAuthor);
            this.overlayPanel.Controls.Add(this.lblCopyright);
            this.overlayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overlayPanel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.overlayPanel.Location = new System.Drawing.Point(0, 0);
            this.overlayPanel.Name = "overlayPanel";
            this.overlayPanel.Padding = new System.Windows.Forms.Padding(28, 28, 28, 20);
            this.overlayPanel.Size = new System.Drawing.Size(594, 306);
            this.overlayPanel.TabIndex = 0;
            // 
            // lblItemDetail
            // 
            this.lblItemDetail.AutoSize = true;
            this.lblItemDetail.BackColor = System.Drawing.Color.Transparent;
            this.lblItemDetail.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemDetail.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblItemDetail.Location = new System.Drawing.Point(31, 262);
            this.lblItemDetail.Name = "lblItemDetail";
            this.lblItemDetail.Size = new System.Drawing.Size(22, 12);
            this.lblItemDetail.TabIndex = 5;
            this.lblItemDetail.Text = string.Empty;
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
            this.lblAppTitle.Text = "XMSDK.Splash";
            // 
            // lblItemDesc
            // 
            this.lblItemDesc.AutoSize = true;
            this.lblItemDesc.BackColor = System.Drawing.Color.Transparent;
            this.lblItemDesc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblItemDesc.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblItemDesc.Location = new System.Drawing.Point(31, 247);
            this.lblItemDesc.Name = "lblItemDesc";
            this.lblItemDesc.Size = new System.Drawing.Size(55, 15);
            this.lblItemDesc.TabIndex = 1;
            this.lblItemDesc.Text = "Initialization...";
            this.lblItemDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.lblDescription.Text = "XMSDK.Splash.Description";
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
            this.lblAuthor.Text = "XMSDK.Splash.Author";
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
            this.lblCopyright.Text = "XMSDK.Splash.Copyright";
            // 
            // SplashWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(594, 306);
            this.Controls.Add(this.overlayPanel);
            this.DoubleBuffered = true;
            this.Name = "SplashWindow";
            this.Text = "XMSDK.SplashWindow";
            this.overlayPanel.ResumeLayout(false);
            this.overlayPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}