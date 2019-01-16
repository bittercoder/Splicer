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
using System.Drawing;

namespace Splicer.Renderer
{
    public class ImageWatermarkParticipant : AbstractWatermarkParticipant
    {
        private Point _location;
        private Image _watermark;

        public ImageWatermarkParticipant(short bitCount, int width, int height, bool flipImages, Image watermark,
                                         Point location)
            : base(bitCount, width, height, flipImages)
        {
            if (watermark == null) throw new ArgumentNullException("watermark");
            _watermark = watermark;
            _location = location;
        }

        public override void UpdateImage(double sampleTime, Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImageUnscaled(_watermark, _location);
            }
        }
    }
}