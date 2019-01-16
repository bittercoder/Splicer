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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Splicer.Properties;

namespace Splicer.Timeline
{
    public sealed class VirtualClipCollection : IVirtualClipCollection
    {
        private readonly List<VirtualClip> _clips = new List<VirtualClip>();

        #region IVirtualClipCollection Members

        public IVirtualClip this[int index]
        {
            get { return _clips[index]; }
        }

        public int Count
        {
            get { return _clips.Count; }
        }

        public IEnumerator<IVirtualClip> GetEnumerator()
        {
            return new List<IVirtualClip>(_clips.ToArray()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _clips).GetEnumerator();
        }

        #endregion

        public void AddVirtualClip(IClip clip)
        {
            AdjustForNewclip(clip);
            AddClip(clip);
        }

        private void AdjustForNewclip(IClip clip)
        {
            double newClipStart = clip.Offset;
            double newClipEnd = clip.Duration + clip.Offset;

            foreach (VirtualClip virtualClip in _clips.ToArray())
            {
                double curClipStart = virtualClip.Offset;
                double curClipEnd = virtualClip.Duration + virtualClip.Offset;

                if ((curClipStart >= newClipStart) && (curClipEnd <= newClipEnd))
                {
                    // scenario 1, full occlusion, remove the clip
                    _clips.Remove(virtualClip);
                }
                else if ((curClipStart < newClipStart) && (curClipEnd > newClipEnd))
                {
                    // scenario 2, split the clip in two
                    double frontDelta = newClipStart - curClipStart;
                    double endDelta = curClipEnd - newClipEnd;

                    // reduce duration of part of clip that falls over front
                    virtualClip.Duration = frontDelta;
                    // add a new clip for the part of the clip falls over the end of the new clip
                    AddClip(newClipEnd, endDelta, (newClipEnd - curClipStart) + virtualClip.MediaStart,
                            virtualClip.SourceClip);
                }
                else if ((newClipStart <= curClipStart) && (newClipEnd < curClipEnd) && (newClipEnd > curClipStart))
                {
                    // scenario 3, trim front of clip
                    double delta = newClipEnd - curClipStart;
                    virtualClip.Duration -= delta;
                    virtualClip.MediaStart += delta;
                    virtualClip.Offset += delta;
                }
                else if ((newClipStart > curClipStart) && (newClipStart < curClipEnd) && (newClipEnd >= curClipEnd))
                {
                    // scenario 4, trim end of clip
                    double delta = curClipEnd - newClipStart;
                    virtualClip.Duration -= delta;
                }
            }
        }

        private void AddClip(IClip clip)
        {
            AddClip(clip.Offset, clip.Duration, clip.MediaStart, clip);
        }

        private void AddClip(double offset, double duration, double mediaStart, IClip clip)
        {
            _clips.Add(new VirtualClip(offset, duration, mediaStart, clip));
            _clips.Sort();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (VirtualClip clip in _clips)
            {
                if (builder.Length > 0) builder.Append(Environment.NewLine);
                builder.Append(clip.ToString());
            }

            return builder.ToString();
        }
    }

    public class VirtualClip : IVirtualClip, IComparable<IVirtualClip>, IComparable
    {
        private readonly IClip _sourceClip;
        private double _offset;

        public VirtualClip(double offset, double duration, double mediaStart, IClip sourceClip)
        {
            _offset = offset;
            Duration = duration;
            MediaStart = mediaStart;
            _sourceClip = sourceClip;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return CompareTo(obj as VirtualClip);
        }

        #endregion

        #region IComparable<IVirtualClip> Members

        public int CompareTo(IVirtualClip other)
        {
            if (other == null) return -1;

            return _offset.CompareTo(other.Offset);
        }

        #endregion

        #region IVirtualClip Members

        public double Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public double Duration { get; set; }

        public double MediaStart { get; set; }

        public IClip SourceClip
        {
            get { return _sourceClip; }
        }

        public string Name
        {
            get { return _sourceClip.Name; }
        }

        #endregion

        public override bool Equals(Object obj)
        {
            if (!(obj is IVirtualClip)) return false;

            return (CompareTo(obj) == 0);
        }

        public override int GetHashCode()
        {
            return (int) _offset;
        }

        public static bool operator ==(VirtualClip r1, VirtualClip r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(VirtualClip r1, VirtualClip r2)
        {
            return !(r1 == r2);
        }

        public static bool operator <(VirtualClip r1, VirtualClip r2)
        {
            return (r1.CompareTo(r2) < 0);
        }

        public static bool operator >(VirtualClip r1, VirtualClip r2)
        {
            return (r1.CompareTo(r2) > 0);
        }

        public override string ToString()
        {
            return
                string.Format(CultureInfo.CurrentUICulture, Resources.VirtualClipToStringTemplate, Offset,
                              Offset + Duration, SourceClip.File.FileName, MediaStart);
        }
    }
}