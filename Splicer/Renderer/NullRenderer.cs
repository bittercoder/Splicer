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
using DirectShowLib.DES;
using Splicer.Timeline;

namespace Splicer.Renderer
{
    /// <summary>
    /// Renders the audio and or video to nowhere, normally used during testing
    /// or where the result of the callbacks are being consumed (frame grabs)
    /// </summary>
    public class NullRenderer : AbstractRenderer, IDisposable
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public NullRenderer(ITimeline timeline)
            : this(timeline, null, null)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public NullRenderer(ITimeline timeline, ICallbackParticipant[] audioParticipants,
                            ICallbackParticipant[] videoParticipants)
            : base(timeline)
        {
            RenderToNullRenderer(audioParticipants, videoParticipants);

            ChangeState(RendererState.Initialized);
        }

        #region IDisposable Members

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void RenderToNullRenderer(ICallbackParticipant[] audioParticipants,
                                          ICallbackParticipant[] videoParticipants)
        {
            int hr;

            var graphBuilder = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = graphBuilder.SetFiltergraph(Graph);
                DESError.ThrowExceptionForHR(hr);

                IBaseFilter audioDest = StandardFilters.RenderNull(Cleanup, Graph);
                IBaseFilter videoDest = StandardFilters.RenderNull(Cleanup, Graph);

                try
                {
                    RenderGroups(graphBuilder, null, null, audioDest, videoDest, audioParticipants, videoParticipants);
                }
                finally
                {
                    if (audioDest != null) Marshal.ReleaseComObject(audioDest);
                    if (videoDest != null) Marshal.ReleaseComObject(videoDest);
                }

                DisableClock();
            }
            finally
            {
                Marshal.ReleaseComObject(graphBuilder);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~NullRenderer()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected virtual void Dispose(bool disposing)
        {
            DisposeRenderer(disposing);
        }
    }
}