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

namespace Splicer.Timeline.Tests
{
    public class MockMediaFileAssistant : IMediaFileAssistant, IDisposable
    {
        private readonly bool _willAssist;
        private int _executionCount;

        public MockMediaFileAssistant(bool willAssist)
        {
            _willAssist = willAssist;
            _executionCount = 0;
        }

        public int ExecutionCount
        {
            get { return _executionCount; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _executionCount = ExecutionCount + 1;
            ;
        }

        #endregion

        #region IMediaFileAssistant Members

        public bool WillAssist(MediaFile file)
        {
            return _willAssist;
        }

        public IDisposable Assist(MediaFile file)
        {
            return this;
        }

        #endregion
    }
}