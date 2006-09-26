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

namespace Splicer.Renderer
{
    public class WindowsMediaRenderer : AbstractRenderer
    {
        private const string AudioInputPinName = "Audio Input";
        private const string VideoInputPinName = "Video Input";

        public WindowsMediaRenderer(ITimeline timeline, string file, string profileData)
            : this(timeline, file, profileData, null, null)
        {
        }

        public WindowsMediaRenderer(ITimeline timeline, string file, string profileData, IDESCombineCB pVideoCallback,
                                    IDESCombineCB pAudioCallback)
            : base(timeline)
        {
            RenderToAsfWriter(file, profileData, pVideoCallback, pAudioCallback);

            ChangeState(RendererState.Initialized);
        }

        private void ValidateAsfWriterIsSuitable(IBaseFilter asfWriterFilter)
        {
            foreach (PinQueryInfo info in FilterGraphTools.EnumeratePins(asfWriterFilter))
            {
                if (info.Name.StartsWith(AudioInputPinName))
                {
                    if (!_timeline.Groups.Exists(delegate(IGroup group)
                        {
                            return group.Type == GroupType.Audio;
                        }))
                    {
                        throw new SplicerException(
                            "The selected windows media profile encodes audio information, yet no audio group exists");
                    }
                }
                else if (info.Name.StartsWith(VideoInputPinName))
                {
                    if (!_timeline.Groups.Exists(delegate(IGroup group)
                        {
                            return group.Type == GroupType.Video;
                        }))
                    {
                        throw new SplicerException(
                            "The selected windows media profile encodes video information, yet no video group exists");
                    }
                }
            }
        }

        protected void RenderToAsfWriter(
            string file,
            string profileData,
            IDESCombineCB pVideoCallback,
            IDESCombineCB pAudioCallback)
        {
            int hr;

            if (file == null)
            {
                throw new SplicerException("Output file name cannot be null");
            }

            // Contains useful routines for creating the graph
            ICaptureGraphBuilder2 icgb = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

            try
            {
                hr = icgb.SetFiltergraph(_graph);
                DESError.ThrowExceptionForHR(hr);

                IBaseFilter pMux = StandardFilters.RenderAsfWriterWithProfile(_dc, _graph, profileData, file);

                ValidateAsfWriterIsSuitable(pMux);

                _dc.Add(pMux);

                try
                {
                    RenderGroups(icgb, null, null, pMux, pAudioCallback, pVideoCallback);
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