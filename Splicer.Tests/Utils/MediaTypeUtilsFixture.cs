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

using DirectShowLib;
using NUnit.Framework;

namespace Splicer.Utils.Tests
{
    [TestFixture]
    public class MediaTypeUtilsFixture
    {
        [Test]
        public void GetAudioMediaType()
        {
            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeUtils.GetAudioMediaType();
                Assert.IsNotNull(mediaType);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }
        }

        [Test]
        public void GetVideoMediaType()
        {
            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeUtils.GetVideoMediaType(16, 320, 200);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }

            try
            {
                mediaType = MediaTypeUtils.GetVideoMediaType(24, 172, 160);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }

            try
            {
                mediaType = MediaTypeUtils.GetVideoMediaType(32, 172, 160);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }
        }
    }
}