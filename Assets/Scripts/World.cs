using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Tile
{
    public Vector2Int coord;
    public Building building;

    public bool IsOcupied() => building;
}

public class World : MonoBehaviour
{
    public static World instance;
    void Awake() { instance = this; }

    public Tile[] tiles;

    public const int size = 32;

    private void Start()
    {
        tiles = new Tile[size * size];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].coord.x = i % size;
            tiles[i].coord.y = i / size;
        }
    }

    public Tile GetTile(int x, int y)
    {
        x = Mathf.Clamp(x, 0, size - 1);
        y = Mathf.Clamp(y, 0, size - 1);

        return tiles[y * size + x];
    }

    int ToTile(Vector2Int coord)
    {
        return coord.y * size + coord.x;
    }

    public void PlaceBuilding(Building building, Vector2Int coord)
    {
        ref Tile tile = ref tiles[ToTile(coord)];
        tile.building = building;
    }
}
