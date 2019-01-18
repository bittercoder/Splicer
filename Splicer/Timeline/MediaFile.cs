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
using System.IO;
using System.Security.Permissions;
using Splicer.Properties;
using Splicer.Utilities;

namespace Splicer.Timeline
{
    /// <summary>
    /// Media file represents the file used to create a clip.
    /// </summary>
    public sealed class MediaFile : IDisposable
    {
        #region Data members

        /// <summary>
        /// When true, this media file will attempt to destroy the file when it's disposed
        /// </summary>
        private readonly bool _manageLifespan;

        /// <summary>
        /// Duration of the media in Units, as reported by IMediaDet
        /// </summary>
        private readonly long _realLengthInUnits;

        /// <summary>
        /// file this instance is reporting information for
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Amount of the real length to use when rendering
        /// </summary>
        private long _lengthUsedInUnits;

        // <summary>
        // UsingLength reported in # of frames (only available after the file has been added to the timeline) 
        // </summary>

        #endregion

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public MediaFile(string fileName, bool manageLifespan)
        {
            _manageLifespan = manageLifespan;
            _fileName = fileName;
            _realLengthInUnits = MediaInspector.GetLength(fileName);
            _lengthUsedInUnits = _realLengthInUnits;
            LengthInFrames = -1;
        }

        /// <summary>
        /// Constructor takes a file path+name
        /// </summary>
        /// <param name="fileName">File path+name</param>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public MediaFile(string fileName)
            : this(fileName, false)
        {
        }

        /// <summary>
        /// Return or set the length of the media file.  When setting the
        /// value, you can set it from zero to the duration of the media file.
        /// If the duration of the media file is zero (jpg, bmp, etc), you can
        /// set the duration to any time.  Useful for intro, credits, etc.
        /// </summary>
        public long LengthInUnits
        {
            get { return _lengthUsedInUnits; }
            set
            {
                if ((value < _realLengthInUnits || _realLengthInUnits == 0) && value >= 0)
                {
                    _lengthUsedInUnits = value;
                }
                else
                {
                    throw new SplicerException(Resources.ErrorInvalidLengthSpecified);
                }
            }
        }

        /// <summary>
        /// Duration of the clip, in seconds
        /// </summary>
        public double Length
        {
            get { return TimelineBuilder.ToSeconds(LengthInUnits); }
        }

        /// <summary>
        /// Return the file name
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        /// <summary>
        /// Return the length in frames
        /// </summary>
        public int LengthInFrames { set; get; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~MediaFile()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_manageLifespan)
            {
                File.Delete(_fileName);
            }

            if (disposing)
            {
                _fileName = null;
            }
        }
    }
}