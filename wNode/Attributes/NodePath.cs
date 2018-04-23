using System;

namespace wNode.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodePathAttribute : Attribute
    {
        public string Path { get; private set; }

        public NodePathAttribute(string path)
        {
            Path = path;
        }
    }
}