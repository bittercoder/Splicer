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
using Splicer.Properties;
using Splicer.Timeline;
using Splicer.Utilities;

namespace Splicer.Renderer
{
    public class WindowRenderer : AbstractRenderer, IDisposable
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WindowRenderer(ITimeline timeline, IntPtr windowHandle, ICallbackParticipant[] videoParticipants,
                              ICallbackParticipant[] audioParticipants)
            : base(timeline)
        {
            RenderToWindow(windowHandle, videoParticipants, audioParticipants);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WindowRenderer(ITimeline timeline, IntPtr windowHandle)
            : this(timeline, windowHandle, null, null)
        {
        }

        #region IDisposable Members

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Configure the graph to output the results to a video window.
        /// </summary>
        /// <remarks>
        /// The callback routines are invoked once for each sample.  This allows for additional processing to
        /// be performed on the video or audio buffers.
        /// </remarks>
        /// <param name="windowHandle">Window handle to render to, or IntPtr.Zero to render to its own window</param>
        /// <param name="videoParticipants">Callback routine to be called for each video frame or null for no callback</param>
        /// <param name="audioParticipants">Callback routine to be called for each audio frame or null for no callback</param>
        private void RenderToWindow(IntPtr windowHandle, ICallbackParticipant[] videoParticipants,
                                    ICallbackParticipant[] audioParticipants)
        {
            int hr;
            IPin pin;
            IVideoWindow videoWindow;
            IAMTimelineObj group;
            IAMTimeline desTimeline = Timeline.DesTimeline;

            // Contains useful routines for creating the graph
            var graphBuilder = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = graphBuilder.SetFiltergraph(Graph);
                DESError.ThrowExceptionForHR(hr);

                int NumGroups;
                hr = desTimeline.GetGroupCount(out NumGroups);
                DESError.ThrowExceptionForHR(hr);

                // Walk the groups.  For DESCombine, there is one group that 
                // contains all the video, and a second group for the audio.
                for (int i = 0; i < NumGroups; i++)
                {
                    hr = desTimeline.GetGroup(out group, i);
                    DESError.ThrowExceptionForHR(hr);

                    try
                    {
                        // Inform the graph we will be previewing (rather than writing to disk)
                        var pTLGroup = (IAMTimelineGroup) group;
                        hr = pTLGroup.SetPreviewMode(true);
                        DESError.ThrowExceptionForHR(hr);
                    }
                    finally
                    {
                        // Release the group
                        Marshal.ReleaseComObject(group);
                    }

                    // Get the IPin for the current group
                    hr = RenderEngine.GetGroupOutputPin(i, out pin);
                    DESError.ThrowExceptionForHR(hr);

                    try
                    {
                        // If this is the video pin
                        if (FilterGraphTools.IsVideo(pin))
                        {
                            // Get a video renderer
                            var ibfVideoRenderer = (IBaseFilter) new VideoRenderer();

                            try
                            {
                                // Create a sample grabber, add it to the graph and connect it all up
                                var mcb =
                                    new CallbackHandler(videoParticipants);
                                RenderWindowHelper(graphBuilder, mcb, "Video", pin, ibfVideoRenderer);
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(ibfVideoRenderer);
                            }
                        }
                        else
                        {
                            // Get an audio renderer
                            var ibfAudioRenderer = (IBaseFilter) new AudioRender();

                            try
                            {
                                // Create a sample grabber, add it to the graph and connect it all up
                                var mcb =
                                    new CallbackHandler(audioParticipants);
                                RenderWindowHelper(graphBuilder, mcb, "Audio", pin, ibfAudioRenderer);
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(ibfAudioRenderer);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(pin);
                    }
                }

                // Configure the video window
                videoWindow = (IVideoWindow) Graph;

                // If a window handle was supplied, use it
                if (windowHandle != IntPtr.Zero)
                {
                    hr = videoWindow.put_Owner(windowHandle);
                    DESError.ThrowExceptionForHR(hr);
                }
                else
                {
                    // Use our own window

                    hr = videoWindow.put_Caption(Resources.DefaultVideoRenderingWindowCaption);
                    DESError.ThrowExceptionForHR(hr);

                    // since no user interaction is allowed, remove
                    // system menu and maximize/minimize buttons
                    WindowStyle lStyle = 0;
                    hr = videoWindow.get_WindowStyle(out lStyle);
                    DESError.ThrowExceptionForHR(hr);

                    lStyle &= ~(WindowStyle.MinimizeBox | WindowStyle.MaximizeBox | WindowStyle.SysMenu);
                    hr = videoWindow.put_WindowStyle(lStyle);
                    DESError.ThrowExceptionForHR(hr);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(graphBuilder);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~WindowRenderer()
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