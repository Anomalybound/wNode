using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using wNode.Core;
using wNode.Utilities;
using NodeData = wNode.Wrapper.NodeData;

namespace wNode.Editors
{
    public partial class BaseNodeWindow
    {
        public NodeEditor GetNodeEditor(NodeData nodeData)
        {
            var node = nodeData.Node;
            var nodeType = node.GetType();
            if (NodeCache.NodeEditors.ContainsKey(node))
            {
                return NodeCache.NodeEditors[node];
            }

            NodeEditor nodeEditor;
            if (!NodeCache.CachedNodeEditors.ContainsKey(nodeType))
            {
                nodeEditor = CreateInstance(typeof(BaseNodeEditor)) as NodeEditor;
            }
            else
            {
                var editorType = NodeCache.CachedNodeEditors[nodeType];
                nodeEditor = CreateInstance(editorType) as NodeEditor;
            }

            nodeEditor.InitializeEditor(nodeData);
            NodeCache.NodeEditors.Add(node, nodeEditor);
            return nodeEditor;
        }

        #region Coords Calculations

        public static Vector2 ScreenToGraphSpace(Vector2 screenPos)
        {
            var graphRect = Instance.GridSize;
            var center = graphRect.size / 2f;
            return (screenPos - center) * Instance.ZoomScale - Instance.Offset;
        }

        public static Vector2 GraphToScreenSpace(Vector2 graphPos)
        {
            return graphPos + Instance._zoomAdjustment + Instance.Offset;
        }

        public Vector2 GraphMousePosition()
        {
            return ScreenToGraphSpace(Event.current.mousePosition);
        }

        #endregion

        #region Detections

        private string MouseOverNode()
        {
            var mousePosition = GraphMousePosition();
            foreach (var nodeData in LoadedGraph.Nodes.Values)
            {
                var checkRect = nodeData.HeaderRect;
                if (checkRect.Contains(mousePosition))
                {
                    return nodeData.Id;
                }
            }

            return null;
        }

        private string MouseOverPort()
        {
            var mousePosition = GraphMousePosition();
            foreach (var portData in LoadedGraph.Ports.Values)
            {
                if (portData.Rect.Contains(mousePosition))
                {
                    return portData.PortId;
                }
            }

            return null;
        }

        #endregion

        private void PanBackground(Event evt)
        {
            Offset += evt.delta * ZoomScale;
        }

        public void Zoom(float zoomDirection)
        {
            var scale = zoomDirection < 0f
                ? 1f - NodePreferences.Instance.ZoomDelta
                : 1f + NodePreferences.Instance.ZoomDelta;
            _zoom *= scale;

            var cap = Mathf.Clamp(_zoom.x, NodePreferences.Instance.MinZoom, NodePreferences.Instance.MaxZoom);
            _zoom.Set(cap, cap);
        }

        #region Debug Panels

        private bool _debugToggle;

        private static readonly Dictionary<string, object> _debugMessages = new Dictionary<string, object>();

        public static void UpdateDebugMessage(string key, object value)
        {
            if (_debugMessages.ContainsKey(key))
            {
                _debugMessages[key] = value;
            }
            else
            {
                _debugMessages.Add(key, value);
            }
        }

        public void DrawDebugPanel()
        {
            GUILayout.BeginArea(new Rect(0, 17, 300, 1000));
            GUILayout.BeginVertical(EditorStyles.helpBox);

            UpdateDebugMessage("Pan Offset", Offset);
            UpdateDebugMessage("Zoom", _zoom);
            UpdateDebugMessage("Selected Nodes", SelectedNodes.Count);
            UpdateDebugMessage("Mouse Position(ori)", Event.current.mousePosition);
            UpdateDebugMessage("Mouse Position", GraphMousePosition());

            var hoverNode = MouseOverNode();
            if (hoverNode != null)
            {
                GUILayout.Label("Hover Node: " + hoverNode);
            }
            else
            {
                GUILayout.Label("Hover Node: " + "None");
            }

            foreach (var pair in _debugMessages)
            {
                EditorGUILayout.LabelField(pair.Key, pair.Value != null ? pair.Value.ToString() : "Null");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            Repaint();
        }

        #endregion

        private void OpenFilePanel(Rect displayRect)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Open"), false, () =>
            {
                var path = EditorUtility.OpenFilePanelWithFilters("Open Node Graph", Application.dataPath,
                    new[] {"Graph", "asset"});

                if (!string.IsNullOrEmpty(path))
                {
                    path = path.Remove(path.IndexOf(Application.dataPath), Application.dataPath.Length);
                    LoadedGraph = AssetDatabase.LoadAssetAtPath<NodeGraph>("Assets" + path);
                }
            });

            if (LoadedGraph != null)
            {
                menu.AddItem(new GUIContent("Save"), false, () =>
                {
                    EditorUtility.SetDirty(LoadedGraph);
                    AssetDatabase.SaveAssets();
                });
            }

            menu.DropDown(displayRect);
        }
    }
}