using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace UniFlow
{
    public abstract class ConnectorBase : ConnectableBase, IConnector
    {
        [SerializeField] [Tooltip("Specify instances of IEventConnectable directly")]
        private List<ConnectableBase> targetComponents = default;

        [SerializeField] [Tooltip("Specify identifiers of IEventConnectable that resolve from Zenject.DiContainer")]
        private List<string> targetIds = default;

        [SerializeField] [Tooltip("Set true to allow to act as the entry point of events")]
        private bool actAsTrigger = false;

        [SerializeField] [Tooltip("Set true to allow to act as the receiver")]
        private bool actAsReceiver = false;

        private IEnumerable<IConnectable> TargetConnectors =>
            new List<IConnectable>()
                .Concat(targetComponents ?? new List<ConnectableBase>())
                .Concat((targetIds ?? new List<string>()).SelectMany(x => Container?.ResolveIdAll<IConnectable>(x)))
                .Where(x => !ReferenceEquals(x, this))
                .ToArray();

        [UsedImplicitly] public bool ActAsTrigger
        {
            get => actAsTrigger;
            set => actAsTrigger = value;
        }

        [UsedImplicitly] public bool ActAsReceiver
        {
            get => actAsReceiver;
            set => actAsReceiver = value;
        }

        [Inject] private DiContainer Container { get; }

        protected virtual void Start()
        {
            if (ActAsTrigger)
            {
                ((IConnector) this).Connect(Observable.Return<EventMessages>(default));
            }
        }

        void IConnector.Connect(IObservable<EventMessages> source)
        {
            var observable = source
                .SelectMany(
                    eventMessages => (this as IConnector)
                        .OnConnectAsObservable()
                        .Select(x => (eventMessages ?? EventMessages.Create()).Append(x))
                );
            TargetConnectors
                .OfType<IConnector>()
                .ToList()
                .ForEach(x => x.Connect(observable));
            TargetConnectors
                .OfType<IReceiver>()
                .ToList()
                .ForEach(x => observable.Subscribe(x.OnReceive));

            if (ActAsReceiver)
            {
                observable.Subscribe();
            }
        }

        public abstract IObservable<EventMessage> OnConnectAsObservable();

        public void AddConnectable(ConnectableBase connectable)
        {
            if (targetComponents == default)
            {
                targetComponents = new List<ConnectableBase>();
            }
            targetComponents.Add(connectable);
        }
    }
}