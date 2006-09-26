using System;
using System.Threading;

namespace Splicer.Renderer
{
    public class RendererAsyncResult : IAsyncResult, IDisposable
    {
        private AsyncCallback _callback;
        private object _asyncState;
        private ManualResetEvent _waitHandle;
        private Exception _exception;
        private bool _cancelled;
        private bool _consumed;

        public RendererAsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            _asyncState = state;
            _waitHandle = new ManualResetEvent(false);
        }

        public object AsyncState
        {
            get { return _asyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _waitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return _waitHandle.WaitOne(0, false); }
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public bool Cancelled
        {
            get { return _cancelled; }
        }

        public bool Consumed
        {
            get { return _consumed; }
        }

        public void Consume()
        {
            _consumed = true;
        }

        public void Complete(Exception ex)
        {
            _exception = ex;
            Complete(false);
        }

        public void Complete(bool cancelled)
        {
            _cancelled = cancelled;
            _waitHandle.Set();
            if (_callback != null) _callback(this);
        }

        public void Dispose()
        {
            if (_waitHandle != null)
            {
                _waitHandle.Close();
                _waitHandle = null;
            }
        }
    }
}