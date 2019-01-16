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
    /// <summary>
    /// The base class you can derive sample grabbing participants from.
    /// </summary>
    public abstract class AbstractSampleGrabbingParticipant : ICallbackParticipant
    {
        private readonly bool _flipImages;
        private readonly PixelFormat _format;
        private readonly int _height;
        private readonly int _width;
        private int _count;

        protected AbstractSampleGrabbingParticipant(short bitCount, int width, int height, bool flipImages)
        {
            _format = MediaTypeTools.GetPixelFormatForBitCount(bitCount);
            _width = width;
            _height = height;
            _flipImages = flipImages;
        }

        #region ICallbackParticipant Members

        /// <summary>
        /// Processes the buffer and takes the snapshot
        /// </summary>
        /// <param name="sampleTime"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        public int ProcessBuffer(double sampleTime, IntPtr buffer, int bufferLength)
        {
            if (TakeSnapshot(sampleTime))
            {
                using (var bitmap = new Bitmap(_width, _height, _format))
                {
                    var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                    BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, _format);

                    NativeMethods.CopyMemory(data.Scan0, buffer, (uint) bufferLength);

                    bitmap.UnlockBits(data);

                    if (_flipImages) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    ConsumeImage(sampleTime, _count, bitmap);
                }

                _count++;
            }

            return 0;
        }

        #endregion

        /// <summary>
        /// Implement in derived classes to return true for sampleTimes we wish to take a snapshot of.
        /// </summary>
        /// <param name="sampleTime"></param>
        /// <returns></returns>
        public abstract bool TakeSnapshot(double sampleTime);

        /// <summary>
        /// Implement in derived classes to handle the resultant image
        /// </summary>
        /// <param name="sampleTime"></param>
        /// <param name="count"></param>
        /// <param name="bitmap"></param>
        public abstract void ConsumeImage(double sampleTime, int count, Bitmap bitmap);
    }
}