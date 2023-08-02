using System.Collections.Generic;
using UnityEngine;

public static class PrimAlgorithm
{
    public class Edge
    {
        public int vertexIndex;
        public float weight;

        public Edge(int vertexIndex, float weight)
        {
            this.vertexIndex = vertexIndex;
            this.weight = weight;
        }
    }

    public class Graph
    {
        public List<Vector2> vertices;
        public List<List<Edge>> adjacencyList;

        public Graph(List<Vector2> vertices)
        {
            this.vertices = vertices;
            adjacencyList = new List<List<Edge>>();
            for (int i = 0; i < vertices.Count; i++)
            {
                adjacencyList.Add(new List<Edge>());
            }
        }

        public void AddEdge(int fromVertex, int toVertex, float weight)
        {
            adjacencyList[fromVertex].Add(new Edge(toVertex, weight));
            adjacencyList[toVertex].Add(new Edge(fromVertex, weight));
        }
    }

    public static Graph BuildMinimumSpanningTree(List<Vector2> points)
    {
        Graph graph = new Graph(points);

        for (int i = 0; i < points.Count; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                float weight = Mathf.Abs(points[i].x - points[j].x) + Mathf.Abs(points[i].y - points[j].y);
                graph.AddEdge(i, j, weight);
            }
        }
        return BuildMinimumSpanningTreeUsingPrim(graph);
    }

    public static Graph BuildMinimumSpanningTreeUsingPrim(Graph graph)
    {
        int vertexCount = graph.vertices.Count;
        bool[] visited = new bool[vertexCount];

        visited[0] = true;

        Graph minimumSpanningTree = new Graph(graph.vertices);

        int[] parents = new int[vertexCount];
        parents[0] = -1;

        while (!AllVerticesVisited(visited))
        {
            float minWeight = float.MaxValue;
            Edge minEdge = null;
            int parentVertex = -1;

            for (int i = 0; i < vertexCount; i++)
            {
                if (visited[i])
                {
                    foreach (var edge in graph.adjacencyList[i])
                    {
                        if (!visited[edge.vertexIndex] && edge.weight < minWeight)
                        {
                            minWeight = edge.weight;
                            minEdge = edge;
                            parentVertex = i;
                        }
                    }
                }
            }

            if (minEdge != null)
            {
                visited[minEdge.vertexIndex] = true;
                minimumSpanningTree.AddEdge(parentVertex, minEdge.vertexIndex, minEdge.weight);
                parents[minEdge.vertexIndex] = parentVertex;
            }
            else
            {
                break;
            }
        }

        return minimumSpanningTree;
    }
    private static bool AllVerticesVisited(bool[] visited)
    {
        foreach (bool v in visited)
        {
            if (!v)
            {
                return false;
            }
        }
        return true;
    }
}
