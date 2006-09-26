using System;
using System.Collections.Generic;
using DirectShowLib.DES;
using Splicer.Utils;

namespace Splicer.Timeline
{
    public interface IBelongsToGroup
    {
        IGroup Group { get; }
    }

    public abstract class AfterAddedToContainerEventArgs<TItem, TContainer> : EventArgs
    {
        private TItem _item;
        private TContainer _container;

        public AfterAddedToContainerEventArgs(TItem item, TContainer container)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (container == null) throw new ArgumentNullException("container");
            _item = item;
            _container = container;
        }

        public TItem Item
        {
            get { return _item; }
        }

        public TContainer Container
        {
            get { return _container; }
        }
    }

    public class AfterEffectAddedEventArgs : AfterAddedToContainerEventArgs<IEffect, IEffectContainer>
    {
        public AfterEffectAddedEventArgs(IEffect item, IEffectContainer container)
            : base(item, container)
        {
        }
    }

    public class AfterTransitionAddedEventArgs : AfterAddedToContainerEventArgs<ITransition, ITransitionContainer>
    {
        public AfterTransitionAddedEventArgs(ITransition item, ITransitionContainer container)
            : base(item, container)
        {
        }
    }

    public class AfterTrackAddedEventArgs : AfterAddedToContainerEventArgs<ITrack, ITrackContainer>
    {
        public AfterTrackAddedEventArgs(ITrack item, ITrackContainer container)
            : base(item, container)
        {
        }
    }

    public class AfterClipAddedEventArgs : AfterAddedToContainerEventArgs<IClip, IClipContainer>
    {
        public AfterClipAddedEventArgs(IClip item, IClipContainer container)
            : base(item, container)
        {
        }
    }

    public class AfterGroupAddedEventArgs : AfterAddedToContainerEventArgs<IGroup, IGroupContainer>
    {
        public AfterGroupAddedEventArgs(IGroup item, IGroupContainer container)
            : base(item, container)
        {
        }
    }

    public class AfterCompositionAddedEventArgs : AfterAddedToContainerEventArgs<IComposition, ICompositionContainer>
    {
        public AfterCompositionAddedEventArgs(IComposition item, ICompositionContainer container)
            : base(item, container)
        {
        }
    }

    public interface IName
    {
        string Name { get; }
    }

    public interface IPriority
    {
        int Priority { get; }
    }

    internal interface IPrioritySetter
    {
        void SetPriority(int newValue);
    }

    public interface IEffect : IName, IPriority, IDisposable, IBelongsToGroup
    {
        double Offset { get; }
        double Duration { get; }
        EffectDefinition EffectDefinition { get; }
        IEffectContainer Container { get; }
    }

    public interface ITransition : IName, IDisposable, IBelongsToGroup
    {
        double Offset { get; }
        double Duration { get; }
        bool SwapInputs { get; }
        TransitionDefinition TransitionDefinition { get; }
        ITransitionContainer Container { get; }
    }

    public interface IEffectContainer : IBelongsToGroup
    {
        event EventHandler<AfterEffectAddedEventArgs> AfterEffectAdded;
        event EventHandler BeforeEffectAdded;
        IEffect AddEffect(double offset, double duration, EffectDefinition effectDefinition);
        IEffect AddEffect(string name, int priority, double offset, double duration, EffectDefinition effectDefinition);
        AddOnlyList<IEffect> Effects { get; }
    }

    public interface ITransitionContainer : IBelongsToGroup
    {
        event EventHandler BeforeTransitionAdded;
        event EventHandler<AfterTransitionAddedEventArgs> AfterTransitionAdded;

        ITransition AddTransition(double offset, double duration, TransitionDefinition transitionDefinition);

        ITransition AddTransition(double offset, double duration, TransitionDefinition transitionDefinition,
                                  bool swapInputs);

        ITransition AddTransition(string name, double offset, double duration, TransitionDefinition transitionDefinition,
                                  bool swapInputs);

        AddOnlyList<ITransition> Transitions { get; }
    }

    public interface ITrackContainer : IBelongsToGroup
    {
        event EventHandler BeforeTrackAdded;
        event EventHandler<AfterTrackAddedEventArgs> AfterTrackAdded;
        ITrack AddTrack(string name, int priority);
        ITrack AddTrack();
        AddOnlyList<ITrack> Tracks { get; }
    }

    public interface IClipContainer : IBelongsToGroup
    {
        event EventHandler BeforeClipAdded;
        event EventHandler<AfterClipAddedEventArgs> AfterClipAdded;

        IClip AddClip(string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddVideo(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddVideo(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipEnd);
        IClip AddVideo(string fileName, double offset);
        IClip AddVideo(string fileName);

        IClip AddImage(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddImage(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipEnd);
        IClip AddImage(string fileName, double offset);
        IClip AddImage(string fileName);

        IClip AddAudio(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddAudio(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipEnd);
        IClip AddAudio(string fileName, double offset);
        IClip AddAudio(string fileName);

        AddOnlyList<IClip> Clips { get; }
    }

    public interface ICompositionContainer : IBelongsToGroup
    {
        event EventHandler BeforeCompositionAdded;
        event EventHandler<AfterCompositionAddedEventArgs> AfterCompositionAdded;
        IComposition AddComposition(string name, int priority);
        IComposition AddComposition();
        AddOnlyList<IComposition> Compositions { get; }
    }

    public interface IComposition : IName, IPriority, ICompositionContainer, ITrackContainer, IEffectContainer,
                                    ITransitionContainer,
                                    IDisposable, IBelongsToGroup
    {
        event EventHandler BeforeClipAdded;
        event EventHandler<AfterClipAddedEventArgs> AfterClipAdded;
        ICompositionContainer Container { get; }
    }

    public interface ITrack : IName, IPriority, IEffectContainer, ITransitionContainer, IClipContainer, IDisposable,
                              IBelongsToGroup
    {
        double Duration { get; }
        IVirtualClipCollection VirtualClips { get; }
        ITrackContainer Container { get; }
    }

    public interface IVirtualClip : IName
    {
        double Offset { get; }
        double Duration { get; }
        double MediaStart { get; }
        IClip SourceClip { get; }
    }

    public interface IVirtualClipCollection : IEnumerable<IVirtualClip>
    {
        IVirtualClip this[int index] { get; }
        int Count { get; }
    }

    public interface IClip : IName, IEffectContainer, IDisposable
    {
        double Offset { get; }
        double Duration { get; }
        double MediaStart { get; }
        MediaFile File { get; }
        ResizeFlags StretchMode { get; set; }
        IClipContainer Container { get; }
    }

    public interface IGroup : IComposition, IDisposable
    {
        ITimeline Timeline { get; }
        GroupType Type { get; }
        double FPS { get; }
    }

    public interface IGroupContainer
    {
        event EventHandler BeforeGroupAdded;
        event EventHandler<AfterGroupAddedEventArgs> AfterGroupAdded;
        AddOnlyList<IGroup> Groups { get; }
        IGroup AddAudioGroup(string name, double fps);
        IGroup AddVideoGroup(string name, double fps, short bitCount, int width, int height);
        IGroup AddAudioGroup(string name);
        IGroup AddVideoGroup(string name, short bitCount, int width, int height);
        IGroup AddAudioGroup();
        IGroup AddVideoGroup(short bitCount, int width, int height);
        IGroup FindFirstGroupOfType(GroupType type);
    }

    public interface IAudioVideoClipPair
    {
        IClip AudioClip { get; }
        IClip VideoClip { get; }
    }

    public interface ITimeline : IGroupContainer, IDisposable
    {
        event EventHandler BeforeClipAdded;
        event EventHandler<AfterClipAddedEventArgs> AfterClipAdded;
        event EventHandler BeforeCompositionAdded;
        event EventHandler<AfterCompositionAddedEventArgs> AfterCompositionAdded;
        event EventHandler BeforeTrackAdded;
        event EventHandler<AfterTrackAddedEventArgs> AfterTrackAdded;
        event EventHandler BeforeTransitionAdded;
        event EventHandler<AfterTransitionAddedEventArgs> AfterTransitionAdded;
        event EventHandler<AfterEffectAddedEventArgs> AfterEffectAdded;
        event EventHandler BeforeEffectAdded;

        double FPS { get; }
        IAMTimeline DesTimeline { get; }
        double Duration { get; }
        long LengthInUnits { get; }

        IClip AddClip(string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddVideo(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddVideo(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipEnd);
        IClip AddVideo(string fileName, double offset);
        IClip AddVideo(string fileName);

        IClip AddImage(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddImage(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipEnd);
        IClip AddImage(string fileName, double offset);
        IClip AddImage(string fileName);

        IClip AddAudio(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddAudio(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipEnd);
        IClip AddAudio(string fileName, double offset);
        IClip AddAudio(string fileName);

        // TODO: add support for shadowing inputs (copy source file)

        IAudioVideoClipPair AddVideoWithAudio(string name, string fileName, InsertPosition position, double offset,
                                              double clipStart, double clipEnd);

        IAudioVideoClipPair AddVideoWithAudio(string fileName, InsertPosition position, double offset, double clipStart,
                                              double clipEnd);

        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipStart, double clipEnd);
        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipEnd);
        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset);
        IAudioVideoClipPair AddVideoWithAudio(string fileName);

        void InstallAssistant(IMediaFileAssistant assistant);
        IEnumerable<IMediaFileAssistant> Assitants { get; }
    }

    public interface IMediaFileAssistant
    {
        bool WillAssist(MediaFile file);
        IDisposable Assist(MediaFile file);
    }
}