using System;

namespace wNode.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeEditorAttribute : Attribute
    {
        public Type NodeType { get; set; }

        public CustomNodeEditorAttribute(Type nodeType)
        {
            NodeType = nodeType;
        }
    }
}