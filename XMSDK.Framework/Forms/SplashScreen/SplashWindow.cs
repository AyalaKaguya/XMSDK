using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace XMSDK.Framework.Forms.SplashScreen;

public sealed partial class SplashWindow : Form
{
    /// <summary>
    /// Splash 任务项状态
    /// </summary>
    internal class SplashProgressState
    {
        public int Index { get; set; }
        public int Total { get; set; }
        public string Description { get; set; }
        public int Percent { get; set; }
        public Exception Error { get; set; }
        public bool CompletedAll { get; set; }
        public bool Aborted { get; set; }
    }

    private readonly List<SplashItem> _items = new();
    private readonly BackgroundWorker _worker;
    private int _totalWeight;
    private int _doneWeight;
    private bool _started;
    private bool _aborted;
    private Timer _topMostTimer;
        
    private int _lastProgressIndex = -1;
    private readonly SplashContext _context; // 上下文供任务使用

    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppTitle
    {
        get => lblAppTitle.Text;
        set => lblAppTitle.Text = value ?? string.Empty;
    }

    /// <summary>
    /// 应用描述
    /// </summary>
    public string AppDescription
    {
        get => lblDescription.Text;
        set => lblDescription.Text = value ?? string.Empty;
    }

    /// <summary>
    /// 作者信息
    /// </summary>
    public string Author
    {
        get => lblAuthor.Text;
        set
        {
            lblAuthor.Text = value;
            lblAuthor.Visible = !string.IsNullOrWhiteSpace(value);
        }
    }

    /// <summary>
    /// 版权信息
    /// </summary>
    public string Copyright
    {
        get => lblCopyright.Text;
        set
        {
            lblCopyright.Text = value;
            lblCopyright.Visible = !string.IsNullOrWhiteSpace(value);
        }
    }

    /// <summary>
    /// 任务执行异常回调: (item, ex) => bool  返回true表示继续后续任务，false则中断
    /// </summary>
    public Func<SplashItem, Exception, bool> OnItemError { get; set; }

    /// <summary>
    /// 致命错误回调
    /// </summary>
    public Action<Exception> OnFatalError { get; set; }

    /// <summary>
    /// 所有任务完成回调
    /// </summary>
    public Action OnAllCompleted { get; set; }

    /// <summary>
    /// 是否在全部完成后自动关闭窗口
    /// </summary>
    public bool AutoClose { get; set; } = true;

    /// <summary>
    /// 错误中止时是否也自动关闭
    /// </summary>
    private bool AutoCloseOnAbort { get; set; } = false;

    /// <summary>
    /// 关闭前延迟毫秒
    /// </summary>
    public int CloseDelayMs { get; set; } = 1000;

    /// <summary>
    /// 取消令牌源
    /// </summary>
    private CancellationTokenSource CancelSource { get; } = new();

    /// <summary>
    /// 是否强制保持窗口置顶
    /// </summary>
    public bool ForceAlwaysOnTop { get; set; } = true;

    /// <summary>
    /// 背景图片
    /// </summary>
    public Image SplashBackgroundImage
    {
        get => BackgroundImage;
        set
        {
            BackgroundImage = value;
            BackgroundImageLayout = value != null ? ImageLayout.Stretch : ImageLayout.None;
        }
    }

    /// <summary>
    /// 背景图片布局方式
    /// </summary>
    public ImageLayout SplashBackgroundImageLayout
    {
        get => BackgroundImageLayout;
        set => BackgroundImageLayout = value;
    }

    public SplashWindow()
    {
        InitializeComponent();
            
        // 初始化窗口属性
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = true;
        DoubleBuffered = true;
        TopMost = true;

        // 初始化BackgroundWorker
        _worker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        _worker.DoWork += Worker_DoWork;
        _worker.ProgressChanged += Worker_ProgressChanged;
        _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

        // 保持置顶的定时器
        _topMostTimer = new Timer { Interval = 1500 };
        _topMostTimer.Tick += (_, _) =>
        {
            if (!ForceAlwaysOnTop) return;
            if (!TopMost) TopMost = true;
            if (Visible) BringToFront();
        };
        _topMostTimer.Start();

        // 初始化上下文（标签现在由设计器创建）
        _context = new SplashContext(SetDetailInternal);
    }

    /// <summary>
    /// 设置背景图片
    /// </summary>
    /// <param name="img">背景图片</param>
    /// <param name="layout">图片布局方式，默认为拉伸</param>
    public void SetBackgroundImage(Image img, ImageLayout layout = ImageLayout.Stretch)
    {
        BackgroundImage = img;
        BackgroundImageLayout = layout;
    }

    /// <summary>
    /// 从文件设置背景图片
    /// </summary>
    /// <param name="imagePath">图片文件路径</param>
    /// <param name="layout">图片布局方式，默认为拉伸</param>
    public void SetBackgroundImageFromFile(string imagePath, ImageLayout layout = ImageLayout.Stretch)
    {
        if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
        {
            BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.None;
            return;
        }

        try
        {
            BackgroundImage = Image.FromFile(imagePath);
            BackgroundImageLayout = layout;
        }
        catch (Exception ex)
        {
            // 图片加载失败时的处理
            Debug.WriteLine($"Failed to load background image: {ex.Message}");
            BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.None;
        }
    }

    /// <summary>
    /// 清除背景图片
    /// </summary>
    public void ClearBackgroundImage()
    {
        BackgroundImage?.Dispose();
        BackgroundImage = null;
        BackgroundImageLayout = ImageLayout.None;
    }

    /// <summary>
    /// 添加启动项
    /// </summary>
    public void AddItem(SplashItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (_started) throw new InvalidOperationException("加载已开始，无法再添加");
        _items.Add(item);
        _totalWeight += item.Weight;
    }

    /// <summary>
    /// 开始加载
    /// </summary>
    private void StartLoading()
    {
        if (_started) return;
        _started = true;
        _worker.RunWorkerAsync(_items);
    }

    private void Worker_DoWork(object sender, DoWorkEventArgs e)
    {
        var list = (List<SplashItem>)e.Argument;
        var index = 0;
            
        foreach (var item in list)
        {
            if (_worker.CancellationPending || CancelSource.Token.IsCancellationRequested)
            {
                _aborted = true;
                break;
            }

            // 报告开始处理当前项
            _worker.ReportProgress(0, new SplashProgressState
            {
                Index = index,
                Total = list.Count,
                Description = item.Description,
                Percent = CalcPercent(0, false),
                CompletedAll = false,
                Aborted = false
            });

            Exception error = null;
            try
            {
                item.Action(_context);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var cont = false;
                try 
                { 
                    if (OnItemError != null) 
                        cont = OnItemError(item, error); 
                } 
                catch { /* 忽略回调异常 */ }

                // 报告错误状态
                _worker.ReportProgress(0, new SplashProgressState
                {
                    Index = index,
                    Total = list.Count,
                    Description = item.Description,
                    Percent = CalcPercent(0, false),
                    Error = error,
                    CompletedAll = false,
                    Aborted = false
                });

                if (!cont)
                {
                    _aborted = true;
                    break;
                }
            }
            else
            {
                // 成功完成当前项
                _doneWeight += item.Weight;
                _worker.ReportProgress(0, new SplashProgressState
                {
                    Index = index,
                    Total = list.Count,
                    Description = item.Description,
                    Percent = CalcPercent(item.Weight, false),
                    CompletedAll = false,
                    Aborted = false
                });
            }
                
            index++;
        }

        // 报告最终状态
        var finalDesc = list.Count > 0 ? list[list.Count - 1].Description : string.Empty;
        _worker.ReportProgress(0, new SplashProgressState
        {
            Index = Math.Max(0, list.Count - 1),
            Total = list.Count,
            Description = finalDesc,
            Percent = _aborted ? CalcPercent(0, false) : 100,
            CompletedAll = !_aborted,
            Aborted = _aborted
        });
    }

    private int CalcPercent(int weight, bool advance)
    {
        var currentDone = _doneWeight;
        if (advance) currentDone += weight;
            
        if (_totalWeight <= 0) return 100;
            
        var ratio = (double)currentDone * 100 / _totalWeight;
        return Math.Min(100, (int)Math.Round(ratio));
    }

    private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        if (!(e.UserState is SplashProgressState state)) return;
        if (lblItemDesc == null || lblItemDetail == null) return;

        // 切换到新项时清空详细信息
        if (state.Index != _lastProgressIndex)
        {
            _lastProgressIndex = state.Index;
            SetDetailInternal(string.Empty);
            lblItemDesc.ForeColor = Color.White; // 重置颜色
        }

        // 错误时改变颜色
        if (state.Error != null)
        {
            lblItemDesc.ForeColor = Color.OrangeRed;
        }

        // 更新描述文本
        if (state.CompletedAll || state.Aborted)
        {
            lblItemDesc.Text = state.Aborted 
                ? $"已中止 ({state.Percent}%)" 
                : $"完成 ({state.Percent}%)";
        }
        else
        {
            var idxDisplay = state.Total > 0 ? state.Index + 1 : 0;
            lblItemDesc.Text = $@"[{idxDisplay}/{state.Total}] {state.Description}  {state.Percent}%";
        }

        // 更新窗口标题
        try
        {
            Text = state.Aborted 
                ? $"{AppTitle} - 已中止" 
                : $"{AppTitle} - {state.Percent}%";
        }
        catch { /* 忽略可能的异常 */ }
    }

    private async void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        try
        {
            // 只有在没有中止的情况下才调用完成回调
            if (!_aborted && OnAllCompleted != null)
            {
                OnAllCompleted();
            }
        }
        catch (Exception ex)
        {
            try
            {
                OnFatalError?.Invoke(ex);
            } 
            catch { /* 忽略回调异常 */ }
        }
        finally
        {
            // 根据设置决定是否自动关闭
            if (AutoClose && (!_aborted || AutoCloseOnAbort))
            {
                try
                {
                    await Task.Delay(CloseDelayMs, CancelSource.Token);
                    if (!IsDisposed && !Disposing) 
                        Close();
                }
                catch (TaskCanceledException) 
                { 
                    /* 取消时忽略 */ 
                }
            }
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (_topMostTimer != null)
        {
            _topMostTimer.Stop();
            _topMostTimer.Dispose();
            _topMostTimer = null;
        }
        base.OnFormClosed(e);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!_started) StartLoading();
    }

    /// <summary>
    /// 设置是否强制置顶
    /// </summary>
    public void SetForceAlwaysOnTop(bool enable)
    {
        ForceAlwaysOnTop = enable;
        TopMost = enable;
        if (enable && Visible) BringToFront();
    }

    /// <summary>
    /// 取消加载
    /// </summary>
    public void CancelLoading()
    {
        if (!_worker.IsBusy) return;
        CancelSource.Cancel();
        _worker.CancelAsync();
    }

    /// <summary>
    /// 线程安全设置详细信息文本
    /// </summary>
    private void SetDetailInternal(string? text)
    {
        if (IsDisposed || Disposing) return;
        if (lblItemDetail == null) return;
            
        if (InvokeRequired)
        {
            try 
            { 
                BeginInvoke(new Action(() => lblItemDetail.Text = text ?? string.Empty)); 
            } 
            catch { /* 忽略可能的异常 */ }
        }
        else
        {
            lblItemDetail.Text = text ?? string.Empty;
        }
    }
}