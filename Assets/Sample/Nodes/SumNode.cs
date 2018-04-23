using Sample.Impl;
using wNode;
using wNode.Attributes;

[NodePath("Math/Display/Sum")]
public class SumNode : ResultNode
{
    public override void SetValue(string fieldName, object value)
    {
        var sum = 0f;
        for (var i = 0; i < Inputs.Count; i++)
        {
            var input = (ReflectionConnection) Inputs[i].Connection;
            if (input != null)
            {
                sum += (float) input.GetValue();
            }
        }

        base.SetValue(fieldName, sum);
    }
}