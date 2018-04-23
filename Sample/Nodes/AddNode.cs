using Sample.Impl;
using wNode.Attributes;

namespace Sample.Nodes
{
    [NodePath("Math/Operation/Add")]
    public class AddNode : OperationNode
    {
        public override void Operation(ref float input)
        {
            input += Factor;
        }
    }
}