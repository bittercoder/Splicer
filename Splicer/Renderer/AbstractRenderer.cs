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
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Timeline;
using Splicer.Utils;

namespace Splicer.Renderer
{
    public enum RendererState
    {
        Initializing,
        Initialized,
        GraphStarted,
        GraphCompleting,
        GraphCompleted,
        Cancelling,
        Cancelled
    }

    public abstract class AbstractRenderer : IRenderer
    {
        private event EventHandler _renderCompleted;

        /// <summary>
        /// the state of the renderer
        /// </summary>
        protected RendererState _state = RendererState.Initializing;

        /// <summary>
        /// Object we lock against to avoid cross-thread problems with setting state.
        /// </summary>
        protected object _stateLock = new object();

        /// <summary>
        /// Event code indicating a video file has finished being processed
        /// </summary>
        protected const EventCode EC_VideoFileComplete = (EventCode) 0x8000;

        /// <summary>
        /// Event code indicating an audio file has finished being processed
        /// </summary>
        protected const EventCode EC_AudioFileComplete = (EventCode) 0x8001;

        /// <summary>
        /// The timeline this class will render into a graph
        /// </summary>
        protected ITimeline _timeline;

        /// <summary>
        /// IGraphBuilder object for the timeline
        /// </summary>
        protected IGraphBuilder _graph;

        /// <summary>
        /// Media control interface from _graph
        /// </summary>
        protected IMediaControl _mediaControl;

        /// <summary>
        /// the first video group, or null if no video
        /// </summary>
        protected IGroup _firstVideoGroup;

        /// <summary>
        /// The first audio group, or null if no audio
        /// </summary>
        protected IGroup _firstAudioGroup;

        /// <summary>
        /// The engine to process the timeline (can't be released
        /// until the graph processing is complete)
        /// </summary>
        protected IRenderEngine _renderEngine;

        /// <summary>
        /// Handles cleanup of objects we aren't retaining references to
        /// </summary>
        protected DisposalCleanup _dc = new DisposalCleanup();

        protected RendererAsyncResult _renderResult;

        protected RendererAsyncResult _cancelResult;

        protected readonly object _renderLock = new object();

        public AbstractRenderer(ITimeline timeline)
        {
            _timeline = timeline;

            int hr = 0;

            // create the render engine
            _renderEngine = (IRenderEngine) new RenderEngine();
            _dc.Add(_renderEngine);

            // tell the render engine about the timeline it should use
            hr = _renderEngine.SetTimelineObject(_timeline.DesTimeline);
            DESError.ThrowExceptionForHR(hr);

            // connect up the front end
            hr = _renderEngine.ConnectFrontEnd();
            DESError.ThrowExceptionForHR(hr);

            // Get the filtergraph - used all over the place
            hr = _renderEngine.GetFilterGraph(out _graph);
            _dc.Add(_graph);
            DESError.ThrowExceptionForHR(hr);

            // find the first (and usually last) audio and video group, we use these
            // when rendering to track progress
            _firstAudioGroup = _timeline.FindFirstGroupOfType(GroupType.Audio);
            _firstVideoGroup = _timeline.FindFirstGroupOfType(GroupType.Video);
        }

        public RendererState State
        {
            get { return _state; }
        }

        public event EventHandler RenderCompleted
        {
            add { _renderCompleted += value; }
            remove { _renderCompleted -= value; }
        }

        protected void OnRenderCompleted()
        {
            if (_renderCompleted != null)
            {
                _renderCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Returns an XML description of the capture graph (as seen by DES).
        /// </summary>
        /// <remarks>
        /// Might be useful for debugging, handy for implementing some unit tests (where you 
        /// want to make sure changes to implementation don't alter the expected DES capture graph).
        /// </remarks>
        /// <returns>String containing XML</returns>
        public string ToXml()
        {
            IXml2Dex pXML;
            string sRet;
            int hr;

            pXML = (IXml2Dex) new Xml2Dex();

            try
            {
                hr = pXML.WriteXML(_timeline.DesTimeline, out sRet);
                DESError.ThrowExceptionForHR(hr);
            }
            finally
            {
                Marshal.ReleaseComObject(pXML);
            }

            // the xml sometimes contains strange characters, so we "normalize" it
            // on the way out
            return NormalizeXml(sRet);
        }

        public string NormalizeXml(string input)
        {
            input = input.Substring(input.IndexOf('<')).Substring(0, input.LastIndexOf('>'));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);
            return doc.OuterXml;
        }

        public void SaveToGraphFile(string filename)
        {
            FilterGraphTools.SaveGraphFile(_graph, filename);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_dc != null)
            {
                ((IDisposable) _dc).Dispose();
                _dc = null;
            }

            if (_timeline != null)
            {
                _timeline.Dispose();
                _timeline = null;
            }

            if (_renderEngine != null)
            {
                Marshal.ReleaseComObject(_renderEngine);
                _renderEngine = null;
            }

            if (_mediaControl != null)
            {
                Marshal.ReleaseComObject(_mediaControl);
                _mediaControl = null;
            }

            if (_graph != null)
            {
                Marshal.ReleaseComObject(_graph);
                _graph = null;
            }
        }

        #endregion

        /// <summary>
        /// Common routine used by RenderTo*  
        /// </summary>
        /// <param name="icgb">ICaptureGraphBuilder2 to use</param>
        /// <param name="pCallback">Callback to use (or null)</param>
        /// <param name="sType">string to use in creating filter graph object descriptions</param>
        /// <param name="pPin">Pin to connect from</param>
        /// <param name="ibfCompressor">Compressor to use, or null for none</param>
        /// <param name="pOutput">Endpoint (renderer or file writer) to connect to</param>        
        protected void RenderHelper(ICaptureGraphBuilder2 icgb, CallbackHandler pCallback, string sType, IPin pPin,
                                    IBaseFilter ibfCompressor, IBaseFilter pOutput)
        {
            int hr;
            IBaseFilter ibfSampleGrabber = null;

            try
            {
                // If no callback was provided, don't create a samplegrabber
                if (pCallback != null)
                {
                    ISampleGrabber isg = (ISampleGrabber) new SampleGrabber();
                    ibfSampleGrabber = (IBaseFilter) isg;
                    _dc.Add(ibfSampleGrabber);

                    hr = isg.SetCallback(pCallback, 1);
                    DESError.ThrowExceptionForHR(hr);

                    hr = _graph.AddFilter(ibfSampleGrabber, sType + " sample grabber");
                    DESError.ThrowExceptionForHR(hr);
                }

                // If a compressor was provided, add it to the graph and connect it up
                if (ibfCompressor != null)
                {
                    // Connect the pin.
                    hr = _graph.AddFilter(ibfCompressor, sType + " Compressor");
                    DESError.ThrowExceptionForHR(hr);

                    FilterGraphTools.ConnectFilters(_graph, pPin, ibfSampleGrabber, true);

                    FilterGraphTools.ConnectFilters(_graph, ibfSampleGrabber, ibfCompressor, true);

                    FilterGraphTools.ConnectFilters(_graph, ibfCompressor, pOutput, true);
                }
                else
                {
                    // Just connect the SampleGrabber (if any)
                    hr = icgb.RenderStream(null, null, pPin, ibfSampleGrabber, pOutput);
                    DESError.ThrowExceptionForHR(hr);
                }
            }
            finally
            {
                if (ibfSampleGrabber != null)
                {
                    Marshal.ReleaseComObject(ibfSampleGrabber);
                }
            }
        }

        /// <summary>
        /// Called from RenderWindow to add the renderer to the graph, create a sample grabber, add it 
        /// to the graph and connect it all up
        /// </summary>
        /// <param name="icgb">ICaptureGraphBuilder2 to use</param>
        /// <param name="pCallback">ICaptureGraphBuilder2 to use</param>
        /// <param name="sType">String to use in creating filter graph object descriptions</param>
        /// <param name="pPin">Pin to connect from</param>
        /// <param name="ibfRenderer">Renderer to add</param>
        protected void RenderWindowHelper(ICaptureGraphBuilder2 icgb, CallbackHandler pCallback, string sType, IPin pPin,
                                          IBaseFilter ibfRenderer)
        {
            int hr;

            // Add the renderer to the graph
            hr = _graph.AddFilter(ibfRenderer, sType + " Renderer");
            _dc.Add(ibfRenderer);
            DESError.ThrowExceptionForHR(hr);

            // Do everything else
            RenderHelper(icgb, pCallback, sType, pPin, null, ibfRenderer);
        }

        protected void RenderGroups(ICaptureGraphBuilder2 icgb, IBaseFilter audioCompressor, IBaseFilter videoCompressor,
                                    IBaseFilter pMux, IDESCombineCB pAudioCallback, IDESCombineCB pVideoCallback)
        {
            RenderGroups(icgb, audioCompressor, videoCompressor, pMux, pMux, pAudioCallback, pVideoCallback);
        }

        protected void RenderGroups(ICaptureGraphBuilder2 icgb, IBaseFilter audioCompressor, IBaseFilter videoCompressor,
                                    IBaseFilter audioDest, IBaseFilter videoDest, IDESCombineCB pAudioCallback,
                                    IDESCombineCB pVideoCallback)
        {
            int hr = 0;

            if (audioCompressor != null) _dc.Add(audioCompressor);
            if (videoCompressor != null) _dc.Add(videoCompressor);
            if (audioDest != null) _dc.Add(audioDest);
            if ((videoDest != null) && (audioDest != videoDest)) _dc.Add(videoDest);

            IAMTimeline desTimeline = _timeline.DesTimeline;

            int NumGroups;
            hr = desTimeline.GetGroupCount(out NumGroups);
            DESError.ThrowExceptionForHR(hr);

            // Walk the groups.  For this class, there is one group that 
            // contains all the video, and a second group for the audio.
            for (int i = (NumGroups - 1); i >= 0; i--)
            {
                IAMTimelineObj pGroup;

                hr = desTimeline.GetGroup(out pGroup, i);
                DESError.ThrowExceptionForHR(hr);

                try
                {
                    // Inform the graph we will be writing to disk (rather than previewing)
                    IAMTimelineGroup pTLGroup = (IAMTimelineGroup) pGroup;
                    hr = pTLGroup.SetPreviewMode(false);
                    DESError.ThrowExceptionForHR(hr);
                }
                finally
                {
                    Marshal.ReleaseComObject(pGroup);
                }

                IPin pPin;

                // Get the IPin for the current group
                hr = _renderEngine.GetGroupOutputPin(i, out pPin);
                _dc.Add(pPin);
                DESError.ThrowExceptionForHR(hr);

                try
                {
                    if (PinUtils.IsVideo(pPin))
                    {
                        // Create a sample grabber, add it to the graph and connect it all up
                        CallbackHandler mcb =
                            new CallbackHandler(_firstVideoGroup, pVideoCallback, (IMediaEventSink) _graph,
                                                EC_VideoFileComplete);
                        RenderHelper(icgb, mcb, "Video", pPin, videoCompressor, videoDest);
                    }
                    else
                    {
                        // Create a sample grabber, add it to the graph and connect it all up
                        CallbackHandler mcb =
                            new CallbackHandler(_firstAudioGroup, pAudioCallback, (IMediaEventSink) _graph,
                                                EC_AudioFileComplete);
                        RenderHelper(icgb, mcb, "Audio", pPin, audioCompressor, audioDest);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(pPin);
                }
            }
        }

        /// <summary>
        /// Begins rendering and returns immediately.
        /// </summary>
        /// <remarks>
        /// Final status is sent as a <see cref="AbstractRenderer.RenderCompleted"/> event.
        /// </remarks>
        public IAsyncResult BeginRender(AsyncCallback callback, object state)
        {
            lock (_renderLock)
            {
                if ((_renderResult != null) && (_renderResult.Consumed))
                {
                    throw new SplicerException(
                        "A previous render request has not yet been completed - have you remembered to call EndRender?");
                }

                ChangeState(RendererState.GraphStarted);

                _renderResult = new RendererAsyncResult(callback, state);
                _cancelResult = null;

                StartRender();

                return _renderResult;
            }
        }

        public void Render()
        {
            IAsyncResult result = BeginRender(null, null);
            EndRender(result);
        }

        public void Cancel()
        {
            IAsyncResult result = BeginCancel(null, null);
            EndCancel(result);
        }

        private void StartRender()
        {
            int hr;

            _mediaControl = (IMediaControl) _graph;
            _dc.Add(_mediaControl);

            Thread eventThread = new Thread(new ThreadStart(EventWait));
            eventThread.Name = "Media Event Thread";
            eventThread.Start();

            hr = _mediaControl.Run();
            DESError.ThrowExceptionForHR(hr);
        }

        public IAsyncResult BeginCancel(AsyncCallback callback, object state)
        {
            lock (_renderLock)
            {
                if ((_cancelResult != null) && (!_cancelResult.Consumed))
                {
                    throw new SplicerException(
                        "You can not cancel, a request to cancel has already been issued - have you remembered to call EndCancel?");
                }
                else if (_state < RendererState.GraphStarted)
                {
                    throw new SplicerException("Graph not yet started");
                }
                else if (_state >= RendererState.GraphCompleting)
                {
                    throw new SplicerException("You can not cancel this renderer, it is already completing/canceling");
                }

                ChangeState(RendererState.GraphStarted, RendererState.Cancelling);

                _cancelResult = new RendererAsyncResult(callback, state);

                return _cancelResult;
            }
        }

        public void EndCancel(IAsyncResult result)
        {
            lock (_renderLock)
            {
                if (_cancelResult != null)
                {
                    if (_cancelResult != result)
                    {
                        throw new SplicerException("The supplied async result was not issued by this instance");
                    }

                    if (_cancelResult.Consumed)
                    {
                        throw new SplicerException("EndCancel has already been called fo this async result");
                    }

                    _cancelResult.AsyncWaitHandle.WaitOne();

                    _cancelResult.Consume();

                    if (_cancelResult.Exception != null)
                    {
                        throw new SplicerException(
                            "Exception occured while attempting to cancel render request, see inner exception for details",
                            _cancelResult.Exception);
                    }

                    _cancelResult = null;
                }
                else
                {
                    throw new SplicerException("You must call BeginCancel before EndCancel");
                }
            }
        }

        public void EndRender(IAsyncResult result)
        {
            lock (_renderLock)
            {
                if (_renderResult != null)
                {
                    if (_renderResult != result)
                    {
                        throw new SplicerException("The supplied async result was not issued by this instance");
                    }

                    if (_renderResult.Consumed)
                    {
                        throw new SplicerException("EndRender has already been called for this async result");
                    }

                    _renderResult.AsyncWaitHandle.WaitOne();

                    _renderResult.Consume();

                    if (_renderResult.Exception != null)
                    {
                        throw new SplicerException(
                            "Exception occured while attempting to render, see inner exception for details",
                            _cancelResult.Exception);
                    }
                    else if (_renderResult.Cancelled)
                    {
                        throw new SplicerException("The render request was cancelled by the user");
                    }
                }
                else
                {
                    throw new SplicerException("You must call BeginCancel before EndCancel");
                }
            }
        }

        /// <summary>
        /// Called on a new thread to process events from the graph.  The thread
        /// exits when the graph finishes.  Cancelling is done here.
        /// </summary>
        private void EventWait()
        {
            try
            {
                // Returned when GetEvent is called but there are no events
                const int E_ABORT = unchecked((int) 0x80004004);

                int hr;
                int p1, p2;
                EventCode ec;
                EventCode exitCode = 0;

                IMediaEvent pEvent = (IMediaEvent) _graph;

                do
                {
                    // Read the event
                    for (
                        hr = pEvent.GetEvent(out ec, out p1, out p2, 100);
                        hr >= 0 && _state < RendererState.GraphCompleted;
                        hr = pEvent.GetEvent(out ec, out p1, out p2, 100)
                        )
                    {
                        switch (ec)
                        {
                                // If the clip is finished playing
                            case EventCode.Complete:
                            case EventCode.ErrorAbort:
                                ChangeState(RendererState.GraphStarted, RendererState.GraphCompleting);
                                exitCode = ec;

                                // Release any resources the message allocated
                                hr = pEvent.FreeEventParams(ec, p1, p2);
                                DESError.ThrowExceptionForHR(hr);
                                break;

                            default:
                                // Release any resources the message allocated
                                hr = pEvent.FreeEventParams(ec, p1, p2);
                                DESError.ThrowExceptionForHR(hr);
                                break;
                        }
                    }

                    // If the error that exited the loop wasn't due to running out of events
                    if (hr != E_ABORT)
                    {
                        DESError.ThrowExceptionForHR(hr);
                    }
                } while (_state < RendererState.GraphCompleting);

                // If the user cancelled
                if (_state == RendererState.Cancelling)
                {
                    // Stop the graph, send an appropriate exit code
                    hr = _mediaControl.Stop();
                    exitCode = EventCode.UserAbort;
                }

                if (_state == RendererState.GraphCompleting)
                {
                    ChangeState(RendererState.GraphCompleted);
                    OnRenderCompleted();
                    if (_renderResult != null) _renderResult.Complete(false);
                    if (_cancelResult != null) _cancelResult.Complete(false);
                }
                else
                {
                    ChangeState(RendererState.Cancelled);
                    OnRenderCompleted();
                    if (_renderResult != null) _renderResult.Complete(true);
                    if (_cancelResult != null) _cancelResult.Complete(false);
                }
            }
            catch (Exception ex)
            {
                if (_renderResult != null) _renderResult.Complete(ex);
                if (_cancelResult != null) _cancelResult.Complete(ex);
            }
        }

        protected void ChangeState(RendererState newState)
        {
            lock (_stateLock)
            {
                _state = newState;
            }
        }

        protected void ChangeState(RendererState expectedState, RendererState newState)
        {
            lock (_stateLock)
            {
                if (_state == expectedState)
                {
                    _state = newState;
                }
            }
        }
    }
}