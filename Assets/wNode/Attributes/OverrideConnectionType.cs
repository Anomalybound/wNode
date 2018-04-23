using System;

namespace wNode.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OverrideConnectionTypeAttribute : Attribute
    {
        public Type ConnectionType { get; private set; }

        public OverrideConnectionTypeAttribute(Type connectionType)
        {
            ConnectionType = connectionType;
        }
    }
}