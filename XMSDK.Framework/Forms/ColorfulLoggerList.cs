using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace XMSDK.Framework.Forms
{
    public partial class ColorfulLoggerList : UserControl
    {
        private readonly object _lockObject = new object();
        private readonly List<LogEntry> _allLogEntries = new List<LogEntry>();
        private int _maxLogCount = 1000;
        private bool _autoScroll = true; // 自动滚动开关，默认开启
        
        // 性能优化相关字段
        private readonly Dictionary<LogLevel, bool> _lastVisibleLevels = new Dictionary<LogLevel, bool>();
        private bool _isRefreshing;
        
        // 可配置的颜色设置 - 支持前景色和背景色
        private readonly Dictionary<LogLevel, LogLevelColorConfig> _logLevelColors = new Dictionary<LogLevel, LogLevelColorConfig>
        {
            { LogLevel.Information, new LogLevelColorConfig { ForeColor = Color.Green, BackColor = Color.Transparent } },
            { LogLevel.Warning, new LogLevelColorConfig { ForeColor = Color.Orange, BackColor = Color.Transparent } },
            { LogLevel.Error, new LogLevelColorConfig { ForeColor = Color.Red, BackColor = Color.Transparent } }
        };

        public ColorfulLoggerList()
        {
            InitializeComponent();
            InitializeListView();
            InitializeEventHandlers();
            InitializePerformanceSettings();
        }

        /// <summary>
        /// 最大日志条数，默认1000
        /// </summary>
        public int MaxLogCount
        {
            get => _maxLogCount;
            set
            {
                _maxLogCount = value;
                TrimLogEntries();
            }
        }

        /// <summary>
        /// 自动滚动开关，默认开启
        /// </summary>
        public new bool AutoScroll
        {
            get => _autoScroll;
            set
            {
                _autoScroll = value;
                autoScrollMenuItem.Checked = value;
            }
        }

        /// <summary>
        /// 配置日志级别颜色（仅前景色）
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <param name="color">前景色</param>
        public void SetLogLevelColor(LogLevel logLevel, Color color)
        {
            if (!_logLevelColors.ContainsKey(logLevel))
                _logLevelColors[logLevel] = new LogLevelColorConfig();
            
            _logLevelColors[logLevel].ForeColor = color;
            RefreshDisplay();
        }

        /// <summary>
        /// 配置日志级别颜色（前景色和背景色）
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <param name="foreColor">前景色</param>
        /// <param name="backColor">背景色</param>
        public void SetLogLevelColor(LogLevel logLevel, Color foreColor, Color backColor)
        {
            if (!_logLevelColors.ContainsKey(logLevel))
                _logLevelColors[logLevel] = new LogLevelColorConfig();
            
            _logLevelColors[logLevel].ForeColor = foreColor;
            _logLevelColors[logLevel].BackColor = backColor;
            RefreshDisplay();
        }

        /// <summary>
        /// 获取日志级别的颜色配置
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <returns>颜色配置</returns>
        public LogLevelColorConfig GetLogLevelColor(LogLevel logLevel)
        {
            return _logLevelColors.TryGetValue(logLevel, out var config) 
                ? config 
                : new LogLevelColorConfig { ForeColor = Color.Black, BackColor = Color.Transparent };
        }

        private void InitializeListView()
        {
            listViewLogs.Columns.Add("时间", 150);
            listViewLogs.Columns.Add("重要性", 80);
            listViewLogs.Columns.Add("信息", 600);
            listViewLogs.Columns.Add("来源", 400);
            
            listViewLogs.MultiSelect = true;
        }

        private void InitializeEventHandlers()
        {
            copyMenuItem.Click += CopyMenuItem_Click;
            clearMenuItem.Click += ClearMenuItem_Click;
            autoScrollMenuItem.Click += AutoScrollMenuItem_Click;
            
            traceMenuItem.CheckedChanged += LogLevelMenuItem_CheckedChanged;
            debugMenuItem.CheckedChanged += LogLevelMenuItem_CheckedChanged;
            infoMenuItem.CheckedChanged += LogLevelMenuItem_CheckedChanged;
            warningMenuItem.CheckedChanged += LogLevelMenuItem_CheckedChanged;
            errorMenuItem.CheckedChanged += LogLevelMenuItem_CheckedChanged;
            criticalMenuItem.CheckedChanged += LogLevelMenuItem_CheckedChanged;
        }

        private void InitializePerformanceSettings()
        {
            // 启用双缓冲以减少闪烁
            typeof(ListView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, listViewLogs, new object[] { true });
            
            // 初始化日志级别过滤状态
            _lastVisibleLevels[LogLevel.Trace] = true;
            _lastVisibleLevels[LogLevel.Debug] = true;
            _lastVisibleLevels[LogLevel.Information] = true;
            _lastVisibleLevels[LogLevel.Warning] = true;
            _lastVisibleLevels[LogLevel.Error] = true;
            _lastVisibleLevels[LogLevel.Critical] = true;
        }

        /// <summary>
        /// 添加日志条目 - 性能优化版本
        /// </summary>
        public void AddLogEntry(DateTime timestamp, LogLevel logLevel, string message, string source)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddLogEntry(timestamp, logLevel, message, source)));
                return;
            }

            lock (_lockObject)
            {
                var logEntry = new LogEntry
                {
                    Timestamp = timestamp,
                    LogLevel = logLevel,
                    Message = message,
                    Source = source
                };

                _allLogEntries.Add(logEntry);
                
                // 检查是否需要清理旧条目
                if (_allLogEntries.Count > _maxLogCount)
                {
                    TrimLogEntriesOptimized(logEntry);
                }
                else if (ShouldDisplayLogLevel(logLevel))
                {
                    // 直接添加单个条目，避免全量刷新
                    AddListViewItem(logEntry);
                }
            }
        }

        private void TrimLogEntries()
        {
            lock (_lockObject)
            {
                if (_allLogEntries.Count <= _maxLogCount) return;
                var itemsToRemove = _allLogEntries.Count - _maxLogCount;
                // 删除最老的条目（从索引0开始），保留最新的1000条
                _allLogEntries.RemoveRange(0, itemsToRemove);
            }
            RefreshDisplay();
        }

        private void TrimLogEntriesOptimized(LogEntry newLogEntry)
        {
            // 删除最老的条目，保留最新的1000条
            var itemsToRemove = _allLogEntries.Count - _maxLogCount;
            _allLogEntries.RemoveRange(0, itemsToRemove);

            // 优化：只处理新添加的这一条日志，不重建整个ListView
            if (ShouldDisplayLogLevel(newLogEntry.LogLevel))
            {
                // 移除ListView中对应数量的最老条目
                RemoveOldestListViewItems(itemsToRemove);
                
                // 添加新的日志条目
                AddListViewItem(newLogEntry);
            }
        }

        private void RemoveOldestListViewItems(int itemsToRemove)
        {
            if (listViewLogs.Items.Count == 0) return;
            
            listViewLogs.BeginUpdate();
            try
            {
                // 计算实际需要删除的ListView项目数量
                var actualItemsToRemove = Math.Min(itemsToRemove, listViewLogs.Items.Count);
                
                // 从前面开始删除最老的条目
                for (int i = 0; i < actualItemsToRemove; i++)
                {
                    listViewLogs.Items.RemoveAt(0);
                }
            }
            finally
            {
                listViewLogs.EndUpdate();
            }
        }

        private void AddListViewItem(LogEntry logEntry)
        {
            // 暂停绘制以提高性能
            listViewLogs.BeginUpdate();
            try
            {
                var item = CreateListViewItem(logEntry);
                listViewLogs.Items.Add(item);
                
                // 自动滚动到最新条目（仅当自动滚动开启时）
                if (_autoScroll && listViewLogs.Items.Count > 0)
                {
                    listViewLogs.EnsureVisible(listViewLogs.Items.Count - 1);
                }
            }
            finally
            {
                listViewLogs.EndUpdate();
            }
        }

        private ListViewItem CreateListViewItem(LogEntry logEntry)
        {
            var item = new ListViewItem(new[]
            {
                logEntry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                GetLogLevelDisplayName(logEntry.LogLevel),
                logEntry.Message,
                logEntry.Source
            });

            // 应用颜色配置
            if (_logLevelColors.TryGetValue(logEntry.LogLevel, out var colorConfig))
            {
                item.ForeColor = colorConfig.ForeColor;
                if (colorConfig.BackColor != Color.Transparent)
                {
                    item.BackColor = colorConfig.BackColor;
                }
            }

            item.Tag = logEntry;
            return item;
        }

        private static string GetLogLevelDisplayName(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "跟踪",
                LogLevel.Debug => "调试",
                LogLevel.Information => "信息",
                LogLevel.Warning => "警告",
                LogLevel.Error => "错误",
                LogLevel.Critical => "严重",
                _ => logLevel.ToString()
            };
        }

        private bool ShouldDisplayLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => traceMenuItem.Checked,
                LogLevel.Debug => debugMenuItem.Checked,
                LogLevel.Information => infoMenuItem.Checked,
                LogLevel.Warning => warningMenuItem.Checked,
                LogLevel.Error => errorMenuItem.Checked,
                LogLevel.Critical => criticalMenuItem.Checked,
                _ => true
            };
        }

        private void RefreshDisplay()
        {
            if (_isRefreshing) return; // 防止重入
            
            _isRefreshing = true;
            
            try
            {
                // 准备可见的日志条目数据
                var visibleEntries = new List<LogEntry>();
                
                lock (_lockObject)
                {
                    foreach (var logEntry in _allLogEntries)
                    {
                        if (ShouldDisplayLogLevel(logEntry.LogLevel))
                        {
                            visibleEntries.Add(logEntry);
                        }
                    }
                }
                
                // 在UI线程中快速更新ListView
                UpdateListViewItems(visibleEntries);
                
                // 更新过滤状态缓存
                UpdateLastVisibleLevels();
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private bool HasFilterChanged()
        {
            return _lastVisibleLevels[LogLevel.Trace] != traceMenuItem.Checked ||
                   _lastVisibleLevels[LogLevel.Debug] != debugMenuItem.Checked ||
                   _lastVisibleLevels[LogLevel.Information] != infoMenuItem.Checked ||
                   _lastVisibleLevels[LogLevel.Warning] != warningMenuItem.Checked ||
                   _lastVisibleLevels[LogLevel.Error] != errorMenuItem.Checked ||
                   _lastVisibleLevels[LogLevel.Critical] != criticalMenuItem.Checked;
        }

        private void UpdateLastVisibleLevels()
        {
            _lastVisibleLevels[LogLevel.Trace] = traceMenuItem.Checked;
            _lastVisibleLevels[LogLevel.Debug] = debugMenuItem.Checked;
            _lastVisibleLevels[LogLevel.Information] = infoMenuItem.Checked;
            _lastVisibleLevels[LogLevel.Warning] = warningMenuItem.Checked;
            _lastVisibleLevels[LogLevel.Error] = errorMenuItem.Checked;
            _lastVisibleLevels[LogLevel.Critical] = criticalMenuItem.Checked;
        }

        private void UpdateListViewItems(List<LogEntry> visibleEntries)
        {
            // 暂停绘制
            listViewLogs.BeginUpdate();
            
            try
            {
                // 清空现有项目
                listViewLogs.Items.Clear();
                
                // 批量添加项目
                var items = new ListViewItem[visibleEntries.Count];
                for (int i = 0; i < visibleEntries.Count; i++)
                {
                    items[i] = CreateListViewItem(visibleEntries[i]);
                }
                
                // 一次性添加所有项目
                listViewLogs.Items.AddRange(items);
                
                // 自动滚动到最新条目
                if (_autoScroll && listViewLogs.Items.Count > 0)
                {
                    listViewLogs.EnsureVisible(listViewLogs.Items.Count - 1);
                }
            }
            finally
            {
                // 恢复绘制
                listViewLogs.EndUpdate();
            }
        }

        private void CopyMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewLogs.SelectedItems.Count == 0)
                return;

            var sb = new StringBuilder();
            foreach (ListViewItem item in listViewLogs.SelectedItems)
            {
                var logEntry = (LogEntry)item.Tag;
                sb.AppendLine($"{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{GetLogLevelDisplayName(logEntry.LogLevel)}] {logEntry.Message} - 来源: {logEntry.Source}");
            }

            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearMenuItem_Click(object sender, EventArgs e)
        {
            lock (_lockObject)
            {
                _allLogEntries.Clear();
                listViewLogs.Items.Clear();
            }
        }

        private void AutoScrollMenuItem_Click(object sender, EventArgs e)
        {
            _autoScroll = !_autoScroll;
            autoScrollMenuItem.Checked = _autoScroll;
        }

        private void LogLevelMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            // 使用优化的刷新方法
            RefreshDisplay();
        }

        /// <summary>
        /// 清空所有日志 - 性能优化版本
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _allLogEntries.Clear();
                if (InvokeRequired)
                {
                    Invoke(new Action(() => {
                        listViewLogs.BeginUpdate();
                        try
                        {
                            listViewLogs.Items.Clear();
                        }
                        finally
                        {
                            listViewLogs.EndUpdate();
                        }
                    }));
                }
                else
                {
                    listViewLogs.BeginUpdate();
                    try
                    {
                        listViewLogs.Items.Clear();
                    }
                    finally
                    {
                        listViewLogs.EndUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前日志条数
        /// </summary>
        public int LogCount => _allLogEntries.Count;
    }

    /// <summary>
    /// 日志条目数据结构
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// 日志级别颜色配置
    /// </summary>
    public class LogLevelColorConfig
    {
        public Color ForeColor { get; set; } = Color.Black;
        public Color BackColor { get; set; } = Color.Transparent;
    }
}

