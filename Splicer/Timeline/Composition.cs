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
    public class Composition : AbstractComposition
    {
        private ICompositionContainer _container;

        public Composition(ICompositionContainer container, IAMTimeline timeline, IAMTimelineComp timelineComposition,
                           string name, int priority)
            : base(timeline, name, priority)
        {
            _container = container;
            _timelineComposition = timelineComposition;
        }

        public override ICompositionContainer Container
        {
            get { return _container; }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_timelineComposition != null)
            {
                Marshal.ReleaseComObject(_timelineComposition);
                _timelineComposition = null;
            }
        }
    }
}