using System;
using System.Collections.Generic;
using UnityEngine;
using wNode.Attributes;
using wNode.Utilities;
using wNode.Wrapper;

namespace wNode.Core
{
    public abstract class Node : ScriptableObject, IGraphBasedElement
    {
        public Action<Node> OnNodeUpdated;

        public List<ConnectionData> Outputs = new List<ConnectionData>();

        public List<ConnectionData> Inputs = new List<ConnectionData>();

        [SerializeField]
        private NodeGraph _nodeGraph;

        public NodeGraph Graph
        {
            get { return _nodeGraph; }
            set { _nodeGraph = value; }
        }

        public string NodeName { get; protected set; }

        public virtual Color NodeColor
        {
            get { return Color.white; }
        }

        #region Runtime Variables

        private Dictionary<string, MemberGetter<object, object>> _cachedGetters =
            new Dictionary<string, MemberGetter<object, object>>();

        private Dictionary<string, MemberSetter<object, object>> _cachedSetters =
            new Dictionary<string, MemberSetter<object, object>>();

        #endregion

        private void OnEnable()
        {
            if (NodeName != null) return;

            var attributes = GetType().GetCustomAttributes(typeof(NodeNameAttribute), false);
            NodeName = attributes.Length > 0 ? ((NodeNameAttribute) attributes[0]).Name : GetType().Name;
            name = NodeName;
        }

        public virtual void SetValue(string fieldName, object value)
        {
            if (_cachedSetters == null)
            {
                _cachedSetters = new Dictionary<string, MemberSetter<object, object>>();
            }

            if (!_cachedSetters.ContainsKey(fieldName))
            {
                _cachedSetters.Add(fieldName, GetType().GetField(fieldName).DelegateForSet());
            }

            var nodeTarget = (object) this;
            _cachedSetters[fieldName](ref nodeTarget, value);
        }

        public virtual object GetValue(string fieldName)
        {
            if (_cachedGetters == null)
            {
                _cachedGetters = new Dictionary<string, MemberGetter<object, object>>();
            }
            
            if (!_cachedGetters.ContainsKey(fieldName))
            {
                _cachedGetters.Add(fieldName, GetType().GetField(fieldName).DelegateForGet());
            }

            return _cachedGetters[fieldName](this);
        }
    }
}