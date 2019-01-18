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
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib.DES;
using Splicer.Utilities;

namespace Splicer.Timeline
{
    public sealed class Clip : IClip
    {
        private readonly IClipContainer _container;
        private readonly double _duration;
        private readonly double _mediaStart;
        private readonly string _name;
        private readonly double _offset;
        private readonly IAMTimeline _timeline;
        private AddOnlyCollection<IEffect> _effects = new AddOnlyCollection<IEffect>();
        private MediaFile _file;
        private IAMTimelineSrc _timelineSource;

        public Clip(IClipContainer container, IAMTimeline timeline, IAMTimelineSrc timelineSource, string name,
                    double offset, double duration, double mediaStart, MediaFile file)
        {
            _container = container;
            _name = name;
            _timeline = timeline;
            _timelineSource = timelineSource;
            _offset = offset;
            _duration = duration;
            _file = file;
            _mediaStart = mediaStart;
        }

        #region IClip Members

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
                _timelineSource.GetStretchMode(out mode);
                return mode;
            }
            set { _timelineSource.SetStretchMode(value); }
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

        public AddOnlyCollection<IEffect> Effects
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
            OnAddingEffects();

            IEffect effect =
                TimelineBuilder.AddEffectToCollection(this, _timeline, (IAMTimelineEffectable) _timelineSource, _effects,
                                                      name,
                                                      priority, offset, duration, effectDefinition);

            OnAddedEffect(effect);

            return effect;
        }

        public event EventHandler<AddedEffectEventArgs> AddedEffect
        {
            add { _afterEffectAdded += value; }
            remove { _afterEffectAdded -= value; }
        }

        public event EventHandler AddingEffect
        {
            add { _beforeEffectAdded += value; }
            remove { _beforeEffectAdded -= value; }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private event EventHandler<AddedEffectEventArgs> _afterEffectAdded;
        private event EventHandler _beforeEffectAdded;

        private void OnAddedEffect(IEffect effect)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(this, new AddedEffectEventArgs(effect, this));
            }
        }

        private void OnAddingEffects()
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(this, EventArgs.Empty);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~Clip()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_effects != null)
                {
                    foreach (IEffect effect in _effects)
                    {
                        effect.Dispose();
                    }
                    _effects = null;
                }

                if (_file != null)
                {
                    _file.Dispose();
                    _file = null;
                }
            }

            if (_timelineSource != null)
            {
                Marshal.ReleaseComObject(_timelineSource);
                _timelineSource = null;
            }
        }
    }
}