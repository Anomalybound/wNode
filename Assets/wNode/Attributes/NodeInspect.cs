using System;

namespace wNode.Attributes
{
    public enum InspectType
    {
        None,
        Input,
        Output,
        Both
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NodeInspectAttribute : Attribute
    {
        public InspectType Type { get; private set; }

        public NodeInspectAttribute(InspectType type = InspectType.None)
        {
            Type = type;
        }
    }
}