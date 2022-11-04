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

    public ref Tile GetTile(int x, int y)
    {
        x = Mathf.Clamp(x, 0, size - 1);
        y = Mathf.Clamp(y, 0, size - 1);

        return ref tiles[y * size + x];
    }

    int ToTile(Vector2Int coord)
    {
        return coord.y * size + coord.x;
    }

    Vector3 ToWorld(Vector2Int coord)
    {
        return new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
    }

    public bool CanPlace(Vector2Int from, Vector2Int size)
    {

        for (int y = from.y; y < from.y + size.y; y++)
        {
            for (int x = from.x; x < from.x + size.x; x++)
            {
                ref var tile = ref GetTile(x, y);

                if (tile.IsOcupied())
                {
                    Debug.Log(from + " " + tile.coord);
                    Debug.DrawRay(ToWorld(tile.coord), Vector3.up * 10, Color.blue);
                    return false;
                }
            }
        }

        return true;
    }

    public void PlaceBuilding(Building building, Vector2Int from)
    {
        Vector2Int size = building.size;

        for (int y = from.y; y < from.y + size.y; y++)
        {
            for (int x = from.x; x < from.x + size.x; x++)
            {
                ref Tile tile = ref GetTile(x, y);
                tile.building = building;
            }
        }
    }
}
