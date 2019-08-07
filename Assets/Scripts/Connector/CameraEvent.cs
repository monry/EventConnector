using System;
using EventConnector.Message;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace EventConnector.Connector
{
    public class CameraEvent : EventConnector
    {
        [SerializeField] private CameraEventType cameraEventType = default;
        [SerializeField]
        [Tooltip("If you do not specify it will be used self instance")]
        private Component component = default;

        private CameraEventType CameraEventType => cameraEventType;
        private Component Component => component ? component : component = this;

        public override IObservable<EventMessage> FooAsObservable() =>
            OnEventAsObservable()
                .Select(_ => EventMessage.Create(EventType.CameraEvent, Component, CameraEventData.Create(CameraEventType)));

        protected override void Connect(EventMessages eventMessages)
        {
            OnEventAsObservable()
                .SubscribeWithState(
                    eventMessages,
                    (_, em) => OnConnect(em.Append(EventMessage.Create(EventType.CameraEvent, Component, CameraEventData.Create(CameraEventType))))
                );
        }

        private IObservable<Unit> OnEventAsObservable()
        {
            switch (CameraEventType)
            {
                case CameraEventType.BecomeVisible:
                    return Component.OnBecameVisibleAsObservable();
                case CameraEventType.BecomeInvisible:
                    return Component.OnBecameInvisibleAsObservable();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum CameraEventType
    {
        BecomeVisible,
        BecomeInvisible,
    }
}