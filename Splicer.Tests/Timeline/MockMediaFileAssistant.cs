using System;

namespace Splicer.Timeline.Tests
{
    public class MockMediaFileAssistant : IMediaFileAssistant, IDisposable
    {
        private int _executionCount;
        private bool _willAssist;

        public MockMediaFileAssistant(bool willAssist)
        {
            _willAssist = willAssist;
            _executionCount = 0;
        }

        public int ExecutionCount
        {
            get { return _executionCount; }
        }

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

        #region IDisposable Members

        public void Dispose()
        {
            _executionCount = ExecutionCount + 1;
            ;
        }

        #endregion
    }
}