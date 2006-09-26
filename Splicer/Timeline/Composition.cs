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