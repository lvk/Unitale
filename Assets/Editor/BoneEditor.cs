using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoneVerticalProjectile))]
[CanEditMultipleObjects]
public class BoneEditor : Editor
{
    private SerializedProperty property;

    // Use this for initialization
    public void OnEnable()
    {
        property = serializedObject.FindProperty("Height");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        property.intValue = EditorGUILayout.IntField("Bone Height", property.intValue);
        foreach (Object target in serializedObject.targetObjects)
        {
            BoneVerticalProjectile bone = (BoneVerticalProjectile)target;
            bone.OnStart();
            bone.UpdateHeight(property.intValue);
        }
        serializedObject.ApplyModifiedProperties();
    }
}