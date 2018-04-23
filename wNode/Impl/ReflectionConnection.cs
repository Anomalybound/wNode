using System.Reflection;
using UnityEngine;
using wNode.Core;
using wNode.Utilities;
using wNode.Wrapper;

namespace wNode
{
    public class ReflectionConnection : NodeConnection
    {
        [SerializeField]
        protected Node InNode;

        [SerializeField]
        protected Node OutNode;

        private NodeData _inNodeData;
        private NodeData _outNodeData;

        private FieldInfo _inFieldInfo;
        private FieldInfo _outFieldInfo;
        private MethodInfo _getValueMethodInfo;

        private MethodCaller<object, object> _valueGetter;
        private MemberGetter<object, object> _getter;
        private MemberSetter<object, object> _setter;

        private object _cachedObject;

        #region Getters

        private object CachedObject
        {
            get
            {
                _cachedObject = GetValue();
                return _cachedObject;
            }
        }

        protected NodeData InNodeData
        {
            get
            {
                if (_inNodeData == null)
                {
                    var portData = Graph.Ports[InPortId];
                    _inNodeData = Graph.Nodes[portData.NodeDataId];
                }

                return _inNodeData;
            }
        }

        protected NodeData OutNodeData
        {
            get
            {
                if (_outNodeData == null)
                {
                    var portData = Graph.Ports[OutPortId];
                    _outNodeData = Graph.Nodes[portData.NodeDataId];
                }

                return _outNodeData;
            }
        }

        protected MemberGetter<object, object> Getter
        {
            get { return _getter ?? (_getter = OutFieldInfo.DelegateForGet()); }
        }

        protected MemberSetter<object, object> Setter
        {
            get { return _setter ?? (_setter = InFieldInfo.DelegateForSet()); }
        }

        protected MethodCaller<object, object> ValueGetter
        {
            get { return _valueGetter ?? (_valueGetter = GetValueMethodInfo.DelegateForCall()); }
        }

        protected FieldInfo InFieldInfo
        {
            get
            {
                if (_inFieldInfo == null)
                {
                    var portData = Graph.Ports[InPortId];
                    _inFieldInfo = InNodeData.Node.GetType().GetField(portData.FieldName);
                }

                return _inFieldInfo;
            }
        }

        protected FieldInfo OutFieldInfo
        {
            get
            {
                if (_outFieldInfo == null)
                {
                    var portData = Graph.Ports[OutPortId];
                    _outFieldInfo = OutNodeData.Node.GetType().GetField(portData.FieldName);
                }

                return _outFieldInfo;
            }
        }

        protected MethodInfo GetValueMethodInfo
        {
            get
            {
                if (_getValueMethodInfo == null)
                {
                    _getValueMethodInfo = OutNodeData.Node.GetType().GetMethod("GetValue");
                }

                return _getValueMethodInfo;
            }
        }

        #endregion

        public virtual object GetValue()
        {
            return ValueGetter(OutNode, new object[] {FieldName});
        }

        public override string DisplayLabel()
        {
            return CachedObject.ToString();
        }

        public override void OnConnect(Node leftNode, Node rightNode)
        {
            OutNode = leftNode;
            InNode = rightNode;

            var portData = Graph.Ports[InPortId];
            InNodeData.OnFieldUpdated(portData.FieldName, CachedObject);
        }

        public override void OnNodeUpdated(Node node)
        {
            var portData = Graph.Ports[InPortId];
            InNodeData.OnFieldUpdated(portData.FieldName, CachedObject);
        }
    }
}