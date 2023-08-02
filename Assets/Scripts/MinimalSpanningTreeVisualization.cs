using System.Collections.Generic;
using UnityEngine;


public class MinimalSpanningTreeVisualization : DungeonGeneration
{
    [Header("Settings")]
    public Vector2Int centerPoint;
    public int pointsCount;
    public int randomPointsArea;
    public GameObject pointPrefab;

    private List<GameObject> gameObjects = new List<GameObject>();
    private List<Vector2> points = new List<Vector2>();
    public override void Clear()
    {
        foreach (var o in gameObjects)
            DestroyImmediate(o);

        gameObjects.Clear();
        points.Clear();
    }
    protected override void RunGeneration()
    {
        Clear();
        for (int i = 0; i < pointsCount; i++)
        {
            GameObject obj = Instantiate(pointPrefab);
            gameObjects.Add(obj);
            Vector2 objPos = new Vector2((int)Random.Range(centerPoint.x - randomPointsArea, centerPoint.x + randomPointsArea), (int)Random.Range(centerPoint.y - randomPointsArea, centerPoint.y + randomPointsArea));
            obj.transform.position = objPos;
            points.Add(new Vector2Int((int)objPos.x, (int)objPos.y));
        }

        PrimAlgorithm.Graph minimumSpanningTree = PrimAlgorithm.BuildMinimumSpanningTree(points);


        for (int i = 0; i < minimumSpanningTree.adjacencyList.Count; i++)
        {
            foreach (var edge in minimumSpanningTree.adjacencyList[i])
            {

                Vector3 start = new Vector3(minimumSpanningTree.vertices[i].x, minimumSpanningTree.vertices[i].y, 0);
                Vector3 end = new Vector3(minimumSpanningTree.vertices[edge.vertexIndex].x, minimumSpanningTree.vertices[edge.vertexIndex].y, 0);
                Debug.DrawLine(start, end, Color.red, 3f);
            }
        }
    }
}
