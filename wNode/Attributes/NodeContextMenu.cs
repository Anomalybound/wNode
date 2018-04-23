using System;

namespace wNode.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NodeContextMenuAttribute : Attribute
    {
        public string MethodName { get; private set; }

        public NodeContextMenuAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}