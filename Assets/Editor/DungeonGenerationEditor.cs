using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGeneration), true)]
public class DungeonGenerationEditor : Editor
{
    DungeonGeneration generator;
    private void Awake()
    {
        generator = (DungeonGeneration)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create"))
        {
            generator.GenerateDungeon();
        }
        if (GUILayout.Button("Clear"))
        {
            generator.Clear();
        }
    }
}
