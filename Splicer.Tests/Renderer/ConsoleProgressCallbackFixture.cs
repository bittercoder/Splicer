using NUnit.Framework;
using Splicer.Timeline;

namespace Splicer.Renderer.Tests
{
    [TestFixture]
    public class ConsoleProgressCallbackFixture : AbstractFixture
    {
        [Test]
        public void UseInRender()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                using (NullRenderer renderer = new NullRenderer(timeline, new ConsoleProgressCallback(), null))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""testinput.wav"" mstart=""0""/>
        </track>
    </group>
</timeline>");
                }
            }
        }
    }
}