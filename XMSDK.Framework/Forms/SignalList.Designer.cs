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
        if (_notifier != null)
        {
            _notifier.SignalValueChanged -= OnSignalValueChanged;
        }
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
        this.components = new Container();
        this.listViewSignals = new ListView();
        this.contextMenuStrip = new ContextMenuStrip(this.components);
        this.menuItemRefresh = new ToolStripMenuItem();
        this.menuItemSeparator1 = new ToolStripSeparator();
        this.menuItemEdit = new ToolStripMenuItem();
        this.menuItemSeparator2 = new ToolStripSeparator();
        this.menuItemColumns = new ToolStripMenuItem();
        this.menuItemShowDescription = new ToolStripMenuItem();
        this.menuItemShowUnit = new ToolStripMenuItem();
        this.menuItemShowFormat = new ToolStripMenuItem();
        this.menuItemShowReadOnly = new ToolStripMenuItem();
        this.menuItemSeparator3 = new ToolStripSeparator();
        this.menuItemResetColumns = new ToolStripMenuItem();
        
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
        
        // 默认列将在运行时动态创建
        
        // 
        // contextMenuStrip
        // 
        this.contextMenuStrip.Items.AddRange(new ToolStripItem[] {
            this.menuItemRefresh,
            this.menuItemSeparator1,
            this.menuItemEdit,
            this.menuItemSeparator2,
            this.menuItemColumns
        });
        this.contextMenuStrip.Name = "contextMenuStrip";
        
        // 
        // menuItemRefresh
        // 
        this.menuItemRefresh.Name = "menuItemRefresh";
        this.menuItemRefresh.Text = "立即刷新(&R)";
        this.menuItemRefresh.Click += new System.EventHandler(this.MenuItemRefresh_Click);
        
        // 
        // menuItemSeparator1
        // 
        this.menuItemSeparator1.Name = "menuItemSeparator1";
        
        // 
        // menuItemEdit
        // 
        this.menuItemEdit.Name = "menuItemEdit";
        this.menuItemEdit.Text = "编辑信号值(&E)";
        this.menuItemEdit.Click += new System.EventHandler(this.MenuItemEdit_Click);
        
        // 
        // menuItemSeparator2
        // 
        this.menuItemSeparator2.Name = "menuItemSeparator2";
        
        // 
        // menuItemColumns
        // 
        this.menuItemColumns.DropDownItems.AddRange(new ToolStripItem[] {
            this.menuItemShowDescription,
            this.menuItemShowUnit,
            this.menuItemShowFormat,
            this.menuItemShowReadOnly,
            this.menuItemSeparator3,
            this.menuItemResetColumns
        });
        this.menuItemColumns.Name = "menuItemColumns";
        this.menuItemColumns.Text = "显示列(&C)";
        
        // 
        // menuItemShowDescription
        // 
        this.menuItemShowDescription.CheckOnClick = true;
        this.menuItemShowDescription.Name = "menuItemShowDescription";
        this.menuItemShowDescription.Text = "描述";
        this.menuItemShowDescription.Click += new System.EventHandler(this.MenuItemShowDescription_Click);
        
        // 
        // menuItemShowUnit
        // 
        this.menuItemShowUnit.CheckOnClick = true;
        this.menuItemShowUnit.Name = "menuItemShowUnit";
        this.menuItemShowUnit.Text = "单位";
        this.menuItemShowUnit.Click += new System.EventHandler(this.MenuItemShowUnit_Click);
        
        // 
        // menuItemShowFormat
        // 
        this.menuItemShowFormat.CheckOnClick = true;
        this.menuItemShowFormat.Name = "menuItemShowFormat";
        this.menuItemShowFormat.Text = "格式";
        this.menuItemShowFormat.Click += new System.EventHandler(this.MenuItemShowFormat_Click);
        
        // 
        // menuItemShowReadOnly
        // 
        this.menuItemShowReadOnly.CheckOnClick = true;
        this.menuItemShowReadOnly.Name = "menuItemShowReadOnly";
        this.menuItemShowReadOnly.Text = "读写状态";
        this.menuItemShowReadOnly.Click += new System.EventHandler(this.MenuItemShowReadOnly_Click);
        
        // 
        // menuItemSeparator3
        // 
        this.menuItemSeparator3.Name = "menuItemSeparator3";
        
        // 
        // menuItemResetColumns
        // 
        this.menuItemResetColumns.Name = "menuItemResetColumns";
        this.menuItemResetColumns.Text = "恢复默认列";
        this.menuItemResetColumns.Click += new System.EventHandler(this.MenuItemResetColumns_Click);
        
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
    private ToolStripSeparator menuItemSeparator1;
    private ToolStripMenuItem menuItemEdit;
    private ToolStripSeparator menuItemSeparator2;
    private ToolStripMenuItem menuItemColumns;
    private ToolStripMenuItem menuItemShowDescription;
    private ToolStripMenuItem menuItemShowUnit;
    private ToolStripMenuItem menuItemShowFormat;
    private ToolStripMenuItem menuItemShowReadOnly;
    private ToolStripSeparator menuItemSeparator3;
    private ToolStripMenuItem menuItemResetColumns;
}