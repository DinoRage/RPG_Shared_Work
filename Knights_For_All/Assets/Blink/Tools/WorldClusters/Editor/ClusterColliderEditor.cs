using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BLINK.WorldClusters
{
    [CustomEditor(typeof(ClusterCollider))]
    public class ClusterColliderEditor : Editor
    {
        private ClusterCollider _ref;
        private GUISkin _skin;
        private void OnEnable()
        {
            _ref = (ClusterCollider) target;
            _skin = Resources.Load<GUISkin>("EditorData/WorldClustersEditorSkin");
        }

        public override void OnInspectorGUI()
        {
            if (_skin == null) return;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((ClusterCollider) target),
                typeof(Cluster),
                false);
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();

            _ref.cluster = (Cluster) EditorGUILayout.ObjectField("Cluster:", _ref.cluster, typeof(Cluster), true);
            if (_ref.cluster != null)
            {
                string[] names = GetClusterGroupNames(_ref.cluster).ToArray();
                var tempIndex2 = EditorGUILayout.Popup("Cluster Group", _ref.clusterGroupIndex, names);
                if (names.Length > 0)
                    _ref.clusterGroupIndex = tempIndex2;
            }

            if (!EditorGUI.EndChangeCheck()) return;
            EditorUtility.SetDirty(_ref);
            serializedObject.ApplyModifiedProperties();
        }

        private List<string> GetClusterGroupNames(Cluster cluster)
        {
            List<string> names = new List<string>();
            foreach (var cGroup in cluster.clusterGroups)
            {
                names.Add(cGroup.clusterGroupName);
            }

            return names;
        }
    }
}

