using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch
{
    [CustomEditor(typeof(ThreeMatch.UIButton), true)]
    public class UIButton_Editor : ImageEditor
    {
        SerializedProperty _onClickProp;
        SerializedProperty _useTweenProp;
        protected override void OnEnable()
        {
            base.OnEnable();
            _onClickProp = serializedObject.FindProperty("onClick");
            _useTweenProp = serializedObject.FindProperty("UseTween");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("UIButton Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_useTweenProp, new GUIContent("Use Tween Animation"));
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("OnClick Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onClickProp);
            serializedObject.ApplyModifiedProperties();
        }
    }
}