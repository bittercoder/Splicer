using System.Runtime.InteropServices;
using DirectShowLib.DES;

namespace Splicer.Timeline
{
    public class Transition : ITransition
    {
        private string _name;
        private double _offset;
        private double _duration;
        private bool _swapInputs;
        private TransitionDefinition _transitionDefinition;
        private IAMTimelineObj _timelineObj;
        private ITransitionContainer _container;

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
    }
}