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

namespace Splicer.Timeline
{
    public sealed class Transition : ITransition
    {
        private readonly ITransitionContainer _container;
        private readonly double _duration;
        private readonly string _name;
        private readonly double _offset;
        private readonly bool _swapInputs;
        private readonly TransitionDefinition _transitionDefinition;
        private IAMTimelineObj _timelineObj;

        public Transition(ITransitionContainer container, IAMTimelineObj timelineObj, string name, double offset,
                          double duration, bool swapInputs,
                          TransitionDefinition transitionDefinition)
        {
            _container = container;
            _timelineObj = timelineObj;
            _name = name;
            _offset = offset;
            _duration = duration;
            _swapInputs = swapInputs;
            _transitionDefinition = transitionDefinition;
        }

        #region ITransition Members

        public IGroup Group
        {
            get { return Container.Group; }
        }

        public ITransitionContainer Container
        {
            get { return _container; }
        }

        public string Name
        {
            get { return _name; }
        }

        public double Offset
        {
            get { return _offset; }
        }

        public double Duration
        {
            get { return _duration; }
        }

        public bool SwapInputs
        {
            get { return _swapInputs; }
        }

        public TransitionDefinition TransitionDefinition
        {
            get { return _transitionDefinition; }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private void Dispose(bool disposing)
        {
            if (_timelineObj != null)
            {
                Marshal.ReleaseComObject(_timelineObj);
                _timelineObj = null;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~Transition()
        {
            Dispose(false);
        }
    }
}