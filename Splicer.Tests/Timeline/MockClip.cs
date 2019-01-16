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
using DirectShowLib.DES;
using Splicer.Utilities;

namespace Splicer.Timeline.Tests
{
    public class MockClip : IClip
    {
        private readonly double _duration;
        private readonly double _mediaStart;
        private readonly string _name;
        private readonly double _offset;

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

        #region IClip Members

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

#pragma warning disable 67

        public event EventHandler<AddedEffectEventArgs> AddedEffect;

        public event EventHandler AddingEffect;

#pragma warning restore 67

        public IEffect AddEffect(double offset, double duration, EffectDefinition effectDefinition)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEffect AddEffect(string name, int priority, double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public AddOnlyCollection<IEffect> Effects
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

        #endregion
    }
}