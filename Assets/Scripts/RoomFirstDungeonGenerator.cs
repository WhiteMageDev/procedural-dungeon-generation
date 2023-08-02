using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : DungeonGeneration
{
    [Header("Generation settings")]
    [SerializeField]
    private Vector2Int minRoomSize;

    [SerializeField]
    private Vector2Int dungeonAreaSize;

    [SerializeField]
    private int spaceBetweenRooms;

    [SerializeField]
    [Range(1, 3)]
    private int corridorWidth;

    [SerializeField]
    [Range(1, 1000)]
    private int roomsCount;

    [SerializeField]
    private bool fixedRoomsCount;

    [Header("Random walk settings")]
    [SerializeField]
    private bool randomWalkRooms;

    [SerializeField]
    private bool startRandomly;

    [SerializeField]
    private int walkLength;

    [SerializeField]
    private int steps;



    [Header("Output values")]
    public List<BoundsInt> MainRooms = new List<BoundsInt>();
    private HashSet<Vector2Int> DungeonPoints = new HashSet<Vector2Int>();
    public override void Clear()
    {
        visualizer.Clear();
    }
    protected override void RunGeneration()
    {
        BoundsInt spaceToSplit = new BoundsInt((Vector3Int)startPosition, (Vector3Int)dungeonAreaSize);

        List<BoundsInt> roomsList = DungeonGenerationAlgorithms.BinarySpacePartitioning(spaceToSplit, minRoomSize.x, minRoomSize.y, roomsCount, fixedRoomsCount);
        MainRooms = new(roomsList);

        HashSet<Vector2Int> floor = randomWalkRooms ?
            CreateRandomWalkRooms(roomsList) : CreateSimpleRooms(roomsList);

        List<Vector2Int> roomCenters = new();
        foreach (var room in roomsList)
            roomCenters.Add(new Vector2Int(Vector3Int.RoundToInt(room.center).x, Vector3Int.RoundToInt(room.center).y));

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        DungeonPoints = new(floor);

        visualizer.VisualizeTiles(floor);
        WallGeneration.CreateWalls(floor, visualizer);
    }
    private HashSet<Vector2Int> CreateRandomWalkRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new();
        for (int i = 0; i < roomsList.Count; i++)
        {
            BoundsInt roomBounds = roomsList[i];
            Vector2Int roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));

            HashSet<Vector2Int> roomFloor = DungeonGenerationAlgorithms.RandomWalk(roomCenter, walkLength, steps, startRandomly);

            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + spaceBetweenRooms) && position.x <= (roomBounds.xMax - spaceBetweenRooms) &&
                    position.y >= (roomBounds.yMin - spaceBetweenRooms) && position.y <= (roomBounds.yMax - spaceBetweenRooms))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }
    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPoint(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }
    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = DungeonGenerationAlgorithms.SimpleCorridor(currentCenter, destination).ToHashSet();
        if (corridorWidth != 1)
        {
            List<Vector2Int> expandedRoad = DungeonGenerationAlgorithms.IncreaseCorridorSize(corridor.ToList(), corridorWidth);
            corridor.UnionWith(expandedRoad);
        }
        return corridor;
    }
    private Vector2Int FindClosestPoint(Vector2Int currentCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;

        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2Int.Distance(position, currentCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }
    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomList)
    {
        HashSet<Vector2Int> floor = new();
        foreach (var room in roomList)
        {
            for (int col = spaceBetweenRooms; col < room.size.x - spaceBetweenRooms; col++)
            {
                for (int row = spaceBetweenRooms; row < room.size.y - spaceBetweenRooms; row++)
                {
                    Vector2Int position = new Vector2Int(col, row) + new Vector2Int(room.min.x, room.min.y);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }
}
