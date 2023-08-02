using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap, wallTilemap;
    [SerializeField]
    private TileBase tile, wallTop, wallBottm, wallSideLeft, wallSideRight, wallFull,
        wallInnerCornerDownLeft, wallInnerCornerDownRight,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;
    public void VisualizeTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, tilemap, tile);
    }

    internal void PaintSingleBasicWall(Vector2Int pos, int binaryType)
    {
        TileBase paintTile = null;
        if (WallByteTypes.wallTop.Contains(binaryType))
            paintTile = wallTop;
        else if (WallByteTypes.wallBottm.Contains(binaryType))
            paintTile = wallBottm;
        else if (WallByteTypes.wallSideLeft.Contains(binaryType))
            paintTile = wallSideLeft;
        else if (WallByteTypes.wallSideRight.Contains(binaryType))
            paintTile = wallSideRight;
        else if (WallByteTypes.wallFull.Contains(binaryType))
            paintTile = wallFull;


        if (paintTile != null)
            PaintTile(wallTilemap, paintTile, pos);
    }
    internal void PaintSingleCornerWall(Vector2Int pos, int binaryType)
    {
        TileBase paintTile = null;
        if (WallByteTypes.wallInnerCornerDownLeft.Contains(binaryType))
            paintTile = wallInnerCornerDownLeft;
        else if (WallByteTypes.wallInnerCornerDownRight.Contains(binaryType))
            paintTile = wallInnerCornerDownRight;
        else if (WallByteTypes.wallDiagonalCornerDownLeft.Contains(binaryType))
            paintTile = wallDiagonalCornerDownLeft;
        else if (WallByteTypes.wallDiagonalCornerDownRight.Contains(binaryType))
            paintTile = wallDiagonalCornerDownRight;
        else if (WallByteTypes.wallDiagonalCornerUpLeft.Contains(binaryType))
            paintTile = wallDiagonalCornerUpLeft;
        else if (WallByteTypes.wallDiagonalCornerUpRight.Contains(binaryType))
            paintTile = wallDiagonalCornerUpRight;
        else if (WallByteTypes.wallFullEightDirections.Contains(binaryType))
            paintTile = wallFull;
        else if (WallByteTypes.wallBottmEightDirections.Contains(binaryType))
            paintTile = wallBottm;


        if (paintTile != null)
            PaintTile(wallTilemap, paintTile, pos);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var pos in positions)
        {
            PaintTile(tilemap, tile, pos);
        }
    }
    private void PaintTile(Tilemap tilemap, TileBase tile, Vector2Int pos)
    {
        var tilePos = tilemap.WorldToCell(new Vector3Int(pos.x, pos.y, 0));
        tilemap.SetTile(tilePos, tile);
    }
    public void Clear()
    {
        tilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }
}
