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
    public class WindowsMediaRenderer : AbstractRenderer, IDisposable
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WindowsMediaRenderer(ITimeline timeline, string file, string profileData)
            : this(timeline, file, profileData, null, null)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WindowsMediaRenderer(ITimeline timeline, string file, string profileData,
                                    ICallbackParticipant[] videoParticipants,
                                    ICallbackParticipant[] audioParticipants)
            : base(timeline)
        {
            RenderToAsfWriter(file, profileData, videoParticipants, audioParticipants);

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

        private void ValidateAsfWriterIsSuitable(IBaseFilter asfWriterFilter)
        {
            foreach (PinQueryInfo info in FilterGraphTools.EnumeratePins(asfWriterFilter))
            {
                if (info.Name.StartsWith(Resources.AudioInputPinNamePrefix))
                {
                    if (!Timeline.Groups.Exists(delegate(IGroup group) { return group.Type == GroupType.Audio; }))
                    {
                        throw new SplicerException(Resources.ErrorWMProfileRequiresAudioGroup);
                    }
                }
                else if (info.Name.StartsWith(Resources.VideoInputPinNamePrefix))
                {
                    if (!Timeline.Groups.Exists(delegate(IGroup group) { return group.Type == GroupType.Video; }))
                    {
                        throw new SplicerException(Resources.ErrorWMProfileRequiresVideoGroup);
                    }
                }
            }
        }

        private void RenderToAsfWriter(
            string file,
            string profileData,
            ICallbackParticipant[] videoParticipants,
            ICallbackParticipant[] audioParticipants)
        {
            int hr;

            if (file == null)
            {
                throw new SplicerException(Resources.ErrorInvalidOutputFileName);
            }

            // Contains useful routines for creating the graph
            var graphBuilder = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = graphBuilder.SetFiltergraph(Graph);
                DESError.ThrowExceptionForHR(hr);

                IBaseFilter pMux = StandardFilters.RenderAsfWriterWithProfile(Cleanup, Graph, profileData, file);

                ValidateAsfWriterIsSuitable(pMux);

                Cleanup.Add(pMux);

                try
                {
                    RenderGroups(graphBuilder, null, null, pMux, audioParticipants, videoParticipants);
                }
                finally
                {
                    Marshal.ReleaseComObject(pMux);
                }

                DisableClock();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                Marshal.ReleaseComObject(graphBuilder);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~WindowsMediaRenderer()
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