using System;
using EventConnector.Message;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace EventConnector.Connector
{
    public class CameraEvent : EventConnector, IEventPublisher
    {
        [SerializeField] private CameraEventType cameraEventType = default;
        [SerializeField]
        [Tooltip("If you do not specify it will be used self instance")]
        private Component component = default;

        private CameraEventType CameraEventType => cameraEventType;
        private Component Component => component ? component : component = this;

        IObservable<EventMessage> IEventPublisher.OnPublishAsObservable() =>
            OnEventAsObservable()
                .Select(_ => EventMessage.Create(EventType.CameraEvent, Component, CameraEventData.Create(CameraEventType)));

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