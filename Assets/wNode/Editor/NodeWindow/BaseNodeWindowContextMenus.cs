using System.Linq;
using UnityEditor;
using UnityEngine;
using wNode.Attributes;
using wNode.Wrapper;

namespace wNode.Editors
{
    public partial class BaseNodeWindow
    {
        [NodeContextMenu("Clear All Nodes")]
        public void ClearAllNodes()
        {
            var nodeIds = LoadedGraph.Nodes.Select(x => x.Key);
            for (var i = nodeIds.Count() - 1; i >= 0; i--)
            {
                LoadedGraph.RemoveNodeById(nodeIds.ElementAt(i));
            }

            AssetDatabase.SaveAssets();
        }

        [NodeContextMenu("Destroy Node")]
        private void DestroyNode(NodeData node)
        {
            LoadedGraph.RemoveNodeById(node.Id);

            Repaint();
            AssetDatabase.SaveAssets();
        }
    }
}