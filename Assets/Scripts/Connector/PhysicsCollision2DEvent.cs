using System;
using EventConnector.Message;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace EventConnector.Connector
{
    public class PhysicsCollision2DEvent : EventConnector, IEventPublisher
    {
        [SerializeField] private PhysicsCollision2DEventType physicsCollision2DEventType = default;
        [SerializeField]
        [Tooltip("If you do not specify it will be used self instance")]
        private Component component = default;

        private PhysicsCollision2DEventType PhysicsCollision2DEventType => physicsCollision2DEventType;
        private Component Component => component ? component : component = this;

        IObservable<EventMessage> IEventPublisher.OnPublishAsObservable() =>
            OnEventAsObservable()
                .Select(x => EventMessage.Create(EventType.PhysicsCollision2DEvent, Component, PhysicsCollision2DEventData.Create(PhysicsCollision2DEventType, x)));

        private IObservable<Collision2D> OnEventAsObservable()
        {
            switch (PhysicsCollision2DEventType)
            {
                case PhysicsCollision2DEventType.CollisionEnter2D:
                    return Component.OnCollisionEnter2DAsObservable();
                case PhysicsCollision2DEventType.CollisionExit2D:
                    return Component.OnCollisionExit2DAsObservable();
                case PhysicsCollision2DEventType.CollisionStay2D:
                    return Component.OnCollisionStay2DAsObservable();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum PhysicsCollision2DEventType
    {
        CollisionEnter2D,
        CollisionExit2D,
        CollisionStay2D,
    }
}