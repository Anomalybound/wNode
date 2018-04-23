using System;
using UnityEngine;
using wNode.Core;

namespace wNode.Wrapper
{
    [Serializable]
    public class ConnectionData
    {
        [SerializeField]
        private string _connectionId;

        [SerializeField]
        private string _fieldName;

        public NodeConnection Connection;

        #region Runtime Operation

        public bool Hovering;

        #endregion

        #region Getters

        public string ConnectionId
        {
            get { return _connectionId; }
        }

        public string InputPortId
        {
            get { return Connection.InPortId; }
        }

        public string OutputPortId
        {
            get { return Connection.OutPortId; }
        }

        public string FieldName
        {
            get { return _fieldName; }
        }

        #endregion

        public ConnectionData(NodeConnection connection, string fieldName)
        {
            _connectionId = Guid.NewGuid().ToString();
            _fieldName = fieldName;
            Connection = connection;
        }
    }
}