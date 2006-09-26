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

using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Timeline;

namespace Splicer.Renderer
{
    /// <summary>
    /// Renders the audio and or video to nowhere, normally used during testing
    /// or where the result of the callbacks are being consumed (frame grabs)
    /// </summary>
    public class NullRenderer : AbstractRenderer
    {
        public NullRenderer(ITimeline timeline)
            : this(timeline, null, null)
        {
        }

        public NullRenderer(ITimeline timeline, IDESCombineCB audioCallback, IDESCombineCB videoCallback)
            : base(timeline)
        {
            RenderToNullRenderer(audioCallback, videoCallback);

            ChangeState(RendererState.Initialized);
        }

        private void RenderToNullRenderer(IDESCombineCB audioCallback, IDESCombineCB videoCallback)
        {
            int hr;

            ICaptureGraphBuilder2 icgb = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = icgb.SetFiltergraph(_graph);
                DESError.ThrowExceptionForHR(hr);

                IBaseFilter audioDest = StandardFilters.RenderNull(_dc, _graph);
                IBaseFilter videoDest = StandardFilters.RenderNull(_dc, _graph);

                try
                {
                    RenderGroups(icgb, null, null, audioDest, videoDest, audioCallback, videoCallback);
                }
                finally
                {
                    if (audioDest != null) Marshal.ReleaseComObject(audioDest);
                    if (videoDest != null) Marshal.ReleaseComObject(videoDest);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(icgb);
            }
        }
    }
}