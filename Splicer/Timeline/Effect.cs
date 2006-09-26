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

using System.Runtime.InteropServices;
using DirectShowLib.DES;

namespace Splicer.Timeline
{
    public class Effect : IEffect, IPrioritySetter
    {
        private int _priority;
        private double _offset;
        private double _duration;
        private string _name;
        private EffectDefinition _effectDefinition;
        private IAMTimelineObj _timelineObj;
        private IEffectContainer _container;

        public Effect(IEffectContainer container, IAMTimelineObj timelineObj, string name, int priority, double offset,
                      double duration,
                      EffectDefinition effectDefinition)
        {
            _container = container;
            _timelineObj = timelineObj;
            _name = name;
            _priority = priority;
            _offset = offset;
            _duration = duration;
            _effectDefinition = effectDefinition;
        }

        #region IEffect Members

        public IGroup Group
        {
            get { return Container.Group; }
        }

        public IEffectContainer Container
        {
            get { return _container; }
        }

        public string Name
        {
            get { return _name; }
        }

        public int Priority
        {
            get { return _priority; }
        }

        public double Offset
        {
            get { return _offset; }
        }

        public double Duration
        {
            get { return _duration; }
        }

        public EffectDefinition EffectDefinition
        {
            get { return _effectDefinition; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_timelineObj != null)
            {
                Marshal.ReleaseComObject(_timelineObj);
                _timelineObj = null;
            }
        }

        #endregion

        #region IPrioritySetter Members

        void IPrioritySetter.SetPriority(int newValue)
        {
            _priority = newValue;
        }

        #endregion
    }
}