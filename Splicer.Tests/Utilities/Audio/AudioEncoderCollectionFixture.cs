using System;
using System.Collections.Specialized;
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Splicer.Utilities.Audio.Tests
{
    [TestClass]
    public class AudioEncoderCollectionFixture
    {
        [TestMethod]
        public void ListAllEncoders()
        {
            var encoders = new AudioEncoderCollection();
            Assert.IsTrue(encoders.Count > 1);

            var names = new StringCollection();
            foreach (AudioEncoder encoder in encoders)
            {
                Assert.IsFalse(string.IsNullOrEmpty(encoder.FriendlyName));
                Assert.IsFalse(names.Contains(encoder.FriendlyName));
                names.Add(encoder.FriendlyName);
            }

            // if you want to dump a list out, a better example 
            foreach (AudioEncoder encoder in encoders)
            {
                Console.WriteLine(string.Format("encoder: {0}\r\n------------------------------\r\n",
                                                encoder.FriendlyName));
                foreach (WavFormatInfo info in encoder.Formats)
                {
                    Console.WriteLine(info.ToString());
                }
            }
        }
    }
}