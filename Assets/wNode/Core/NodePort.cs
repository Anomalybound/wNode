using System;
using wNode.Attributes;

namespace wNode.Core
{
    [Serializable]
    public struct NodePort
    {
        public string FieldName;
        public InspectType InspectType;

        public NodePort(string fieldName, InspectType inspectType)
        {
            FieldName = fieldName;
            InspectType = inspectType;
        }
    }
}