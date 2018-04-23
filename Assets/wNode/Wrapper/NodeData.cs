using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using wNode.Core;
using wNode.Utilities;

namespace wNode.Wrapper
{
    [Serializable]
    public class NodeData
    {
        [SerializeField]
        private string _id;

        [SerializeField]
        private string _nodeName;

        [SerializeField]
        private Rect _headerRect;

        [SerializeField]
        private Rect _bodyRect;

        [SerializeField]
        private Node _node;

        public List<PortData> PortDatas = new List<PortData>();

        #region Variable Getters

        public string Id
        {
            get { return _id; }
        }

        public string NodeName
        {
            get { return _nodeName; }
        }

        public Node Node
        {
            get { return _node; }
        }

        public Rect HeaderRect
        {
            get { return _headerRect; }
            set { _headerRect = value; }
        }

        public Rect BodyRect
        {
            get { return _bodyRect; }
            set { _bodyRect = value; }
        }

        #endregion

        #region Runtime Variables

        private Dictionary<string, MethodCaller<object, object>> Setters =
            new Dictionary<string, MethodCaller<object, object>>();

        #endregion

        /// <summary>
        /// Create new node from GUI.
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="node"></param>
        public NodeData(string nodeName, Node node)
        {
            _id = Guid.NewGuid().ToString();
            _nodeName = nodeName;
            _node = node;
        }

        public virtual void OnFieldUpdated(string fieldName, object value)
        {
            if (Setters == null)
            {
                Setters = new Dictionary<string, MethodCaller<object, object>>();
            }

            if (!Setters.ContainsKey(fieldName))
            {
                var methodInfo = _node.GetType().GetMethod("SetValue");
                Setters.Add(fieldName, methodInfo.DelegateForCall());
            }

            var setter = Setters[fieldName];
            setter.Invoke(_node, new[] {fieldName, value});

            for (var i = 0; i < _node.Outputs.Count; i++)
            {
                var connectionData = _node.Outputs[i];
                if (connectionData.FieldName == fieldName)
                {
                    connectionData.Connection.OnNodeUpdated(_node);
                }
            }
        }
    }
}