using UnityEditor;
using UnityEngine;

namespace wNode.Editors
{
    [ResourcePath("Styles")]
    public class NodeStyles : ResourcesSingleton<NodeStyles>
    {
        [Header("Node")]
        public GUIStyle NodeHeader;

        public GUIStyle NodeBody;
        public GUIStyle NodeTitleLabel;
        public Color NodeLabelColor = Color.white;
        public Color NodeBodyColor = Color.black;

        [Header("Port")]
        public Texture2D NodeInputPort;

        public Texture2D NodeOutputPort;

        [Header("Connection")]
        public Vector2 ConnectionKnobSize = new Vector2(40, 16);

        public GUIStyle ConnectionLabel;
        public Texture2D ConnectionKnob;
        public Texture2D LineTexture;

        [Header("Inspector")]
        public GUIStyle InspectorTitleLabel;

        public void DefaultStyles()
        {
            NodeTitleLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                name = "Node Title Label",
                alignment = TextAnchor.MiddleCenter
            };

            NodeHeader = new GUIStyle(EditorStyles.helpBox)
            {
                name = "Node Header"
            };

            NodeBody = new GUIStyle(EditorStyles.helpBox)
            {
                name = "Node Body",
                border = new RectOffset(32, 32, 32, 32),
                padding = new RectOffset(16, 16, 4, 16)
            };

            ConnectionLabel = new GUIStyle(EditorStyles.whiteLabel)
            {
                name = "Connection Label"
            };

            InspectorTitleLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                name = "Inspector Title Label",
                alignment = TextAnchor.MiddleCenter,
                fontSize = 25
            };
        }
    }

    [CustomEditor(typeof(NodeStyles))]
    public class NodeStyleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Use defaults"))
            {
                ((NodeStyles) target).DefaultStyles();
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}