using Sample.Impl;
using wNode.Attributes;

namespace Sample.Nodes
{
    [NodePath("Math/Input/Int")]
    public class IntInputNode : InputNode
    {
        [NodeInspect(InspectType.Output)]
        public int Output;
    }
}