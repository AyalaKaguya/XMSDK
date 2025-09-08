using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XMSDK.Framework.Demo;
using XMSDK.Framework.Forms;
using XMSDK.Framework.Forms.SplashScreen;
using XMSDK.Framework.Logger;

namespace ConsoleApplication1;

internal static class Program
{
    private static IServiceProvider _serviceProvider;

    [STAThread]
    public static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

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

            // 设置背景图片（可选）
            // 方式1：直接设置属性
            splash.SplashBackgroundImage = Resources.DefaultSplashBackground; // 如果有资源文件

            // 方式2：从文件设置（如果图片文件存在）
            // splash.SetBackgroundImageFromFile(@"C:\Users\ayala\Downloads\DefaultSplashBackground.png");

            // 方式3：使用内置方法设置
            // splash.SetBackgroundImage(someImage, ImageLayout.Stretch);

            // 如果没有背景图片，可以设置一个更好看的背景色
            // splash.BackColor = Color.FromArgb(45, 45, 48); // 深灰色背景

            splash.AddItem(new SplashItem("准备服务集合", 1, ctx =>
            {
                ctx.SetDetail("创建 ServiceCollection...");
                var services = new ServiceCollection();
                services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));
                ctx.SetDetail("注册主窗体...");
                services.AddTransient<LoggerDemoForm>();
                _serviceProvider = services.BuildServiceProvider();
                ctx.SetDetail("服务集合构建完成");
            }));

            splash.AddItem(new SplashItem("加载配置", 3, ctx =>
            {
                ctx.SetDetail("读取 app-settings.json (模拟)...");
                System.Threading.Thread.Sleep(1500);
                ctx.SetDetail("绑定配置对象 (模拟)...");
                System.Threading.Thread.Sleep(1500);
                ctx.SetDetail("配置加载完成");
            }));

            splash.AddItem(new SplashItem("预热日志系统", 2, ctx =>
            {
                ctx.SetDetail("获取 LoggerFactory...");
                var factory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = factory.CreateLogger("Bootstrap");
                ctx.SetDetail("写入测试日志...");
                logger.LogInformation("日志系统预热完成");
                ctx.SetDetail("日志预热完成");
            }));

            splash.AddItem(new SplashItem("测试日志控件", 2, ctx =>
            {
                ctx.SetDetail("创建日志控件...");
                var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                var loggerList = new LoggerList();
                loggerFactory.AddProvider(new ListViewLoggerProvider(loggerList));
                var logger = _serviceProvider.GetRequiredService<ILogger<BootstrapContext>>();
                logger.LogTrace("这是跟踪日志消息");
                ctx.SetDetail("发送测试日志...");
                System.Threading.Thread.Sleep(200);
                ctx.SetDetail("测试日志控件释放");
                loggerList.Dispose();
                System.Threading.Thread.Sleep(200);
                logger.LogTrace("这是第二条跟踪日志消息");
                System.Threading.Thread.Sleep(200);
                ctx.SetDetail("日志控件测试完成");
                ctx.SetDetail("已是最新版本");
                System.Threading.Thread.Sleep(200);
            }));

            splash.AddItem(new SplashItem("最终处理", 1, ctx =>
            {
                ctx.SetDetail("执行最终收尾...");
                System.Threading.Thread.Sleep(1500);
                ctx.SetDetail("启动即将完成");
            }));

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
            var mainForm = _serviceProvider.GetRequiredService<LoggerDemoForm>();
            mainForm.FormClosed += (_, _) => ExitThread();
            MainForm = mainForm;
            mainForm.Show();
        }

        private void OnFatalError(Exception ex)
        {
            MessageBox.Show($"启动失败: {ex.Message}", "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ExitThread();
        }
    }
}