using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XMSDK.Framework.Demo;
using XMSDK.Framework.Forms.SplashScreen;

namespace ConsoleApplication1
{
    internal static class Program
    {
        private static IServiceProvider _serviceProvider;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 使用自定义 ApplicationContext，避免 Splash 作为主窗体导致关闭后退出
            var appContext = new BootstrapContext();
            Application.Run(appContext);
        }

        /// <summary>
        /// 自定义启动上下文
        /// </summary>
        private class BootstrapContext : ApplicationContext
        {
            public BootstrapContext()
            {
                var splash = new SplashWindow
                {
                    AppTitle = "XMSDK 演示",
                    AppDescription = "演示初始化组件与依赖注入",
                    Author = "作者: AyalaKaguya",
                    Copyright = "© 2025 AyalaKaguya"
                };

                // 注册启动任务
                splash.AddItem(new SplashItem("准备服务集合", 1, () =>
                {
                    var services = new ServiceCollection();
                    services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));
                    // 注册主窗体 (Transient 以便每次需要都能重新创建；此处一个实例即可)
                    services.AddTransient<LoggerDemoForm>();
                    _serviceProvider = services.BuildServiceProvider();
                }));

                splash.AddItem(new SplashItem("加载配置", 3, () =>
                {
                    // 模拟读取配置
                    System.Threading.Thread.Sleep(300);
                }));

                splash.AddItem(new SplashItem("预热日志系统", 2, () =>
                {
                    var factory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                    var logger = factory.CreateLogger("Bootstrap");
                    logger.LogInformation("日志系统预热完成");
                }));

                splash.AddItem(new SplashItem("检查更新 (模拟)", 2, () => { System.Threading.Thread.Sleep(200); }));

                splash.AddItem(new SplashItem("最终处理", 1, () => { System.Threading.Thread.Sleep(150); }));

                splash.OnAllCompleted = OnSplashCompleted;
                splash.OnItemError = (item, ex) =>
                {
                    MessageBox.Show($"启动项失败: {item.Description}\n{ex.Message}", "启动错误", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false; // 发生错误即终止
                };
                splash.OnFatalError = OnFatalError;
                splash.AutoClose = true;
                splash.CloseDelayMs = 800;
                splash.Show();
            }

            private void OnSplashCompleted()
            {
                // 解析主窗体并显示
                var mainForm = _serviceProvider.GetRequiredService<LoggerDemoForm>();
                // 设置为主窗体 (关闭主窗体即退出应用)
                mainForm.FormClosed += (_, __) => ExitThread();
                MainForm = mainForm; // 设置上下文主窗体
                mainForm.Show();
            }

            private void OnFatalError(Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitThread();
            }
        }
    }
}