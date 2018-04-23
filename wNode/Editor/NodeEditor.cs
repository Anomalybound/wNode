using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using wNode.Attributes;
using wNode.Core;
using wNode.Utilities;
using wNode.Wrapper;
using Random = UnityEngine.Random;

namespace wNode.Editors
{
    public abstract class NodeEditor : Editor
    {
        protected SerializedObject NodeSerializedObject;
        protected NodeData NodeData;

        private Dictionary<string, Rect> _fieldPositions = new Dictionary<string, Rect>();
        private Dictionary<string, PortData> _inputPortDatas = new Dictionary<string, PortData>();
        private Dictionary<string, PortData> _outPutPortDatas = new Dictionary<string, PortData>();

        private List<NodePort> _nodeFields;

        public void DrawNode()
        {
            if (NodeSerializedObject == null)
            {
                NodeSerializedObject = new SerializedObject(NodeData.Node);
            }

            var headerRect = NodeData.HeaderRect;
            var bodyRect = NodeData.BodyRect;

            headerRect.position = BaseNodeWindow.GraphToScreenSpace(headerRect.position);
            bodyRect.position = BaseNodeWindow.GraphToScreenSpace(bodyRect.position);

            var contentCol = GUI.contentColor;
            GUI.backgroundColor = NodeData.Node.NodeColor;
            GUILayout.BeginArea(headerRect);
            GUILayout.BeginVertical(NodeStyles.Instance.NodeHeader);
            GUI.backgroundColor = Color.white;
            OnNodeHeaderGUI();
            GUILayout.EndArea();
            GUILayout.EndVertical();

            GUI.backgroundColor = NodeStyles.Instance.NodeBodyColor;
            GUI.contentColor = NodeStyles.Instance.NodeTextColor;
            GUILayout.BeginArea(bodyRect);
            GUILayout.BeginVertical(NodeStyles.Instance.NodeBody);
            GUI.backgroundColor = Color.white;
            OnNodeBodyGUI();
            GUILayout.EndArea();
            GUILayout.EndVertical();
            GUI.contentColor = contentCol;

            // Draw ports
            DrawPorts();
        }

        public float GetNodeHeaderHeight()
        {
            return 20;
        }

        public virtual float GetNodeWidth()
        {
            return 160;
        }

        protected virtual void OnNodeHeaderGUI()
        {
            GUILayout.Label(NodeData.NodeName, NodeStyles.Instance.NodeTitleLabel);
        }

        protected virtual void OnNodeBodyGUI()
        {
            var nodeWidth = GetNodeWidth();
            using (new LabelWidthScope(nodeWidth / 2f))
            {
                DrawFields(NodeData.Node);
            }
        }

        private void DrawFields(Node node)
        {
            if (_nodeFields == null)
            {
                _nodeFields = NodeCache.NodePortCaches[node.GetType()];
                var removedPorts = NodeData.PortDatas.Where(x => _nodeFields.All(f => f.FieldName != x.FieldName))
                    .ToList();

                for (var i = 0; i < removedPorts.Count; i++)
                {
                    var removedPort = removedPorts[i];
                    NodeData.PortDatas.Remove(removedPort);
                }

                var shouldAddPorts = _nodeFields.Where(x => NodeData.PortDatas.All(f => x.FieldName != f.FieldName))
                    .ToList();
                var nodeType = NodeData.Node.GetType();

                for (var i = 0; i < shouldAddPorts.Count; i++)
                {
                    var addPort = shouldAddPorts[i];
                    var portType = addPort.InspectType;
                    var fieldInfo = nodeType.GetField(addPort.FieldName);
                    Type overrideConnectionType = null;
                    var attributes = fieldInfo.GetCustomAttributes(typeof(OverrideConnectionTypeAttribute), true);
                    if (attributes.Length > 0)
                    {
                        overrideConnectionType = ((OverrideConnectionTypeAttribute) attributes[0]).ConnectionType;
                    }

                    if (portType == InspectType.Both || portType == InspectType.Input)
                    {
                        var newPort = new PortData(NodeData.Id, addPort.FieldName, fieldInfo.FieldType,
                            overrideConnectionType, PortDirection.In)
                        {
                            Graph = NodeData.Node.Graph
                        };

                        NodeData.PortDatas.Add(newPort);
                    }

                    if (portType == InspectType.Both || portType == InspectType.Output)
                    {
                        var newPort = new PortData(NodeData.Id, addPort.FieldName, fieldInfo.FieldType,
                            overrideConnectionType, PortDirection.Out)
                        {
                            Graph = NodeData.Node.Graph
                        };

                        NodeData.PortDatas.Add(newPort);
                    }
                }

                if (BaseNodeWindow.Instance != null && BaseNodeWindow.Instance.LoadedGraph != null)
                {
                    BaseNodeWindow.Instance.LoadedGraph.RefreshPortDatas();
                }
            }

            NodeSerializedObject.Update();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                for (var i = 0; i < _nodeFields.Count; i++)
                {
                    var nodePort = _nodeFields[i];

                    GUILayout.BeginHorizontal();

                    var disabled = _inputPortDatas.ContainsKey(nodePort.FieldName) &&
                                   _inputPortDatas[nodePort.FieldName].Connections.Count > 0;
                    using (new EditorGUI.DisabledGroupScope(disabled))
                    {
                        EditorGUILayout.PropertyField(NodeSerializedObject.FindProperty(nodePort.FieldName),
                            new GUIContent(nodePort.FieldName), true);
                    }

                    GUILayout.EndHorizontal();

                    if (nodePort.InspectType != InspectType.None
                        && Event.current != null && Event.current.type == EventType.Repaint)
                    {
                        var rect = GUILayoutUtility.GetLastRect();
                        rect.position += NodeData.BodyRect.position;

                        if (_fieldPositions.ContainsKey(nodePort.FieldName))
                        {
                            _fieldPositions[nodePort.FieldName] = rect;
                        }
                        else
                        {
                            _fieldPositions.Add(nodePort.FieldName, rect);
                        }

                        if (nodePort.InspectType != InspectType.Output)
                        {
                            if (!_inputPortDatas.ContainsKey(nodePort.FieldName))
                            {
                                var portData = NodeData.PortDatas.Find(x =>
                                    x.FieldName == nodePort.FieldName && x.Direction == PortDirection.In);
                                _inputPortDatas.Add(nodePort.FieldName, portData);
                            }
                        }

                        if (nodePort.InspectType != InspectType.Input)
                        {
                            if (!_outPutPortDatas.ContainsKey(nodePort.FieldName))
                            {
                                var portData = NodeData.PortDatas.Find(x =>
                                    x.FieldName == nodePort.FieldName && x.Direction == PortDirection.Out);
                                _outPutPortDatas.Add(nodePort.FieldName, portData);
                            }
                        }
                    }
                }

                if (check.changed)
                {
                    if (node.OnNodeUpdated != null)
                    {
                        node.OnNodeUpdated(node);
                    }

                    NodeSerializedObject.ApplyModifiedProperties();

                    if (node.Outputs.Count > 0)
                    {
                        for (var i = 0; i < node.Outputs.Count; i++)
                        {
                            var connection = node.Outputs[i];
                            if (connection != null)
                            {
                                connection.Connection.OnNodeUpdated(node);
                            }
                        }
                    }
                }
            }
        }

        private void DrawPorts()
        {
            var nodeType = NodeData.Node.GetType();
            var ports = NodeCache.NodePortCaches[nodeType];
            var portSize = NodePreferences.Instance.PortSize;

            for (var i = 0; i < ports.Count; i++)
            {
                var portWrapper = ports[i];
                var fieldName = portWrapper.FieldName;

                if (!_fieldPositions.ContainsKey(fieldName))
                {
                    continue;
                }

                var fieldRect = _fieldPositions[fieldName];
                var fieldPosition = fieldRect.position;
                var portCol = Color.white;
                var portOffset = new Vector2(NodePreferences.Instance.PortOffset, (fieldRect.size.y - portSize.y) / 2);

                var inputPos = fieldPosition - portSize.x * Vector2.right - portOffset;
                var outputPos = fieldPosition + fieldRect.width * Vector2.right + portOffset;

                if (_inputPortDatas.ContainsKey(fieldName))
                {
                    var portData = _inputPortDatas[fieldName];
                    portData.Rect = new Rect(inputPos, portSize);

                    var filedTypeName = portData.TypeName;

                    if (!NodePreferences.Instance.FieldTypeColor.ContainsKey(filedTypeName))
                    {
                        NodePreferences.Instance.FieldTypeColor.Add(filedTypeName, Color.white);
                    }

                    portCol = NodePreferences.Instance.FieldTypeColor[filedTypeName];
                }

                if (_outPutPortDatas.ContainsKey(fieldName))
                {
                    var portData = _outPutPortDatas[fieldName];
                    portData.Rect = new Rect(outputPos, portSize);
                    var filedTypeName = portData.TypeName;
                    if (!NodePreferences.Instance.FieldTypeColor.ContainsKey(filedTypeName))
                    {
                        NodePreferences.Instance.FieldTypeColor.Add(filedTypeName, Color.white);
                    }

                    portCol = NodePreferences.Instance.FieldTypeColor[filedTypeName];
                }

                inputPos = BaseNodeWindow.GraphToScreenSpace(inputPos);
                outputPos = BaseNodeWindow.GraphToScreenSpace(outputPos);

                var backgroundCol = GUI.color;
                GUI.color = portCol;

                // Inpus
                if (portWrapper.InspectType == InspectType.Input || portWrapper.InspectType == InspectType.Both)
                {
                    GUI.DrawTexture(new Rect(inputPos, portSize), NodeStyles.Instance.NodeInputPort);
                }

                // Outputs
                if (portWrapper.InspectType == InspectType.Output || portWrapper.InspectType == InspectType.Both)
                {
                    GUI.DrawTexture(new Rect(outputPos, portSize), NodeStyles.Instance.NodeOutputPort);
                }

                GUI.color = backgroundCol;
            }
        }

        public void InitializeEditor(NodeData nodeData)
        {
            NodeData = nodeData;
        }
    }
}