using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BLINK.WorldClusters
{
    [CustomEditor(typeof(Cluster))]
    public class ClusterEditor : Editor
    {
        private Cluster _ref;
        private GUISkin _skin;
        private WorldClustersEditorData _editorData;

        private string currentSearch;
        private void OnEnable()
        {
            _ref = (Cluster) target;
            _skin = Resources.Load<GUISkin>("EditorData/WorldClustersEditorSkin");
            _editorData = Resources.Load<WorldClustersEditorData>("EditorData/WorldClustersEditorData");
            EditorApplication.update += Update;
        }
        
        void OnDisable() { EditorApplication.update -= Update; }

        void Update()
        {
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            if (_skin == null) return;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((Cluster) target),
                typeof(Cluster),
                false);
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();

            GUIStyle titleStyle = GetStyle("title");

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("CLUSTER GROUP: " + _ref.clusterGroups.Count, titleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(15);
        
            if (GUILayout.Button("+ ADD", _skin.GetStyle(_editorData.addButtonStyle),
                GUILayout.Height(30))) _ref.clusterGroups.Add(new ClusterDATA());
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            if (_ref.clusterGroups.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                        
                if (GUILayout.Button("Collapse All", _skin.GetStyle(_editorData.buttonOffStyle),
                    GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    foreach (var clusterGroup in _ref.clusterGroups)
                    {
                        foreach (var entry in clusterGroup.Entries)
                        {
                            entry.show = false;
                        }

                        clusterGroup.show = false;
                    }
                }
                GUILayout.Space(10);
                if (GUILayout.Button("Expand All", _skin.GetStyle(_editorData.buttonOffStyle),
                    GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    foreach (var clusterGroup in _ref.clusterGroups)
                    {
                        foreach (var entry in clusterGroup.Entries)
                        {
                            entry.show = true;
                        }

                        clusterGroup.show = true;
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SEARCH:", GetStyle("text2"), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
                currentSearch = EditorGUILayout.TextField(currentSearch);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(20);
            }

            var clusterGroupList = serializedObject.FindProperty("clusterGroups");
            _ref.clusterGroups = GetTargetObjectOfProperty(clusterGroupList) as List<ClusterDATA>;

            if (_ref.clusterGroups != null)
            {
                for (var cGroup = 0; cGroup < _ref.clusterGroups.Count; cGroup++)
                {
                    if (ShouldShowClusterGroup(_ref.clusterGroups[cGroup].clusterGroupName))
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("",
                            _ref.clusterGroups[cGroup].show
                                ? _skin.GetStyle(_editorData.openButtonStyle)
                                : _skin.GetStyle(_editorData.collapseButtonStyle),
                            GUILayout.Width(_editorData.removeButtonSize),
                            GUILayout.Height(_editorData.removeButtonSize)))
                        {
                            _ref.clusterGroups[cGroup].show = !_ref.clusterGroups[cGroup].show;
                            return;
                        }

                        GUILayout.Space(5);
                        _ref.clusterGroups[cGroup].clusterGroupName = EditorGUILayout.TextField(
                            GetGUIContent("", "The name of this cluster group"),
                            _ref.clusterGroups[cGroup].clusterGroupName, _skin.GetStyle("TextField"),
                            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        GUILayout.Space(5);
                        if (GUILayout.Button("", _skin.GetStyle(_editorData.removeButtonStyle),
                            GUILayout.Width(_editorData.removeButtonSize),
                            GUILayout.Height(_editorData.removeButtonSize)))
                        {
                            _ref.clusterGroups.RemoveAt(cGroup);
                            return;
                        }

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(7.5f);
                        if (_ref.clusterGroups[cGroup].show)
                        {
                            EditorGUILayout.BeginHorizontal();

                            if (GUILayout.Button("ENTRIES",
                                _ref.clusterGroups[cGroup].showEntries
                                    ? _skin.GetStyle(_editorData.buttonSelectedStyle)
                                    : _skin.GetStyle(_editorData.buttonOffStyle),
                                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                            {
                                _ref.clusterGroups[cGroup].showEntries = true;
                                _ref.clusterGroups[cGroup].showOverrides = false;
                            }

                            GUILayout.Space(10);
                            if (GUILayout.Button("OVERRIDES",
                                _ref.clusterGroups[cGroup].showOverrides
                                    ? _skin.GetStyle(_editorData.buttonSelectedStyle)
                                    : _skin.GetStyle(_editorData.buttonOffStyle),
                                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                            {
                                _ref.clusterGroups[cGroup].showEntries = false;
                                _ref.clusterGroups[cGroup].showOverrides = true;
                            }

                            EditorGUILayout.EndHorizontal();

                            if (_ref.clusterGroups[cGroup].showEntries)
                            {
                                DrawGroupEntries(cGroup);
                            }
                            else if (_ref.clusterGroups[cGroup].showOverrides)
                            {
                                DrawEntryOverrides(cGroup);
                            }
                        }
                    }
                }
            }

            if (!EditorGUI.EndChangeCheck()) return;
            EditorUtility.SetDirty(_ref);
            serializedObject.ApplyModifiedProperties();
        }

        private bool ShouldShowClusterGroup(string clusterName)
        {
            if (string.IsNullOrEmpty(currentSearch)) return true;
            string search = currentSearch.ToLower();
            string realClusterName = clusterName.ToLower();
            return realClusterName.Contains(search);
        }

        private void DrawEntryOverrides(int cGroup)
        {
            GUIStyle textStyle = GetStyle("text");
            GUIStyle textStyle2 = GetStyle("text2");
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            int count4 = _ref.clusterGroups[cGroup].overrides.Count;
            GUILayout.Label(count4 + (count4 > 1 ? " Overrides" : " Override"),
                textStyle,
                GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            if (GUILayout.Button("", _skin.GetStyle(_editorData.addSmallButtonStyle),
                GUILayout.Width(_editorData.removeButtonSize),
                GUILayout.Height(_editorData.removeButtonSize)))
            {
                _ref.clusterGroups[cGroup].overrides.Add(new ClusterOverride());
                _ref.clusterGroups[cGroup].showOverrides = true;
                return;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(7.5f);
            if (_ref.clusterGroups[cGroup].showOverrides)
            {
                for (var cAction = 0;
                    cAction < _ref.clusterGroups[cGroup].overrides.Count;
                    cAction++)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("", _skin.GetStyle(_editorData.removeButtonStyle),
                        GUILayout.Width(_editorData.removeButtonSize),
                        GUILayout.Height(_editorData.removeButtonSize)))
                    {
                        _ref.clusterGroups[cGroup].overrides.RemoveAt(cAction);
                        return;
                    }

                    GUILayout.Space(5);
                    GUILayout.Label("" + (cAction + 1) + ":", textStyle2,
                        GUILayout.ExpandWidth(false));
                    _ref.clusterGroups[cGroup].overrides[cAction].type =
                        (ClUSTER_OVERRIDE_TYPE) EditorGUILayout.EnumPopup(
                            GetGUIContent("", "The type of this action event"),
                            _ref.clusterGroups[cGroup].overrides[cAction]
                                .type, GUILayout.MaxWidth(100));
                    
                    string[] names = GetClusterGroupNames(_ref).ToArray();
                    var tempIndex2 = EditorGUILayout.Popup(_ref.clusterGroups[cGroup].overrides[cAction].clusterGroupIndex, names, GUILayout.MaxWidth(100));
                    if (names.Length > 0)
                        _ref.clusterGroups[cGroup].overrides[cAction].clusterGroupIndex = tempIndex2;
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
            }
            GUILayout.Space(10);
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

        private void DrawGroupEntries(int cGroup)
        {
            GUIStyle textStyle = GetStyle("text");
            GUIStyle textStyle2 = GetStyle("text2");

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            int count1 = _ref.clusterGroups[cGroup].Entries.Count;
            GUILayout.Label(count1 + (count1 > 1 ? " Entries" : " Entry"), textStyle,
                GUILayout.ExpandWidth(false));
            GUILayout.Space(15);
            if (GUILayout.Button("", _skin.GetStyle(_editorData.addSmallButtonStyle),
                GUILayout.Width(_editorData.removeButtonSize),
                GUILayout.Height(_editorData.removeButtonSize)))
            {
                _ref.clusterGroups[cGroup].show = true;
                _ref.clusterGroups[cGroup].Entries.Add(new ClusterEntry());
                return;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(7.5f);

            for (var cEntry = 0; cEntry < _ref.clusterGroups[cGroup].Entries.Count; cEntry++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("",
                    _ref.clusterGroups[cGroup].Entries[cEntry].show
                        ? _skin.GetStyle(_editorData.openButtonStyle)
                        : _skin.GetStyle(_editorData.collapseButtonStyle),
                    GUILayout.Width(_editorData.removeButtonSize),
                    GUILayout.Height(_editorData.removeButtonSize)))
                {
                    _ref.clusterGroups[cGroup].Entries[cEntry].show =
                        !_ref.clusterGroups[cGroup].Entries[cEntry].show;
                    return;
                }

                GUILayout.Space(5);
                GUILayout.Label("" + (cEntry + 1) + ":", textStyle2, GUILayout.ExpandWidth(false));
                GUILayout.Space(5);
                _ref.clusterGroups[cGroup].Entries[cEntry].Type =
                    (ClUSTER_ENTRY_TYPE) EditorGUILayout.EnumPopup(
                        GetGUIContent("", "The type of this entry"),
                        _ref.clusterGroups[cGroup].Entries[cEntry].Type, GUILayout.ExpandWidth(false));
                GUILayout.Space(5);
                if (GUILayout.Button("", _skin.GetStyle(_editorData.removeButtonStyle),
                    GUILayout.Width(_editorData.removeButtonSize),
                    GUILayout.Height(_editorData.removeButtonSize)))
                {
                    _ref.clusterGroups[cGroup].Entries.RemoveAt(cEntry);
                    return;
                }

                EditorGUILayout.EndHorizontal();

                if (_ref.clusterGroups[cGroup].Entries[cEntry].show)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    EditorGUILayout.BeginVertical();
                    var clusterReference = new SerializedObject(_ref);
                    SerializedProperty clusterGroup = clusterReference.FindProperty("clusterGroups");
                    clusterGroup = clusterGroup.GetArrayElementAtIndex(cGroup);
                    SerializedProperty clusterEntry = clusterGroup.FindPropertyRelative("Entries");
                    clusterEntry = clusterEntry.GetArrayElementAtIndex(cEntry);
                    switch (_ref.clusterGroups[cGroup].Entries[cEntry].Type)
                    {
                        case ClUSTER_ENTRY_TYPE.GameObject:
                            SerializedProperty gameObjectsList = clusterEntry.FindPropertyRelative("GameObjectList");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(gameObjectsList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            break;
                        case ClUSTER_ENTRY_TYPE.TagChange:
                            SerializedProperty gameObjectsTagList = clusterEntry.FindPropertyRelative("GameObjectList");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(gameObjectsTagList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            break;
                        case ClUSTER_ENTRY_TYPE.LayerChange:
                            SerializedProperty gameObjectsLayerList = clusterEntry.FindPropertyRelative("GameObjectList");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(gameObjectsLayerList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            _ref.clusterGroups[cGroup].Entries[cEntry].layerAppliedOnChild =
                                EditorGUILayout.Toggle("Layer Applied To Childs?",
                                    _ref.clusterGroups[cGroup].Entries[cEntry].layerAppliedOnChild);
                            break;
                        case ClUSTER_ENTRY_TYPE.Light:
                            SerializedProperty lightList = clusterEntry.FindPropertyRelative("LightList");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(lightList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            _ref.clusterGroups[cGroup].Entries[cEntry].lightTransition =
                                DrawHorizontalToggle("Intensity Blend?",
                                    "Should the light intensity lerp up or down instead of being instantly enabled or disabled?",
                                    _editorData.labelHeight,
                                    _ref.clusterGroups[cGroup].Entries[cEntry].lightTransition);
                            if (_ref.clusterGroups[cGroup].Entries[cEntry].lightTransition)
                            {
                                _ref.clusterGroups[cGroup].Entries[cEntry].lightTransitionTime =
                                    EditorGUILayout.FloatField("Transition Time",
                                        _ref.clusterGroups[cGroup].Entries[cEntry].lightTransitionTime);
                                _ref.clusterGroups[cGroup].Entries[cEntry].enableLightIntensityAmount =
                                    EditorGUILayout.FloatField("Intensity (Enabled)",
                                        _ref.clusterGroups[cGroup].Entries[cEntry].enableLightIntensityAmount);
                                _ref.clusterGroups[cGroup].Entries[cEntry].disableLightIntensityAmount =
                                    EditorGUILayout.FloatField("Intensity (Disabled)",
                                        _ref.clusterGroups[cGroup].Entries[cEntry].disableLightIntensityAmount);
                            }

                            break;
                        case ClUSTER_ENTRY_TYPE.Renderer:
                            SerializedProperty rendererList = clusterEntry.FindPropertyRelative("RendererList");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(rendererList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            _ref.clusterGroups[cGroup].Entries[cEntry].hideRendererNotDisable =
                                DrawHorizontalToggle("Keep Shadows?",
                                    "Should the shadow casted by these renderers still be rendered?",
                                    _editorData.labelHeight,
                                    _ref.clusterGroups[cGroup].Entries[cEntry].hideRendererNotDisable);
                            break;
                        case ClUSTER_ENTRY_TYPE.MaterialChange:
                            SerializedProperty rendererMaterialList = clusterEntry.FindPropertyRelative("RendererList");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(rendererMaterialList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            break;
                        case ClUSTER_ENTRY_TYPE.ParticleSystem:
                            SerializedProperty particleSystemList =
                                clusterEntry.FindPropertyRelative("ParticleSystemList");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(particleSystemList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            break;
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    GUILayout.Label("Actions", textStyle, GUILayout.ExpandWidth(false));
                    GUILayout.Space(10);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    EditorGUI.BeginDisabledGroup(true);
                    _ref.clusterGroups[cGroup].Entries[cEntry].action.ActionEventType1 =
                        (ClUSTER_ACTION_EVENT_TYPE) EditorGUILayout.EnumPopup(
                            GetGUIContent("", "The type of this action event"),
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.ActionEventType1,
                            GUILayout.MaxWidth(100));
                    EditorGUI.EndDisabledGroup();

                    
                    SerializedProperty clusterAction = clusterEntry.FindPropertyRelative("action");
                    //clusterAction = clusterAction.GetArrayElementAtIndex(cEntry);
                    switch (_ref.clusterGroups[cGroup].Entries[cEntry].Type)
                    {
                        case ClUSTER_ENTRY_TYPE.GameObject:
                        case ClUSTER_ENTRY_TYPE.Light:
                        case ClUSTER_ENTRY_TYPE.Renderer:
                        case ClUSTER_ENTRY_TYPE.ParticleSystem:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.enterActionType =
                                (ClUSTER_ACTION_TYPE) EditorGUILayout.EnumPopup(
                                    GetGUIContent("", "The type of this action"),
                                    _ref.clusterGroups[cGroup].Entries[cEntry].action.enterActionType,
                                    GUILayout.MaxWidth(100));
                            break;
                        case ClUSTER_ENTRY_TYPE.MaterialChange:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.enterMaterial =
                                (Material) EditorGUILayout.ObjectField(_ref.clusterGroups[cGroup]
                                        .Entries[cEntry].action.enterMaterial, typeof(Material), false,
                                    GUILayout.MaxWidth(100));
                            break;
                        case ClUSTER_ENTRY_TYPE.TagChange:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.enterTag =
                                EditorGUILayout.TagField(_ref.clusterGroups[cGroup].Entries[cEntry].action.enterTag, GUILayout.MaxWidth(100));
                            break;
                        case ClUSTER_ENTRY_TYPE.LayerChange:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.enterLayer =
                                EditorGUILayout.LayerField(_ref.clusterGroups[cGroup].Entries[cEntry].action
                                    .enterLayer, GUILayout.MaxWidth(100));
                            break;
                        
                        case ClUSTER_ENTRY_TYPE.UnityEvent:
                            SerializedProperty uEventList = clusterAction.FindPropertyRelative("enterEvents");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(uEventList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            break;
                    }

                    _ref.clusterGroups[cGroup].Entries[cEntry].action.enterDelay =
                        EditorGUILayout.FloatField(
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.enterDelay,
                            GUILayout.MaxWidth(100));

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    EditorGUI.BeginDisabledGroup(true);
                    _ref.clusterGroups[cGroup].Entries[cEntry].action.ActionEventType2 =
                        (ClUSTER_ACTION_EVENT_TYPE) EditorGUILayout.EnumPopup(
                            GetGUIContent("", "The type of this action event"),
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.ActionEventType2,
                            GUILayout.MaxWidth(100));
                    EditorGUI.EndDisabledGroup();

                    switch (_ref.clusterGroups[cGroup].Entries[cEntry].Type)
                    {
                        case ClUSTER_ENTRY_TYPE.GameObject:
                        case ClUSTER_ENTRY_TYPE.Light:
                        case ClUSTER_ENTRY_TYPE.Renderer:
                        case ClUSTER_ENTRY_TYPE.ParticleSystem:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.exitActionType =
                                (ClUSTER_ACTION_TYPE) EditorGUILayout.EnumPopup(
                                    GetGUIContent("", "The type of this action"),
                                    _ref.clusterGroups[cGroup].Entries[cEntry].action.exitActionType,
                                    GUILayout.MaxWidth(100));
                            break;
                        case ClUSTER_ENTRY_TYPE.MaterialChange:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.exitMaterial =
                                (Material) EditorGUILayout.ObjectField(_ref.clusterGroups[cGroup]
                                        .Entries[cEntry].action.exitMaterial, typeof(Material), false,
                                    GUILayout.MaxWidth(100));
                            break;
                        case ClUSTER_ENTRY_TYPE.TagChange:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.exitTag =
                                EditorGUILayout.TagField(_ref.clusterGroups[cGroup].Entries[cEntry].action.exitTag, GUILayout.MaxWidth(100));
                            break;
                        case ClUSTER_ENTRY_TYPE.LayerChange:
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.exitLayer =
                                EditorGUILayout.LayerField(_ref.clusterGroups[cGroup].Entries[cEntry].action
                                    .exitLayer, GUILayout.MaxWidth(100));
                            break;
                        case ClUSTER_ENTRY_TYPE.UnityEvent:
                            SerializedProperty uEventList = clusterAction.FindPropertyRelative("exitEvents");
                            clusterReference.Update();
                            EditorGUILayout.PropertyField(uEventList, true, GUILayout.ExpandWidth(false));
                            clusterReference.ApplyModifiedProperties();
                            break;
                    }

                    _ref.clusterGroups[cGroup].Entries[cEntry].action.exitDelay =
                        EditorGUILayout.FloatField(
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.exitDelay,
                            GUILayout.MaxWidth(100));

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(45);

                    EditorGUILayout.LabelField("Conditions:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(100));
                    _ref.clusterGroups[cGroup].Entries[cEntry].action.conditions =
                        (ClusterConditions) EditorGUILayout.ObjectField(
                            _ref.clusterGroups[cGroup].Entries[cEntry].action.conditions,
                            typeof(ClusterConditions), false, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(202));
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(10);
            }
            GUILayout.Space(10);
        }

        private GUIStyle GetStyle(string styleName)
        {
            var style = new GUIStyle();
            switch (styleName)
            {
                case "title":
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 22;
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = Color.white;
                    break;
                case "text":
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 16;
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
        
        private string DrawHorizontalTextField(string labelName, string tooltip, float smallFieldHeight, string content)
        {
            GUILayout.BeginHorizontal();
            if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GUILayout.Width(_editorData.labelWidth), GUILayout.Height(smallFieldHeight));
            content = EditorGUILayout.TextField(content, GUILayout.Height(smallFieldHeight));
            GUILayout.EndHorizontal();
            return content;
        }
        
        private int DrawHorizontalIntField(string labelName, string tooltip, float smallFieldHeight, int content)
        {
            GUILayout.BeginHorizontal();
            if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GUILayout.Width(_editorData.labelWidth), GUILayout.Height(smallFieldHeight));
            content = EditorGUILayout.IntField(content, GUILayout.Height(smallFieldHeight));
            GUILayout.EndHorizontal();
            return content;
        }
        private float DrawHorizontalFloatField(string labelName, string tooltip, float smallFieldHeight, float content)
        {
            GUILayout.BeginHorizontal();
            if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GUILayout.Width(_editorData.labelWidth), GUILayout.Height(smallFieldHeight));
            content = EditorGUILayout.FloatField(content, GUILayout.Height(smallFieldHeight));
            GUILayout.EndHorizontal();
            return content;
        }
        private bool DrawHorizontalToggle(string labelName, string tooltip, float smallFieldHeight, bool toggle)
        {
            GUILayout.BeginHorizontal();
            if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GUILayout.Width(_editorData.labelWidth), GUILayout.Height(smallFieldHeight));
            toggle = EditorGUILayout.Toggle(toggle, GUILayout.Height(smallFieldHeight));
            GUILayout.EndHorizontal();
            return toggle;
        }
        private Material DrawHorizontalMaterialField(string labelName, string tooltip, float smallFieldHeight, Material content)
        {
            GUILayout.BeginHorizontal();
            if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GUILayout.Width(_editorData.labelWidth), GUILayout.Height(smallFieldHeight));
            content = (Material)EditorGUILayout.ObjectField(content, typeof(Material),false,GUILayout.Height(smallFieldHeight));
            GUILayout.EndHorizontal();
            return content;
        }
        
        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }

            return obj;
        }
        private object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (var i = 0; i <= index; i++)
                if (!enm.MoveNext()) return null;
            return enm.Current;
        }
    }
}
