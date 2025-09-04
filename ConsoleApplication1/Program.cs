using System;
using System.Diagnostics;
using System.Threading.Tasks;
using XMSDK.Framework.Logger;

namespace ConsoleApplication1
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // 测试日志记录
            Console.WriteLine("\n=== 测试 Trace 和 Debug 输出 ===");
            new TraceMessageReceiver()
                // .AddToDebug() // 只输出到Debug
                .AddToTrace() // 同时输出到Trace和Debug
                .AddProcesser(new LogToConsoleProcesser())
                .AddProcesser(new LogToFolderProcesser(@"D:\logs"));
            
            Trace.WriteLine("This is a test message for Trace.");
            Debug.WriteLine("This is a test message for Debug."); // Release模式下不会输出
            
            
            TransmissionTest.TestTransmissionCreate();
            TransmissionTest.TestSignalTransmission();
            TransmissionTest.TestCommandTransmission();
            
            Console.WriteLine("=== 测试完成 ===");
        }
        
        
    }
}