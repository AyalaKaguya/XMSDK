using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer; // 新增

namespace XMSDK.Framework.Forms.SplashScreen
{
    public sealed partial class SplashWindow : Form
    {
        /// <summary>
        /// Splash 任务项
        /// </summary>
        internal class SplashProgressState
        {
            public int Index { get; set; }
            public int Total { get; set; }
            public string Description { get; set; }
            public int Percent { get; set; }
            public Exception Error { get; set; }
            public bool CompletedAll { get; set; }
        }

        private readonly List<SplashItem> _items = new List<SplashItem>();
        private readonly BackgroundWorker _worker;
        private int _totalWeight;
        private int _doneWeight;
        private bool _started;
        private Timer _topMostTimer; // 保持置顶

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppTitle
        {
            get => lblAppTitle.Text;
            set => lblAppTitle.Text = value ?? string.Empty;
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string AppDescription
        {
            get => lblDescription.Text;
            set => lblDescription.Text = value ?? string.Empty;
        }

        /// <summary>
        /// 作者(可选)
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
        /// 版权(可选)
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
        /// 当启动应用失败时的回调，代表着启动过程遇到了致命错误并中断
        /// </summary>
        public Action<Exception> OnFatalError { get; set; }

        /// <summary>
        /// 所有任务完成回调
        /// </summary>
        public Action OnAllCompleted { get; set; }

        /// <summary>
        /// 是否在全部完成后自动关闭窗口 (延迟1秒)
        /// </summary>
        public bool AutoClose { get; set; } = true;

        /// <summary>
        /// 关闭前延迟毫秒
        /// </summary>
        public int CloseDelayMs { get; set; } = 1000;

        /// <summary>
        /// 允许外部取消
        /// </summary>
        public CancellationTokenSource CancelSource { get; } = new CancellationTokenSource();

        /// <summary>
        /// 是否强制保持窗口置顶 (默认启用)
        /// </summary>
        public bool ForceAlwaysOnTop { get; set; } = true;

        public SplashWindow()
        {
            InitializeComponent();

            // 初始化额外UI属性
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true; // 需要显示在任务栏以显示进度
            DoubleBuffered = true;
            TopMost = true; // 初始置顶

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            // 定时器保持置顶
            _topMostTimer = new Timer { Interval = 1500 };
            _topMostTimer.Tick += (s, e) =>
            {
                if (!ForceAlwaysOnTop) return;
                if (!TopMost) TopMost = true;
                // BringToFront 可以防止偶发被遮挡
                if (Visible) BringToFront();
            };
            _topMostTimer.Start();
        }

        /// <summary>
        /// 设置背景图片（拉伸填充）
        /// </summary>
        /// <param name="img"></param>
        public void SetBackgroundImage(Image img)
        {
            BackgroundImage = img;
            BackgroundImageLayout = ImageLayout.Stretch;
        }

        /// <summary>
        /// 添加Splash任务项
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddItem(SplashItem item)
        {
            if (_started) throw new InvalidOperationException("加载已开始，无法再添加");
            _items.Add(item);
            _totalWeight += item.Weight;
        }

        public void StartLoading()
        {
            if (_started) return;
            _started = true;
            // 预填充步骤列表（UI 线程）
            if (listBoxSteps != null)
            {
                listBoxSteps.BeginUpdate();
                listBoxSteps.Items.Clear();
                foreach (var it in _items)
                {
                    listBoxSteps.Items.Add("[ ] " + it.Description);
                }

                listBoxSteps.EndUpdate();
            }

            lblStepCounter.Text = $"(0/{_items.Count})";
            _worker.RunWorkerAsync(_items.Count == 0 ? new List<SplashItem>() : _items);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var list = (List<SplashItem>)e.Argument;
            var index = 0;
            foreach (var item in list)
            {
                if (_worker.CancellationPending || CancelSource.Token.IsCancellationRequested)
                {
                    break;
                }

                Exception error = null;
                try
                {
                    var sw = Stopwatch.StartNew();
                    item.Action();
                    sw.Stop();
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
                        cont = OnItemError?.Invoke(item, error) ?? false;
                    }
                    catch
                    {
                        /* 用户回调异常忽略 */
                    }

                    // 报告错误状态
                    _worker.ReportProgress(0, new SplashProgressState
                    {
                        Index = index,
                        Total = list.Count,
                        Description = item.Description,
                        Percent = CalcPercent(item.Weight, advanceWeight: false),
                        Error = error,
                        CompletedAll = false
                    });
                    if (!cont) break; // 中断
                }
                else
                {
                    _doneWeight += item.Weight;
                    _worker.ReportProgress(0, new SplashProgressState
                    {
                        Index = index,
                        Total = list.Count,
                        Description = item.Description,
                        Percent = CalcPercent(item.Weight, advanceWeight: true),
                        CompletedAll = false
                    });
                }

                index++;
            }

            // 结束
            _worker.ReportProgress(0, new SplashProgressState
            {
                Index = list.Count - 1,
                Total = list.Count,
                Description = list.Count == 0 ? "" : list[list.Count - 1].Description,
                Percent = 100,
                CompletedAll = true
            });
        }

        private int CalcPercent(int currentWeight, bool advanceWeight)
        {
            var done = _doneWeight + (advanceWeight ? currentWeight : 0);
            if (_totalWeight <= 0) return 100;
            var percent = (int)Math.Min(100, Math.Round(done * 100.0 / _totalWeight));
            return percent;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!(e.UserState is SplashProgressState state)) return;

            // 更新当前项文本
            if (state.Error != null)
            {
                lblCurrentItem.Text =
                    $"({state.Index + 1}/{state.Total}) {state.Description} 失败: {state.Error.Message}";
            }
            else if (!state.CompletedAll)
            {
                lblCurrentItem.Text = $"({state.Index + 1}/{state.Total}) {state.Description}...";
            }

            // 步骤计数标签 (x/n)
            if (state.Total > 0 && !state.CompletedAll)
            {
                lblStepCounter.Text = $"({Math.Min(state.Index + 1, state.Total)}/{state.Total})";
            }

            if (state.CompletedAll)
            {
                lblStepCounter.Text = $"({state.Total}/{state.Total})";
            }

            // 更新步骤列表状态
            if (listBoxSteps != null && state.Total > 0 && state.Index >= 0 && state.Index < listBoxSteps.Items.Count)
            {
                // 将已完成的步骤标记
                for (int i = 0; i <= state.Index && i < listBoxSteps.Items.Count; i++)
                {
                    var original = _items[i].Description;
                    var prefix = (i == state.Index && state.Error != null) ? "[X] " : "[✓] ";
                    listBoxSteps.Items[i] = prefix + original;
                }

                // 标记下一待处理步骤(如果还没完成)
                if (!state.CompletedAll && state.Index + 1 < listBoxSteps.Items.Count)
                {
                    var nextOriginal = _items[state.Index + 1].Description;
                    if (!listBoxSteps.Items[state.Index + 1].ToString().StartsWith("[✓] ") &&
                        !listBoxSteps.Items[state.Index + 1].ToString().StartsWith("[X] "))
                        listBoxSteps.Items[state.Index + 1] = "[>] " + nextOriginal;
                }
            }

            // 进度条
            progressBar.Value = Math.Min(100, Math.Max(0, state.Percent));
            lblPercent.Text = progressBar.Value + "%";


            if (state.CompletedAll)
            {
                lblCurrentItem.Text = "加载完成";
            }
        }

        private async void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                OnAllCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                OnFatalError?.DynamicInvoke(ex);
            }
            finally
            {
                if (AutoClose)
                {
                    await Task.Delay(CloseDelayMs, CancelSource.Token);
                    if (!IsDisposed && !Disposing) Close();
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

        // 允许外部切换保持置顶状态
        public void SetForceAlwaysOnTop(bool enable)
        {
            ForceAlwaysOnTop = enable;
            TopMost = enable;
            if (enable && Visible) BringToFront();
        }

        public void CancelLoading()
        {
            if (!_worker.IsBusy) return;
            CancelSource.Cancel();
            _worker.CancelAsync();
        }
    }
}