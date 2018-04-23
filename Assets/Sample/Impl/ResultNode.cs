using UnityEngine;
using wNode.Attributes;
using wNode.Core;

namespace Sample.Impl
{
    public class ResultNode : Node
    {
        [NodeInspect(InspectType.Input)]
        public float Result;

        public override Color NodeColor
        {
            get { return Color.magenta; }
        }
    }
}