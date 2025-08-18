using System.Diagnostics;
using System.Threading.Tasks;
using XMSDK.Framework.Logger;

namespace ConsoleApplication1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (new TraceMessageReceiver()
                       // .AddToDebug()
                       .AddToTrace()
                       .AddProcesser(new LogToConsoleProcesser())
                       .AddProcesser(new LogToFolderProcesser(@"D:\logs")))
            {
                Trace.WriteLine("This is a test message for Trace.");
                Debug.WriteLine("This is a test message for Debug.");
            }
        }
    }
}