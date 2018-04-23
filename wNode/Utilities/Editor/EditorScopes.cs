using UnityEditor;
using UnityEngine;

namespace wNode.Utilities
{
    public class LabelWidthScope : GUI.Scope
    {
        private readonly float originWidth;

        public LabelWidthScope(float newWidth)
        {
            originWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = newWidth;
        }

        protected override void CloseScope()
        {
            EditorGUIUtility.labelWidth = originWidth;
        }
    }
}