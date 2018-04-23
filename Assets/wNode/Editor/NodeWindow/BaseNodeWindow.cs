using System.Collections.Generic;
using System.Linq;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using wNode.Core;
using wNode.Utilities;
using wNode.Wrapper;

namespace wNode.Editors
{
    public partial class BaseNodeWindow : EditorWindow
    {
        public static BaseNodeWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetWindow<BaseNodeWindow>();
                }

                return _instance;
            }
        }

        private static BaseNodeWindow _instance;

        private NodeGraph _loadedGraph;

        public NodeGraph LoadedGraph
        {
            get { return _loadedGraph; }
            private set
            {
                _loadedGraph = value;
                LoadedGraph.RefreshPortDatas();
            }
        }

        [MenuItem("wNode/Node Window %#n")]
        private static void OpenWindow()
        {
            if (_instance == null)
            {
                _instance = GetWindow<BaseNodeWindow>();
            }

            _instance.minSize = new Vector2(500, 350);
            _instance.titleContent = new GUIContent("wNode Window");
            _instance.Show();
        }

        #region Runtime variables

        public readonly List<string> SelectedNodes = new List<string>();

        protected string _hoverNode;
        protected string _hoverPort;

        protected ConnectionData _hoveringConnection;

        public Rect GridSize
        {
            get { return new Rect(Vector2.zero, position.size - Vector2.right * InspectorWidth); }
        }

        #region Nodes

        #endregion

        #region Zoom

        private Vector2 _zoom = Vector2.one;

        // To keep track of zooming.
        private Vector2 _zoomAdjustment;

        public float ZoomScale
        {
            get { return _zoom.x; }
        }

        #endregion

        #region Panning

        private Vector2 Offset = Vector2.zero;

        #endregion

        #endregion

        private void OnEnable()
        {
            BuildGraphWindow();
        }

        private void OnGUI()
        {
            DrawBackgrounds();

            if (LoadedGraph != null)
            {
                #region Scalable Contents

                // Begin scale
                var graphRect = GridSize;
                var center = graphRect.size / 2f;
                _zoomAdjustment = GUIScaleUtility.BeginScale(ref graphRect, center, ZoomScale, false);

                DrawNodes();
                DrawConnections();

                GUIScaleUtility.EndScale();

                #endregion
            }

            DrawInspector();
            DrawHeader();
            DrawFooter();

            if (LoadedGraph != null)
            {
                if (_debugToggle)
                {
                    DrawDebugPanel();
                }

                ControlActions();
            }
        }

        private void BuildGraphWindow()
        {
            GUIScaleUtility.CheckInit();
            NodeCache.BuildCahces(this);
        }

        private void ShowNodeContextMenu(string hoverNode)
        {
            var menu = new GenericMenu();
            var node = LoadedGraph.FindNodeData(hoverNode);

            foreach (var nodeContextMethod in NodeCache.NodeContextMethods)
            {
                var methodInfo = nodeContextMethod.Value;
                if (methodInfo != null)
                {
                    menu.AddItem(new GUIContent(nodeContextMethod.Key.MethodName), false,
                        () => { methodInfo.Invoke(this, new object[] {node}); });
                }
            }

            menu.ShowAsContext();
        }

        private void ShowGraphContextMenu()
        {
            var mousePosition = GraphMousePosition();
            var menu = new GenericMenu();

            foreach (var nodeContextMethod in NodeCache.NodeGraphContextMethods)
            {
                var methodInfo = nodeContextMethod.Value;
                if (methodInfo != null)
                {
                    menu.AddItem(new GUIContent(nodeContextMethod.Key.MethodName), false,
                        () => methodInfo.Invoke(this, null));
                }
            }

            menu.AddSeparator("");

            foreach (var pair in NodeCache.CachedNodePath)
            {
                var nodeType = pair.Value;
                menu.AddItem(new GUIContent(pair.Key), false, () => CreateNewNode(nodeType, mousePosition));
            }

            menu.ShowAsContext();
        }

        #region Guardians

        [DidReloadScripts]
        private static void CheckNodeCacheRebuild()
        {
            var windowInstances = Resources.FindObjectsOfTypeAll<BaseNodeWindow>();
            if (windowInstances != null && windowInstances.Length > 0)
            {
                NodeCache.BuildCahces(windowInstances[0]);
            }
        }

        [OnOpenAsset]
        private static bool OnOpenNodeGraph(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            var graph = obj as NodeGraph;
            if (graph != null)
            {
                var window = GetWindow<BaseNodeWindow>();
                window.LoadedGraph = graph;
                window.minSize = new Vector2(500, 350);
                window.titleContent = new GUIContent("wNode Window");
                return true;
            }

            return false;
        }

        #endregion
    }
}