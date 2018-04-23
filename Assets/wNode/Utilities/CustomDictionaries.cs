using System;
using System.Collections.Generic;
using UnityEngine;
using wNode.Core;
using wNode.Wrapper;

namespace wNode.Utilities
{
    [Serializable]
    public class NodePortDictionary : SerializableDictionary<Type, List<NodePort>> { }

    [Serializable]
    public class NodeLookupDictionary : SerializableDictionary<string, NodeData> { }

    [Serializable]
    public class PortLookupDictionary : SerializableDictionary<string, PortData> { }

    [Serializable]
    public class ConnectionLookupDictionary : SerializableDictionary<string, ConnectionData> { }

    [Serializable]
    public class FieldTypeColorLookup : SerializableDictionary<string, Color> { }
}