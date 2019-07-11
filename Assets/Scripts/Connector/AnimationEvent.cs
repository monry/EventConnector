using System;
using UniRx;
using UnityEngine;

namespace EventConnector.Connector
{
    // AnimationEvent cannot fire to Component attaching to another GameObject
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Event Connector/AnimationEvent")]
    public class AnimationEvent : EventConnector
    {
        private ISubject<UnityEngine.AnimationEvent> Subject { get; } = new Subject<UnityEngine.AnimationEvent>();

        protected override IObservable<EventMessages> Connect(EventMessages eventMessages) =>
            Subject
                .Select(x => eventMessages.Append((this, x)));

        public void Dispatch(UnityEngine.AnimationEvent animationEvent)
        {
            Subject.OnNext(animationEvent);
        }
    }
}