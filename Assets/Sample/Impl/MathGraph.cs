using System;
using UnityEngine;
using wNode;
using wNode.Core;

namespace Sample.Impl
{
    [CreateAssetMenu(menuName = "Math/New Graph")]
    public class MathGraph : NodeGraph
    {
        public override Type DefaultConnectionType
        {
            get { return typeof(ReflectionConnection); }
        }
    }
}