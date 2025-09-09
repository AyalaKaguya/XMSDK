using System;
using System.Windows.Forms;

namespace XMSDK.Framework.Logger.TraceMessage;

public class LogToListBoxProcesser(ListBox listBox) : ITraceMessageProcesser
{
    public void OnMessage(string msg)
    {
        var fullMessage = $"{DateTime.Now:HH:mm:ss}-->  {msg}";

        UpdateListBox(fullMessage);
    }

    private void UpdateListBox(string message)
    {
        // 检查控件是否已释放
        if (listBox.IsDisposed)
            return;

        listBox.BeginInvoke(new Action(() =>
        {
            listBox.Items.Add(message);

            // 限制listBox项目数量，避免内存溢出
            if (listBox.Items.Count > 1000)
            {
                // 批量删除，提高性能
                for (var i = 0; i < 100; i++)
                {
                    if (listBox.Items.Count > 0)
                        listBox.Items.RemoveAt(0);
                }
            }

            // 安全设置选中项
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = listBox.Items.Count - 1;
            }
        }));
    }
}