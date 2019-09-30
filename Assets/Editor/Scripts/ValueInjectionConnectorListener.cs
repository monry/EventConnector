using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Events;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

namespace UniFlow.Editor
{
    public class ValueInjectionConnectorListener : IEdgeConnectorListener
    {
        public ValueInjectionConnectorListener(FlowGraphView flowGraphView)
        {
            FlowGraphView = flowGraphView;
        }

        private FlowGraphView FlowGraphView { get; }

        private static IDictionary<Type, Action<object, object>> AddPersistentListenerCallbackMap { get; } = new Dictionary<Type, Action<object, object>>
        {
            {typeof(bool), (unityEvent, unityAction) => UnityEventTools.AddBoolPersistentListener(unityEvent as UnityEvent<bool>, unityAction as UnityAction<bool>, default)},
            {typeof(int), (unityEvent, unityAction) => UnityEventTools.AddIntPersistentListener(unityEvent as UnityEvent<int>, unityAction as UnityAction<int>, default)},
            {typeof(float), (unityEvent, unityAction) => UnityEventTools.AddFloatPersistentListener(unityEvent as UnityEvent<float>, unityAction as UnityAction<float>, default)},
            {typeof(string), (unityEvent, unityAction) => UnityEventTools.AddStringPersistentListener(unityEvent as UnityEvent<string>, unityAction as UnityAction<string>, default)},
        };

        void IEdgeConnectorListener.OnDropOutsidePort(Edge edge, Vector2 position)
        {
            // Do nothing
        }

        void IEdgeConnectorListener.OnDrop(GraphView graphView, Edge edge)
        {
            FlowEdge registeredEdge = default;
            switch (edge.output)
            {
                case FlowValuePublishPort _ when edge.input is FlowValueReceivePort:
                    registeredEdge = FlowGraphView.AddEdge(edge.output, edge.input);
                    break;
                case FlowValueReceivePort _ when edge.input is FlowValuePublishPort:
                    registeredEdge = FlowGraphView.AddEdge(edge.input, edge.output);
                    break;
            }

            if (registeredEdge == default)
            {
                return;
            }

            FlowGraphView.AddElement(registeredEdge);

            if (!(registeredEdge.output is FlowValuePublishPort publishPort) || !(registeredEdge.input is FlowValueReceivePort receivePort))
            {
                return;
            }

            var targetInstance = receivePort.ValueReceiverInfo.Instance;
            var setMethodInfo = receivePort.ValueReceiverInfo.PropertyInfo.GetSetMethod();
            var unityEvent = publishPort.ValuePublisherInfo.PropertyInfo.GetValue(publishPort.ValuePublisherInfo.Instance);
            var unityAction = Delegate
                .CreateDelegate(
                    typeof(UnityAction<>).MakeGenericType(setMethodInfo.GetParameters().First().ParameterType),
                    targetInstance,
                    setMethodInfo.Name,
                    false
                );

            if (AddPersistentListenerCallbackMap.ContainsKey(publishPort.ValuePublisherInfo.Type))
            {
                AddPersistentListenerCallbackMap[publishPort.ValuePublisherInfo.Type].Invoke(unityEvent, unityAction);
            }
            else if (publishPort.ValuePublisherInfo.Type.IsEnum)
            {
                AddPersistentListenerCallbackMap[typeof(int)].Invoke(unityEvent, unityAction);
            }
            else
            {
                // unityEvent.AddPersistentListener()
                unityEvent
                    .GetType()
                    .BaseType?
                    .GetMethodsRecursive(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
                    .First(x => x.Name == "AddPersistentListener" && !x.GetParameters().Any())
                    .Invoke(unityEvent, null);
                // unityEvent.m_PersistentCalls
                var persistentCalls = unityEvent
                    .GetType()
                    .GetFieldRecursive("m_PersistentCalls", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .GetValue(unityEvent);
                // unityEvent.m_PersistentCalls.RegisterObjectPersistentListener(unityEvent.GetPersistentEventCount() - 1, targetInstance, null, methodInfo.Name);
                persistentCalls
                    .GetType()
                    .GetMethodRecursive("RegisterObjectPersistentListener", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(
                        persistentCalls,
                        new[]
                        {
                            (unityEvent as UnityEventBase)?.GetPersistentEventCount() - 1,
                            targetInstance,
                            null,
                            setMethodInfo.Name
                        }
                    );
                // unityEvent.m_PersistentCalls.GetListener(unityEvent.GetPersistentEventCount() - 1)
                var persistentCall = persistentCalls
                    .GetType()
                    .GetMethodRecursive("GetListener", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(persistentCalls, new object[] {(unityEvent as UnityEventBase)?.GetPersistentEventCount() - 1});
                // unityEvent.m_PersistentCalls.GetListener(unityEvent.GetPersistentEventCount() - 1).m_Arguments
                var argumentCache = persistentCall
                    .GetType()
                    .GetFieldRecursive("m_Arguments", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .GetValue(persistentCall);
                // unityEvent.m_PersistentCalls.GetListener(unityEvent.GetPersistentEventCount() - 1).m_Arguments.m_ObjectArgumentAssemblyTypeName = setMethodInfo.GetParameters().First().ParameterType.AssemblyQualifiedName
                argumentCache
                    .GetType()
                    .GetFieldRecursive("m_ObjectArgumentAssemblyTypeName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .SetValue(argumentCache, setMethodInfo.GetParameters().First().ParameterType.AssemblyQualifiedName);
            }
        }
    }
}
