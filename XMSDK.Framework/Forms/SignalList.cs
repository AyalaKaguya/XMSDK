using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using XMSDK.Framework.Communication.Signal;
using System.Reflection;

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
        EnableDoubleBuffering();
    }

    private void EnableDoubleBuffering()
    {
        // 为UserControl开启双缓冲
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        UpdateStyles();

        // 为ListView开启双缓冲（受保护属性，使用反射）
        try
        {
            var prop = typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(listViewSignals, true, null);
        }
        catch
        {
            // 忽略反射失败
        }
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
    /// 手动刷新信号值（仅用于右键菜单的"立即���新"功能）
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

    private void RefreshSignalList()
    {
        listViewSignals.BeginUpdate();
        try
        {
            listViewSignals.Items.Clear();

            foreach (var signal in _signals.OrderBy(s => s.Group).ThenBy(s => s.Name))
            {
                var item = new ListViewItem(string.IsNullOrEmpty(signal.Name) ? signal.Address : signal.Name);

                // 按照列的顺序添加子项，需要与UpdateColumnVisibility中的顺序保持一致
                if ((_visibleColumns & DisplayColumns.Address) != 0)
                    item.SubItems.Add(signal.Address);
                
                if ((_visibleColumns & DisplayColumns.Value) != 0)
                    item.SubItems.Add(FormatSignalValue(signal));
                
                if ((_visibleColumns & DisplayColumns.UpdateTime) != 0)
                    item.SubItems.Add(signal.LastUpdated.ToString("HH:mm:ss"));
                
                if ((_visibleColumns & DisplayColumns.Type) != 0)
                    item.SubItems.Add(GetTypeDisplayName(signal.SignalType));
                
                if ((_visibleColumns & DisplayColumns.Group) != 0)
                    item.SubItems.Add(signal.Group);
                
                if ((_visibleColumns & DisplayColumns.Description) != 0)
                    item.SubItems.Add(signal.Description);
                
                if ((_visibleColumns & DisplayColumns.Unit) != 0)
                    item.SubItems.Add(signal.Unit);
                
                if ((_visibleColumns & DisplayColumns.Format) != 0)
                    item.SubItems.Add(signal.Format);
                
                if ((_visibleColumns & DisplayColumns.ReadOnlyStatus) != 0)
                    item.SubItems.Add(signal.IsReadOnly ? "只读" : "读/写");

                SetItemColors(item, signal);

                item.Tag = signal;
                listViewSignals.Items.Add(item);
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
            _ when type == typeof(bool) => "布尔",
            _ when type == typeof(int) => "整数",
            _ when type == typeof(long) => "长整数",
            _ when type == typeof(float) => "单精度",
            _ when type == typeof(double) => "双精度",
            _ when type == typeof(byte) => "字节",
            _ when type == typeof(short) => "短整数",
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
                    // 动态计算列索引
                    var columnIndex = 0;
                    
                    // 名称列总是存在（作为主列），跳过
                    if ((_visibleColumns & DisplayColumns.Name) != 0) columnIndex++;
                    
                    // 检查地址列
                    if ((_visibleColumns & DisplayColumns.Address) != 0)
                    {
                        if (columnIndex < item.SubItems.Count)
                            item.SubItems[columnIndex].Text = signal.Address;
                        columnIndex++;
                    }
                    
                    // 检查值列
                    if ((_visibleColumns & DisplayColumns.Value) != 0)
                    {
                        if (columnIndex < item.SubItems.Count)
                            item.SubItems[columnIndex].Text = FormatSignalValue(signal);
                        columnIndex++;
                    }
                    
                    // 检查更新时间列
                    if ((_visibleColumns & DisplayColumns.UpdateTime) != 0)
                    {
                        if (columnIndex < item.SubItems.Count)
                            item.SubItems[columnIndex].Text = timestamp.ToString("HH:mm:ss");
                        columnIndex++;
                    }
                    
                    SetItemColors(item, signal);
                    break;
                }
            }
            
            // 当有信号值变化时，更新所有项目的背景色（基于timeSinceUpdate逻辑）
            UpdateAllItemsBackgroundColors();
        }
    }

    private void UpdateAllItemsBackgroundColors()
    {
        foreach (ListViewItem item in listViewSignals.Items)
        {
            if (item.Tag is SignalInfo signal)
            {
                var timeSinceUpdate = DateTime.Now - signal.LastUpdated;
                if (timeSinceUpdate.TotalSeconds > 30)
                {
                    item.BackColor = SystemColors.Window;
                }
                else if (timeSinceUpdate.TotalSeconds < 2)
                {
                    item.BackColor = Color.LightGreen;
                }
                else
                {
                    item.BackColor = Color.LightYellow;
                }
            }
        }
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
                using var dlg = new SignalEditDialog(signal);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var updated = SignalReflectionHelper.UpdateSignalValue(_signalSource, signal.Address, dlg.NewValue);
                    if (updated)
                    {
                        signal.CurrentValue = dlg.NewValue;
                        signal.LastUpdated = DateTime.Now;

                        // 更新该项显示
                        var columnIndex = 0;
                        if ((_visibleColumns & DisplayColumns.Name) != 0) columnIndex++;
                        if ((_visibleColumns & DisplayColumns.Address) != 0) columnIndex++;
                        if ((_visibleColumns & DisplayColumns.Value) != 0)
                        {
                            if (columnIndex < item.SubItems.Count)
                                item.SubItems[columnIndex].Text = FormatSignalValue(signal);
                            columnIndex++;
                        }
                        if ((_visibleColumns & DisplayColumns.UpdateTime) != 0)
                        {
                            if (columnIndex < item.SubItems.Count)
                                item.SubItems[columnIndex].Text = signal.LastUpdated.ToString("HH:mm:ss");
                        }

                        SetItemColors(item, signal);
                        UpdateAllItemsBackgroundColors();
                    }
                    else
                    {
                        MessageBox.Show("写入失败：值或类型不匹配，或信号不可写。", "写入失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
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


    // 列显示选项事件处理
    private void MenuItemShowDescription_Click(object? sender, EventArgs e)
    {
        ToggleColumnVisibility(DisplayColumns.Description);
    }

    private void MenuItemShowUnit_Click(object? sender, EventArgs e)
    {
        ToggleColumnVisibility(DisplayColumns.Unit);
    }


    private void MenuItemShowFormat_Click(object? sender, EventArgs e)
    {
        ToggleColumnVisibility(DisplayColumns.Format);
    }

    private void MenuItemShowReadOnly_Click(object? sender, EventArgs e)
    {
        ToggleColumnVisibility(DisplayColumns.ReadOnlyStatus);
    }

    private void MenuItemResetColumns_Click(object? sender, EventArgs e)
    {
        VisibleColumns = DisplayColumns.Default;
    }

    private void ToggleColumnVisibility(DisplayColumns column)
    {
        if ((_visibleColumns & column) != 0)
        {
            _visibleColumns &= ~column; // 移除该列
        }
        else
        {
            _visibleColumns |= column; // 添加该列
        }

        UpdateColumnVisibility();
        UpdateContextMenuState();
    }

    #endregion
}
