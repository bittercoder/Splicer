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
using DirectShowLib;

namespace Splicer.Renderer
{
    /// <summary>
    /// Base class to use when monitoring progress, see ConsoleProgressParticipant for a simple
    /// implementation.  This doesn't interact with the buffer, so it's suitable for placing
    /// anywhere in a chain of callback participants.
    /// </summary>
    public abstract class AbstractProgressParticipant : ICallbackParticipant
    {
        private int _frameCount;
        private bool _keepTime;
        private double _lastSampleTime;
        private DateTime _startTime;
        private long _totalBytes;

        /// <summary>
        /// The last sample time (ie. position in the resultant video clips timeframe)
        /// </summary>
        public double LastSampleTime
        {
            get { return _lastSampleTime; }
        }

        /// <summary>
        /// The time we started processing
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
        }

        /// <summary>
        /// Total bytes processor
        /// </summary>
        public long TotalBytes
        {
            get { return _totalBytes; }
        }

        /// <summary>
        /// Number of frames processor
        /// </summary>
        public int FrameCount
        {
            get { return _frameCount; }
        }

        #region ICallbackParticipant Members

        /// <summary>
        /// Processes the buffer, and keeps count of the various values - shouldn't be necessary
        /// to override this in derived classes.
        /// </summary>
        /// <param name="sampleTime"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        public virtual int ProcessBuffer(double sampleTime, IntPtr buffer, int bufferLength)
        {
            if (_frameCount == 0)
            {
                _startTime = DateTime.Now;
            }

            if (_keepTime)
            {
                TimeSpan ts = DateTime.Now - StartTime;
                var iSleep = (int) ((sampleTime - ts.TotalSeconds)*1000);
                if (iSleep > 0)
                {
                    Thread.Sleep(iSleep);
                }
            }

            _frameCount++;

            _totalBytes += bufferLength;

            _lastSampleTime = sampleTime;

            try
            {
                HandleProgress(sampleTime, bufferLength);
            }
            catch
            {
                // TODO: add support for logging the actual exception
                return DsResults.E_RunTimeError;
            }

            return 0;
        }

        #endregion

        /// <summary>
        /// Once invoked, the progress callback will keep time 
        /// (ensuring media time and elapsed align by sleeping to slow the encoding
        /// process down, as required).
        /// </summary>
        public void KeepTime()
        {
            _keepTime = true;
        }

        /// <summary>
        /// Implement this method fo abstract classes - you can probably ignore all parameters.
        /// </summary>
        /// <param name="sampleTime"></param>
        /// <param name="bufferLength"></param>
        protected abstract void HandleProgress(double sampleTime, int bufferLength);
    }
}