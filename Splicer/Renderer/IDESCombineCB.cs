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

namespace Splicer.Renderer
{
    /// <summary>
    /// A callback interface that can be implemented by callers to DESCombine 
    /// who wish to perform processing on video or audio frames.
    /// </summary>
    /// <remarks>
    /// Classes which implement this interfaces can be passed to <see cref="DESCombine.RenderToWindow"/>
    /// or <see cref="DESCombine.RenderToAVI"/>.  Each audio or video frame that is processed by DES
    /// will be passed to this callback which can perform additional processing.
    /// </remarks>
    public interface IDESCombineCB
    {
        /// <summary>
        /// Callback routine - called once for each audio or video frame
        /// </summary>
        /// <remarks>
        /// The buffer can be examined or modified.
        /// </remarks>
        /// <param name="sFileName">Filename currently being processed</param>
        /// <param name="sampleTime">Time stamp in seconds</param>
        /// <param name="buffer">Pointer to the buffer</param>
        /// <param name="bufferLength">Duration of the buffer</param>
        /// <returns>Return S_OK if successful, or an HRESULT error code otherwise.  This value is sent as 
        /// the return value to ISampleGrabberCB::BufferCB</returns>
        int BufferCB(
            string sFileName,
            double sampleTime,
            IntPtr buffer,
            int bufferLength
            );
    }
}