using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CorridorFirstDungeonGenerator : DungeonGeneration
{
    [Header("Corridor Settings")]
    [SerializeField]
    private int corridorLength;

    [SerializeField]
    private int buildIterationsCount;

    [SerializeField]
    [Range(1, 3)]
    private int corridorWidth;

    [SerializeField]
    [Range(0, 1)]
    private float roomPercent;


    [Header("Random walk settings")]
    [SerializeField]
    private Vector2Int minRoomSize;
    [SerializeField]
    private Vector2Int roomSizeDelta;

    [SerializeField]
    private bool randomWalkRooms;

    [SerializeField]
    private int walkLength;

    [SerializeField]
    private int steps;

    [SerializeField]
    private bool startRandomly;


    [Header("Optional settings")]
    [SerializeField]
    private bool expandFloor;

    [SerializeField]
    [Range(1, 10)]
    private int expandStrength;

    [SerializeField]
    private bool removeHolesInsideRoom;

    [SerializeField]
    [Range(1, 20)]
    private int removeHolesInerations;

    [Header("Output values")]
    public List<HashSet<Vector2Int>> MainRooms = new List<HashSet<Vector2Int>>();
    private HashSet<Vector2Int> DungeonPoints = new HashSet<Vector2Int>();
    public override void Clear()
    {
        visualizer.Clear();
    }
    protected override void RunGeneration()
    {
        HashSet<Vector2Int> floorPositions = new();
        HashSet<Vector2Int> potentialRoomPositions = new();
        List<HashSet<Vector2Int>> corridors = CreateCorridors(floorPositions, potentialRoomPositions);

        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);
        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateRoomsAtDeadEnds(deadEnds, roomPositions);

        if (corridorWidth != 1)
            for (int i = 0; i < corridors.Count; i++)
            {
                corridors[i] = DungeonGenerationAlgorithms.IncreaseCorridorSize(corridors[i].ToList(), corridorWidth).ToHashSet();
                floorPositions.UnionWith(corridors[i]);
            }

        floorPositions.UnionWith(roomPositions);
        DungeonPoints = new(floorPositions);

        visualizer.VisualizeTiles(floorPositions);
        WallGeneration.CreateWalls(floorPositions, visualizer);
    }
    private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomPositions)
    {
        foreach (var position in deadEnds)
        {
            if (!roomPositions.Contains(position))
                CreateOneRoom(position, roomPositions);
        }
    }
    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new();
        foreach (var position in floorPositions)
        {
            int neighboursCount = 0;
            foreach (var direction in Directions.Cardinal)
            {
                if (floorPositions.Contains(position + direction))
                    neighboursCount++;
            }
            if (neighboursCount == 1)
                deadEnds.Add(position);
        }
        return deadEnds;
    }
    private List<HashSet<Vector2Int>> CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);
        List<HashSet<Vector2Int>> corridors = new();
        for (int i = 0; i < buildIterationsCount; i++)
        {
            var corridor = DungeonGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            corridors.Add(corridor);
            currentPosition = corridor.ToList()[^1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(corridor);
        }
        return corridors;
    }
    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);
        List<Vector2Int> roomToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();

        foreach (var roomPosition in roomToCreate)
            CreateOneRoom(roomPosition, roomPositions);

        return roomPositions;
    }
    private void CreateOneRoom(Vector2Int roomPosition, HashSet<Vector2Int> roomPositions)
    {
        Vector2Int rndRoomSize = minRoomSize + new Vector2Int(Random.Range(0, roomSizeDelta.x), Random.Range(0, roomSizeDelta.y));
        HashSet<Vector2Int> roomFloor = randomWalkRooms ?
            DungeonGenerationAlgorithms.RandomWalk(roomPosition, walkLength, steps, startRandomly) : CreateBoxRoom(roomPosition, rndRoomSize);
        if (removeHolesInsideRoom)
            RemoveHolesInsideRoom(roomFloor);
        if (expandFloor)
            ExpandFloor(roomFloor);

        MainRooms.Add(roomFloor);
        roomPositions.UnionWith(roomFloor);
    }
    private void RemoveHolesInsideRoom(HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> pointsToDelete = new();
        HashSet<Vector2Int> pointsToAdd = new();

        for (int i = 0; i < removeHolesInerations; i++)
        {
            foreach (var pos in floorPositions)
            {
                foreach (var dir in Directions.Cardinal)
                {
                    var near = pos + dir;
                    if (!floorPositions.Contains(near))
                        pointsToDelete.Add(near);
                }
            }
            foreach (var point in pointsToDelete)
            {
                int counter = 0;
                foreach (var dir in Directions.Cardinal)
                {
                    var next = point + dir;
                    if (floorPositions.Contains(next))
                        counter++;
                }
                if (counter >= 3)
                    pointsToAdd.Add(point);
            }
            floorPositions.UnionWith(pointsToAdd);
        }

    }
    private void ExpandFloor(HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> pointsToDelete = new();
        for (int i = 0; i < expandStrength; i++)
        {
            foreach (var pos in floorPositions)
            {
                foreach (var dir in Directions.Cardinal)
                {
                    var near = pos + dir;
                    if (!floorPositions.Contains(near))
                        pointsToDelete.Add(near);
                }
            }
            floorPositions.UnionWith(pointsToDelete);
        }

    }
    private HashSet<Vector2Int> CreateBoxRoom(Vector2Int startPos, Vector2Int roomSize)
    {
        startPos = new Vector2Int(startPos.x - roomSize.x / 2, startPos.y - roomSize.y / 2);
        HashSet<Vector2Int> room = new();
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int add = new Vector2Int(x, y) + startPos;
                room.Add(add);
            }
        }
        return room;

    }
}
