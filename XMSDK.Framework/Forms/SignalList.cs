using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using XMSDK.Framework.Communication.Signal;

namespace XMSDK.Framework.Forms;

/// <summary>
/// 可观测信号列表控件，用于显示和管理信号集合。
/// 支持完整的ObservableSignal特性描述展示和可配置的列显示选项。
/// 所有刷新都由SignalSource的事件触发，不提供自动刷新功能。
/// </summary>
public partial class SignalList : UserControl
{
    private object? _signalSource;
    private List<SignalInfo> _signals = new();
    private IVariableSignalNotifier? _notifier;

    /// <summary>
    /// 信号值变化时触发的事件
    /// </summary>
    public event Action<string, object?, object?>? SignalValueChanged;

    /// <summary>
    /// 可配置的显示列选项
    /// </summary>
    [Flags]
    public enum DisplayColumns
    {
        None = 0,
        Name = 1 << 0,
        Address = 1 << 1,
        Value = 1 << 2,
        UpdateTime = 1 << 3,
        Type = 1 << 4,
        Group = 1 << 5,
        Description = 1 << 6,
        Unit = 1 << 7,
        Format = 1 << 8,
        ReadOnlyStatus = 1 << 9,
        Default = Name | Address | Value | UpdateTime | Type | Group
    }

    private DisplayColumns _visibleColumns = DisplayColumns.Default;

    /// <summary>
    /// 获取或设置要显示的列
    /// </summary>
    public DisplayColumns VisibleColumns
    {
        get => _visibleColumns;
        set
        {
            _visibleColumns = value;
            UpdateColumnVisibility();
            UpdateContextMenuState();
        }
    }

    public SignalList()
    {
        InitializeComponent();
        SetupListView();
    }

    /// <summary>
    /// 设置信号源对象
    /// </summary>
    /// <param name="signalSource">包含可观测信号的对象</param>
    public void SetSignalSource(object? signalSource)
    {
        // 取消之前的事件订阅
        if (_notifier != null)
        {
            _notifier.SignalValueChanged -= OnSignalValueChanged;
        }

        _signalSource = signalSource;
        _signals.Clear();

        if (signalSource != null)
        {
            // 提取信号信息
            _signals = SignalReflectionHelper.ExtractSignals(signalSource);

            // 检查是否实现了通知接口
            if (signalSource is IVariableSignalNotifier notifier)
            {
                _notifier = notifier;
                _notifier.SignalValueChanged += OnSignalValueChanged;
            }
            else
            {
                _notifier = null;
            }
        }

        RefreshSignalList();
    }

    /// <summary>
    /// 手动刷新信号值（仅用于右键菜单的"立即刷新"功能）
    /// </summary>
    public void RefreshSignalValues()
    {
        if (_signalSource != null)
        {
            SignalReflectionHelper.RefreshSignalValues(_signalSource, _signals);
            RefreshSignalList();
        }
    }
    private void UpdateColumnVisibility()
    {
        listViewSignals.BeginUpdate();
        try
        {
            listViewSignals.Columns.Clear();

            void AddColumn(string text, int width, DisplayColumns columnFlag)
            {
                if ((_visibleColumns & columnFlag) != 0)
                {
                    var column = new ColumnHeader
                    {
                        Text = text,
                        Width = width,
                        TextAlign = HorizontalAlignment.Left
                    };
                    listViewSignals.Columns.Add(column);
                }
            }

            AddColumn("名称", 120, DisplayColumns.Name);
            AddColumn("地址", 100, DisplayColumns.Address);
            AddColumn("值", 100, DisplayColumns.Value);
            AddColumn("更新时间", 100, DisplayColumns.UpdateTime);
            AddColumn("类型", 80, DisplayColumns.Type);
            AddColumn("分组", 100, DisplayColumns.Group);
            AddColumn("描述", 150, DisplayColumns.Description);
            AddColumn("单位", 80, DisplayColumns.Unit);
            AddColumn("格式", 80, DisplayColumns.Format);
            AddColumn("只读", 60, DisplayColumns.ReadOnlyStatus);

            RefreshSignalList();
        }
        finally
        {
            listViewSignals.EndUpdate();
        }
    }
    
    private void SetupListView()
    {
        // ListView的基本设置已在Designer中完成
        // 这里只需要确保一些运行时设置
        listViewSignals.UseCompatibleStateImageBehavior = false;
        listViewSignals.View = View.Details;
        listViewSignals.FullRowSelect = true;
        listViewSignals.GridLines = true;
        listViewSignals.HideSelection = false;
        listViewSignals.MultiSelect = false;
    }

    private void RefreshSignalList()
    {
        listViewSignals.BeginUpdate();
        try
        {
            listViewSignals.Items.Clear();

            foreach (var signal in _signals.OrderBy(s => s.Group).ThenBy(s => s.Name))
            {
                var item = new ListViewItem(string.IsNullOrEmpty(signal.Name) ? signal.Address : signal.Name);
                
                item.SubItems.Add(signal.Address);
                item.SubItems.Add(FormatSignalValue(signal));
                item.SubItems.Add(signal.LastUpdated.ToString("HH:mm:ss"));
                item.SubItems.Add(GetTypeDisplayName(signal.SignalType));
                item.SubItems.Add(signal.Group);

                SetItemColors(item, signal);
                
                item.Tag = signal;
                listViewSignals.Items.Add(item);
            }

            // 自动调整列宽
            foreach (ColumnHeader column in listViewSignals.Columns)
            {
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }

        }
        finally
        {
            listViewSignals.EndUpdate();
        }
    }
    private void UpdateContextMenuState()
    {
        menuItemShowDescription.Checked = (_visibleColumns & DisplayColumns.Description) != 0;
        menuItemShowUnit.Checked = (_visibleColumns & DisplayColumns.Unit) != 0;
        menuItemShowFormat.Checked = (_visibleColumns & DisplayColumns.Format) != 0;
        menuItemShowReadOnly.Checked = (_visibleColumns & DisplayColumns.ReadOnlyStatus) != 0;
    }

    private string FormatSignalValue(SignalInfo signal)
    {
        var valueStr = SignalConverterManager.ConvertToString(signal.CurrentValue, signal.SignalType, signal.Format);
        
        if (!string.IsNullOrEmpty(signal.Unit))
        {
            valueStr += " " + signal.Unit;

        }

        return valueStr;
    }

    private void SetItemColors(ListViewItem item, SignalInfo signal)
    {
        if (signal.IsReadOnly)
        {
            item.ForeColor = Color.Gray;
        }
        else if (signal.SignalType == typeof(bool))
        {
            if (signal.CurrentValue is bool boolValue)
            {
                item.ForeColor = boolValue ? Color.Green : Color.Red;
            }
        }
        else
        {
            item.ForeColor = SystemColors.WindowText;
        }

        var timeSinceUpdate = DateTime.Now - signal.LastUpdated;
        if (timeSinceUpdate.TotalSeconds > 30)
        {
            item.BackColor = Color.LightYellow;
        }
        else if (timeSinceUpdate.TotalSeconds < 2)
        {
            item.BackColor = Color.LightGreen;
        }
        else
        {
            item.BackColor = SystemColors.Window;
        }
    }

    private string GetTypeDisplayName(Type type)
    {
        return type switch
        {
            var t when t == typeof(bool) => "布尔",
            var t when t == typeof(int) => "整数",
            var t when t == typeof(long) => "长整数",
            var t when t == typeof(float) => "单精度",
            var t when t == typeof(double) => "双精度",
            var t when t == typeof(byte) => "字节",
            var t when t == typeof(short) => "短整数",
            _ => type.Name
        };
    }

    private void OnSignalValueChanged(string address, object? oldValue, object? newValue, DateTime timestamp)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => OnSignalValueChanged(address, oldValue, newValue, timestamp));
            return;
        }

        var signal = _signals.FirstOrDefault(s => s.Address == address);
        if (signal != null)
        {
            signal.CurrentValue = newValue;
            signal.LastUpdated = timestamp;

            // 更新对应的ListView项
            foreach (ListViewItem item in listViewSignals.Items)
            {
                if (item.Tag == signal)
                {
                    item.SubItems[2].Text = FormatSignalValue(signal);
                    item.SubItems[3].Text = timestamp.ToString("HH:mm:ss");
                    SetItemColors(item, signal);
                    break;
                }
            }
        }

        SignalValueChanged?.Invoke(address, oldValue, newValue);
    }

    #region 事件处理

    private void MenuItemRefresh_Click(object? sender, EventArgs e)
    {
        RefreshSignalValues();
    }

    private void MenuItemEdit_Click(object? sender, EventArgs e)
    {
        if (listViewSignals.SelectedItems.Count == 1)
        {
            var item = listViewSignals.SelectedItems[0];
            var signal = (SignalInfo)item.Tag;
            
            if (!signal.IsReadOnly)
            {
                new SignalEditDialog(signal).Show();
            }
            else
            {
                MessageBox.Show("此信号为只读，无法编辑。", "信息", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void ListViewSignals_DoubleClick(object? sender, EventArgs e)
    {
        MenuItemEdit_Click(sender, e);
    }

    #endregion
}