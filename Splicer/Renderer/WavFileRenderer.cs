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
using Splicer.Utilities.Audio;

namespace Splicer.Renderer
{
    public class WavFileRenderer : AbstractRenderer, IDisposable
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WavFileRenderer(ITimeline timeline, string outputFile)
            : this(timeline, outputFile, null, null, null)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WavFileRenderer(ITimeline timeline, string outputFile, AudioFormat format,
                               ICallbackParticipant[] audioParticipants)
            : base(timeline)
        {
            AudioCompressor compressor = null;

            try
            {
                compressor = AudioCompressorFactory.Create(format);

                Cleanup.Add(compressor.Filter);

                RenderToWavDest(outputFile, compressor.Filter, compressor.MediaType, audioParticipants);

                ChangeState(RendererState.Initialized);
            }
            finally
            {
                if ((compressor != null) && (compressor.MediaType != null))
                {
                    DsUtils.FreeAMMediaType(compressor.MediaType);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WavFileRenderer(ITimeline timeline, string outputFile, IBaseFilter audioCompressor, AMMediaType mediaType,
                               ICallbackParticipant[] audioParticipants)
            : base(timeline)
        {
            RenderToWavDest(outputFile, audioCompressor, mediaType, audioParticipants);

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

        private void RenderToWavDest(
            string outputFile,
            IBaseFilter audioCompressor,
            AMMediaType mediaType,
            ICallbackParticipant[] audioParticipants)
        {
            if (audioCompressor != null) Cleanup.Add(audioCompressor);

            int hr;

            if (FirstAudioGroup == null)
            {
                throw new SplicerException(Resources.ErrorNoAudioStreamToRender);
            }

            if (outputFile == null)
            {
                throw new SplicerException(Resources.ErrorInvalidOutputFileName);
            }

            // Contains useful routines for creating the graph
            var graphBuilder = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();
            Cleanup.Add(graphBuilder);

            try
            {
                hr = graphBuilder.SetFiltergraph(Graph);
                DESError.ThrowExceptionForHR(hr);

                IBaseFilter wavDestFilter = StandardFilters.RenderWavDestination(Cleanup, Graph);
                IBaseFilter fileSink = StandardFilters.RenderFileDestination(Cleanup, Graph, outputFile);

                try
                {
                    RenderGroups(graphBuilder, audioCompressor, null, wavDestFilter, audioParticipants, null);

                    FilterGraphTools.ConnectFilters(Graph, wavDestFilter, fileSink, true);

                    // if supplied, apply the media type to the filter
                    if (mediaType != null)
                    {
                        FilterGraphTools.SetFilterFormat(mediaType, audioCompressor);
                    }

                    DisableClock();
                }
                finally
                {
                    if (wavDestFilter != null) Marshal.ReleaseComObject(wavDestFilter);
                    if (fileSink != null) Marshal.ReleaseComObject(fileSink);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(graphBuilder);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~WavFileRenderer()
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