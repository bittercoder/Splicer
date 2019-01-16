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
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib;

namespace Splicer.Renderer
{
    /// <summary>
    /// Class used by both audio and video callback
    /// </summary>
    public class CallbackHandler : ISampleGrabberCB
    {
        #region Data members

        /// <summary>
        /// Client callback routine
        /// </summary>
        private readonly ICallbackParticipant[] _participants;

        #endregion

        public CallbackHandler(params ICallbackParticipant[] participants)
        {
            _participants = participants;
        }

        #region ISampleGrabberCB Members

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        /// <summary>
        /// Callback for the buffer, will pass on the information to any participants assigned
        /// to this instance.  Each participant is invoked in turn, until they are exhausted, or
        /// they return a value other then 0 (S_OK)
        /// </summary>
        /// <param name="SampleTime"></param>
        /// <param name="pBuffer"></param>
        /// <param name="BufferLen"></param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            int result = 0;

            if ((_participants != null) && (_participants.Length > 0))
            {
                foreach (ICallbackParticipant participant in _participants)
                {
                    result = participant.ProcessBuffer(SampleTime, pBuffer, BufferLen);
                    if (result != 0) break;
                }
            }

            return result;
        }

        #endregion
    }
}