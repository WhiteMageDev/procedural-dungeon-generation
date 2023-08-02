using UnityEngine;

public abstract class DungeonGeneration : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer visualizer;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;

    public void GenerateDungeon()
    {
        if (visualizer != null)
            visualizer.Clear();
        RunGeneration();
    }
    public abstract void Clear();
    protected abstract void RunGeneration();
}
