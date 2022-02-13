using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

namespace BLINK.Controller
{
    [CustomEditor(typeof(TopDownWASDController))]
    public class TopDownWASDControllerEditor : Editor
    {
        private TopDownWASDController REF;
        
        private void OnEnable()
        {
            REF = (TopDownWASDController) target;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((TopDownWASDController) target),
                typeof(TopDownWASDController),
                false);
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();

            GUIStyle titleStyle = GetStyle("title");
            
            GUILayout.Space(5);
            GUILayout.Label("REFERENCES", titleStyle);
            GUILayout.Space(5);
            
            REF._anim = (Animator) EditorGUILayout.ObjectField(GetGUIContent("Animator:", "The animator used by the character"),
                REF._anim, typeof(Animator), true);
            REF._characterController = (CharacterController) EditorGUILayout.ObjectField(GetGUIContent("Character Controller:", "The Character Controller used by the character"),
                REF._characterController, typeof(CharacterController), true);
            
            GUILayout.Space(5);
            GUILayout.Label("CAMERA", titleStyle);
            GUILayout.Space(5);
            REF.cameraEnabled = EditorGUILayout.Toggle(GetGUIContent("Camera Enabled?", "Should the camera logic and input be executed?"),
                REF.cameraEnabled);
            if (REF.cameraEnabled)
            {
                REF.initCameraOnSpawn = EditorGUILayout.Toggle(
                    GetGUIContent("Init Camera?", "Should the camera be initialized after spawning the player?"),
                    REF.initCameraOnSpawn);
                if (REF.initCameraOnSpawn)
                {
                    REF.cameraName = EditorGUILayout.TextField(
                        GetGUIContent("Camera Name:", "The GameObject name of the camera to find on start"),
                        REF.cameraName);
                }
                else
                {
                    REF.playerCamera = (Camera) EditorGUILayout.ObjectField(
                        GetGUIContent("Camera:", "The camera used to follow the player"),
                        REF.playerCamera, typeof(Camera), true);
                }

                REF.cameraPositionOffset = EditorGUILayout.Vector3Field(
                    GetGUIContent("Position:", "The position offset that the camera will have relative to the player"),
                    REF.cameraPositionOffset);
                REF.cameraRotationOffset = EditorGUILayout.Vector3Field(
                    GetGUIContent("Rotation:", "The rotation at which the camera will start"),
                    REF.cameraRotationOffset);
                REF.minCameraHeight = EditorGUILayout.FloatField(
                    GetGUIContent("Min. Height:", "The minimum height at which the camera can be"),
                    REF.minCameraHeight);
                REF.maxCameraHeight = EditorGUILayout.FloatField(
                    GetGUIContent("Max. Height:", "The maximum height at which the camera can be"),
                    REF.maxCameraHeight);
                REF.minCameraVertical = EditorGUILayout.FloatField(
                    GetGUIContent("Min. Vertical:",
                        "The minimum distance on the Z axis that the camera can be relative to the character"),
                    REF.minCameraVertical);
                REF.maxCameraVertical = EditorGUILayout.FloatField(
                    GetGUIContent("Max. Vertical:",
                        "The maximum distance on the Z axis that the camera can be relative to the character"),
                    REF.maxCameraVertical);
                REF.cameraZoomSpeed = EditorGUILayout.FloatField(
                    GetGUIContent("Zoom Speed:", "The speed at which the camera zoom is applied each fixed frame"),
                    REF.cameraZoomSpeed);
                REF.cameraZoomPower = EditorGUILayout.FloatField(
                    GetGUIContent("Zoom Power:", "How strong the zoom is for each input"),
                    REF.cameraZoomPower);
                
                REF.isDraggable = EditorGUILayout.Toggle(
                    GetGUIContent("Drag Rotate?", "Should the camera rotate when dragging?"),
                    REF.isDraggable);

                REF.cameraDampTime = EditorGUILayout.FloatField(
                    GetGUIContent("Damp Time:", "How fast should the camera follow the character?"),
                    REF.cameraDampTime);
                REF.offsetDampTime = EditorGUILayout.FloatField(
                    GetGUIContent("Offset Damp Time:", "How fast should the camera follow the character relative to its offset?"),
                    REF.offsetDampTime);
                REF.maxOffset = EditorGUILayout.FloatField(
                    GetGUIContent("Max Offset", "How far from the character can the camera go?"),
                    REF.maxOffset);
            }

            GUILayout.Space(5);
            GUILayout.Label("NAVIGATION SETTINGS", titleStyle);
            GUILayout.Space(5);
            REF.movementEnabled = EditorGUILayout.Toggle(GetGUIContent("Movement Enabled?", "Should the movement logic and input be executed?"),
                REF.movementEnabled);
            if (REF.movementEnabled)
            {
                
                REF.moveSpeed = EditorGUILayout.FloatField(
                    GetGUIContent("Movement Speed", "How fast does the character move?"),
                    REF.moveSpeed);
                REF.jumpHeight = EditorGUILayout.FloatField(
                    GetGUIContent("Jump Height", "How can the character jump?"),
                    REF.jumpHeight);
                REF.gravity = EditorGUILayout.FloatField(
                    GetGUIContent("Gravity", "How strong is the gravity?"),
                    REF.gravity);
            }

            GUILayout.Space(5);
            GUILayout.Label("INPUT SETTINGS", titleStyle);
            GUILayout.Space(5);
            REF.moveUpKey = (KeyCode) EditorGUILayout.EnumPopup(GetGUIContent("Move Up Key:", "The key to use to move up"), REF.moveUpKey);
            REF.moveDownKey = (KeyCode) EditorGUILayout.EnumPopup(GetGUIContent("Move Down Key:", "The key to use to move down"), REF.moveDownKey);
            REF.moveLeftKey = (KeyCode) EditorGUILayout.EnumPopup(GetGUIContent("Move Left Key:", "The key to use to move left"), REF.moveLeftKey);
            REF.moveRightKey = (KeyCode) EditorGUILayout.EnumPopup(GetGUIContent("Move Right Key:", "The key to use to move right"), REF.moveRightKey);
            REF.jumpKey = (KeyCode) EditorGUILayout.EnumPopup(GetGUIContent("Jump Key:", "The key to use to jump"), REF.jumpKey);
            
            GUILayout.Space(5);
            GUILayout.Label("IK SETTINGS", titleStyle);
            GUILayout.Space(5);
            REF.bodyWeightIK = EditorGUILayout.FloatField(
                GetGUIContent("Body Weight", "How affected the body is by IK?"),
                REF.bodyWeightIK);
            REF.headWeightIK = EditorGUILayout.FloatField(
                GetGUIContent("Head Weight", "How affected the head is by IK?"),
                REF.headWeightIK);
            REF.dampSmoothTimeIK = EditorGUILayout.FloatField(
                GetGUIContent("Smooth Time", "How quick is the IK smoothing?"),
                REF.dampSmoothTimeIK);
            REF.dampSmoothTimeRotation = EditorGUILayout.FloatField(
                GetGUIContent("Rotation Time", "How quick is the IK rotation?"),
                REF.dampSmoothTimeRotation);
            
            GUILayout.Space(5);
            GUILayout.Label("ANIMATOR SETTINGS", titleStyle);
            GUILayout.Space(5);
            REF.animatorSmoothTime = EditorGUILayout.FloatField(
                GetGUIContent("Animation Smooth Time", "How quick is the blending between animations?"),
                REF.animatorSmoothTime);

            if (!EditorGUI.EndChangeCheck()) return;
            EditorUtility.SetDirty(REF);
            serializedObject.ApplyModifiedProperties();
            
        }

        private GUIStyle GetStyle(string styleName)
        {
            var style = new GUIStyle();
            switch (styleName)
            {
                case "title":
                    style.alignment = TextAnchor.UpperLeft;
                    style.fontSize = 17;
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = Color.white;
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
