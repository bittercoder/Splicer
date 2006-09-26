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

using Splicer.Utils;

namespace Splicer.Timeline
{
    /// <summary>
    /// Media file represents the file used to create a clip.
    /// </summary>
    public class MediaFile
    {
        #region Data members

        /// <summary>
        /// file this instance is reporting information for
        /// </summary>
        private string _filename;

        /// <summary>
        /// Duration of the media in UNITS, as reported by IMediaDet
        /// </summary>
        private long _realLengthInUnits;

        /// <summary>
        /// Amount of the real length to use when rendering
        /// </summary>
        private long _lengthUsedInUnits;

        // <summary>
        // UsingLength reported in # of frames (only available after the file has been added to the timeline) 
        // </summary>
        private int _lengthInFrames;

        #endregion

        /// <summary>
        /// Constructor takes a file path+name
        /// </summary>
        /// <param name="filename">File path+name</param>
        public MediaFile(string filename)
        {
            _filename = filename;
            _realLengthInUnits = MediaDetUtils.GetLength(filename);
            _lengthUsedInUnits = _realLengthInUnits;
            _lengthInFrames = -1;
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
                    throw new SplicerException("Invalid length specified");
                }
            }
        }

        /// <summary>
        /// Duration of the clip, in seconds
        /// </summary>
        public double Length
        {
            get { return TimelineUtils.ToSeconds(LengthInUnits); }
        }

        /// <summary>
        /// Return the file name
        /// </summary>
        public string FileName
        {
            get { return _filename; }
        }

        /// <summary>
        /// Return the length in frames
        /// </summary>
        public int LengthInFrames
        {
            set { _lengthInFrames = value; }
            get { return _lengthInFrames; }
        }
    }
}