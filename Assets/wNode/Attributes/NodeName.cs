using System;

namespace wNode.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public NodeNameAttribute(string name)
        {
            Name = name;
        }
    }
}