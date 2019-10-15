using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniFlow.Attribute;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UniFlow.Editor
{
    public class FlowGraphView : GraphView
    {
        private IList<IConnector> Connectors { get; set; } = new List<IConnector>();
        private IDictionary<IConnector, IList<IConnector>> DestinationConnectors { get; } = new Dictionary<IConnector, IList<IConnector>>();
        private IDictionary<IConnector, IList<IConnector>> SourceConnectors { get; } = new Dictionary<IConnector, IList<IConnector>>();
        private IDictionary<IConnector, FlowNode> RenderedNodes { get; } = new Dictionary<IConnector, FlowNode>();
        private IDictionary<FlowNode, Vector2Int> NormalizedPositionDictionary { get; } = new Dictionary<FlowNode, Vector2Int>();
        private SearchWindowProvider SearchWindowProvider { get; set; }
        private EdgeConnectorListener EdgeConnectorListener { get; set; }
        private ValueInjectionConnectorListener ValueInjectionConnectorListener { get; set; }
        private MessageConnectorListener MessageConnectorListener { get; set; }

        private const float NodeWidth = 300.0f;
        private static Vector2 NodesOffset { get; } = new Vector2(50.0f, 50.0f);
        private static Vector2 NodeMargin { get; } = new Vector2(50.0f, 50.0f);

        public void Initialize()
        {
            UpdateViewTransform(UniFlowSettings.instance.LatestPosition, UniFlowSettings.instance.LatestScale);
            styleSheets.Add(AssetReferences.Instance.FlowGraphView);
            SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            var flowGridBackground = new FlowGridBackground
            {
                name = typeof(FlowGridBackground).Name,
            };
            Insert(0, flowGridBackground);

            SearchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            SearchWindowProvider.Initialize(this);
            EdgeConnectorListener = new EdgeConnectorListener(this, SearchWindowProvider);
            ValueInjectionConnectorListener = new ValueInjectionConnectorListener(this);
            MessageConnectorListener = new MessageConnectorListener(this);

            viewTransformChanged += graphView =>
            {
                UniFlowSettings.instance.LatestPosition = graphView.viewTransform.position;
                UniFlowSettings.instance.LatestScale = graphView.viewTransform.scale;
            };

            nodeCreationRequest += context =>
            {
                SearchWindowProvider.FlowPort = null;
                SearchWindow
                    .Open(
                        new SearchWindowContext(context.screenMousePosition),
                        SearchWindowProvider
                    );
            };

            deleteSelection += (operationName, user) =>
            {
                DeleteSelection();
                SetupActAsTrigger();
            };

            elementsRemovedFromStackNode += (stackNode, removedGraphElements) =>
            {
                removedGraphElements.ToList().ForEach(Debug.Log);
                SetupActAsTrigger();
            };
            elementsRemovedFromGroup += (group, removedGraphElements) =>
            {
                removedGraphElements.ToList().ForEach(Debug.Log);
                SetupActAsTrigger();
            };

            graphViewChanged += change =>
            {
                change
                    .elementsToRemove?
                    .OfType<IRemovableElement>()
                    .ToList()
                    .ForEach(
                        x => x.RemoveFromGraphView()
                    );
                SetupActAsTrigger();
                return change;
            };

            CreateNodesFromInstance();

            CreateEdges();

            CreateMessageConnectorEdges();

#region Will be removed

            CreateValueInjectionEdges();

#endregion

            RegisterCallback(
                (GeometryChangedEvent e) =>
                {
                    Relocation();
                }
            );
            RegisterCallback(
                (KeyDownEvent e) =>
                {
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (e.keyCode)
                    {
                        case KeyCode.C:
                            SearchWindowProvider.FlowPort = null;
                            SearchWindow
                                .Open(
                                    new SearchWindowContext(GUIUtility.GUIToScreenPoint(layout.position) + new Vector2(125.0f, 55.0f)),
                                    SearchWindowProvider
                                );
                            break;
                    }
                }
            );
        }

        public void Relocation(bool forceReset = false)
        {
            var nextYList = new List<float>();

            var groupId = Undo.GetCurrentGroup();

            if (!NormalizedPositionDictionary.Any())
            {
                return;
            }

            var maxX = NormalizedPositionDictionary.Max(x => x.Value.x);
            for (var i = 0; i <= maxX; i++)
            {
                nextYList.Add(0.0f);
            }

            foreach (var pair in NormalizedPositionDictionary)
            {
                var node = pair.Key;
                var normalizedPosition = pair.Value;

                if (!forceReset && node.GetRecordedPosition().magnitude > 0.0f)
                {
                    node.SetPosition(new Rect(node.GetRecordedPosition(), node.layout.size));
                }
                else
                {
                    node.SetPosition(
                        new Rect(
                            new Vector2(normalizedPosition.x * (NodeWidth + NodeMargin.x) + NodesOffset.x, nextYList[normalizedPosition.x] + NodesOffset.y),
                            node.layout.size
                        )
                    );
                    node.ApplyPosition();
                }

                nextYList[normalizedPosition.x] += node.layout.height + NodeMargin.y;
            }

            Undo.CollapseUndoOperations(groupId);
        }

        internal Vector2 NormalizeMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer
                .WorldToLocal(
                    FlowEditorWindow
                        .Window
                        .rootVisualElement
                        .ChangeCoordinatesTo(
                            FlowEditorWindow.Window.rootVisualElement.parent,
                            mousePosition - FlowEditorWindow.Window.position.position
                        )
                );
        }

        internal void SetupActAsTrigger()
        {
            this.Query()
                .Children<FlowNode>()
                .Where(x => x.ConnectorInfo.Connector is ConnectorBase)
                .ForEach(x => ((ConnectorBase) x.ConnectorInfo.Connector).ActAsTrigger = (x.InputPort.connections == null || !x.InputPort.connections.Any()));
        }

        private void CreateNodesFromInstance()
        {
            Connectors = UniFlowSettings.instance.IsPrefabMode
                ? UniFlowSettings.instance.SelectedGameObject
                    .GetComponentsInChildren<ConnectorBase>(true)
                    .OfType<IConnector>()
                    .ToList()
                : (UniFlowSettings.instance.SelectedGameObject == default ? SceneManager.GetActiveScene() : UniFlowSettings.instance.SelectedGameObject.scene)
                    .GetRootGameObjects()
                    .SelectMany(x => x.GetComponentsInChildren<ConnectorBase>(true))
                    .OfType<IConnector>()
                    .ToList();


            foreach (var connector in Connectors)
            {
                if (!DestinationConnectors.ContainsKey(connector))
                {
                    DestinationConnectors[connector] = new List<IConnector>();
                }

                if (connector is ConnectorBase connectorBase)
                {
                    foreach (var targetConnector in connectorBase.TargetComponents)
                    {
                        if (targetConnector == default)
                        {
                            continue;
                        }

                        if (!SourceConnectors.ContainsKey(targetConnector))
                        {
                            SourceConnectors[targetConnector] = new List<IConnector>();
                        }

                        DestinationConnectors[connectorBase].Add(targetConnector);
                        SourceConnectors[targetConnector].Add(connectorBase);
                    }
                }
            }

            var rootConnectables = Connectors.Where(x => !SourceConnectors.ContainsKey(x)).ToArray();
            if (!rootConnectables.Any() && Connectors.Any())
            {
                rootConnectables = new[] {Connectors.First()};
            }

            if (UniFlowSettings.instance.SelectedGameObject != default && !UniFlowSettings.instance.IsPrefabMode)
            {
                rootConnectables = rootConnectables.Where(x => x is ConnectorBase connector && connector != default && ContainsSelectedGameObjectInConnectableTree(connector)).ToArray();
            }

            foreach (var (rootConnectable, index) in rootConnectables.Select((v, i) => (v, i)))
            {
                CreateNodeRecursive(rootConnectable, new Vector2Int(0, index));
            }
        }

        private static bool ContainsSelectedGameObjectInConnectableTree(ConnectorBase haystack)
        {
            if (haystack.gameObject == UniFlowSettings.instance.SelectedGameObject)
            {
                return true;
            }

            return haystack
                .TargetComponents
                .Where(x => x != default)
                .Any(
                    x =>
                        x.gameObject == UniFlowSettings.instance.SelectedGameObject
                        || x is ConnectorBase child && child != default && ContainsSelectedGameObjectInConnectableTree(child)
                );
        }

        private void CreateNodeRecursive(IConnector connectable, Vector2Int normalizedPosition)
        {
            var node = AddNode(connectable.GetType(), connectable, Vector2.zero);
            NormalizedPositionDictionary[node] = normalizedPosition;
            RenderedNodes[connectable] = node;

            // ReSharper disable once InvertIf
            if (DestinationConnectors.ContainsKey(connectable))
            {
                foreach (var (targetConnectable, index) in DestinationConnectors[connectable].Select((v, i) => (v, i)))
                {
                    if (!RenderedNodes.ContainsKey(targetConnectable))
                    {
                        CreateNodeRecursive(targetConnectable, new Vector2Int(normalizedPosition.x + 1, normalizedPosition.y + index));
                    }
                    else if (NormalizedPositionDictionary[RenderedNodes[targetConnectable]].x < normalizedPosition.x + 1)
                    {
                        NormalizedPositionDictionary[RenderedNodes[targetConnectable]] = new Vector2Int(normalizedPosition.x + 1, normalizedPosition.y + index);
                    }
                }
            }
        }

        private void CreateEdges()
        {
            foreach (var (sourceConnector, targetConnectors) in DestinationConnectors.Select(x => (x.Key, x.Value)))
            {
                foreach (var targetConnector in targetConnectors)
                {
                    if (!RenderedNodes.ContainsKey(sourceConnector) || !RenderedNodes.ContainsKey(targetConnector))
                    {
                        continue;
                    }

                    AddEdge((FlowPort) RenderedNodes[sourceConnector].OutputPort, (FlowPort) RenderedNodes[targetConnector].InputPort);
                }
            }
        }

        private void CreateMessageConnectorEdges()
        {
            foreach (var connector in Connectors)
            {
                var valueCollectors = connector.GetType()
                    .GetPropertiesRecursive(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .Where(x => typeof(IValueCollector).IsAssignableFrom(x.PropertyType))
                    .Select(x => x.GetMethod.Invoke(connector, null) as IValueCollector)
                    .ToArray();
                foreach (var valueCollector in valueCollectors)
                {
                    if (!RenderedNodes.ContainsKey(valueCollector.Connector) || !(valueCollector.Connector is IMessageComposable) || !(connector is IMessageCollectable))
                    {
                        continue;
                    }

                    var messageComposeNode = RenderedNodes[valueCollector.Connector];
                    var messageCollectNode = RenderedNodes[connector];
                    var messageComposePort = messageComposeNode.MessageComposePorts.FirstOrDefault(x => x.ComposableMessageAnnotation.Type.AssemblyQualifiedName == valueCollector.TypeString && (string.IsNullOrEmpty(valueCollector.ComposerKey) || x.ComposableMessageAnnotation.Key == valueCollector.ComposerKey));
                    var messageCollectPort = messageCollectNode.MessageCollectPorts.FirstOrDefault(x => x.CollectableMessageAnnotation.Type.AssemblyQualifiedName == valueCollector.TypeString && (string.IsNullOrEmpty(valueCollector.ComposerKey) || x.CollectableMessageAnnotation.Label == valueCollector.CollectorLabel));
                    if (messageComposePort != null && messageCollectPort != null)
                    {
                        AddEdge(messageComposePort, messageCollectPort);
                    }
                }
            }
        }

#region Will be removed

        private void CreateValueInjectionEdges()
        {
            foreach (var connector in Connectors)
            {
                var persistentEventsList = connector.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .Where(x => x.GetCustomAttribute<ValuePublisherAttribute>() != null && typeof(UnityEventBase).IsAssignableFrom(x.PropertyType))
                    .Select(x => (propertyInfo: x, unityEvent: x.GetValue(connector) as UnityEventBase))
                    .Where(x => x.unityEvent != null)
                    .Select(x => Enumerable.Range(0, x.unityEvent.GetPersistentEventCount()).Select(index => (x.propertyInfo, target: x.unityEvent.GetPersistentTarget(index), methodName: x.unityEvent.GetPersistentMethodName(index))))
                    .ToArray();
                foreach (var persistentEvents in persistentEventsList)
                {
                    foreach (var (propertyInfo, target, methodName) in persistentEvents)
                    {
                        if (!(target is IConnector targetConnector) || !RenderedNodes.ContainsKey(targetConnector) || !RenderedNodes.ContainsKey(connector))
                        {
                            continue;
                        }

                        var publisherNode = RenderedNodes[connector];
                        var receiverNode = RenderedNodes[targetConnector];
                        var publisherPort = publisherNode.ValuePublishPorts.FirstOrDefault(x => x.ValuePublisherInfo.PropertyInfo == propertyInfo);
                        var receiverPort = receiverNode.ValueReceivePorts.FirstOrDefault(x => x.ValueReceiverInfo.PropertyInfo.GetSetMethod(true).Name == methodName);
                        if (publisherPort != null && receiverPort != null)
                        {
                            AddEdge(publisherPort, receiverPort);
                        }
                    }
                }
            }
        }

#endregion

        public FlowNode AddNode(Type connectableType, IConnector connectableInstance, Vector2 position)
        {
            var connectableInfo = ConnectorInfo.Create(connectableType, connectableInstance);
            FlowEditorWindow.Window.ConnectableInfoList.Add(connectableInfo);
            var node = new FlowNode(connectableInfo, EdgeConnectorListener, ValueInjectionConnectorListener, MessageConnectorListener);
            node.Initialize();
            node.SetPosition(new Rect(position.x, position.y, 0, 0));
            AddElement(node);
            return node;
        }

        public FlowEdge AddEdge(Port outputPort, Port inputPort)
        {
            var edge = new FlowEdge
            {
                output = outputPort,
                input = inputPort,
            };
            edge.output.Connect(edge);
            edge.input.Connect(edge);
            AddElement(edge);
            return edge;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (startPort is FlowValuePublishPort startFlowValuePublishPort)
            {
                return ports
                    .ToList()
                    .Where(
                        x =>
                            x is FlowValueReceivePort flowValueReceivePort
                            && (
                                flowValueReceivePort.ValueReceiverInfo.Type.IsAssignableFrom(startFlowValuePublishPort.ValuePublisherInfo.Type)
                                || typeof(ScriptableObject).IsAssignableFrom(startFlowValuePublishPort.ValuePublisherInfo.Type) && typeof(ScriptableObject).IsAssignableFrom(flowValueReceivePort.ValueReceiverInfo.Type)
                            )
                            && x.direction != startPort.direction
                            && x.node != startPort.node
                    )
                    .ToList();
            }

            if (startPort is FlowValueReceivePort startFlowValueReceivePort)
            {
                return ports
                    .ToList()
                    .Where(
                        x =>
                            x is FlowValuePublishPort flowValuePublishPort
                            && startFlowValueReceivePort.ValueReceiverInfo.Type.IsAssignableFrom(flowValuePublishPort.ValuePublisherInfo.Type)
                            && x.direction != startPort.direction
                            && x.node != startPort.node
                    )
                    .ToList();
            }

            if (startPort is FlowMessageComposePort startFlowMessageComposePort)
            {
                return ports
                    .ToList()
                    .Where(
                        x =>
                            x is FlowMessageCollectPort flowMessageCollectPort
                            && (
                                flowMessageCollectPort.CollectableMessageAnnotation.Type.IsAssignableFrom(startFlowMessageComposePort.ComposableMessageAnnotation.Type)
                                || typeof(ScriptableObject).IsAssignableFrom(startFlowMessageComposePort.ComposableMessageAnnotation.Type) && typeof(ScriptableObject).IsAssignableFrom(flowMessageCollectPort.CollectableMessageAnnotation.Type)
                            )
                            && x.direction != startPort.direction
                            && x.node != startPort.node
                    )
                    .ToList();
            }

            if (startPort is FlowMessageCollectPort startFlowMessageCollectPort)
            {
                return ports
                    .ToList()
                    .Where(
                        x =>
                            x is FlowMessageComposePort flowMessageComposePort
                            && startFlowMessageCollectPort.CollectableMessageAnnotation.Type.IsAssignableFrom(flowMessageComposePort.ComposableMessageAnnotation.Type)
                            && x.direction != startPort.direction
                            && x.node != startPort.node
                    )
                    .ToList();
            }

            return ports
                .ToList()
                .Where(x => x is FlowPort && x.direction != startPort.direction && x.node != startPort.node)
                .ToList();
        }
    }
}
