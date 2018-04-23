using Sample.Impl;
using wNode.Attributes;

[NodePath("Math/Operation/Multiply")]
public class MultiplyNode : OperationNode
{
    public override void Operation(ref float input)
    {
        input *= Factor;
    }
}