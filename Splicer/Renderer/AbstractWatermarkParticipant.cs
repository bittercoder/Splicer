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
using System.Drawing.Imaging;
using Splicer.Utilities;

namespace Splicer.Renderer
{
    public abstract class AbstractWatermarkParticipant : ICallbackParticipant
    {
        private readonly Rectangle _bounds;
        private bool _flipImages;
        private PixelFormat _format;
        private int _height;
        private int _width;

        protected AbstractWatermarkParticipant(short bitCount, int width, int height, bool flipImages)
        {
            _format = MediaTypeTools.GetPixelFormatForBitCount(bitCount);
            _width = width;
            _height = height;
            _flipImages = flipImages;
            _bounds = new Rectangle(0, 0, width, height);
        }

        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        #region ICallbackParticipant Members

        public int ProcessBuffer(double sampleTime, IntPtr buffer, int bufferLength)
        {
            using (Bitmap bitmap = new Bitmap(_width, _height, _format))
            {
                BitmapData data = bitmap.LockBits(_bounds, ImageLockMode.ReadWrite, _format);

                NativeMethods.CopyMemory(data.Scan0, buffer, (uint) bufferLength);

                bitmap.UnlockBits(data);

                if (_flipImages) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                UpdateImage(sampleTime, bitmap);

                if (_flipImages) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                data = bitmap.LockBits(_bounds, ImageLockMode.ReadOnly, _format);

                NativeMethods.CopyMemory(buffer, data.Scan0, (uint) bufferLength);

                bitmap.UnlockBits(data);
            }

            return 0;
        }

        #endregion

        public abstract void UpdateImage(double sampleTime, Bitmap bitmap);
    }
}