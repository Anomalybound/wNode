using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using wNode.Attributes;
using wNode.Core;
using wNode.Editors;
using wNode.Wrapper;
using Node = wNode.Core.Node;

namespace wNode.Utilities
{
    public static class NodeCache
    {
        private static NodePortDictionary _nodePortCaches;

        public static NodePortDictionary NodePortCaches
        {
            get
            {
                if (_nodePortCaches == null)
                {
                    Debug.LogError("Cached Not Built.");
                }

                return _nodePortCaches;
            }
        }

        private static Dictionary<NodeContextMenuAttribute, MethodInfo> _nodeContextMethods;

        public static Dictionary<NodeContextMenuAttribute, MethodInfo> NodeContextMethods
        {
            get
            {
                if (_nodeContextMethods == null)
                {
                    Debug.LogError("Cached Not Built.");
                }

                return _nodeContextMethods;
            }
        }

        private static Dictionary<NodeContextMenuAttribute, MethodInfo> _nodeGraphContextMethods;

        public static Dictionary<NodeContextMenuAttribute, MethodInfo> NodeGraphContextMethods
        {
            get
            {
                if (_nodeGraphContextMethods == null)
                {
                    Debug.LogError("Cached Not Built.");
                }

                return _nodeGraphContextMethods;
            }
        }

        private static Dictionary<string, Type> _cachedNodePath;

        public static Dictionary<string, Type> CachedNodePath
        {
            get
            {
                if (_cachedNodePath == null)
                {
                    Debug.LogError("Cached Not Built.");
                }

                return _cachedNodePath;
            }
        }

        private static Dictionary<Type, Type> _cachedNodeEditors;

        public static Dictionary<Type, Type> CachedNodeEditors
        {
            get
            {
                if (_cachedNodeEditors == null)
                {
                    Debug.LogError("Cached Not Built.");
                }

                return _cachedNodeEditors;
            }
        }

        #region Runtime Caches

        public static Dictionary<Node, NodeEditor> NodeEditors = new Dictionary<Node, NodeEditor>();

        #endregion

        private static BaseNodeWindow _nodeWindow;

        public static void BuildCahces(BaseNodeWindow nodeWindow)
        {
            if (_nodeWindow != null)
            {
                return;
            }

            _nodeWindow = nodeWindow;
            _nodePortCaches = new NodePortDictionary();

            #region NodePorts

//            var nodeBaseType = typeof(Node);
//            var nodeAssembly = nodeBaseType.Assembly;
            var nodeTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(Node).IsAssignableFrom(x));

            foreach (var nodeType in nodeTypes)
            {
                var fieldInfos = nodeType.GetFields();
                var portWrappers = new List<NodePort>();

                for (var i = 0; i < fieldInfos.Length; i++)
                {
                    var fieldInfo = fieldInfos[i];
                    var nodeInspects = fieldInfo.GetCustomAttributes(typeof(NodeInspectAttribute), false);
                    if (nodeInspects.Length > 0)
                    {
                        var inspect = nodeInspects[0] as NodeInspectAttribute;
                        if (inspect != null)
                        {
                            portWrappers.Add(new NodePort(fieldInfo.Name, inspect.Type));
                        }
                    }
                }

                NodePortCaches.Add(nodeType, portWrappers);
            }

            #endregion

            #region Node Context methods

            _nodeContextMethods = new Dictionary<NodeContextMenuAttribute, MethodInfo>();
            _nodeGraphContextMethods = new Dictionary<NodeContextMenuAttribute, MethodInfo>();

            var methodInfos = nodeWindow.GetType().GetMethods(BindingFlags.NonPublic
                                                              | BindingFlags.Public | BindingFlags.Instance |
                                                              BindingFlags.Static);

            for (var i = 0; i < methodInfos.Length; i++)
            {
                var methodInfo = methodInfos[i];
                var attributes = methodInfo.GetCustomAttributes(typeof(NodeContextMenuAttribute), false);
                if (attributes.Length > 0)
                {
                    for (var j = 0; j < attributes.Length; j++)
                    {
                        var parms = methodInfo.GetParameters();
                        var att = attributes[j] as NodeContextMenuAttribute;

                        if (parms.Length == 1 && parms[0].ParameterType == typeof(NodeData))
                        {
                            _nodeContextMethods.Add(att, methodInfo);
                        }
                        else if (parms.Length == 0)
                        {
                            _nodeGraphContextMethods.Add(att, methodInfo);
                        }
                    }
                }
            }

            #endregion

            #region Node path

            _cachedNodePath = new Dictionary<string, Type>();

            for (var i = 0; i < nodeTypes.Count(); i++)
            {
                var type = nodeTypes.ElementAt(i);
                var attributes = type.GetCustomAttributes(typeof(NodePathAttribute), false);
                if (attributes.Length > 0)
                {
                    var nodePath = ((NodePathAttribute) attributes[0]).Path;
                    _cachedNodePath.Add(nodePath, type);
                }
            }

            #endregion

            #region Node Editors

            _cachedNodeEditors = new Dictionary<Type, Type>();

            var editorAssembly = _nodeWindow.GetType().Assembly;
            var nodeEditorTypes = editorAssembly.GetTypes().Where(x => typeof(NodeEditor).IsAssignableFrom(x));

            for (var i = 0; i < nodeEditorTypes.Count(); i++)
            {
                var type = nodeEditorTypes.ElementAt(i);
                var attributes = type.GetCustomAttributes(typeof(CustomNodeEditorAttribute), false);
                if (attributes.Length > 0)
                {
                    var nodeType = ((CustomNodeEditorAttribute) attributes[0]).NodeType;
//                    var nodeEditor = ScriptableObject.CreateInstance(type) as NodeEditor;
                    _cachedNodeEditors.Add(nodeType, type);
                }
            }

            #endregion

//            Debug.Log("Node cahce built.");
        }
    }
}