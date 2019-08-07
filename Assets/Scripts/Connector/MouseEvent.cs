using System;
using EventConnector.Message;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace EventConnector.Connector
{
    public class MouseEvent : EventConnector
    {
        [SerializeField] private MouseEventType mouseEventType = default;
        [SerializeField]
        [Tooltip("If you do not specify it will be used self instance")]
        private Component component = default;

        private MouseEventType MouseEventType => mouseEventType;
        private Component Component => component ? component : component = this;

        public override IObservable<EventMessage> FooAsObservable() =>
            OnEventAsObservable()
                .Select(_ => EventMessage.Create(EventType.MouseEvent, Component, MouseEventData.Create(MouseEventType)));

        protected override void Connect(EventMessages eventMessages)
        {
            OnEventAsObservable()
                .SubscribeWithState(
                    eventMessages,
                    (_, em) => em.Append(EventMessage.Create(EventType.MouseEvent, Component, MouseEventData.Create(MouseEventType)))
                );
        }

        private IObservable<Unit> OnEventAsObservable()
        {
            switch (MouseEventType)
            {
                case MouseEventType.MouseDown:
                    return Component.OnMouseDownAsObservable();
                case MouseEventType.MouseUp:
                    return Component.OnMouseUpAsObservable();
                case MouseEventType.MouseUpAsButton:
                    return Component.OnMouseUpAsButtonAsObservable();
                case MouseEventType.MouseEnter:
                    return Component.OnMouseEnterAsObservable();
                case MouseEventType.MouseExit:
                    return Component.OnMouseExitAsObservable();
                case MouseEventType.MouseOver:
                    return Component.OnMouseOverAsObservable();
                case MouseEventType.MouseDrag:
                    return Component.OnMouseDragAsObservable();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum MouseEventType
    {
        MouseDown,
        MouseUp,
        MouseUpAsButton,
        MouseEnter,
        MouseExit,
        MouseOver,
        MouseDrag,
    }
}