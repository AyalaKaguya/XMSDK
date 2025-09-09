using System;
using System.Windows.Forms;

namespace XMSDK.Framework.Demo
{
    /// <summary>
    /// 信号系统演示程序启动器
    /// </summary>
    public static class SignalSystemDemoProgram
    {
        /// <summary>
        /// 启动信号系统演示
        /// </summary>
        [STAThread]
        public static void RunSignalSystemDemo()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using var demoForm = new SignalSystemDemoForm();
                Application.Run(demoForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动演示失败：{ex.Message}\n\n{ex.StackTrace}", 
                    "演示程序错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
