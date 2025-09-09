using System.ComponentModel;
using System.Windows.Forms;

namespace XMSDK.Framework.Forms;

partial class SignalList
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new Container();
        this.listViewSignals = new ListView();
        this.contextMenuStrip = new ContextMenuStrip(this.components);
        this.menuItemRefresh = new ToolStripMenuItem();
        this.menuItemSeparator = new ToolStripSeparator();
        this.menuItemEdit = new ToolStripMenuItem();
        
        this.contextMenuStrip.SuspendLayout();
        this.SuspendLayout();
        
        // 
        // listViewSignals
        // 
        this.listViewSignals.Dock = DockStyle.Fill;
        this.listViewSignals.FullRowSelect = true;
        this.listViewSignals.GridLines = true;
        this.listViewSignals.View = View.Details;
        this.listViewSignals.ContextMenuStrip = this.contextMenuStrip;
        this.listViewSignals.Name = "listViewSignals";
        this.listViewSignals.UseCompatibleStateImageBehavior = false;
        this.listViewSignals.HideSelection = false;
        this.listViewSignals.MultiSelect = false;
        this.listViewSignals.DoubleClick += new System.EventHandler(this.ListViewSignals_DoubleClick);
        
        // 添加列
        this.listViewSignals.Columns.Add("描述", 150);
        this.listViewSignals.Columns.Add("地址", 100);
        this.listViewSignals.Columns.Add("信号值", 120);
        this.listViewSignals.Columns.Add("更新时间", 120);
        this.listViewSignals.Columns.Add("类型", 80);
        this.listViewSignals.Columns.Add("分组", 80);
        
        // 
        // contextMenuStrip
        // 
        this.contextMenuStrip.Items.AddRange(new ToolStripItem[] {
            this.menuItemRefresh,
            this.menuItemSeparator,
            this.menuItemEdit
        });
        this.contextMenuStrip.Name = "contextMenuStrip";
        
        // 
        // menuItemRefresh
        // 
        this.menuItemRefresh.Name = "menuItemRefresh";
        this.menuItemRefresh.Text = "立即刷新(&R)";
        this.menuItemRefresh.Click += new System.EventHandler(this.MenuItemRefresh_Click);
        
        // 
        // menuItemSeparator
        // 
        this.menuItemSeparator.Name = "menuItemSeparator";
        
        // 
        // menuItemEdit
        // 
        this.menuItemEdit.Name = "menuItemEdit";
        this.menuItemEdit.Text = "编辑信号值(&E)";
        this.menuItemEdit.Click += new System.EventHandler(this.MenuItemEdit_Click);
        
        // 
        // SignalList
        // 
        this.Controls.Add(this.listViewSignals);
        
        this.contextMenuStrip.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    #endregion

    private ListView listViewSignals;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem menuItemRefresh;
    private ToolStripSeparator menuItemSeparator;
    private ToolStripMenuItem menuItemEdit;
}