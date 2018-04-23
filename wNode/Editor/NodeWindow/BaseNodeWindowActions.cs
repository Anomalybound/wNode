using System;
using UnityEditor;
using UnityEngine;
using wNode.Core;
using wNode.Wrapper;

namespace wNode.Editors
{
    public partial class BaseNodeWindow
    {
        private void CreateNewNode(Type type, Vector2 graphPosition)
        {
            if (LoadedGraph == null) return;

            var newNode = CreateInstance(type) as Node;

            if (newNode == null)
            {
                Debug.LogError("Can't create node instance :" + type);
                return;
            }

            AssetDatabase.AddObjectToAsset(newNode, LoadedGraph);
            newNode.Graph = LoadedGraph;

            var newNodeData = new NodeData(newNode.NodeName, newNode);
            var editor = GetNodeEditor(newNodeData);
            var nodePos = graphPosition;

            var headerPos = new Rect(nodePos, new Vector2(editor.GetNodeWidth(), editor.GetNodeHeaderHeight()));
            var bodyPos = new Rect(nodePos + Vector2.up * editor.GetNodeHeaderHeight(),
                new Vector2(editor.GetNodeWidth(), 1000));

            newNodeData.HeaderRect = headerPos;
            newNodeData.BodyRect = bodyPos;

            LoadedGraph.Nodes.Add(newNodeData.Id, newNodeData);
            LoadedGraph.RefreshPortDatas();

            EditorUtility.SetDirty(LoadedGraph);
            AssetDatabase.SaveAssets();

            Repaint();
        }

        private void CreateNewConnection(Type connectionType, PortData sourcePortData, PortData targetPortData)
        {
            var connection = CreateInstance(connectionType) as NodeConnection;

            if (connection == null)
            {
                Debug.LogError("Create connection failed: " + connectionType);
                return;
            }

            connection.name = connectionType.Name;
            connection.Graph = LoadedGraph;

            Node inNode;
            Node outNode;

            // add ports & modes
            if (sourcePortData.Direction == PortDirection.In)
            {
                connection.InPortId = sourcePortData.PortId;
                connection.OutPortId = targetPortData.PortId;
                inNode = LoadedGraph.Nodes[sourcePortData.NodeDataId].Node;
                outNode = LoadedGraph.Nodes[targetPortData.NodeDataId].Node;
            }
            else
            {
                connection.InPortId = targetPortData.PortId;
                connection.OutPortId = sourcePortData.PortId;
                inNode = LoadedGraph.Nodes[targetPortData.NodeDataId].Node;
                outNode = LoadedGraph.Nodes[sourcePortData.NodeDataId].Node;
            }

            // add nodes
            connection.FieldName = sourcePortData.FieldName;
            connection.OnConnect(outNode, inNode);

            // create ConnectionData
            var connectionData = new ConnectionData(connection, sourcePortData.FieldName);
            
            // add connection datas
            outNode.Outputs.Add(connectionData);
            inNode.Inputs.Add(connectionData);

            sourcePortData.Connections.Add(connectionData.ConnectionId);
            targetPortData.Connections.Add(connectionData.ConnectionId);
            LoadedGraph.Connections.Add(connectionData.ConnectionId, connectionData);

            AssetDatabase.AddObjectToAsset(connection, LoadedGraph);
            EditorUtility.SetDirty(LoadedGraph);
            AssetDatabase.SaveAssets();

            Repaint();
        }

        private void SelectActions()
        {
            _hoverNode = MouseOverNode();
            _hoverPort = MouseOverPort();
            if (_hoverNode != null)
            {
                if (!SelectedNodes.Contains(_hoverNode))
                {
                    SelectedNodes.Clear();
                    SelectedNodes.Add(_hoverNode);
                }

                Event.current.Use();
            }
            else if (_hoverPort != null)
            {
                Event.current.Use();
            }
            else
            {
                SelectedNodes.Clear();
            }
        }
    }
}