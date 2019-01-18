using System;
using System.Collections.Generic;
using System.Text;
using Splicer.Timeline;

namespace Splicer.Renderer
{
    public class ProgressChangedEventArgs : EventArgs
    {
        private double _progress;
        
        public ProgressChangedEventArgs(double progress)
        {
            _progress = progress;
        }
        
        public double Progress
        {
            get { return _progress; }
        }
    }
    
    /// <summary>
    /// Fires events (100, over the duration of the entire conversion) - useful in user interfaces.
    /// </summary>
    public class PercentageProgressParticipant : AbstractProgressParticipant
    {
        private double _duration;
        private double _lastProgress;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public PercentageProgressParticipant(double duration)
        {
            _duration = duration;
            _lastProgress = 0;
        }
        
        public PercentageProgressParticipant(ITimeline timeline)
            : this(timeline.Duration)
        {            
        }
        
        protected override void HandleProgress(double sampleTime, int bufferLength)
        {
            double newProgress = (sampleTime/_duration);
            if ((newProgress-_lastProgress) > 0.01)
            {
                _lastProgress = newProgress;
                if (ProgressChanged != null) ProgressChanged(this, new ProgressChangedEventArgs(newProgress));
            }
        }
    }
}
