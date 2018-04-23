using UnityEngine;
using wNode.Attributes;
using wNode.Core;

namespace Sample.Impl
{
    public abstract class OperationNode : Node
    {
        [NodeInspect]
        public float Factor;

        [NodeInspect(InspectType.Both)]
        public float Data;

        public override Color NodeColor
        {
            get { return Color.green; }
        }

        public abstract void Operation(ref float input);

        public override object GetValue(string fieldName)
        {
            var value = (float) base.GetValue(fieldName);
            Operation(ref value);
            return value;
        }
    }
}