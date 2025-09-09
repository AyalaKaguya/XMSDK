using System.ComponentModel;
using System.Windows.Forms;

namespace XMSDK.Framework.Forms;

partial class SignalList
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new Container();
        
        listViewSignals = new ListView();
        contextMenuStrip = new ContextMenuStrip(components);
        menuItemRefresh = new ToolStripMenuItem();
        menuItemSeparator = new ToolStripSeparator();
        menuItemEdit = new ToolStripMenuItem();
        timerRefresh = new Timer(components);
        
        SuspendLayout();
        
        // 
        // listViewSignals
        // 
        listViewSignals.Dock = DockStyle.Fill;
        listViewSignals.FullRowSelect = true;
        listViewSignals.GridLines = true;
        listViewSignals.View = View.Details;
        listViewSignals.ContextMenuStrip = contextMenuStrip;
        listViewSignals.DoubleClick += ListViewSignals_DoubleClick;
        
        // 添加列
        listViewSignals.Columns.Add("描述", 150);
        listViewSignals.Columns.Add("地址", 100);
        listViewSignals.Columns.Add("信号值", 120);
        listViewSignals.Columns.Add("更新时间", 120);
        listViewSignals.Columns.Add("类型", 80);
        listViewSignals.Columns.Add("分组", 80);
        
        // 
        // contextMenuStrip
        // 
        contextMenuStrip.Items.AddRange(new ToolStripItem[] {
            menuItemRefresh,
            menuItemSeparator,
            menuItemEdit
        });
        
        // 
        // menuItemRefresh
        // 
        menuItemRefresh.Text = "立即刷新(&R)";
        menuItemRefresh.Click += MenuItemRefresh_Click;
        
        // 
        // menuItemEdit
        // 
        menuItemEdit.Text = "编辑信号值(&E)";
        menuItemEdit.Click += MenuItemEdit_Click;
        
        // 
        // timerRefresh
        // 
        timerRefresh.Interval = 1000; // 1秒刷新一次
        timerRefresh.Tick += TimerRefresh_Tick;
        
        // 
        // SignalList
        // 
        Controls.Add(listViewSignals);
        
        ResumeLayout(false);
    }

    #endregion

    private ListView listViewSignals;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem menuItemRefresh;
    private ToolStripSeparator menuItemSeparator;
    private ToolStripMenuItem menuItemEdit;
    private Timer timerRefresh;
}