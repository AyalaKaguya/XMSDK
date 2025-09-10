using XMSDK.Framework.Forms;

namespace HslCommunicationDemoForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.signalList1 = new XMSDK.Framework.Forms.SignalList();
            this.SuspendLayout();
            // 
            // signalList1
            // 
            this.signalList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.signalList1.Location = new System.Drawing.Point(0, 0);
            this.signalList1.Name = "signalList1";
            this.signalList1.Size = new System.Drawing.Size(800, 450);
            this.signalList1.TabIndex = 0;
            this.signalList1.VisibleColumns = (SignalList.DisplayColumns.Detailed);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.signalList1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
        }

        private XMSDK.Framework.Forms.SignalList signalList1;

        #endregion
    }
}