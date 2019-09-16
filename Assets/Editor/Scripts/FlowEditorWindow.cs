using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UniFlow.Editor
{
    internal class UniFlowSettings : ScriptableSingleton<UniFlowSettings>
    {
        [SerializeField] private Vector3 latestPosition = Vector3.zero;
        [SerializeField] private Vector3 latestScale = Vector3.one;

        internal Vector3 LatestPosition
        {
            get => latestPosition;
            set => latestPosition = value;
        }
        internal Vector3 LatestScale
        {
            get => latestScale;
            set => latestScale = value;
        }
        internal bool IsPrefabMode => SelectedGameObject != default && !SelectedGameObject.scene.IsValid();
        internal GameObject SelectedGameObject { get; set; } = null;
    }

    public class FlowEditorWindow : EditorWindow
    {
        internal static FlowEditorWindow Window { get; private set; } = default;

        internal FlowGraphView FlowGraphView { get; private set; }

        [SerializeField] private List<ConnectableInfo> connectableInfoList = new List<ConnectableInfo>();
        [SerializeField] [UsedImplicitly] private int counter = default;

        public IList<ConnectableInfo> ConnectableInfoList => connectableInfoList;

        [MenuItem("Window/UniFlow/Open UniFlow Graph")]
        public static void Open()
        {
            GetWindow<FlowEditorWindow>();
            Window.titleContent = new GUIContent("UniFlow Graph");

        }

        public void ForceRegisterUndo()
        {
            counter++;
        }

        private void Reload()
        {
            AssetReferences.Reload();

            rootVisualElement.Clear();
            connectableInfoList = new List<ConnectableInfo>();
            var flowVisualElement = new FlowVisualElement
            {
                name = typeof(FlowVisualElement).Name,
            };
            flowVisualElement.Initialize();
            rootVisualElement.Add(flowVisualElement);
            FlowGraphView = flowVisualElement.FlowGraphView;

            Repaint();
        }

        private void OnEnable()
        {
            Window = this;
            Undo.undoRedoPerformed += Reload;
            Reload();
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Reload;
        }

        private void OnDestroy()
        {
            Window = null;
        }
    }
}
