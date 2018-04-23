using System;
using System.Collections.Generic;
using UnityEngine;

namespace wNode.Core
{
    public enum PortDirection
    {
        In,
        Out,
    }

    [Serializable]
    public class PortData
    {
        [SerializeField]
        private string _portId;

        [SerializeField]
        private string _nodeDataId;

        [SerializeField]
        private string _fieldName;

        [SerializeField]
        private string _typeName;

        [SerializeField]
        private PortDirection _direction;

        [SerializeField]
        private string _overrideConnectionTypeName;

        private Type _overrideConnectionType;

        [SerializeField]
        private NodeGraph _nodeGraph;

        public NodeGraph Graph
        {
            get { return _nodeGraph; }
            set { _nodeGraph = value; }
        }

        public Rect Rect;

        public Vector2 ConnectPoint
        {
            get
            {
                var point = Rect.position;
                point.y += Rect.size.y / 2;
                if (_direction == PortDirection.Out)
                {
                    point.x += Rect.size.x;
                }

                return point;
            }
        }

        public List<string> Connections = new List<string>();

        #region Getters

        public string PortId
        {
            get { return _portId; }
        }

        public string NodeDataId
        {
            get { return _nodeDataId; }
        }

        public string FieldName
        {
            get { return _fieldName; }
        }

        public PortDirection Direction
        {
            get { return _direction; }
        }

        public Type OverrideConnectionType
        {
            get
            {
                if (_overrideConnectionType == null && !string.IsNullOrEmpty(_overrideConnectionTypeName))
                {
                    _overrideConnectionType = GetType().Assembly.GetType(_overrideConnectionTypeName);
                }

                return _overrideConnectionType;
            }
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        #endregion

        public PortData(string nodeDataId, string fieldName, Type type, Type overrideConnectionType,
            PortDirection direction)
        {
            _portId = Guid.NewGuid().ToString();
            _overrideConnectionTypeName = overrideConnectionType != null ? overrideConnectionType.FullName : null;
            _nodeDataId = nodeDataId;
            _fieldName = fieldName;
            _typeName = type.Name;
            _direction = direction;
        }

        public virtual bool VerifyConnection(PortData target)
        {
            // Self
            if (target._nodeDataId == _nodeDataId)
            {
                return false;
            }

            // Type
            if (target.TypeName != TypeName)
            {
                return false;
            }

            // Direction
            if (_direction == PortDirection.In && target._direction != PortDirection.Out ||
                _direction == PortDirection.Out && target._direction != PortDirection.In)
            {
                return false;
            }

            return true;
        }
    }
}