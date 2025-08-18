using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace XMSDK.Framework.Logger
{
    public class TraceMessageReceiver: IDisposable
    {
        private class CustomTraceListener : TraceListener
        {
            public event Action<string> OnMessage;

            public override void Write(string message)
            {
                OnMessage?.Invoke(message);
            }

            public override void WriteLine(string message)
            {
                OnMessage?.Invoke(message);
            }
        }
        
        private readonly CustomTraceListener _diagnosticsListener = new CustomTraceListener();
        private List<IDisposable> _disposables = new List<IDisposable>();

        public TraceMessageReceiver AddToDebug()
        {
            Trace.Listeners.Add(_diagnosticsListener);
            Trace.AutoFlush = true;
            return this;
        }

        public TraceMessageReceiver AddToTrace()
        {
            Debug.Listeners.Add(_diagnosticsListener);
            Debug.AutoFlush = true;
            return this;
        }

        public TraceMessageReceiver AddProcesser(ITraceMessageProcesser processer)
        {
            _diagnosticsListener.OnMessage += processer.OnMessage;

            if (processer is IDisposable disposableProcesser)
            {
                _disposables.Add(disposableProcesser);
            }

            return this;
        }

        public void Dispose()
        {
            Trace.Listeners.Remove(_diagnosticsListener);
            Debug.Listeners.Remove(_diagnosticsListener);
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _diagnosticsListener.Dispose();
        }
    }
}