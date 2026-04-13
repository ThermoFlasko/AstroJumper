using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GroundLevelGenerator))]
public class GroundLevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Generate a chunk preview directly in the scene view. Use Clear Preview to remove the generated chunks.",
            MessageType.Info);

        GroundLevelGenerator generator = (GroundLevelGenerator)target;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Generate Preview"))
                generator.GenerateLevel();

            if (GUILayout.Button("Clear Preview"))
                generator.ClearGeneratedChunks();
        }
    }
}
