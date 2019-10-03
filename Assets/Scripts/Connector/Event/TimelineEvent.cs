using System;
using System.Linq;
using JetBrains.Annotations;
using UniFlow.Attribute;
using UniFlow.Signal;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UniFlow.Connector.Event
{
    [AddComponentMenu("UniFlow/Event/TimelineEvent", (int) ConnectorType.TimelineEvent)]
    public class TimelineEvent : ConnectorBase
    {
        [SerializeField] private GameObject baseGameObject = default;
        [SerializeField] private PlayableDirector playableDirector = default;
        [SerializeField] private TimelineEventType timelineEventType = TimelineEventType.Play;
        [SerializeField] private TimelineAsset timelineAsset = default;

        [ValueReceiver] public GameObject BaseGameObject
        {
            get => baseGameObject == default ? baseGameObject = gameObject : baseGameObject;
            set => baseGameObject = value;
        }
        [ValuePublisher] public PlayableDirector PlayableDirector
        {
            get =>
                playableDirector != default
                    ? playableDirector
                    : playableDirector =
                        BaseGameObject.GetComponent<PlayableDirector>() != default
                            ? BaseGameObject.GetComponent<PlayableDirector>()
                            : BaseGameObject.AddComponent<PlayableDirector>();
            set => playableDirector = value;
        }
        [UsedImplicitly] public TimelineEventType TimelineEventType
        {
            get => timelineEventType;
            set => timelineEventType = value;
        }
        [ValuePublisher] public TimelineAsset TimelineAsset
        {
            get => timelineAsset;
            set => timelineAsset = value;
        }

        private static Begin beginSignal = default;
        private static End endSignal = default;
        private static Begin BeginSignal
        {
            get
            {
                // ReSharper disable once InvertIf
                if (beginSignal == default)
                {
                    beginSignal = ScriptableObject.CreateInstance<Begin>();
                    beginSignal.IsTemporaryInstance = true;
                }
                return beginSignal;
            }
        }

        private static End EndSignal
        {
            get
            {
                // ReSharper disable once InvertIf
                if (endSignal == default)
                {
                    endSignal = ScriptableObject.CreateInstance<End>();
                    endSignal.IsTemporaryInstance = true;
                }
                return endSignal;
            }
        }

        private ISubject<TimelineEventType> SignalEmittedSubject { get; } = new Subject<TimelineEventType>();

        protected override void Start()
        {
            RegisterSignal();
            base.Start();
        }

        private void OnDestroy()
        {
            if (baseGameObject == null || playableDirector == null || !(PlayableDirector.playableAsset is TimelineAsset timeline))
            {
                return;
            }

            foreach (var marker in timeline.markerTrack.GetMarkers().OfType<SignalEmitter>().Where(x => x.asset is SignalAssetBase signalAsset && signalAsset.IsTemporaryInstance))
            {
                timeline.markerTrack.DeleteMarker(marker);
            }
        }

        public override IObservable<Unit> OnConnectAsObservable()
        {
            return SignalEmittedSubject
                .AsUnitObservable();
        }

        private void RegisterSignal()
        {
            if (!(PlayableDirector.playableAsset is TimelineAsset timeline))
            {
                return;
            }

            var time = TimelineEventType == TimelineEventType.Play ? 0 : timeline.duration;
            var signal = TimelineEventType == TimelineEventType.Play ? (SignalAsset) BeginSignal : EndSignal;
            var emitter = timeline
                .markerTrack
                .GetMarkers()
                .FirstOrDefault(
                    x => x is SignalEmitter signalEmitter
                        && signalEmitter != default
                        && Math.Abs(signalEmitter.time - time) < double.Epsilon
                        && signalEmitter.asset != default
                        && signalEmitter.asset.GetType() == signal.GetType()
                ) as SignalEmitter;
            if (emitter == default)
            {
                emitter = timeline.markerTrack.CreateMarker<SignalEmitter>(time);
                emitter.asset = signal;
            }

            var receiver = PlayableDirector.gameObject.GetComponent<SignalReceiver>();
            if (receiver == default)
            {
                receiver = PlayableDirector.gameObject.AddComponent<SignalReceiver>();
            }

            if (receiver.GetReaction(signal) == default)
            {
                receiver.AddReaction(signal, new UnityEvent());
            }

            receiver.GetReaction(signal).AddListener(TimelineEventType == TimelineEventType.Play ? (UnityAction) DispatchBegin : DispatchEnd);
        }

        private void DispatchBegin()
        {
            SignalEmittedSubject.OnNext(TimelineEventType.Play);
        }

        private void DispatchEnd()
        {
            SignalEmittedSubject.OnNext(TimelineEventType.Stop);
        }
    }

    public enum TimelineEventType
    {
        Play,
        Stop,
    }
}
