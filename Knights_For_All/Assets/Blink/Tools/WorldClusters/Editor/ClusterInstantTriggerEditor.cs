using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BLINK.WorldClusters
{
    [CustomEditor(typeof(ClusterInstantTrigger))]
    public class ClusterInstantTriggerEditor : Editor
    {
        private ClusterInstantTrigger _ref;
        private GUISkin _skin;
        
        private void OnEnable()
        {
            _ref = (ClusterInstantTrigger) target;
            _skin = Resources.Load<GUISkin>("EditorData/WorldClustersEditorSkin");
        }

        public override void OnInspectorGUI()
        {
            if (_skin == null) return;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((ClusterInstantTrigger) target),
                typeof(Cluster),
                false);
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();

            GUIStyle titleStyle = GetStyle("text");

            GUILayout.Label("CLUSTER SETTINGS", titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);

            _ref.cluster = (Cluster) EditorGUILayout.ObjectField("Cluster:", _ref.cluster, typeof(Cluster), true);
            if (_ref.cluster != null)
            {
                _ref.actionEventType = (ClUSTER_ACTION_EVENT_TYPE) EditorGUILayout.EnumPopup(
                    GetGUIContent("Event Type", "The type of this action event"),
                    _ref.actionEventType);
                
                string[] names = GetClusterGroupNames(_ref.cluster).ToArray();
                var tempIndex2 = EditorGUILayout.Popup("Cluster Group:", _ref.clusterGroupIndex, names);
                if (names.Length > 0)
                    _ref.clusterGroupIndex = tempIndex2;
                _ref.isToggle = EditorGUILayout.Toggle("Toggle?", _ref.isToggle);
                
            }


            GUILayout.Space(10);
            GUILayout.Label("TRIGGER SETTINGS", titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            
            _ref.mouseActions = EditorGUILayout.Toggle("Mouse Actions?", _ref.mouseActions);
            if (_ref.mouseActions)
            {
                _ref.onMouseEnter = EditorGUILayout.Toggle("On Mouse Enter?", _ref.onMouseEnter);
                _ref.onMouseExit = EditorGUILayout.Toggle("On Mouse Exit?", _ref.onMouseExit);
                _ref.onClick = EditorGUILayout.Toggle("On Click?", _ref.onClick);
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

        private GUIStyle GetStyle(string styleName)
        {
            var style = new GUIStyle();
            switch (styleName)
            {
                case "title":
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 20;
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = Color.white;
                    break;
                case "text":
                    style.alignment = TextAnchor.MiddleLeft;
                    style.fontSize = 18;
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = Color.white;
                    break;
                
                case "text2":
                    style.alignment = TextAnchor.UpperLeft;
                    style.fontSize = 16;
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = Color.white;
                    break;
                
                case "removeButton":
                    style.normal.textColor = Color.red;
                    style.fontSize = 30;
                    break;
                
                case "collapseGroup":
                    style.normal.textColor = Color.gray;
                    style.fontSize = 30;
                    break;
                
                case "openGroup":
                    style.normal.textColor = Color.green;
                    style.fontSize = 30;
                    break;
            }

            return style;
        }

        private GUIContent GetGUIContent (string name, string tooltip)
        {
            return new GUIContent(name, tooltip);
        }
    }
}
