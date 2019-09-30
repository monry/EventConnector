using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UniFlow.Connector.Controller
{
    [AddComponentMenu("UniFlow/Controller/RaycasterController", (int) ConnectorType.RaycasterController)]
    public class RaycasterController : ConnectorBase
    {
        [SerializeField] private RaycasterControlMethod raycasterControlMethod = RaycasterControlMethod.Activate;
        [SerializeField] private List<BaseRaycaster> raycasters = default;

        [UsedImplicitly] private RaycasterControlMethod RaycasterControlMethod
        {
            get => raycasterControlMethod;
            set => raycasterControlMethod = value;
        }
        [UsedImplicitly] private IEnumerable<BaseRaycaster> Raycasters
        {
            get => raycasters;
            set => raycasters = value.ToList();
        }

        private IDisposable Disposable { get; } = new CompositeDisposable();

        public override IObservable<IMessage> OnConnectAsObservable(IMessage latestMessage)
        {
            var count = HandleActivation();
            return Observable.Return(Message.Create(this, count));
//            return Observable
//                .Create<IMessage>(
//                    observer =>
//                    {
//                        var count = HandleActivation();
//                        observer.OnNext(Message.Create(this, count));
//                        return Disposable;
//                    }
//                );
        }

        private int HandleActivation()
        {
            var targets = Raycasters.Where(x => x.enabled != (RaycasterControlMethod == RaycasterControlMethod.Activate)).ToList();
            var count = targets.Count;
            targets.ForEach(x => x.enabled = RaycasterControlMethod == RaycasterControlMethod.Activate);
            return count;
        }

        public class Message : MessageBase<RaycasterController, int>
        {
            public static Message Create(RaycasterController sender, int count)
            {
                return Create<Message>(ConnectorType.RaycasterController, sender, count);
            }
        }
    }

    [PublicAPI]
    public enum RaycasterControlMethod
    {
        Activate,
        Deactivate,
    }
}
