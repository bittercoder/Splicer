// Copyright 2004-2006 Castle Project - http://www.castleproject.org/
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