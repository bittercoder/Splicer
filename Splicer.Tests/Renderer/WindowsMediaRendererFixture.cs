using System.IO;
using NUnit.Framework;
using Splicer.Timeline;
using Splicer.WindowsMedia;

namespace Splicer.Renderer.Tests
{
    [TestFixture]
    public class WindowsMediaRendererFixture : AbstractFixture
    {
        [Test]
        [
            ExpectedException(typeof (SplicerException),
                "The selected windows media profile encodes video information, yet no video group exists")]
        public void RenderWithInapropriateProfile1()
        {
            string outputFile = "RenderWithInapropriateProfile2.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 2);

                using (
                    WindowsMediaRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.LowQualityVideo))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
	<group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""2"" src=""testinput.mp3"" mstart=""0"" />
		</track>
	</group>
</timeline>");
                }
            }
        }

        [Test]
        [
            ExpectedException(typeof (SplicerException),
                "The selected windows media profile encodes audio information, yet no audio group exists")]
        public void RenderWithInapropriateProfile2()
        {
            string outputFile = "RenderWithInapropriateProfile2.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 100, 100);
                ITrack rootTrack = videoGroup.AddTrack();
                rootTrack.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, 2);

                using (
                    WindowsMediaRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.LowQualityVideo))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
	<group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""2"" src=""testinput.mp3"" mstart=""0"" />
		</track>
	</group>
</timeline>");
                }
            }
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Output file name cannot be null")]
        public void ConvertWithNullFilename()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 2);

                using (
                    WindowsMediaRenderer renderer =
                        new WindowsMediaRenderer(timeline, null, WindowsMediaProfiles.LowQualityAudio)) ;
            }
        }

        [Test]
        public void ConvertMp3ToWMA()
        {
            string outputFile = "ConvertMp3ToWMA.wma";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 2);

                using (
                    WindowsMediaRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.LowQualityAudio))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""testinput.mp3"" mstart=""0""/>
        </track>
    </group>
</timeline>");
                }

                Assert.IsTrue(File.Exists(outputFile));
                AssertLengths(timeline, 2, outputFile);
            }
        }

        [Test]
        public void ConvertAviToWMV()
        {
            string outputFile = "ConvertAviToWMV.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(0x20, 320, 240);
                ITrack videoTrack = videoGroup.AddTrack();
                IClip clockClip =
                    videoTrack.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, 2);

                Assert.IsTrue(clockClip.Duration > 0);

                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 2);

                using (
                    WindowsMediaRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""video"" bitdepth=""32"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""clock.avi"" mstart=""0"" />
        </track>
    </group>
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""testinput.mp3"" mstart=""0"" />
        </track>
    </group>
</timeline>");
                }

                Assert.IsTrue(File.Exists(outputFile));
                AssertLengths(timeline, 2, outputFile);
            }
        }
    }
}