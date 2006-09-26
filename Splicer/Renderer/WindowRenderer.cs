using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Timeline;
using Splicer.Utils;

namespace Splicer.Renderer
{
    public class WindowRenderer : AbstractRenderer
    {
        public WindowRenderer(ITimeline timeline, IntPtr hWnd, IDESCombineCB pVideoCallback,
                              IDESCombineCB pAudioCallback)
            : base(timeline)
        {
            RenderToWindow(hWnd, pVideoCallback, pAudioCallback);
        }

        public WindowRenderer(ITimeline timeline, IntPtr hWnd)
            : this(timeline, hWnd, null, null)
        {
        }

        /// <summary>
        /// Configure the graph to output the results to a video window.
        /// </summary>
        /// <remarks>
        /// The callback routines are invoked once for each sample.  This allows for additional processing to
        /// be performed on the video or audio buffers.
        /// </remarks>
        /// <param name="hWnd">Window handle to render to, or IntPtr.Zero to render to its own window</param>
        /// <param name="pVideoCallback">Callback routine to be called for each video frame or null for no callback</param>
        /// <param name="pAudioCallback">Callback routine to be called for each audio frame or null for no callback</param>
        private void RenderToWindow(IntPtr hWnd, IDESCombineCB pVideoCallback, IDESCombineCB pAudioCallback)
        {
            int hr;
            IPin pPin;
            IVideoWindow pVidWindow;
            IAMTimelineObj pGroup;
            IAMTimeline desTimeline = _timeline.DesTimeline;

            // Contains useful routines for creating the graph
            ICaptureGraphBuilder2 icgb = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = icgb.SetFiltergraph(_graph);
                DESError.ThrowExceptionForHR(hr);

                int NumGroups;
                hr = desTimeline.GetGroupCount(out NumGroups);
                DESError.ThrowExceptionForHR(hr);

                // Walk the groups.  For DESCombine, there is one group that 
                // contains all the video, and a second group for the audio.
                for (int i = 0; i < NumGroups; i++)
                {
                    hr = desTimeline.GetGroup(out pGroup, i);
                    DESError.ThrowExceptionForHR(hr);

                    try
                    {
                        // Inform the graph we will be previewing (rather than writing to disk)
                        IAMTimelineGroup pTLGroup = (IAMTimelineGroup) pGroup;
                        hr = pTLGroup.SetPreviewMode(true);
                        DESError.ThrowExceptionForHR(hr);
                    }
                    finally
                    {
                        // Release the group
                        Marshal.ReleaseComObject(pGroup);
                    }

                    // Get the IPin for the current group
                    hr = _renderEngine.GetGroupOutputPin(i, out pPin);
                    DESError.ThrowExceptionForHR(hr);

                    try
                    {
                        // If this is the video pin
                        if (PinUtils.IsVideo(pPin))
                        {
                            // Get a video renderer
                            IBaseFilter ibfVideoRenderer = (IBaseFilter) new VideoRenderer();

                            try
                            {
                                // Create a sample grabber, add it to the graph and connect it all up
                                CallbackHandler mcb =
                                    new CallbackHandler(_firstVideoGroup, pVideoCallback, (IMediaEventSink) _graph,
                                                        EC_VideoFileComplete);
                                RenderWindowHelper(icgb, mcb, "Video", pPin, ibfVideoRenderer);
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(ibfVideoRenderer);
                            }
                        }
                        else
                        {
                            // Get an audio renderer
                            IBaseFilter ibfAudioRenderer = (IBaseFilter) new AudioRender();

                            try
                            {
                                // Create a sample grabber, add it to the graph and connect it all up
                                CallbackHandler mcb =
                                    new CallbackHandler(_firstAudioGroup, pAudioCallback, (IMediaEventSink) _graph,
                                                        EC_AudioFileComplete);
                                RenderWindowHelper(icgb, mcb, "Audio", pPin, ibfAudioRenderer);
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(ibfAudioRenderer);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(pPin);
                    }
                }

                // Configure the video window
                pVidWindow = (IVideoWindow) _graph;

                // If a window handle was supplied, use it
                if (hWnd != IntPtr.Zero)
                {
                    hr = pVidWindow.put_Owner(hWnd);
                    DESError.ThrowExceptionForHR(hr);
                }
                else
                {
                    // Use our own window

                    hr = pVidWindow.put_Caption("Video Rendering Window");
                    DESError.ThrowExceptionForHR(hr);

                    // since no user interaction is allowed, remove
                    // system menu and maximize/minimize buttons
                    WindowStyle lStyle = 0;
                    hr = pVidWindow.get_WindowStyle(out lStyle);
                    DESError.ThrowExceptionForHR(hr);

                    lStyle &= ~(WindowStyle.MinimizeBox | WindowStyle.MaximizeBox | WindowStyle.SysMenu);
                    hr = pVidWindow.put_WindowStyle(lStyle);
                    DESError.ThrowExceptionForHR(hr);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(icgb);
            }
        }
    }
}