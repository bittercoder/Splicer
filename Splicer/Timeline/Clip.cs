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

using System;
using System.Runtime.InteropServices;
using DirectShowLib.DES;
using Splicer.Utils;

namespace Splicer.Timeline
{
    public class Clip : IClip
    {
        private event EventHandler<AfterEffectAddedEventArgs> _afterEffectAdded;
        private event EventHandler _beforeEffectAdded;
        private double _offset;
        private double _duration;
        private MediaFile _file;
        private AddOnlyList<IEffect> _effects = new AddOnlyList<IEffect>();
        private IAMTimeline _timeline;
        private IAMTimelineSrc _timelineSrc;
        private string _name;
        private double _mediaStart;
        private IClipContainer _container;

        public Clip(IClipContainer container, IAMTimeline timeline, IAMTimelineSrc timelineSrc, string name,
                    double offset, double duration, double mediaStart, MediaFile file)
        {
            _container = container;
            _name = name;
            _timeline = timeline;
            _timelineSrc = timelineSrc;
            _offset = offset;
            _duration = duration;
            _file = file;
            _mediaStart = mediaStart;
        }

        public IGroup Group
        {
            get { return Container.Group; }
        }

        public IClipContainer Container
        {
            get { return _container; }
        }

        public ResizeFlags StretchMode
        {
            get
            {
                ResizeFlags mode;
                _timelineSrc.GetStretchMode(out mode);
                return mode;
            }
            set { _timelineSrc.SetStretchMode(value); }
        }

        public string Name
        {
            get { return _name; }
        }

        public double Offset
        {
            get { return _offset; }
        }

        public double MediaStart
        {
            get { return _mediaStart; }
        }

        public double Duration
        {
            get { return _duration; }
        }

        public MediaFile File
        {
            get { return _file; }
        }

        public AddOnlyList<IEffect> Effects
        {
            get { return _effects; }
        }

        public IEffect AddEffect(double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            return AddEffect(null, -1, offset, duration, effectDefinition);
        }

        public IEffect AddEffect(string name, int priority, double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            OnBeforeEffectsAdded();

            IEffect effect =
                TimelineUtils.AddEffectToCollection(this, _timeline, (IAMTimelineEffectable) _timelineSrc, _effects,
                                                    name,
                                                    priority, offset, duration, effectDefinition);

            OnAfterEffectAdded(effect);

            return effect;
        }

        public event EventHandler<AfterEffectAddedEventArgs> AfterEffectAdded
        {
            add { _afterEffectAdded += value; }
            remove { _afterEffectAdded -= value; }
        }

        public event EventHandler BeforeEffectAdded
        {
            add { _beforeEffectAdded += value; }
            remove { _beforeEffectAdded -= value; }
        }

        protected void OnAfterEffectAdded(IEffect effect)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(this, new AfterEffectAddedEventArgs(effect, this));
            }
        }

        protected void OnBeforeEffectsAdded()
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(this, EventArgs.Empty);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_timelineSrc != null)
            {
                Marshal.ReleaseComObject(_timelineSrc);
                _timelineSrc = null;
            }

            if (_effects != null)
            {
                foreach (IEffect effect in _effects)
                {
                    effect.Dispose();
                }
                _effects = null;
            }
        }

        #endregion      
    }
}