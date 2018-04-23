using System;
using System.Linq;
using UnityEngine;
using wNode.Utilities;
using wNode.Wrapper;

namespace wNode.Core
{
    public class NodeGraph : ScriptableObject
    {
        public NodeLookupDictionary Nodes = new NodeLookupDictionary();
        public PortLookupDictionary Ports = new PortLookupDictionary();
        public ConnectionLookupDictionary Connections = new ConnectionLookupDictionary();

        public virtual Type DefaultConnectionType
        {
            get { return typeof(NodeConnection); }
        }

        public NodeData FindNodeData(string nodeId)
        {
            if (Nodes.ContainsKey(nodeId))
            {
                return Nodes[nodeId];
            }

            return null;
        }

        public void RemoveNodeById(string nodeId)
        {
            var nodeData = FindNodeData(nodeId);
            var portDatas = Ports.Where(x => x.Value.NodeDataId == nodeId).Select(x => x.Value).ToList();

            for (var i = 0; i < portDatas.Count; i++)
            {
                var portData = portDatas[i];
                if (portData.Connections.Count > 0)
                {
                    for (var j = 0; j < portData.Connections.Count; j++)
                    {
                        var connectionId = portData.Connections[j];
                        RemoveConnectionById(connectionId);
                    }
                }

                var portId = portDatas[i].PortId;
                Ports.Remove(portId);
            }

            if (nodeData != null)
            {
                DestroyImmediate(nodeData.Node, true);
                Nodes.Remove(nodeId);
            }

            RefreshPortDatas();
        }

        public void RemoveConnectionById(string connectionId)
        {
            if (!Connections.ContainsKey(connectionId))
            {
                Debug.LogError("Connection not found : " + connectionId);
                return;
            }

            var connectionData = Connections[connectionId];
            if (connectionData != null)
            {
                var inPortData = Ports.ContainsKey(connectionData.InputPortId)
                    ? Ports[connectionData.InputPortId]
                    : null;

                var outPortData = Ports.ContainsKey(connectionData.OutputPortId)
                    ? Ports[connectionData.OutputPortId]
                    : null;

                Node inNode = null;
                Node outNode = null;

                if (inPortData != null)
                {
                    inNode = Nodes[inPortData.NodeDataId].Node;
                    inPortData.Connections.Remove(connectionData.ConnectionId);
                }

                if (outPortData != null)
                {
                    outNode = Nodes[outPortData.NodeDataId].Node;
                    outPortData.Connections.Remove(connectionData.ConnectionId);
                }

                connectionData.Connection.OnDisconnect(outNode, inNode);
                if (outNode != null)
                {
                    outNode.Outputs.Remove(connectionData);
                }

                if (inNode != null)
                {
                    inNode.Inputs.Remove(connectionData);
                }

                DestroyImmediate(connectionData.Connection, true);
                Connections.Remove(connectionId);
            }
        }

        public PortData FindPortData(string portId)
        {
            if (Ports.ContainsKey(portId))
            {
                return Ports[portId];
            }

            return null;
        }

        public void RefreshPortDatas()
        {
            Ports.Clear();
            {
                foreach (var nodeData in Nodes.Values)
                {
                    // Add newest
                    for (var i = 0; i < nodeData.PortDatas.Count; i++)
                    {
                        var currentNodeData = nodeData.PortDatas[i];
                        Ports.Add(currentNodeData.PortId, currentNodeData);
                    }
                }
            }
        }
    }
}