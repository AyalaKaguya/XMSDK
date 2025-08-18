using System;

namespace XMSDK.Framework.Logger
{
    public class LogToConsoleProcesser: ITraceMessageProcesser
    {
        public void OnMessage(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}