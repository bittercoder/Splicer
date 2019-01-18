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

namespace Splicer.Renderer
{
    /// <summary>
    /// A callback interface which can be implemented to handle additional processing per frame (be it audio or video).
    /// </remarks>
    public interface ICallbackParticipant
    {
        /// <summary>
        /// Callback routine - called once for each audio or video frame
        /// </summary>
        /// <remarks>
        /// The buffer can be examined or modified.
        /// </remarks>        
        /// <param name="sampleTime">Time stamp in seconds</param>
        /// <param name="buffer">Pointer to the buffer</param>
        /// <param name="bufferLength">Duration of the buffer</param>
        /// <returns>Return S_OK if successful, or an HRESULT error code otherwise.  This value is sent as 
        /// the return value to ISampleGrabberCB::ProcessBuffer</returns>
        int ProcessBuffer(double sampleTime, IntPtr buffer, int bufferLength);
    }
}