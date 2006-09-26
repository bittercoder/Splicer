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