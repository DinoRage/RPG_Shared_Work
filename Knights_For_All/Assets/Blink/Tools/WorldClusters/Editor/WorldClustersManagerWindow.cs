using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.WorldClusters
{
    public class WorldClustersManagerWindow : EditorWindow
    {
        private ScriptableObject scriptableObj;
        private SerializedObject serialObj;
        private GUISkin _skin;
        private WorldClustersEditorData _editorData;

        public GameObject[] gameObjectParents;

        private Vector2 viewScrollPosition;

        private enum Categories
        {
            Home = 0,
            Scene = 1,
            Utilities = 2
        }

        private Categories currentCategory;

        [MenuItem("BLINK/World Clusters/Manager")]
        private static void OpenWindow()
        {
            var window = (WorldClustersManagerWindow) GetWindow(typeof(WorldClustersManagerWindow), false,
                "World Clusters Manager");
            window.minSize = new Vector2(400, 250);
            GUI.contentColor = Color.white;
            window.Show();
        }

        private void OnGUI()
        {
            if (_skin == null) return;
            DrawManagerWindow();
        }

        private void OnEnable()
        {
            scriptableObj = this;
            serialObj = new SerializedObject(scriptableObj);
            _skin = Resources.Load<GUISkin>("EditorData/WorldClustersEditorSkin");
            _editorData = Resources.Load<WorldClustersEditorData>("EditorData/WorldClustersEditorData");
        }

        private void Update()
        {
            Repaint();
        }

        private void DrawManagerWindow()
        {
            viewScrollPosition = EditorGUILayout.BeginScrollView(viewScrollPosition, false, false);

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("HOME",
                currentCategory == Categories.Home
                    ? _skin.GetStyle(_editorData.buttonSelectedStyle)
                    : _skin.GetStyle(_editorData.buttonOffStyle),
                GUILayout.ExpandWidth(true)))
            {
                currentCategory = Categories.Home;
            }

            GUILayout.Space(10);
            if (GUILayout.Button("UTILITIES",
                currentCategory == Categories.Utilities
                    ? _skin.GetStyle(_editorData.buttonSelectedStyle)
                    : _skin.GetStyle(_editorData.buttonOffStyle),
                GUILayout.ExpandWidth(true)))
            {
                currentCategory = Categories.Utilities;
            }

            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();

            switch (currentCategory)
            {
                case Categories.Home:
                    DrawHome();
                    break;
                case Categories.Scene:
                    DrawScene();
                    break;
                case Categories.Utilities:
                    DrawUtilities();
                    break;
            }

            serialObj.ApplyModifiedProperties();

            GUILayout.Space(20);
            GUILayout.EndScrollView();
        }

        private void DrawHome()
        {
            GUILayout.Space(15);
            
            EditorGUILayout.LabelField("Watch these videos:", GetStyle("title"),
                GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
                
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Introduction to World Cluster", _skin.GetStyle(_editorData.addButtonStyle),
                GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                Application.OpenURL("https://youtu.be/7x1fe55qsxo");
            }
            GUILayout.Space(15);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawScene()
        {
            
        }
        
        private void DrawUtilities()
        {
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("ADD MANAGER TO SCENE", _skin.GetStyle(_editorData.addButtonStyle), GUILayout.Height(30), GUILayout.ExpandWidth(true)))
            {
                if (FindObjectOfType<WorldClustersManager>() == null)
                {
                    GameObject manager = new GameObject();
                    manager.name = "WorldCluster_MANAGER";
                    manager.AddComponent<WorldClustersManager>();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
                else
                {
                    EditorUtility.DisplayDialog("Hey!", "A World Cluster Manager is already in the scene", "OK");
                }
            }
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Select Components In Child:", GetStyle("title"),
                GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            var serialProp = serialObj.FindProperty("gameObjectParents");
            EditorGUILayout.PropertyField(serialProp, true);
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("Renderers", _skin.GetStyle(_editorData.addButtonStyle),
                GUILayout.Height(30), GUILayout.ExpandWidth(true), GUILayout.MaxWidth(175)))
            {
                Selection.objects = null;
                List<GameObject> renderersGO = new List<GameObject>();
                foreach (var go in gameObjectParents)
                {
                    foreach (var childRenderer in go.GetComponentsInChildren<Renderer>())
                    {
                        if(!IsValidRenderer(childRenderer.GetType().ToString())) continue;
                        renderersGO.Add(childRenderer.gameObject);
                    }
                }
                Selection.objects = renderersGO.ToArray();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Lights", _skin.GetStyle(_editorData.addButtonStyle),
                GUILayout.Height(30), GUILayout.ExpandWidth(true), GUILayout.MaxWidth(175)))
            {
                Selection.objects = null;
                List<GameObject> renderersGO = new List<GameObject>();
                foreach (var go in gameObjectParents)
                {
                    foreach (var childRenderer in go.GetComponentsInChildren<Light>())
                    {
                        renderersGO.Add(childRenderer.gameObject);
                    }
                }
                Selection.objects = renderersGO.ToArray();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Particle Systems", _skin.GetStyle(_editorData.addButtonStyle),
                GUILayout.Height(30), GUILayout.ExpandWidth(true), GUILayout.MaxWidth(175)))
            {
                Selection.objects = null;
                List<GameObject> renderersGO = new List<GameObject>();
                foreach (var go in gameObjectParents)
                {
                    foreach (var childRenderer in go.GetComponentsInChildren<ParticleSystem>())
                    {
                        renderersGO.Add(childRenderer.gameObject);
                    }
                }
                Selection.objects = renderersGO.ToArray();
            }
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();

        }

        private bool IsValidRenderer(string typeName)
        {
            return typeName == "UnityEngine.MeshRenderer" || typeName == "UnityEngine.SkinnedMeshRenderer";
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
                    style.fontSize = 17;
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
    }
}
