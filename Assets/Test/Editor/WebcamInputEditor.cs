using UnityEditor;
using UnityEngine;

namespace MediaPipe {

[CustomEditor(typeof(WebcamInput))]
sealed class WebcamInputEditor : Editor
{
    static readonly GUIContent SelectLabel = new GUIContent("Select");

    SerializedProperty _deviceName;
    SerializedProperty _resolution;

    void OnEnable()
    {
        _deviceName = serializedObject.FindProperty("_deviceName");
        _resolution = serializedObject.FindProperty("_resolution");
    }

    void ShowDeviceSelector(Rect rect)
    {
        var menu = new GenericMenu();

        foreach (var device in WebCamTexture.devices)
            menu.AddItem(new GUIContent(device.name), false,
                         () => { serializedObject.Update();
                                 _deviceName.stringValue = device.name;
                                 serializedObject.ApplyModifiedProperties(); });

        menu.DropDown(rect);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(_deviceName);

        var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(60));
        if (EditorGUI.DropdownButton(rect, SelectLabel, FocusType.Keyboard))
            ShowDeviceSelector(rect);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(_resolution);

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace MediaPipe
