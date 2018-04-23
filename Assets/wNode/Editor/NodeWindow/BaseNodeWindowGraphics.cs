using UnityEditor;
using UnityEngine;
using wNode.Core;
using wNode.Wrapper;

namespace wNode.Editors
{
    public partial class BaseNodeWindow
    {
        private void DrawBackgrounds()
        {
            var size = GridSize.size;
            var center = size / 2f;

            var zoom = ZoomScale;

            var _backgroundTex = NodePreferences.Instance.BackgroundTexture;
            var _gridTex = NodePreferences.Instance.GridTexture;

            // Offset from origin in tile units
            var xOffset = -(center.x * zoom + Offset.x) / _gridTex.width;
            var yOffset = ((center.y - size.y) * zoom + Offset.y) / _gridTex.height;

            var tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            var tileAmountX = Mathf.Round(size.x * zoom) / _gridTex.width;
            var tileAmountY = Mathf.Round(size.y * zoom) / _gridTex.height;

            var tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(GridSize, _backgroundTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(GridSize, _gridTex, new Rect(tileOffset, tileAmount));
        }

        private void ControlActions()
        {
            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0)
                    {
                        SelectActions();
                    }
                    else if (evt.button == 1)
                    {
                        var node = MouseOverNode();
                        if (node != null)
                        {
                            ShowNodeContextMenu(node);
                        }
                        else
                        {
                            ShowGraphContextMenu();
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (_hoverPort != null)
                    {
                        var targetPort = MouseOverPort();
                        if (targetPort != null)
                        {
                            var sourcePortData = LoadedGraph.Ports[_hoverPort];
                            var targetPortData = LoadedGraph.Ports[targetPort];

                            if (sourcePortData.VerifyConnection(targetPortData))
                            {
                                if (_hoveringConnection != null)
                                {
                                    var inPort = sourcePortData.Direction == PortDirection.In
                                        ? sourcePortData.PortId
                                        : targetPortData.PortId;
                                    var outPort = sourcePortData.Direction == PortDirection.Out
                                        ? sourcePortData.PortId
                                        : targetPortData.PortId;

                                    _hoveringConnection.Connection.InPortId = inPort;
                                    _hoveringConnection.Connection.OutPortId = outPort;
                                    _hoveringConnection.Hovering = false;
                                }
                                else
                                {
                                    var connectionType = LoadedGraph.DefaultConnectionType;
                                    if (sourcePortData.OverrideConnectionType != null)
                                    {
                                        connectionType = sourcePortData.OverrideConnectionType;
                                    }

                                    CreateNewConnection(connectionType, sourcePortData, targetPortData);
                                }
                            }
                        }
                        else
                        {
                            if (_hoveringConnection != null)
                            {
                                LoadedGraph.RemoveConnectionById(_hoveringConnection.ConnectionId);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }

                    _hoveringConnection = null;
                    _hoverNode = null;
                    _hoverPort = null;
                    Repaint();
                    break;
                case EventType.MouseDrag:
                    if (evt.alt)
                    {
                        PanBackground(evt);
                    }
                    else
                    {
                        if (_hoverNode != null)
                        {
                            if (SelectedNodes != null && SelectedNodes.Count > 0)
                            {
                                for (var i = 0; i < SelectedNodes.Count; i++)
                                {
                                    var selection = SelectedNodes[i];
                                    var selectNode = LoadedGraph.FindNodeData(selection);

                                    var headerPos = selectNode.HeaderRect;
                                    var bodyPos = selectNode.BodyRect;

                                    headerPos.position += evt.delta * _zoom.x;
                                    bodyPos.position += evt.delta * _zoom.x;

                                    selectNode.HeaderRect = headerPos;
                                    selectNode.BodyRect = bodyPos;
                                }
                            }

                            evt.Use();
                        }
                    }

                    Repaint();

                    break;
                case EventType.ScrollWheel:
                    Zoom(evt.delta.y);
                    Repaint();
                    break;
            }
        }

        #region Connections

        private void DrawConnections()
        {
            // Draw Temporary Connection
            if (_hoverPort != null)
            {
                var portData = LoadedGraph.FindPortData(_hoverPort);
                if (portData == null)
                {
                    return;
                }

                if (portData.Connections.Count > 0 && portData.Direction == PortDirection.In)
                {
                    var connectionId = portData.Connections[portData.Connections.Count - 1];
                    var connectionData = LoadedGraph.Connections[connectionId];
                    connectionData.Hovering = true;
                    _hoverPort = portData.Direction == PortDirection.In
                        ? connectionData.OutputPortId
                        : connectionData.InputPortId;
                    portData = LoadedGraph.Ports[_hoverPort];

                    _hoveringConnection = connectionData;
                }

                var startPos = GraphToScreenSpace(portData.ConnectPoint);
                var endPos = Event.current.mousePosition;
                var direction = portData.Direction == PortDirection.Out ? 1 : -1;

                var mnog = Vector3.Distance(startPos, endPos);

                var startTan = startPos + Vector2.right * (NodePreferences.Instance.StartTanOff * mnog * direction);
                var endTan = endPos + Vector2.left * (NodePreferences.Instance.EndTanOff * mnog * direction);

                DrawCurve(startPos, endPos, startTan, endTan, Color.magenta, 3);
            }

            // Draw Saved Connections
            foreach (var connectionData in LoadedGraph.Connections.Values)
            {
                if (connectionData.Hovering)
                {
                    continue;
                }

                var sourPortData = LoadedGraph.Ports[connectionData.InputPortId];
                var targetPortData = LoadedGraph.Ports[connectionData.OutputPortId];
                var direction = sourPortData.Direction == PortDirection.Out ? 1 : -1;

                var startPos = GraphToScreenSpace(sourPortData.ConnectPoint);
                var endPos = GraphToScreenSpace(targetPortData.ConnectPoint);
                var center = (startPos + endPos) / 2f;
                var mnog = Vector3.Distance(startPos, endPos);

                var startTan = startPos + Vector2.right * (NodePreferences.Instance.StartTanOff * mnog * direction);
                var endTan = endPos + Vector2.left * (NodePreferences.Instance.EndTanOff * mnog * direction);

                DrawCurve(startPos, endPos, startTan, endTan,
                    connectionData.Connection.ConnectionColor, connectionData.Connection.LineWidth);

                DrawKnob(center, connectionData.Connection.KnobColor);
                DrawConnectionLabel(center, connectionData);
            }
        }

        private void DrawConnectionLabel(Vector2 center, ConnectionData connectionData)
        {
            var offset = NodePreferences.Instance.ConnectionLabelOffset;
            var label = new GUIContent(connectionData.Connection.DisplayLabel());
            var labelSize = NodeStyles.Instance.ConnectionLabel.CalcSize(label);
            var labelPosition = center - labelSize / 2f;
            labelPosition += offset;

            GUI.Box(new Rect(labelPosition, labelSize), new GUIContent(label), NodeStyles.Instance.ConnectionLabel);
        }

        private void DrawKnob(Vector2 center, Color col)
        {
            var old = GUI.color;
            GUI.color = col;
            var knobSize = NodeStyles.Instance.ConnectionKnobSize;
            GUI.DrawTexture(new Rect(center - knobSize / 2f, knobSize), NodeStyles.Instance.ConnectionKnob);
            GUI.color = old;
        }

        private void DrawCurve(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, Color lineCol,
            int lineWidth)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, lineCol, NodeStyles.Instance.LineTexture, lineWidth);
        }

        #endregion

        private void DrawNodes()
        {
            if (LoadedGraph == null) return;

            foreach (var nodeData in LoadedGraph.Nodes.Values)
            {
                var editor = GetNodeEditor(nodeData);
                editor.DrawNode();
            }
        }

        private void DrawFooter()
        {
            var footerPos = new Rect(0, position.height - 17, position.width, 17);
            GUI.Box(footerPos, "", EditorStyles.toolbar);

            GUILayout.BeginArea(footerPos);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Zoom", _zoom.ToString());
                EditorGUILayout.LabelField("Pan Offset", Offset.ToString());
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            var headerPos = new Rect(0, 0, position.width, 17);
            GUI.Box(headerPos, "", EditorStyles.toolbar);
            GUILayout.BeginArea(headerPos);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(50)))
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    rect.position += Vector2.up * 17;
                    OpenFilePanel(rect);
                }

                if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    Offset = Vector2.zero;
                    _zoom = Vector2.one;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Debug", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    _debugToggle = !_debugToggle;
                }
            }

            GUILayout.EndArea();
        }

        private float InspectorWidth = 250;

        private void DrawInspector()
        {
            var inspectorPos = new Rect(position.width - InspectorWidth, 16, InspectorWidth, position.height - 32);

            var bg = NodePreferences.Instance.InsepctorTexture2D;
            GUI.DrawTexture(inspectorPos, bg);

            GUILayout.BeginArea(inspectorPos);
            if (LoadedGraph != null)
            {
                var style = NodeStyles.Instance.InspectorTitleLabel;
                var content = new GUIContent(LoadedGraph.name);
                var size = style.CalcSize(content);

                EditorGUILayout.LabelField(content, style, GUILayout.Height(size.y));

                if (SelectedNodes != null && SelectedNodes.Count > 0)
                {
                    EditorGUILayout.LabelField("Selected Nodes:");

                    for (var i = 0; i < SelectedNodes.Count; i++)
                    {
                        var nodeData = LoadedGraph.FindNodeData(SelectedNodes[i]);
                        if (nodeData != null)
                        {
                            EditorGUILayout.LabelField(nodeData.NodeName);
                        }
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}