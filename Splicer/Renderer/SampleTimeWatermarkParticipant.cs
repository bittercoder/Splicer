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

using System.Drawing;
using System.Globalization;
using Splicer.Properties;

namespace Splicer.Renderer
{
    /// <summary>
    /// Renders the time within the sample to the frames of the video.
    /// </summary>
    public class SampleTimeWatermarkParticipant : AbstractWatermarkParticipant
    {
        private const string ArialFont = "Arial";

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleTimeWatermarkParticipant"/> class.
        /// </summary>
        /// <param name="bitCount">The bit count.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="flipImages">if set to <c>true</c> [flip images].</param>
        public SampleTimeWatermarkParticipant(short bitCount, int width, int height, bool flipImages)
            : base(bitCount, width, height, flipImages)
        {
        }

        /// <summary>
        /// Updates the image.
        /// </summary>
        /// <param name="sampleTime">The sample time.</param>
        /// <param name="bitmap">The bitmap.</param>
        public override void UpdateImage(double sampleTime, Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                Font font = new Font(ArialFont, 16);
                string text = string.Format(CultureInfo.CurrentUICulture, Resources.WatermarkTimeStamp, sampleTime);
                g.DrawString(text, font, Brushes.White, 11, 11);
                g.DrawString(text, font, Brushes.Black, 10, 10);
            }
        }
    }
}