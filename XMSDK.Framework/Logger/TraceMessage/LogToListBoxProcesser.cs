using System;
using System.Windows.Forms;

namespace XMSDK.Framework.Logger.TraceMessage;

public class LogToListBoxProcesser : ITraceMessageProcesser
{
    private readonly ListBox _listBox;

    public LogToListBoxProcesser(ListBox listBox)
    {
        _listBox = listBox ?? throw new ArgumentNullException(nameof(listBox), "ListBox cannot be null");
    }

    public void OnMessage(string msg)
    {
        var fullMessage = $"{DateTime.Now:HH:mm:ss}-->  {msg}";

        UpdateListBox(fullMessage);
    }

    private void UpdateListBox(string message)
    {
        // 检查控件是否已释放
        if (_listBox.IsDisposed)
            return;

        _listBox.BeginInvoke(new Action(() =>
        {
            _listBox.Items.Add(message);

            // 限制listBox项目数量，避免内存溢出
            if (_listBox.Items.Count > 1000)
            {
                // 批量删除，提高性能
                for (var i = 0; i < 100; i++)
                {
                    if (_listBox.Items.Count > 0)
                        _listBox.Items.RemoveAt(0);
                }
            }

            // 安全设置选中项
            if (_listBox.Items.Count > 0)
            {
                _listBox.SelectedIndex = _listBox.Items.Count - 1;
            }
        }));
    }
}