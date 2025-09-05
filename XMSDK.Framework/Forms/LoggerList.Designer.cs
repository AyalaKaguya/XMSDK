using System.ComponentModel;
using System.Windows.Forms;

namespace XMSDK.Framework.Forms
{
    partial class LoggerList
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        
        private ListView listViewLogs;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem copyMenuItem;
        private ToolStripMenuItem clearMenuItem;
        private ToolStripSeparator separator1;
        private ToolStripMenuItem autoScrollMenuItem;
        private ToolStripSeparator separator2;
        private ToolStripMenuItem logLevelMenuItem;
        private ToolStripMenuItem traceMenuItem;
        private ToolStripMenuItem debugMenuItem;
        private ToolStripMenuItem infoMenuItem;
        private ToolStripMenuItem warningMenuItem;
        private ToolStripMenuItem errorMenuItem;
        private ToolStripMenuItem criticalMenuItem;
        

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listViewLogs = new System.Windows.Forms.ListView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.autoScrollMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.logLevelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.warningMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.errorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.criticalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewLogs
            // 
            this.listViewLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewLogs.FullRowSelect = true;
            this.listViewLogs.GridLines = true;
            this.listViewLogs.Location = new System.Drawing.Point(0, 0);
            this.listViewLogs.Name = "listViewLogs";
            this.listViewLogs.Size = new System.Drawing.Size(800, 600);
            this.listViewLogs.TabIndex = 0;
            this.listViewLogs.UseCompatibleStateImageBehavior = false;
            this.listViewLogs.View = System.Windows.Forms.View.Details;
            this.listViewLogs.ContextMenuStrip = this.contextMenuStrip;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyMenuItem,
            this.clearMenuItem,
            this.separator1,
            this.autoScrollMenuItem,
            this.separator2,
            this.logLevelMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(153, 98);
            // 
            // copyMenuItem
            // 
            this.copyMenuItem.Name = "copyMenuItem";
            this.copyMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyMenuItem.Text = "复制";
            // 
            // clearMenuItem
            // 
            this.clearMenuItem.Name = "clearMenuItem";
            this.clearMenuItem.Size = new System.Drawing.Size(152, 22);
            this.clearMenuItem.Text = "清空";
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            this.separator1.Size = new System.Drawing.Size(149, 6);
            // 
            // autoScrollMenuItem
            // 
            this.autoScrollMenuItem.CheckOnClick = true;
            this.autoScrollMenuItem.Name = "autoScrollMenuItem";
            this.autoScrollMenuItem.Size = new System.Drawing.Size(152, 22);
            this.autoScrollMenuItem.Text = "自动滚动";
            this.autoScrollMenuItem.Checked = true;
            // 
            // separator2
            // 
            this.separator2.Name = "separator2";
            this.separator2.Size = new System.Drawing.Size(149, 6);
            // 
            // logLevelMenuItem
            // 
            this.logLevelMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.traceMenuItem,
            this.debugMenuItem,
            this.infoMenuItem,
            this.warningMenuItem,
            this.errorMenuItem,
            this.criticalMenuItem});
            this.logLevelMenuItem.Name = "logLevelMenuItem";
            this.logLevelMenuItem.Size = new System.Drawing.Size(152, 22);
            this.logLevelMenuItem.Text = "日志级别";
            // 
            // traceMenuItem
            // 
            this.traceMenuItem.CheckOnClick = true;
            this.traceMenuItem.Name = "traceMenuItem";
            this.traceMenuItem.Size = new System.Drawing.Size(152, 22);
            this.traceMenuItem.Text = "Trace";
            this.traceMenuItem.Checked = true;
            // 
            // debugMenuItem
            // 
            this.debugMenuItem.CheckOnClick = true;
            this.debugMenuItem.Name = "debugMenuItem";
            this.debugMenuItem.Size = new System.Drawing.Size(152, 22);
            this.debugMenuItem.Text = "Debug";
            this.debugMenuItem.Checked = true;
            // 
            // infoMenuItem
            // 
            this.infoMenuItem.CheckOnClick = true;
            this.infoMenuItem.Name = "infoMenuItem";
            this.infoMenuItem.Size = new System.Drawing.Size(152, 22);
            this.infoMenuItem.Text = "Info";
            this.infoMenuItem.Checked = true;
            // 
            // warningMenuItem
            // 
            this.warningMenuItem.CheckOnClick = true;
            this.warningMenuItem.Name = "warningMenuItem";
            this.warningMenuItem.Size = new System.Drawing.Size(152, 22);
            this.warningMenuItem.Text = "Warning";
            this.warningMenuItem.Checked = true;
            // 
            // errorMenuItem
            // 
            this.errorMenuItem.CheckOnClick = true;
            this.errorMenuItem.Name = "errorMenuItem";
            this.errorMenuItem.Size = new System.Drawing.Size(152, 22);
            this.errorMenuItem.Text = "Error";
            this.errorMenuItem.Checked = true;
            // 
            // criticalMenuItem
            // 
            this.criticalMenuItem.CheckOnClick = true;
            this.criticalMenuItem.Name = "criticalMenuItem";
            this.criticalMenuItem.Size = new System.Drawing.Size(152, 22);
            this.criticalMenuItem.Text = "Critical";
            this.criticalMenuItem.Checked = true;
            // 
            // ColorfulLoggerList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listViewLogs);
            this.Name = "LoggerList";
            this.Size = new System.Drawing.Size(800, 600);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}