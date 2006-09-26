using System;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class AudioVideoClipPairFixture
    {
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullAudioClip()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IClip videoClip = timeline.AddVideoGroup(24, 100, 100).AddTrack().AddVideo("1sec.wmv");
                AudioVideoClipPair pair = new AudioVideoClipPair(null, videoClip);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullVideoClip()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IClip audioClip = timeline.AddAudioGroup().AddTrack().AddAudio("1sec.wav");
                AudioVideoClipPair pair = new AudioVideoClipPair(audioClip, null);
            }
        }
    }
}