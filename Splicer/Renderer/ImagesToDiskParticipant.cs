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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using Splicer.Properties;

namespace Splicer.Renderer
{
    /// <summary>
    /// This will trigger on certain time thresholds, and save an image to disk each time.
    /// 
    /// Demonstrates one way of building a frame grabbing participant.
    /// </summary>
    public class ImagesToDiskParticipant : AbstractSampleGrabbingParticipant
    {
        private const string BitmapParameter = "bitmap";
        private readonly string _path;
        private readonly Queue<double> _thresholds;

        public ImagesToDiskParticipant(short bitCount, int width, int height, string path, params double[] thresholds)
            : base(bitCount, width, height, true)
        {
            _path = path;
            Array.Sort(thresholds);
            _thresholds = new Queue<double>(thresholds);
        }

        public override bool TakeSnapshot(double sampleTime)
        {
            if ((_thresholds.Count > 0) && (_thresholds.Peek() < sampleTime))
            {
                _thresholds.Dequeue();
                return true;
            }

            return false;
        }

        public override void ConsumeImage(double sampleTime, int count, Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(BitmapParameter);
            using (bitmap)
            {
                string fileName =
                    Path.Combine(_path,
                                 string.Format(CultureInfo.InvariantCulture,
                                               Resources.ImagesToDiskParticipantFilenameTemplate, count));
                bitmap.Save(fileName, ImageFormat.Jpeg);
            }
        }
    }
}