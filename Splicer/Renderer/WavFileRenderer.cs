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
using Splicer.Utils;
using Splicer.Utils.Audio;

namespace Splicer.Renderer
{
    public class WavFileRenderer : AbstractRenderer
    {
        public WavFileRenderer(ITimeline timeline, string outputFile)
            : this(timeline, outputFile, null, null, null)
        {
        }

        public WavFileRenderer(ITimeline timeline, string outputFile, AudioFormat format, IDESCombineCB audioCallback)
            : base(timeline)
        {
            AudioCompressor compressor = null;

            try
            {
                compressor = AudioCompressorFactory.Create(format);

                _dc.Add(compressor.Filter);

                RenderToWavDest(outputFile, compressor.Filter, compressor.MediaType, audioCallback);

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

        public WavFileRenderer(ITimeline timeline, string outputFile, IBaseFilter audioCompressor, AMMediaType mediaType,
                               IDESCombineCB audioCallback)
            : base(timeline)
        {
            RenderToWavDest(outputFile, audioCompressor, mediaType, audioCallback);

            ChangeState(RendererState.Initialized);
        }


        public void RenderToWavDest(
            string outputFile,
            IBaseFilter audioCompressor,
            AMMediaType mediaType,
            IDESCombineCB audioCallback)
        {
            if (audioCompressor != null) _dc.Add(audioCompressor);

            int hr;

            if (_firstAudioGroup == null)
            {
                throw new SplicerException("No audio stream to render");
            }

            if (outputFile == null)
            {
                throw new SplicerException("Output file name cannot be null");
            }

            // Contains useful routines for creating the graph
            ICaptureGraphBuilder2 icgb = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();
            _dc.Add(icgb);

            try
            {
                hr = icgb.SetFiltergraph(_graph);
                DESError.ThrowExceptionForHR(hr);

                IBaseFilter wavDestFilter = StandardFilters.RenderWavDest(_dc, _graph);
                IBaseFilter fileSink = StandardFilters.RenderFileDestination(_dc, _graph, outputFile);

                try
                {
                    RenderGroups(icgb, audioCompressor, null, wavDestFilter, audioCallback, null);

                    FilterGraphTools.ConnectFilters(_graph, wavDestFilter, fileSink, true);

                    // if supplied, apply the media type to the filter
                    if (mediaType != null)
                    {
                        FilterGraphTools.SetFilterFormat(mediaType, audioCompressor);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(wavDestFilter);
                    Marshal.ReleaseComObject(fileSink);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(icgb);
            }
        }
    }
}