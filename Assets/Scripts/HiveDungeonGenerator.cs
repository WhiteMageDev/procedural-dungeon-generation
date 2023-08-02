using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class HiveDungeonGenerator : DungeonGeneration
{

    [Header("GenerationSettings")]
    [SerializeField]
    private Vector2 generationArea;

    [SerializeField]
    private Vector2Int minimalRoomSize;

    [SerializeField]
    private Vector2Int deltaRoomSize;

    [SerializeField]
    private Vector2 spaceBetweenRooms;

    [SerializeField]
    private int startRoomsCount;

    [SerializeField]
    private float physicsSimulationDeltaTime;

    [SerializeField]
    [Range(1, 3)]
    private int corridorWidth;

    [SerializeField]
    [Range(1, 2)]
    private float mainRoomMultiplier = 1.25f;

    [SerializeField]
    private GameObject roomPrefab;

    [SerializeField]
    private bool onlyMainRooms;

    [SerializeField]
    private bool generateInCircle;


    private List<GameObject> tempListOfRoomsGO = new List<GameObject>();
    private List<GameObject> tempListOfMainRoomsGO = new List<GameObject>();
    private List<Vector2Int> roomCenters = new();



    [Header("Output values")]
    public List<BoundsInt> MainRooms = new List<BoundsInt>();
    public List<BoundsInt> SideRooms = new List<BoundsInt>();
    private HashSet<Vector2Int> DungeonPoints = new HashSet<Vector2Int>();
    protected override async void RunGeneration()
    {
        ClearLists();
        List<Bounds> rooms = CreateRandomRooms();

        List<Vector2> mainRoomCenters = new List<Vector2>();

        if (CreateRoomsVisual(rooms))
        {
            Debug.Log("Simulation start...");
            await RunPhysicsSimulationAsync();
            Debug.Log("Simulation over!");
            foreach (var obj in tempListOfRoomsGO)
            {
                int bottomLeftX = (int)(GetObjLeftCorner(obj).x);
                int bottomLeftY = (int)(GetObjLeftCorner(obj).y);
                float posX = bottomLeftX + obj.transform.localScale.x / 2;
                float posY = bottomLeftY + obj.transform.localScale.y / 2;

                obj.transform.position = new Vector2(posX, posY);

                if (tempListOfMainRoomsGO.Contains(obj))
                {
                    MainRooms.Add(new BoundsInt(bottomLeftX, bottomLeftY, 0, (int)obj.transform.localScale.x, (int)obj.transform.localScale.y, 0));
                    mainRoomCenters.Add(new Vector2(posX, posY));
                }
            }

            PrimAlgorithm.Graph minimumSpanningTree = CalculateMinSpanningTree(mainRoomCenters);

            DungeonPoints = AddRoomsOnTilemap(tempListOfMainRoomsGO);

            BuildCorridorsBetweenMainRooms(minimumSpanningTree, DungeonPoints, corridorWidth);

            if (!onlyMainRooms)
                AddSideRooms(DungeonPoints);

            visualizer.VisualizeTiles(DungeonPoints);
            WallGeneration.CreateWalls(DungeonPoints, visualizer);
            ClearLists();

        }
        else
        {
            return;
        }
    }
    public override void Clear()
    {
        visualizer.Clear();
        ClearLists();
        MainRooms.Clear();
        SideRooms.Clear();
        DungeonPoints.Clear();

    }
    private HashSet<Vector2Int> AddRoomsOnTilemap(List<GameObject> objects)
    {
        HashSet<Vector2Int> floor = new();
        foreach (var obj in objects)
        {
            Vector2Int startPoint = new Vector2Int((int)MathF.Floor(obj.transform.position.x - obj.transform.localScale.x / 2), (int)MathF.Floor(obj.transform.position.y - obj.transform.localScale.y / 2));
            Vector2Int size = new Vector2Int((int)(obj.transform.localScale.x), (int)(obj.transform.localScale.y));
            roomCenters.Add(startPoint + new Vector2Int(size.x / 2, size.y / 2));
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int add = new(x, y);

                    floor.Add(startPoint + add);
                }
            }
        }
        return floor;
    }
    private void AddSideRooms(HashSet<Vector2Int> floor)
    {
        tempListOfRoomsGO.RemoveAll(item => tempListOfMainRoomsGO.Contains(item));

        for (int i = 0; i < tempListOfRoomsGO.Count; i++)
        {
            GameObject obj = tempListOfRoomsGO[i];

            List<Vector2Int> newList = new List<Vector2Int>();
            Vector2Int startPoint = new Vector2Int((int)MathF.Floor(GetObjLeftCorner(obj).x), (int)MathF.Floor(GetObjLeftCorner(obj).y));
            Vector2Int size = new Vector2Int((int)(obj.transform.localScale.x), (int)(obj.transform.localScale.y));

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int add = new(x, y);

                    newList.Add(startPoint + add);
                }
            }
            foreach (var point in newList)
            {
                if (floor.Contains(point))
                {
                    obj.GetComponent<SpriteRenderer>().color = Color.red;
                    SideRooms.Add(new BoundsInt(startPoint.x, startPoint.y, 0, size.x, size.y, 0));
                    floor.UnionWith(newList);
                    break;
                }
            }
        }
    }
    private static void BuildCorridorsBetweenMainRooms(PrimAlgorithm.Graph minimumSpanningTree, HashSet<Vector2Int> floor, int corridorWidth)
    {
        List<(Vector2Int, Vector2Int)> last = new();

        for (int i = 0; i < minimumSpanningTree.adjacencyList.Count; i++)
        {
            foreach (var edge in minimumSpanningTree.adjacencyList[i])
            {

                Vector2Int start = new Vector2Int((int)minimumSpanningTree.vertices[i].x, (int)minimumSpanningTree.vertices[i].y);
                Vector2Int end = new Vector2Int((int)minimumSpanningTree.vertices[edge.vertexIndex].x, (int)minimumSpanningTree.vertices[edge.vertexIndex].y);

                (Vector2Int, Vector2Int) newListItem = (end, start);
                last.Add(newListItem);

                bool check = false;
                foreach (var item in last)
                {
                    if (item == (start, end))
                    {
                        check = true;
                        break;
                    }
                }
                if (!check)
                {
                    List<Vector2Int> corridor = DungeonGenerationAlgorithms.SimpleCorridor(start, end);
                    if (corridorWidth == 1)
                        floor.UnionWith(corridor);
                    else
                    {
                        List<Vector2Int> extendedCorridors = DungeonGenerationAlgorithms.IncreaseCorridorSize(corridor, corridorWidth);
                        floor.UnionWith(extendedCorridors);
                    }
                }
            }
        }
    }
    private static PrimAlgorithm.Graph CalculateMinSpanningTree(List<Vector2> mainRoomCenters)
    {
        PrimAlgorithm.Graph minimumSpanningTree = PrimAlgorithm.BuildMinimumSpanningTree(mainRoomCenters);
        for (int i = 0; i < minimumSpanningTree.adjacencyList.Count; i++)
        {
            foreach (var edge in minimumSpanningTree.adjacencyList[i])
            {
                Vector3 start = new Vector3(minimumSpanningTree.vertices[i].x, minimumSpanningTree.vertices[i].y, 0);
                Vector3 end = new Vector3(minimumSpanningTree.vertices[edge.vertexIndex].x, minimumSpanningTree.vertices[edge.vertexIndex].y, 0);
                Debug.DrawLine(start, end, Color.red, 2f);
            }
        }

        return minimumSpanningTree;
    }
    private bool CreateRoomsVisual(List<Bounds> rooms)
    {
        foreach (var room in rooms)
        {
            GameObject obj = Instantiate(roomPrefab, room.center, Quaternion.identity);
            obj.transform.localScale = room.size;
            tempListOfRoomsGO.Add(obj);

            BoxCollider2D boxCollider = obj.GetComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1 + spaceBetweenRooms.x, 1 + spaceBetweenRooms.y);

            float roomArea = obj.transform.localScale.x * obj.transform.localScale.y;
            float averageRoomArea = (minimalRoomSize.x + deltaRoomSize.x / 2) * (minimalRoomSize.y + deltaRoomSize.y / 2);

            if (roomArea >= averageRoomArea * mainRoomMultiplier)
            {
                tempListOfMainRoomsGO.Add(obj);
                obj.GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
        if (tempListOfMainRoomsGO.Count == 0 && onlyMainRooms)
            tempListOfMainRoomsGO = new(tempListOfRoomsGO);

        if (tempListOfMainRoomsGO.Count == 0 || tempListOfRoomsGO.Count == 0)
        {
            Debug.LogWarning("Is there something wrong. Check generation settings.");
            return false;
        }
        return true;
    }
    private List<Bounds> CreateRandomRooms()
    {
        List<Bounds> rooms = new List<Bounds>();

        for (int i = 0; i < startRoomsCount; i++)
        {
            Vector2 roomRandomCenter = generateInCircle ?
                GetRandomPointInCircle(generationArea.x, generationArea.y) : GetRandomPointInBox(generationArea.x, generationArea.y);

            int rndX = Random.Range(minimalRoomSize.x, minimalRoomSize.x + deltaRoomSize.x);
            int rndY = Random.Range(minimalRoomSize.y, minimalRoomSize.y + deltaRoomSize.y);
            Vector2 roomRandomSize = new(rndX, rndY);

            Bounds room = new Bounds(roomRandomCenter, roomRandomSize);
            rooms.Add(room);
        }

        return rooms;
    }
    private async Task RunPhysicsSimulationAsync()
    {
        List<Task> physicsTasks = new List<Task>();
        foreach (var obj in tempListOfRoomsGO)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            rb.simulated = true;
            physicsTasks.Add(SimulatePhysicsAsync(rb));
        }
        await Task.WhenAll(physicsTasks);
    }
    private async Task SimulatePhysicsAsync(Rigidbody2D rb)
    {
        await Task.Delay(100); // Пауза для обновления состояния перед началом симуляции

        float elapsedTime = 0f;
        while (elapsedTime < physicsSimulationDeltaTime)
        {
            rb.velocity = Vector2.zero;
            Physics2D.Simulate(Time.fixedDeltaTime);
            elapsedTime += Time.fixedDeltaTime;
            await Task.Yield(); // Подождать до следующего фрейма
        }
    }
    public static Vector2 GetRandomPointInCircle(float semiMajorAxis, float semiMinorAxis)
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float distance = Mathf.Sqrt(Random.Range(0f, 1f));

        float x = semiMajorAxis * distance * Mathf.Cos(angle);
        float y = semiMinorAxis * distance * Mathf.Sin(angle);

        return new Vector2(x, y);
    }
    public static Vector2 GetRandomPointInBox(float width, float height)
    {
        float x = Random.Range(0, width);
        float y = Random.Range(0, height);
        return new Vector2(x, y);
    }
    private void ClearLists()
    {

        foreach (var o in tempListOfRoomsGO)
        {
            DestroyImmediate(o);
        }
        foreach (var o in tempListOfMainRoomsGO)
        {
            DestroyImmediate(o);
        }
        tempListOfRoomsGO.Clear();
        tempListOfMainRoomsGO.Clear();
    }
    private Vector2 GetObjLeftCorner(GameObject obj)
    {
        float x = obj.transform.position.x - obj.transform.localScale.x / 2;
        float y = obj.transform.position.y - obj.transform.localScale.y / 2;
        return new Vector2(x, y);
    }
}