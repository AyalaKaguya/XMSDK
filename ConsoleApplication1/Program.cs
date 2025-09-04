using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using XMSDK.Framework.Logger;
using XMSDK.Framework.Demo;

namespace ConsoleApplication1
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // 启用Windows Forms应用程序的视觉样式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoggerDemoForm());
        }
    }
}