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
    /// <summary>
    /// Base class to use when monitoring progress, see ConsoleProgressCallback for a simple
    /// implementation.
    /// </summary>
    public abstract class AbstractProgressCallback : IDESCombineCB
    {
        private int _frameCount;
        private long _totalBytes;
        private DateTime _startTime;
        private double _lastSampleTime;
        private bool _keepTime;

        public AbstractProgressCallback()
        {
            _frameCount = 0;
            _totalBytes = 0;
            _keepTime = false;
        }

        public double LastSampleTime
        {
            get { return _lastSampleTime; }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
        }

        public long TotalBytes
        {
            get { return _totalBytes; }
        }

        public int FrameCount
        {
            get { return _frameCount; }
        }

        /// <summary>
        /// Once invoke, the progress callback will keep time (ensuring media time and elapsed align)
        /// </summary>
        public void KeepTime()
        {
            _keepTime = true;
        }

        public virtual int BufferCB(string filename, double sampleTime, IntPtr buffer, int bufferLength)
        {
            if (_frameCount == 0)
            {
                _startTime = DateTime.Now;
            }

            if (_keepTime)
            {
                TimeSpan ts = DateTime.Now - StartTime;
                int iSleep = (int) ((sampleTime - ts.TotalSeconds)*1000);
                if (iSleep > 0)
                {
                    Thread.Sleep(iSleep);
                }
            }

            _frameCount = _frameCount + 1;
            ;
            _totalBytes = _totalBytes + bufferLength;
            ;

            _lastSampleTime = sampleTime;

            HandleProgress(filename, sampleTime, buffer, bufferLength);

            return 0;
        }

        protected abstract void HandleProgress(string filename, double sampleTime, IntPtr buffer, int bufferLength);
    }
}