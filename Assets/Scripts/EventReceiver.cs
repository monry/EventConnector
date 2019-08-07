using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace EventConnector
{
    public abstract class EventReceiver : EventConnector, IEventReceiver
    {
//        [SerializeField]
//        [Tooltip("Specify instances of IEventConnector directly")]
//        private List<EventConnector> sourceConnectorInstances = default;
//        [SerializeField]
//        [Tooltip("Specify identifiers of IEventConnector that resolve from Zenject.DiContainer")]
//        private List<string> sourceConnectorIds = default;
//
//        private IEnumerable<IEventConnector> SourceConnectors =>
//            new List<IEventConnector>()
//                .Concat(sourceConnectorInstances ?? new List<EventConnector>())
//                .Concat((sourceConnectorIds ?? new List<string>()).SelectMany(Container.ResolveIdAll<IEventConnector>));
//
//        [Inject] private DiContainer Container { get; }

//        private IEnumerator Start()
//        {
//            if (!MainThreadDispatcher.IsInitialized)
//            {
//                MainThreadDispatcher.Initialize();
//
//                while (!MainThreadDispatcher.IsInitialized)
//                {
//                    yield return new WaitForEndOfFrame();
//                }
//            }
//
//            GenerateSourceObservable()
//                .Subscribe(Receive)
//                .AddTo(this);
//        }

        public abstract void Receive(EventMessages eventMessages);

//        private IObservable<EventMessages> GenerateSourceObservable() =>
//            SourceConnectors.Any()
//                ? SourceConnectors.Select(x => x.ConnectAsObservable()).Merge()
//                : Observable.Defer(() => Observable.Return(EventMessages.Create()));
    }
}