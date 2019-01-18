// Copyright 2006-2008 Splicer Project - http://www.codeplex.com/splicer/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;

namespace Splicer.Renderer
{
    public class RendererAsyncResult : IAsyncResult, IDisposable
    {
        private readonly object _asyncState;
        private readonly AsyncCallback _callback;
        private bool _canceled;
        private bool _consumed;
        private Exception _exception;
        private ManualResetEvent _waitHandle;

        public RendererAsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            _asyncState = state;
            _waitHandle = new ManualResetEvent(false);
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public bool Canceled
        {
            get { return _canceled; }
        }

        public bool Consumed
        {
            get { return _consumed; }
        }

        #region IAsyncResult Members

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

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void Consume()
        {
            _consumed = true;
        }

        public void Complete(Exception ex)
        {
            _exception = ex;
            Complete(false);
        }

        public void Complete(bool canceled)
        {
            _canceled = canceled;
            _waitHandle.Set();
            if (_callback != null) _callback(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_waitHandle != null)
            {
                _waitHandle.Close();
                _waitHandle = null;
            }
        }

        ~RendererAsyncResult()
        {
            Dispose(false);
        }
    }
}