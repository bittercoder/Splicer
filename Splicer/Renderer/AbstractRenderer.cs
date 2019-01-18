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
using System.Threading;
using System.Xml;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Properties;
using Splicer.Timeline;
using Splicer.Utilities;

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
        Canceled
    }

    public abstract class AbstractRenderer : IRenderer
    {
        private const string DestinationParameterName = "destination";
        private const string GraphBuilderParameterName = "graphBuilder";
        private const string PinParameterName = "pin";
        private const string TimelineParameterName = "timeline";

        /// <summary>
        /// The first audio group, or null if no audio
        /// </summary>
        private readonly IGroup _firstAudioGroup;

        /// <summary>
        /// the first video group, or null if no video
        /// </summary>
        private readonly IGroup _firstVideoGroup;

        private readonly object _renderLock = new object();

        /// <summary>
        /// Object we lock against to avoid cross-thread problems with setting state.
        /// </summary>
        private readonly object _stateLock = new object();

        private RendererAsyncResult _cancelResult;

        /// <summary>
        /// Handles cleanup of objects we aren't retaining references to
        /// </summary>
        private DisposalCleanup _cleanup = new DisposalCleanup();

        /// <summary>
        /// IGraphBuilder object for the timeline
        /// </summary>
        private IGraphBuilder _graph;

        /// <summary>
        /// Media control interface from _graph
        /// </summary>
        private IMediaControl _mediaControl;

        /// <summary>
        /// The engine to process the timeline (can't be released
        /// until the graph processing is complete)
        /// </summary>
        private IRenderEngine _renderEngine;

        private RendererAsyncResult _renderResult;

        /// <summary>
        /// the state of the renderer
        /// </summary>
        private RendererState _state = RendererState.Initializing;

        /// <summary>
        /// The timeline this class will render into a graph
        /// </summary>
        private ITimeline _timeline;

        protected AbstractRenderer(ITimeline timeline)
        {
            if (timeline == null) throw new ArgumentNullException(TimelineParameterName);

            _timeline = timeline;

            int hr = 0;

            // create the render engine
            _renderEngine = (IRenderEngine) new RenderEngine();
            _cleanup.Add(_renderEngine);

            // tell the render engine about the timeline it should use
            hr = _renderEngine.SetTimelineObject(_timeline.DesTimeline);
            DESError.ThrowExceptionForHR(hr);

            // connect up the front end
            hr = _renderEngine.ConnectFrontEnd();
            DESError.ThrowExceptionForHR(hr);

            // Get the filtergraph - used all over the place
            hr = _renderEngine.GetFilterGraph(out _graph);
            _cleanup.Add(Graph);
            DESError.ThrowExceptionForHR(hr);

            // find the first (and usually last) audio and video group, we use these
            // when rendering to track progress
            _firstAudioGroup = _timeline.FindFirstGroupOfType(GroupType.Audio);
            _firstVideoGroup = _timeline.FindFirstGroupOfType(GroupType.Video);
        }

        protected IRenderEngine RenderEngine
        {
            get { return _renderEngine; }
        }

        protected IGroup FirstVideoGroup
        {
            get { return _firstVideoGroup; }
        }

        protected IGroup FirstAudioGroup
        {
            get { return _firstAudioGroup; }
        }

        protected DisposalCleanup Cleanup
        {
            get { return _cleanup; }
        }

        protected ITimeline Timeline
        {
            get { return _timeline; }
        }

        /// <summary>
        /// IGraphBuilder object for the timeline
        /// </summary>
        protected IGraphBuilder Graph
        {
            get { return _graph; }
        }

        #region IRenderer Members

        public RendererState State
        {
            get { return _state; }
        }

        public event EventHandler RenderCompleted
        {
            add { _renderCompleted += value; }
            remove { _renderCompleted -= value; }
        }

        /// <summary>
        /// Returns an XML description of the capture graph (as seen by DES).
        /// </summary>
        /// <remarks>
        /// Might be useful for debugging, handy for implementing some unit tests (where you 
        /// want to make sure changes to implementation don't alter the expected DES capture graph).
        /// </remarks>
        /// <returns>String containing XML</returns>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
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

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void SaveToGraphFile(string fileName)
        {
            FilterGraphTools.SaveGraphFile(Graph, fileName);
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
                    throw new SplicerException(Resources.ErrorPreviousRenderRequestInProgress);
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

        public IAsyncResult BeginCancel(AsyncCallback callback, object state)
        {
            lock (_renderLock)
            {
                if ((_cancelResult != null) && (!_cancelResult.Consumed))
                {
                    throw new SplicerException(Resources.ErrorAttemptToCancelWhenCancelInProgress);
                }
                else if (_state < RendererState.GraphStarted)
                {
                    throw new SplicerException(Resources.ErrorGraphNotYetStarted);
                }
                else if (_state >= RendererState.GraphCompleting)
                {
                    throw new SplicerException(Resources.ErrorAttemptToCancelWhenCancelingOrCompleting);
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
                        throw new SplicerException(Resources.ErrorAsyncResultNotIssuesByThisInstance);
                    }

                    if (_cancelResult.Consumed)
                    {
                        throw new SplicerException(Resources.ErrorEndCancelAlreadyCalledForAsyncResult);
                    }

                    _cancelResult.AsyncWaitHandle.WaitOne();

                    _cancelResult.Consume();

                    if (_cancelResult.Exception != null)
                    {
                        throw new SplicerException(Resources.ErrorExceptionOccuredDuringCancelRenderRequest,
                                                   _cancelResult.Exception);
                    }

                    _cancelResult = null;
                }
                else
                {
                    throw new SplicerException(Resources.ErrorMustCallBeginCancelBeforeEndCancel);
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
                        throw new SplicerException(Resources.ErrorAsyncResultNotIssuesByThisInstance);
                    }

                    if (_renderResult.Consumed)
                    {
                        throw new SplicerException(Resources.ErrorEndRenderAlreadyCalledForAsyncResult);
                    }

                    _renderResult.AsyncWaitHandle.WaitOne();

                    _renderResult.Consume();

                    if (_renderResult.Exception != null)
                    {
                        throw new SplicerException(Resources.ErrorExceptionOccuredDuringRenderRequest,
                                                   _cancelResult.Exception);
                    }
                    else if (_renderResult.Canceled)
                    {
                        throw new SplicerException(Resources.ErrorRenderRequestCanceledByUser);
                    }
                }
                else
                {
                    throw new SplicerException(Resources.ErrorMustCallBeginRenderBeforeEndRender);
                }
            }
        }

        #endregion

        private event EventHandler _renderCompleted;

        protected void OnRenderCompleted()
        {
            if (_renderCompleted != null)
            {
                _renderCompleted(this, EventArgs.Empty);
            }
        }

        private static string NormalizeXml(string input)
        {
            input = input.Substring(input.IndexOf('<')).Substring(0, input.LastIndexOf('>'));
            var doc = new XmlDocument();
            doc.LoadXml(input);
            return doc.OuterXml;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected virtual void DisposeRenderer(bool disposing)
        {
            if (disposing)
            {
                if (_cleanup != null)
                {
                    (_cleanup).Dispose();
                    _cleanup = null;
                }

                if (_timeline != null)
                {
                    _timeline.Dispose();
                    _timeline = null;
                }
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

            if (Graph != null)
            {
                Marshal.ReleaseComObject(Graph);
                _graph = null;
            }
        }

        /// <summary>
        /// Common routine used by RenderTo*  
        /// </summary>
        /// <param name="graphBuilder">ICaptureGraphBuilder2 to use</param>
        /// <param name="callback">Callback to use (or null)</param>
        /// <param name="typeName">string to use in creating filter graph object descriptions</param>
        /// <param name="pin">Pin to connect from</param>
        /// <param name="compressor">Compressor to use, or null for none</param>
        /// <param name="destination">Endpoint (renderer or file writer) to connect to</param>        
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected void RenderHelper(ICaptureGraphBuilder2 graphBuilder, ISampleGrabberCB callback, string typeName,
                                    IPin pin,
                                    IBaseFilter compressor, IBaseFilter destination)
        {
            if (graphBuilder == null) throw new ArgumentNullException(GraphBuilderParameterName);
            if (pin == null) throw new ArgumentNullException(PinParameterName);
            if (destination == null) throw new ArgumentNullException(DestinationParameterName);

            int hr;
            IBaseFilter ibfSampleGrabber = null;

            try
            {
                // If no callback was provided, don't create a samplegrabber
                if (callback != null)
                {
                    var isg = (ISampleGrabber) new SampleGrabber();
                    ibfSampleGrabber = (IBaseFilter) isg;
                    _cleanup.Add(ibfSampleGrabber);

                    hr = isg.SetCallback(callback, 1);
                    DESError.ThrowExceptionForHR(hr);

                    hr = Graph.AddFilter(ibfSampleGrabber, typeName + " sample grabber");
                    DESError.ThrowExceptionForHR(hr);
                }

                // If a compressor was provided, add it to the graph and connect it up
                if (compressor != null)
                {
                    // Connect the pin.
                    hr = Graph.AddFilter(compressor, typeName + " Compressor");
                    DESError.ThrowExceptionForHR(hr);

                    FilterGraphTools.ConnectFilters(Graph, pin, ibfSampleGrabber, true);

                    FilterGraphTools.ConnectFilters(Graph, ibfSampleGrabber, compressor, true);

                    FilterGraphTools.ConnectFilters(Graph, compressor, destination, true);
                }
                else
                {
                    // Just connect the SampleGrabber (if any)
                    hr = graphBuilder.RenderStream(null, null, pin, ibfSampleGrabber, destination);
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
        /// <param name="graphBuilder">ICaptureGraphBuilder2 to use</param>
        /// <param name="callback">ICaptureGraphBuilder2 to use</param>
        /// <param name="typeName">String to use in creating filter graph object descriptions</param>
        /// <param name="pin">Pin to connect from</param>
        /// <param name="renderer">Renderer to add</param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected void RenderWindowHelper(ICaptureGraphBuilder2 graphBuilder, ISampleGrabberCB callback, string typeName,
                                          IPin pin,
                                          IBaseFilter renderer)
        {
            int hr;

            // Add the renderer to the graph
            hr = Graph.AddFilter(renderer, typeName + " Renderer");
            _cleanup.Add(renderer);
            DESError.ThrowExceptionForHR(hr);

            // Do everything else
            RenderHelper(graphBuilder, callback, typeName, pin, null, renderer);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected void RenderGroups(ICaptureGraphBuilder2 graphBuilder, IBaseFilter audioCompressor,
                                    IBaseFilter videoCompressor,
                                    IBaseFilter multiplexer, ICallbackParticipant[] audioParticipants,
                                    ICallbackParticipant[] videoParticipants)
        {
            RenderGroups(graphBuilder, audioCompressor, videoCompressor, multiplexer, multiplexer, audioParticipants,
                         videoParticipants);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected void RenderGroups(ICaptureGraphBuilder2 graphBuilder, IBaseFilter audioCompressor,
                                    IBaseFilter videoCompressor,
                                    IBaseFilter audioDestination, IBaseFilter videoDestination,
                                    ICallbackParticipant[] audioParticipants,
                                    ICallbackParticipant[] videoParticipants)
        {
            int hr = 0;

            if (audioCompressor != null) _cleanup.Add(audioCompressor);
            if (videoCompressor != null) _cleanup.Add(videoCompressor);
            if (audioDestination != null) _cleanup.Add(audioDestination);
            if ((videoDestination != null) && (audioDestination != videoDestination)) _cleanup.Add(videoDestination);

            IAMTimeline desTimeline = _timeline.DesTimeline;

            int groupCount;
            hr = desTimeline.GetGroupCount(out groupCount);
            DESError.ThrowExceptionForHR(hr);

            // Walk the groups.  For this class, there is one group that 
            // contains all the video, and a second group for the audio.
            for (int i = (groupCount - 1); i >= 0; i--)
            {
                IAMTimelineObj group;

                hr = desTimeline.GetGroup(out group, i);
                DESError.ThrowExceptionForHR(hr);

                try
                {
                    // Inform the graph we will be writing to disk (rather than previewing)
                    var timelineGroup = (IAMTimelineGroup) group;
                    hr = timelineGroup.SetPreviewMode(false);
                    DESError.ThrowExceptionForHR(hr);
                }
                finally
                {
                    Marshal.ReleaseComObject(group);
                }

                IPin pPin;

                // Get the IPin for the current group
                hr = _renderEngine.GetGroupOutputPin(i, out pPin);
                _cleanup.Add(pPin);
                DESError.ThrowExceptionForHR(hr);

                try
                {
                    if (FilterGraphTools.IsVideo(pPin))
                    {
                        // Create a sample grabber, add it to the graph and connect it all up
                        var mcb =
                            new CallbackHandler(videoParticipants);
                        RenderHelper(graphBuilder, mcb, "Video", pPin, videoCompressor, videoDestination);
                    }
                    else
                    {
                        // Create a sample grabber, add it to the graph and connect it all up
                        var mcb =
                            new CallbackHandler(audioParticipants);
                        RenderHelper(graphBuilder, mcb, "Audio", pPin, audioCompressor, audioDestination);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(pPin);
                }
            }
        }

        private void StartRender()
        {
            int hr;

            _mediaControl = (IMediaControl) Graph;
            _cleanup.Add(_mediaControl);

            var eventThread = new Thread(EventWait);
            eventThread.Name = Resources.MediaEventThreadName;
            eventThread.Start();

            hr = _mediaControl.Run();
            DESError.ThrowExceptionForHR(hr);
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
                IntPtr p1, p2;
                EventCode ec;
                // TODO: present the event code in the OnCompleted event
                EventCode exitCode;
                var pEvent = (IMediaEvent) Graph;

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

                // If the user canceled
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
                    ChangeState(RendererState.Canceled);
                    OnRenderCompleted();
                    if (_renderResult != null) _renderResult.Complete(true);
                    if (_cancelResult != null) _cancelResult.Complete(false);
                }
            }
            catch (COMException ex)
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

        protected void DisableClock()
        {
            int hr = ((IMediaFilter) Graph).SetSyncSource(null);
            DsError.ThrowExceptionForHR(hr);
        }
    }
}