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
        private readonly List<LogEntry> _pendingLogEntries = new List<LogEntry>();
        private readonly object _pendingLockObject = new object();
        private Timer _updateTimer;
        private int _maxLogCount = 300;
        private bool _autoScroll = true; // 默认开启
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
            InitializeTimer();
        }

        public int MaxLogCount
        {
            get => _maxLogCount;
            set
            {
                _maxLogCount = value;
                TrimLogEntries();
            }
        }

        public new bool AutoScroll
        {
            get => _autoScroll;
            set
            {
                _autoScroll = value;
                autoScrollMenuItem.Checked = value;
            }
        }

        // 现在的数量即为可见数量
        public int LogCount => listViewLogs.Items.Count;

        public void SetLogLevelColor(LogLevel logLevel, Color color)
        {
            if (!_logLevelColors.ContainsKey(logLevel))
                _logLevelColors[logLevel] = new LogLevelColorConfig();
            _logLevelColors[logLevel].ForeColor = color;
            ApplyColorToExistingItems(logLevel);
        }

        public void SetLogLevelColor(LogLevel logLevel, Color foreColor, Color backColor)
        {
            if (!_logLevelColors.ContainsKey(logLevel))
                _logLevelColors[logLevel] = new LogLevelColorConfig();
            _logLevelColors[logLevel].ForeColor = foreColor;
            _logLevelColors[logLevel].BackColor = backColor;
            ApplyColorToExistingItems(logLevel);
        }

        public LogLevelColorConfig GetLogLevelColor(LogLevel logLevel)
        {
            return _logLevelColors.TryGetValue(logLevel, out var config)
                ? config
                : new LogLevelColorConfig { ForeColor = Color.Black, BackColor = Color.Transparent };
        }

        private void InitializeListView()
        {
            listViewLogs.View = View.Details;
            listViewLogs.FullRowSelect = true;
            listViewLogs.GridLines = true;
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
            typeof(ListView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, listViewLogs, new object[] { true });
        }

        private void InitializeTimer()
        {
            _updateTimer = new Timer
            {
                Interval = 100,
                Enabled = true
            };
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            List<LogEntry> toProcess = null;
            lock (_pendingLockObject)
            {
                if (_pendingLogEntries.Count > 0)
                {
                    toProcess = new List<LogEntry>(_pendingLogEntries);
                    _pendingLogEntries.Clear();
                }
            }
            if (toProcess == null || toProcess.Count == 0) return;

            listViewLogs.BeginUpdate();
            try
            {
                foreach (var entry in toProcess)
                {
                    if (!IsLevelEnabled(entry.LogLevel))
                        continue; // 被过滤: 直接丢弃
                    var item = CreateListViewItem(entry);
                    listViewLogs.Items.Add(item);
                }
                TrimExcessFromHead();
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

        private void TrimExcessFromHead()
        {
            if (_maxLogCount <= 0) return;
            var remove = listViewLogs.Items.Count - _maxLogCount;
            if (remove <= 0) return;
            for (int i = 0; i < remove; i++)
            {
                listViewLogs.Items.RemoveAt(0);
            }
        }

        public void AddLogEntry(DateTime timestamp, LogLevel logLevel, string message, string source)
        {
            var logEntry = new LogEntry
            {
                Timestamp = timestamp,
                LogLevel = logLevel,
                Message = message,
                Source = source
            };
            lock (_pendingLockObject)
            {
                _pendingLogEntries.Add(logEntry);
            }
        }

        private void TrimLogEntries()
        {
            listViewLogs.BeginUpdate();
            try
            {
                TrimExcessFromHead();
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
            })
            {
                Tag = logEntry
            };
            if (_logLevelColors.TryGetValue(logEntry.LogLevel, out var colorConfig))
            {
                item.ForeColor = colorConfig.ForeColor;
                if (colorConfig.BackColor != Color.Transparent)
                {
                    item.BackColor = colorConfig.BackColor;
                }
            }
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

        private bool IsLevelEnabled(LogLevel logLevel)
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

        private void ApplyColorToExistingItems(LogLevel level)
        {
            if (!_logLevelColors.TryGetValue(level, out var cfg)) return;
            listViewLogs.BeginUpdate();
            try
            {
                foreach (ListViewItem item in listViewLogs.Items)
                {
                    if (item.Tag is LogEntry le && le.LogLevel == level)
                    {
                        item.ForeColor = cfg.ForeColor;
                        item.BackColor = cfg.BackColor == Color.Transparent ? listViewLogs.BackColor : cfg.BackColor;
                    }
                }
            }
            finally
            {
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
                if (item.Tag is LogEntry logEntry)
                {
                    sb.AppendLine($"{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{GetLogLevelDisplayName(logEntry.LogLevel)}] {logEntry.Message} - 来源: {logEntry.Source}");
                }
            }
            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("复制失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void AutoScrollMenuItem_Click(object sender, EventArgs e)
        {
            _autoScroll = !_autoScroll;
            autoScrollMenuItem.Checked = _autoScroll;
        }

        private void LogLevelMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            // 仍然只影响后续新增日志。
        }

        public void Clear()
        {
            lock (_pendingLockObject)
            {
                _pendingLogEntries.Clear();
            }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }

    public class LogLevelColorConfig
    {
        public Color ForeColor { get; set; } = Color.Black;
        public Color BackColor { get; set; } = Color.Transparent;
    }
}
