using UnityEditor;
using UnityEngine;

namespace BLINK.WorldClusters
{
    [CustomEditor(typeof(ClusterConditions))]
    public class ClusterConditionsEditor : Editor
    {
        private ClusterConditions _ref;
        private GUISkin _skin;
        private WorldClustersEditorData _editorData;
        
        
        private void OnEnable()
        {
            _ref = (ClusterConditions) target;
            _skin = Resources.Load<GUISkin>("EditorData/WorldClustersEditorSkin");
            _editorData = Resources.Load<WorldClustersEditorData>("EditorData/WorldClustersEditorData");
        }

        public override void OnInspectorGUI()
        {
            if (_skin == null) return;
            EditorGUI.BeginChangeCheck();
            
            if (GUILayout.Button("Add Condition", _skin.GetStyle(_editorData.addButtonStyle),
                GUILayout.Height(30)))
            {
                _ref.collisionConditions.Add(new CollisionCondition());
            }
            GUILayout.Space(10);

            for (int i = 0; i < _ref.collisionConditions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _ref.collisionConditions[i].type =
                    (ClUSTER_COLLISION_CONDITION_TYPE) EditorGUILayout.EnumPopup(_ref.collisionConditions[i].type);
                GUILayout.Space(10);
                if (GUILayout.Button("", _skin.GetStyle(_editorData.removeButtonStyle),
                    GUILayout.Width(_editorData.removeButtonSize),
                    GUILayout.Height(_editorData.removeButtonSize)))
                {
                    _ref.collisionConditions.RemoveAt(i);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                switch (_ref.collisionConditions[i].type)
                {
                    case ClUSTER_COLLISION_CONDITION_TYPE.GameObjectName:
                        _ref.collisionConditions[i].gameObjectName =
                            EditorGUILayout.TextField(_ref.collisionConditions[i].gameObjectName);
                        break;
                    case ClUSTER_COLLISION_CONDITION_TYPE.LayerMask:
                        _ref.collisionConditions[i].layer =
                            EditorGUILayout.LayerField(_ref.collisionConditions[i].layer);
                        break;
                    case ClUSTER_COLLISION_CONDITION_TYPE.Tag:
                        _ref.collisionConditions[i].tagName =
                            EditorGUILayout.TagField(_ref.collisionConditions[i].tagName);
                        break;
                }
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("RULE:", GUILayout.MaxWidth(50));
                _ref.collisionConditions[i].requirementType =
                    (ClUSTER_CONDITION_REQUIREMENT_TYPE) EditorGUILayout.EnumPopup(_ref.collisionConditions[i].requirementType);
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(15);
            }
            
            if (!EditorGUI.EndChangeCheck()) return;
            EditorUtility.SetDirty(_ref);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
