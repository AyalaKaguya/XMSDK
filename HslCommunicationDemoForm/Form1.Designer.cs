namespace HslCommunicationDemoForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.signalList1 = new XMSDK.Framework.Forms.SignalList();
            this.SuspendLayout();
            this.signalList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.signalList1.Location = new System.Drawing.Point(0, 0);
            this.signalList1.TabIndex = 0;
            this.signalList1.VisibleColumns = ((XMSDK.Framework.Forms.SignalList.DisplayColumns)((((((XMSDK.Framework.Forms.SignalList.DisplayColumns.Name | XMSDK.Framework.Forms.SignalList.DisplayColumns.Address) | XMSDK.Framework.Forms.SignalList.DisplayColumns.Value) | XMSDK.Framework.Forms.SignalList.DisplayColumns.UpdateTime) | XMSDK.Framework.Forms.SignalList.DisplayColumns.Type) | XMSDK.Framework.Forms.SignalList.DisplayColumns.Group)));
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