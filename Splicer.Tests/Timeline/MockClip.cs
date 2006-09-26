using System;
using DirectShowLib.DES;
using Splicer.Utils;

namespace Splicer.Timeline.Tests
{
    public class MockClip : IClip
    {
        private double _offset;
        private double _duration;
        private double _mediaStart;
        private string _name;

        public MockClip(string name, double offset, double duration, double mediaStart)
        {
            _name = name;
            _offset = offset;
            _duration = duration;
            _mediaStart = mediaStart;
        }

        public MockClip(double offset, double duration, double mediaStart)
            : this(null, offset, duration, mediaStart)
        {
        }

        public ResizeFlags StretchMode
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public IClipContainer Container
        {
            get { throw new NotImplementedException(); }
        }

        public double Offset
        {
            get { return _offset; }
        }

        public double Duration
        {
            get { return _duration; }
        }

        public double MediaStart
        {
            get { return _mediaStart; }
        }

        public MediaFile File
        {
            get { return null; }
        }

        public string Name
        {
            get { return _name; }
        }

        public event EventHandler<AfterEffectAddedEventArgs> AfterEffectAdded;

        public event EventHandler BeforeEffectAdded;

        public IEffect AddEffect(double offset, double duration, EffectDefinition effectDefinition)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEffect AddEffect(string name, int priority, double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public AddOnlyList<IEffect> Effects
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void Dispose()
        {
        }

        public IGroup Group
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
    }
}