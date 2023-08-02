using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DungeonGenerationAlgorithms
{
    private static HashSet<Vector2Int> RandomWalkStep(Vector2Int startPos, int walkLength)
    {
        HashSet<Vector2Int> path = new();
        path.Add(startPos);
        Vector2Int prevPos = startPos;
        for (int i = 0; i < walkLength; i++)
        {
            Vector2Int newPos = prevPos + GetRandomDirection();
            path.Add(newPos);
            prevPos = newPos;
        }
        return path;
    }
    public static HashSet<Vector2Int> RandomWalk(Vector2Int startPos, int walkLength, int steps, bool startRandomly)
    {
        var currentPos = startPos;
        HashSet<Vector2Int> pathPoints = new();
        for (int i = 0; i < steps; i++)
        {
            var path = RandomWalkStep(currentPos, walkLength);
            pathPoints.UnionWith(path);
            if (startRandomly)
                currentPos = pathPoints.ElementAt(Random.Range(0, pathPoints.Count));
        }
        return pathPoints;
    }

    public static HashSet<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        HashSet<Vector2Int> corridor = new();
        var direction = GetRandomDirection();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);

        while (corridor.Count < corridorLength)
        {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }
        return corridor;
    }

    public static List<Vector2Int> SimpleCorridor(Vector2Int a, Vector2Int b)
    {
        List<Vector2Int> corridor = new();
        var position = a;
        corridor.Add(position);
        Vector2Int xDir = b.x > a.x ? Vector2Int.right : Vector2Int.left;
        Vector2Int yDir = b.y > a.y ? Vector2Int.up : Vector2Int.down;

        while (a.x != b.x)
        {
            a += xDir;
            corridor.Add(a);
        }
        while (a.y != b.y)
        {
            a += yDir;
            corridor.Add(a);
        }
        return corridor;
    }
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight, int roomsCount, bool fixedRoomsCount)
    {
        Queue<BoundsInt> roomsQueue = new();
        List<BoundsInt> roomsList = new();
        roomsQueue.Enqueue(spaceToSplit);

        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                bool canSplitHorizontal = room.size.y >= minHeight * 2;
                bool canSplitVertical = room.size.x >= minWidth * 2;
                bool randomSplit = Random.value < 0.5f;

                if (canSplitHorizontal && canSplitVertical)
                {
                    if (randomSplit)
                    {
                        SplitVertical(minWidth, roomsQueue, room);
                    }
                    else
                    {
                        SplitHorizontal(minHeight, roomsQueue, room);
                    }
                }
                else if (canSplitHorizontal)
                {
                    SplitHorizontal(minHeight, roomsQueue, room);
                }
                else if (canSplitVertical)
                {
                    SplitVertical(minWidth, roomsQueue, room);
                }
                else
                {
                    roomsList.Add(room);
                }
            }
        }
        if (!fixedRoomsCount || roomsList.Count <= roomsCount)
        {
            return roomsList;
        }
        else
        {
            List<BoundsInt> roomsLimitedList = new();
            for (int i = 0; i < roomsCount; i++)
            {
                int index = Random.Range(0, roomsList.Count);
                roomsLimitedList.Add(roomsList[index]);
                roomsList.RemoveAt(index);
            }
            return roomsLimitedList;
        }

    }
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int roomsCount)
    {


        List<BoundsInt> roomsList = new();
        roomsList.Add(spaceToSplit);
        int count = 0;

        while (count < roomsCount)
        {
            List<BoundsInt> roomsListTemp = new(roomsList);
            foreach (var room in roomsListTemp)
            {
                bool randomSplit = Random.value < 0.5f;
                if (randomSplit)
                {
                    SplitVerticalY(room, roomsList);
                }
                else
                {
                    SplitHorizontalX(room, roomsList);
                }
                count = roomsList.Count;
            }
        }
        if (roomsList.Count > roomsCount)
        {
            while (roomsList.Count != roomsCount)
            {
                roomsList.RemoveAt(Random.Range(0, roomsList.Count));
            }
        }
        return roomsList;
    }

    private static void SplitHorizontalX(BoundsInt room, List<BoundsInt> roomsList)
    {
        var ySplit = Random.Range(1, room.size.y);
        BoundsInt room1 = new(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 = new(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
                              new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        roomsList.Add(room1);
        roomsList.Add(room2);
        roomsList.Remove(room);
    }

    private static void SplitVerticalY(BoundsInt room, List<BoundsInt> roomsList)
    {
        var xSplit = Random.Range(1, room.size.x);



        BoundsInt room1 = new(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
                              new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
        roomsList.Add(room1);
        roomsList.Add(room2);
        roomsList.Remove(room);
    }

    private static void SplitVertical(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var xSplit = Random.Range(1, room.size.x);
        BoundsInt room1 = new(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
                              new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    private static void SplitHorizontal(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var ySplit = Random.Range(1, room.size.y);
        BoundsInt room1 = new(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 = new(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
                              new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    public static List<Vector2Int> IncreaseCorridorSize(List<Vector2Int> corridor, int corridorWidth)
    {
        List<Vector2Int> originalRoad = corridor;
        HashSet<Vector2Int> expandedRoad = new(originalRoad);
        if (corridorWidth == 2)
        {
            foreach (Vector2Int point in originalRoad)
            {
                expandedRoad.Add(new Vector2Int(point.x, point.y + 1));
                expandedRoad.Add(new Vector2Int(point.x + 1, point.y));
            }
            expandedRoad.Add(new Vector2Int(corridor[0].x + 1, corridor[0].y + 1));
        }
        else
        {
            foreach (Vector2Int point in originalRoad)
            {
                expandedRoad.Add(new Vector2Int(point.x, point.y + 1));
                expandedRoad.Add(new Vector2Int(point.x, point.y - 1));
                expandedRoad.Add(new Vector2Int(point.x + 1, point.y));
                expandedRoad.Add(new Vector2Int(point.x - 1, point.y));
                expandedRoad.Add(new Vector2Int(point.x + 1, point.y + 1));
                expandedRoad.Add(new Vector2Int(point.x + 1, point.y - 1));
                expandedRoad.Add(new Vector2Int(point.x - 1, point.y + 1));
                expandedRoad.Add(new Vector2Int(point.x - 1, point.y - 1));
            }
        }
        return expandedRoad.ToList();
    }

    static Vector2Int GetRandomDirection()
    {
        List<Vector2Int> directions = Directions.Cardinal;
        return directions[Random.Range(0, directions.Count)];
    }
}
public static class Directions
{
    public static List<Vector2Int> Cardinal = new()
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };
    public static List<Vector2Int> Diagonal = new()
        {
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right + Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left + Vector2Int.up,
        };
    public static List<Vector2Int> All = new()
        {
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right,
            Vector2Int.right + Vector2Int.down,
            Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.left + Vector2Int.up,
        };
}