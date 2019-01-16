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

namespace Splicer.Renderer
{
    public class AviFileRenderer : AbstractRenderer, IDisposable
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public AviFileRenderer(ITimeline timeline, string outputFile)
            : this(timeline, outputFile, null, null, null, null)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public AviFileRenderer(ITimeline timeline, string outputFile, IBaseFilter videoCompressor,
                               IBaseFilter audioCompressor, ICallbackParticipant[] videoParticipants,
                               ICallbackParticipant[] audioParticipants)
            : base(timeline)
        {
            RenderToAVI(outputFile, videoCompressor, audioCompressor, videoParticipants, audioParticipants);

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

        /// <summary>
        /// Renders to AVI.
        /// </summary>
        /// <param name="outputFile">The output file.</param>
        /// <param name="videoCompressor">The video compressor.</param>
        /// <param name="audioCompressor">The audio compressor.</param>
        /// <param name="videoParticipants">The video participants.</param>
        /// <param name="audioParticipants">The audio participants.</param>
        private void RenderToAVI(
            string outputFile,
            IBaseFilter videoCompressor,
            IBaseFilter audioCompressor,
            ICallbackParticipant[] videoParticipants,
            ICallbackParticipant[] audioParticipants)
        {
            if (string.IsNullOrEmpty(outputFile)) throw new ArgumentNullException("outputFile");
            if (FirstVideoGroup == null)
                throw new SplicerException(Resources.ErrorCanNotRenderAviWhenNoVideoGroupExists);

            int hr;

            if (outputFile == null)
            {
                throw new SplicerException(Resources.ErrorInvalidOutputFileName);
            }

            // Contains useful routines for creating the graph
            var graphBuilder = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = graphBuilder.SetFiltergraph(Graph);
                DESError.ThrowExceptionForHR(hr);

                // Create the file writer
                IBaseFilter multiplexer = StandardFilters.RenderAviDestination(Cleanup, graphBuilder, outputFile);

                try
                {
                    RenderGroups(graphBuilder, audioCompressor, videoCompressor, multiplexer, audioParticipants,
                                 videoParticipants);
                }
                finally
                {
                    Marshal.ReleaseComObject(multiplexer);
                }

                DisableClock();
            }
            finally
            {
                Marshal.ReleaseComObject(graphBuilder);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~AviFileRenderer()
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