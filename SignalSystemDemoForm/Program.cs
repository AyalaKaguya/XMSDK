using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalSystemDemoForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
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