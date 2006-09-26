using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Splicer.Timeline
{
    public class VirtualClipCollection : IVirtualClipCollection
    {
        private List<VirtualClip> _clips = new List<VirtualClip>();

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

        public IVirtualClip this[int index]
        {
            get { return _clips[index]; }
        }

        public int Count
        {
            get { return _clips.Count; }
        }

        #region IEnumerable<IVirtualClip> Members

        public IEnumerator<IVirtualClip> GetEnumerator()
        {
            return new List<IVirtualClip>(_clips.ToArray()).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _clips).GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

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
        private double _offset;
        private double _duration;
        private double _mediaStart;
        private IClip _sourceClip;

        public VirtualClip(double offset, double duration, double mediaStart, IClip sourceClip)
        {
            _offset = offset;
            _duration = duration;
            _mediaStart = mediaStart;
            _sourceClip = sourceClip;
        }

        #region IVirtualClip Members

        public double Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public double Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public double MediaStart
        {
            get { return _mediaStart; }
            set { _mediaStart = value; }
        }

        public IClip SourceClip
        {
            get { return _sourceClip; }
        }

        #endregion

        #region IName Members

        public string Name
        {
            get { return _sourceClip.Name; }
        }

        #endregion

        #region IComparable<IVirtualClip> Members

        public int CompareTo(IVirtualClip other)
        {
            if (other == null) return -1;

            return _offset.CompareTo(other.Offset);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return CompareTo(obj as IVirtualClip);
        }

        #endregion

        public override string ToString()
        {
            return
                string.Format("<clip start=\"{0}\" stop=\"{1}\" src=\"{2}\" mstart=\"{3}\" />", Offset,
                              Offset + Duration, SourceClip.File.FileName, MediaStart);
        }
    }
}