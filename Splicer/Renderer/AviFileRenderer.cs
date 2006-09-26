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
    public class AviFileRenderer : AbstractRenderer
    {
        public AviFileRenderer(ITimeline timeline, string outputFile)
            : this(timeline, outputFile, null, null, null, null)
        {
        }

        public AviFileRenderer(ITimeline timeline, string outputFile, IBaseFilter videoCompressor,
                               IBaseFilter audioCompressor, IDESCombineCB pVideoCallback,
                               IDESCombineCB pAudioCallback)
            : base(timeline)
        {
            RenderToAVI(outputFile, videoCompressor, audioCompressor, pVideoCallback, pAudioCallback);

            ChangeState(RendererState.Initialized);
        }

        private void RenderToAVI(
            string sOutputFile,
            IBaseFilter ibfVideoCompressor,
            IBaseFilter ibfAudioCompressor,
            IDESCombineCB pVideoCallback,
            IDESCombineCB pAudioCallback)
        {
            if (_firstVideoGroup == null)
            {
                throw new SplicerException("Can not render to AVI when no video group exists");
            }

            int hr;

            if (sOutputFile == null)
            {
                throw new SplicerException("Output file name cannot be null");
            }

            // Contains useful routines for creating the graph
            ICaptureGraphBuilder2 icgb = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = icgb.SetFiltergraph(_graph);
                DESError.ThrowExceptionForHR(hr);

                // Create the file writer
                IBaseFilter pMux = StandardFilters.RenderAviDest(_dc, icgb, sOutputFile);

                try
                {
                    RenderGroups(icgb, ibfAudioCompressor, ibfVideoCompressor, pMux, pAudioCallback, pVideoCallback);
                }
                finally
                {
                    Marshal.ReleaseComObject(pMux);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(icgb);
            }
        }
    }
}