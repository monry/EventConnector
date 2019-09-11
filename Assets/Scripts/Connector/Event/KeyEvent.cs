using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace UniFlow.Connector.Event
{
    [AddComponentMenu("UniFlow/Event/KeyEvent", (int) ConnectorType.KeyEvent)]
    public class KeyEvent : ConnectorBase
    {
        [SerializeField] private KeyEventType keyEventType = (KeyEventType) (-1);
        [SerializeField] private KeyCode keyCode = default;

        private KeyEventType KeyEventType => keyEventType;
        private KeyCode KeyCode => keyCode;

        public override IObservable<IMessage> OnConnectAsObservable(IMessage latestMessage)
        {
            return KeyEventAsObservable().Select(_ => Message.Create(this));
        }

        private IObservable<Unit> KeyEventAsObservable()
        {
            switch (KeyEventType)
            {
                case KeyEventType.Down:
                    return this
                        .UpdateAsObservable()
                        .Where(_ => Input.GetKeyDown(KeyCode))
                        .AsUnitObservable();
                case KeyEventType.Press:
                    return this
                        .UpdateAsObservable()
                        .Where(_ => Input.GetKey(KeyCode))
                        .AsUnitObservable();
                case KeyEventType.Up:
                    return this
                        .UpdateAsObservable()
                        .Where(_ => Input.GetKeyUp(KeyCode))
                        .AsUnitObservable();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public class Message : MessageBase<KeyEvent>
        {
            public static Message Create(KeyEvent sender)
            {
                return Create<Message>(ConnectorType.KeyEvent, sender);
            }
        }
    }

    public enum KeyEventType
    {
        Down,
        Press,
        Up,
    }
}