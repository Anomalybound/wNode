using Sample.Impl;
using wNode.Attributes;

namespace Sample.Nodes
{
    [NodePath("Math/Input/Float")]
    public class FloatInputNode : InputNode
    {
        [NodeInspect(InspectType.Output)]
        public float Output;
    }
}