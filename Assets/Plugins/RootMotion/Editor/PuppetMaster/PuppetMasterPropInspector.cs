using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion.Dynamics
{
    [CustomEditor(typeof(PuppetMasterProp))]
    public class PuppetMasterPropInspector : Editor
    {

        private PuppetMasterProp script { get { return target as PuppetMasterProp; } }

        private GUIStyle style = new GUIStyle();
        private GUIStyle miniLabelStyle = new GUIStyle();
        private static Color pro = new Color(0.5f, 0.7f, 0.3f, 1f);
        private static Color free = new Color(0.2f, 0.3f, 0.1f, 1f);
        private static Color sceneColor = new Color(0.2f, 0.7f, 1f);

        public override void OnInspectorGUI()
        {
            if (script == null) return;
            serializedObject.Update();

            style.wordWrap = true;
            style.normal.textColor = EditorGUIUtility.isProSkin ? pro : free;

            miniLabelStyle.wordWrap = true;
            miniLabelStyle.fontSize = 10;
            miniLabelStyle.normal.textColor = EditorStyles.miniLabel.normal.textColor;

            DrawDefaultInspector();

            script.muscleProps.group = Muscle.Group.Prop;

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            DrawScene(script);
        }

        protected void DrawScene(PuppetMasterProp script)
        {
            if (script == null) return;
            if (Application.isPlaying) return;

            GUIStyle sceneLabelStyle = new GUIStyle();
            sceneLabelStyle.wordWrap = false;
            sceneLabelStyle.normal.textColor = sceneColor;

            Handles.color = sceneColor;
            float size = GetHandleSize(script.transform.position);

            if (script.additionalPinOffsetAdd != Vector3.zero)
            {
                SphereCapSafe(script.transform.position, Quaternion.identity, size);
                Vector3 aPos = script.transform.position + script.transform.rotation * script.additionalPinOffsetAdd;
                Handles.DrawLine(script.transform.position, aPos);
                SphereCapSafe(aPos, Quaternion.identity, size);

                if (Selection.activeGameObject == script.gameObject)
                {
                    Handles.Label(aPos + script.additionalPinOffsetAdd.normalized * size * 2f, new GUIContent("Additional Pin"), sceneLabelStyle);
                }
            }

            Handles.color = Color.white;
        }


        private static void SphereCapSafe(Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
            Handles.SphereHandleCap(0, position, rotation, size, EventType.Repaint);
#else
            Handles.SphereCap(0, position, rotation, size);
#endif
        }


        private static float GetHandleSize(Vector3 position)
        {
            float s = HandleUtility.GetHandleSize(position) * 0.1f;
            return Mathf.Lerp(s, 0.025f, 0.2f);
        }
    }
}
