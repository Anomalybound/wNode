using UnityEngine;

namespace wNode.Core
{
    public class NodeConnection : ScriptableObject, IGraphBasedElement
    {
        [SerializeField]
        private NodeGraph _nodeGraph;

        [SerializeField]
        private string _connectionName;

        [SerializeField]
        private Color _defaultConnectionColor = new Color(0, 1, 0, 1);

        [SerializeField]
        private Color _defaultKnobColor = new Color(0f, 0f, 0f, 1);

        [SerializeField]
        private int _defaultLineWidth = 2;

        public string InPortId;
        
        public string OutPortId;

        public string FieldName;

        #region Runtime Variables

        public NodeGraph Graph
        {
            get { return _nodeGraph; }
            set { _nodeGraph = value; }
        }

        public string ConnectionName
        {
            get { return _connectionName; }
        }

        public virtual Color ConnectionColor
        {
            get { return _defaultConnectionColor; }
        }

        public virtual Color KnobColor
        {
            get { return _defaultKnobColor; }
        }

        public virtual int LineWidth
        {
            get { return _defaultLineWidth; }
        }

        public virtual string DisplayLabel()
        {
            return GetType().Name;
        }

        #endregion

        private void OnEnable()
        {
            _connectionName = GetType().Name;
        }

        public virtual void OnConnect(Node leftNode, Node rightNode) { }

        public virtual void OnDisconnect(Node leftNode, Node rightNode) { }

        public virtual void OnNodeUpdated(Node node) { }
    }
}