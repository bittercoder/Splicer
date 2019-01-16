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
    public sealed class Composition : AbstractComposition, IDisposable
    {
        private readonly ICompositionContainer _container;

        public Composition(ICompositionContainer container, IAMTimeline timeline, IAMTimelineComp timelineComposition,
                           string name, int priority)
            : base(timeline, name, priority)
        {
            _container = container;
            TimelineComposition = timelineComposition;
        }

        public override ICompositionContainer Container
        {
            get { return _container; }
        }

        #region IDisposable Members

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~Composition()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private void Dispose(bool disposing)
        {
            DisposeComposition(disposing);

            if (TimelineComposition != null)
            {
                Marshal.ReleaseComObject(TimelineComposition);
                TimelineComposition = null;
            }
        }
    }
}