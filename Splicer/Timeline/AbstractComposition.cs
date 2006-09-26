using System;
using DirectShowLib.DES;
using Splicer.Utils;

namespace Splicer.Timeline
{
    public abstract class AbstractComposition : IComposition, IPrioritySetter
    {
        protected event EventHandler _beforeCompositionAdded;
        protected event EventHandler<AfterCompositionAddedEventArgs> _afterCompositionAdded;
        protected event EventHandler _beforeTrackAdded;
        protected event EventHandler<AfterTrackAddedEventArgs> _afterTrackAdded;
        protected event EventHandler<AfterEffectAddedEventArgs> _afterEffectAdded;
        protected event EventHandler _beforeEffectAdded;
        protected event EventHandler _beforeTransitionAdded;
        protected event EventHandler<AfterTransitionAddedEventArgs> _afterTransitionAdded;
        protected event EventHandler _beforeClipAdded;
        protected event EventHandler<AfterClipAddedEventArgs> _afterClipAdded;
        protected string _name;
        protected int _priority;
        protected AddOnlyList<IComposition> _compositions = new AddOnlyList<IComposition>();
        protected AddOnlyList<ITrack> _tracks = new AddOnlyList<ITrack>();
        protected AddOnlyList<ITransition> _transitions = new AddOnlyList<ITransition>();
        protected AddOnlyList<IEffect> _effects = new AddOnlyList<IEffect>();
        protected IAMTimeline _desTimeline;
        protected IAMTimelineComp _timelineComposition;

        public AbstractComposition(IAMTimeline timeline, string name, int priority)
        {
            _desTimeline = timeline;
            _name = name;
            _priority = priority;
        }

        public IGroup Group
        {
            get { return Container.Group; }
        }

        public abstract ICompositionContainer Container { get; }

        public IComposition AddComposition(string name, int priority)
        {
            OnBeforeCompositionAdded();

            IComposition composition =
                TimelineUtils.AddCompositionToCollection(this, _desTimeline, _timelineComposition, _compositions, name,
                                                         priority);

            composition.BeforeEffectAdded += new EventHandler(child_BeforeEffectAdded);
            composition.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(child_AfterEffectAdded);
            composition.AfterClipAdded += new EventHandler<AfterClipAddedEventArgs>(child_AfterClipAdded);
            composition.BeforeClipAdded += new EventHandler(child_BeforeClipAdded);
            composition.AfterTransitionAdded +=
                new EventHandler<AfterTransitionAddedEventArgs>(child_AfterTransitionAdded);
            composition.BeforeTransitionAdded += new EventHandler(child_BeforeTransitionAdded);
            composition.BeforeTrackAdded += new EventHandler(child_BeforeTrackAdded);
            composition.AfterTrackAdded += new EventHandler<AfterTrackAddedEventArgs>(child_AfterTrackAdded);
            composition.BeforeCompositionAdded += new EventHandler(child_BeforeCompositionAdded);
            composition.AfterCompositionAdded +=
                new EventHandler<AfterCompositionAddedEventArgs>(child_AfterCompositionAdded);

            OnAfterCompositionAdded(composition);

            return composition;
        }

        private void child_AfterCompositionAdded(object sender, AfterCompositionAddedEventArgs e)
        {
            if (_afterCompositionAdded != null)
            {
                _afterCompositionAdded(sender, e);
            }
        }

        private void child_BeforeCompositionAdded(object sender, EventArgs e)
        {
            if (_beforeCompositionAdded != null)
            {
                _beforeCompositionAdded(sender, e);
            }
        }

        private void child_AfterTrackAdded(object sender, AfterTrackAddedEventArgs e)
        {
            if (_afterTrackAdded != null)
            {
                _afterTrackAdded(sender, e);
            }
        }

        private void child_BeforeTrackAdded(object sender, EventArgs e)
        {
            if (_beforeTrackAdded != null)
            {
                _beforeTrackAdded(sender, e);
            }
        }

        public AddOnlyList<IComposition> Compositions
        {
            get { return _compositions; }
        }

        public ITrack AddTrack(string name, int priority)
        {
            OnBeforeTrackAdded();

            ITrack track =
                TimelineUtils.AddTrackToCollection(this, _desTimeline, _timelineComposition, _tracks, name, priority);

            track.BeforeEffectAdded += new EventHandler(child_BeforeEffectAdded);
            track.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(child_AfterEffectAdded);
            track.AfterClipAdded += new EventHandler<AfterClipAddedEventArgs>(child_AfterClipAdded);
            track.BeforeClipAdded += new EventHandler(child_BeforeClipAdded);
            track.AfterTransitionAdded += new EventHandler<AfterTransitionAddedEventArgs>(child_AfterTransitionAdded);
            track.BeforeTransitionAdded += new EventHandler(child_BeforeTransitionAdded);

            OnAfterTrackAdded(track);

            return track;
        }

        private void child_BeforeTransitionAdded(object sender, EventArgs e)
        {
            if (_beforeTransitionAdded != null)
            {
                _beforeTransitionAdded(sender, e);
            }
        }

        private void child_AfterTransitionAdded(object sender, AfterTransitionAddedEventArgs e)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(sender, e);
            }
        }

        private void child_BeforeClipAdded(object sender, EventArgs e)
        {
            if (_beforeClipAdded != null)
            {
                _beforeClipAdded(sender, e);
            }
        }

        private void child_AfterClipAdded(object sender, AfterClipAddedEventArgs e)
        {
            if (_afterClipAdded != null)
            {
                _afterClipAdded(sender, e);
            }
        }

        private void child_AfterEffectAdded(object sender, AfterEffectAddedEventArgs e)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(sender, e);
            }
        }

        private void child_BeforeEffectAdded(object sender, EventArgs e)
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(sender, e);
            }
        }

        public AddOnlyList<ITrack> Tracks
        {
            get { return _tracks; }
        }

        public IEffect AddEffect(double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            return AddEffect(null, -1, offset, duration, effectDefinition);
        }

        public IEffect AddEffect(string name, int priority, double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            OnBeforeEffectAdded();

            IEffect effect =
                TimelineUtils.AddEffectToCollection(this, _desTimeline, (IAMTimelineEffectable) _timelineComposition,
                                                    _effects,
                                                    name, priority, offset, duration, effectDefinition);

            OnAfterEffectAdded(effect);

            return effect;
        }

        public AddOnlyList<IEffect> Effects
        {
            get { return _effects; }
        }

        public ITransition AddTransition(double offset, double duration,
                                         TransitionDefinition transitionDefinition)
        {
            return AddTransition(null, offset, duration, transitionDefinition, false);
        }

        public ITransition AddTransition(double offset, double duration,
                                         TransitionDefinition transitionDefinition, bool swapInputs)
        {
            return AddTransition(null, offset, duration, transitionDefinition, swapInputs);
        }

        public ITransition AddTransition(string name, double offset, double duration,
                                         TransitionDefinition transitionDefinition, bool swapInputs)
        {
            OnBeforeTransitionAdded();

            ITransition transition =
                TimelineUtils.AddTransitionToCollection(this, _desTimeline, (IAMTimelineTransable) _timelineComposition,
                                                        _transitions, name, offset, duration, transitionDefinition,
                                                        swapInputs);

            OnAfterTransitionAdded(transition);

            return transition;
        }

        public AddOnlyList<ITransition> Transitions
        {
            get { return _transitions; }
        }

        public int Priority
        {
            get { return _priority; }
        }

        public string Name
        {
            get { return _name; }
        }

        public event EventHandler<AfterEffectAddedEventArgs> AfterEffectAdded
        {
            add { _afterEffectAdded += value; }
            remove { _afterEffectAdded -= value; }
        }

        protected void OnAfterEffectAdded(IEffect effect)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(this, new AfterEffectAddedEventArgs(effect, this));
            }
        }

        public event EventHandler BeforeEffectAdded
        {
            add { _beforeEffectAdded += value; }
            remove { _beforeEffectAdded -= value; }
        }

        protected void OnBeforeEffectAdded()
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(this, EventArgs.Empty);
            }
        }

        public event EventHandler BeforeTransitionAdded
        {
            add { _beforeTransitionAdded += value; }
            remove { _beforeTransitionAdded -= value; }
        }

        protected void OnBeforeTransitionAdded()
        {
            if (_beforeTransitionAdded != null)
            {
                _beforeTransitionAdded(this, EventArgs.Empty);
            }
        }

        public event EventHandler<AfterTransitionAddedEventArgs> AfterTransitionAdded
        {
            add { _afterTransitionAdded += value; }
            remove { _afterTransitionAdded -= value; }
        }

        protected void OnAfterTransitionAdded(ITransition transition)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(this, new AfterTransitionAddedEventArgs(transition, this));
            }
        }

        public event EventHandler BeforeCompositionAdded
        {
            add { _beforeCompositionAdded += value; }
            remove { _beforeCompositionAdded -= value; }
        }

        protected void OnBeforeCompositionAdded()
        {
            if (_beforeCompositionAdded != null)
            {
                _beforeCompositionAdded(this, EventArgs.Empty);
            }
        }

        public event EventHandler<AfterCompositionAddedEventArgs> AfterCompositionAdded
        {
            add { _afterCompositionAdded += value; }
            remove { _afterCompositionAdded -= value; }
        }

        protected void OnAfterCompositionAdded(IComposition composition)
        {
            if (_afterCompositionAdded != null)
            {
                _afterCompositionAdded(this, new AfterCompositionAddedEventArgs(composition, this));
            }
        }

        public event EventHandler BeforeTrackAdded
        {
            add { _beforeTrackAdded += value; }
            remove { _beforeTrackAdded -= value; }
        }

        protected void OnBeforeTrackAdded()
        {
            if (_beforeTrackAdded != null)
            {
                _beforeTrackAdded(this, EventArgs.Empty);
            }
        }

        public event EventHandler<AfterTrackAddedEventArgs> AfterTrackAdded
        {
            add { _afterTrackAdded += value; }
            remove { _afterTrackAdded -= value; }
        }

        protected void OnAfterTrackAdded(ITrack track)
        {
            if (_afterTrackAdded != null)
            {
                _afterTrackAdded(this, new AfterTrackAddedEventArgs(track, this));
            }
        }

        public event EventHandler BeforeClipAdded
        {
            add { _beforeClipAdded += value; }
            remove { _beforeClipAdded -= value; }
        }

        public event EventHandler<AfterClipAddedEventArgs> AfterClipAdded
        {
            add { _afterClipAdded += value; }
            remove { _afterClipAdded -= value; }
        }

        public virtual void Dispose()
        {
            if (_compositions != null)
            {
                foreach (IComposition composition in _compositions)
                {
                    composition.Dispose();
                }

                _compositions = null;
            }

            if (_tracks != null)
            {
                foreach (ITrack track in _tracks)
                {
                    track.Dispose();
                }

                _tracks = null;
            }

            if (_transitions != null)
            {
                foreach (ITransition transition in _transitions)
                {
                    transition.Dispose();
                }

                _transitions = null;
            }

            if (_effects != null)
            {
                foreach (IEffect effect in _effects)
                {
                    effect.Dispose();
                }

                _effects = null;
            }
        }

        public IComposition AddComposition()
        {
            return AddComposition(null, -1);
        }

        public ITrack AddTrack()
        {
            return AddTrack(null, -1);
        }

        #region IPrioritySetter Members

        void IPrioritySetter.SetPriority(int newValue)
        {
            _priority = newValue;
        }

        #endregion
    }
}