using System;
using JetBrains.Annotations;
using UniFlow.Attribute;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace UniFlow.Connector.Event
{
    [AddComponentMenu("UniFlow/Event/MouseEvent", (int) ConnectorType.MouseEvent)]
    public class MouseEvent : ConnectorBase
    {
        [SerializeField] private Component component = default;
        [SerializeField] private MouseEventType mouseEventType = MouseEventType.MouseDown;

        [ValuePublisher] public Component Component
        {
            get => component ? component : component = this;
            set => component = value;
        }
        [UsedImplicitly] public MouseEventType MouseEventType
        {
            get => mouseEventType;
            set => mouseEventType = value;
        }

        public override IObservable<Unit> OnConnectAsObservable()
        {
            return OnEventAsObservable();
        }

        private IObservable<Unit> OnEventAsObservable()
        {
#if !(UNITY_IOS || UNITY_ANDROID || UNITY_METRO)
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
#else
            throw new PlatformNotSupportedException("MouseEvent does not support mobile platform");
#endif
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
