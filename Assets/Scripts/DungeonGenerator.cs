using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

// public class Room
// {
//     public Vector2 Position { get; set; }
//     public Vector2 Size { get; set; }
//     public Vector2 Doors { get; set; }
// }

// public class Dungeon
// {
//     public bool[,] Tiles { get; set; }

//     public Dungeon(int width, int height)
//     {
//         Tiles = new bool[width, height];
//     }

//     public void PlaceTile(int x, int y)
//     {
//         Tiles[x, y] = true;
//     }

//     public bool IsInside(Vector2 pos)
//     {
//         return pos.x >= 0 && pos.y >= 0 && pos.x < Tiles.GetLength(0) && pos.y < Tiles.GetLength(0);
//     }

//     public bool HasTile(Vector2 pos)
//     {
//         if (!IsInside(pos)) return false;
//         return Tiles[(int)pos.x, (int)pos.y];
//     }

//     public void SetTile(Vector2 pos)
//     {
//         if (!IsInside(pos)) return;
//         Tiles[(int)pos.x, (int)pos.y] = true;
//         Debug.Log("Placed a tile in " + pos);
//     }

//     public void SetTiles(IEnumerable<Vector2> positions)
//     {
//         foreach (var pos in positions)
//         {
//             SetTile(pos);
//         }
//     }

//     public bool IsFree(Vector2 pos)
//     {
//         Debug.Log("Checking if " + pos + " is free");
//         for (int i = -1; i <= 1; ++i)
//         {
//             for (int j = -1; j <= 1; ++j)
//             {
//                 if (HasTile(new Vector2(i, j) + pos)) return false;
//             }
//         }

//         Debug.Log("Returned true");
//         return true;
//     }
// }

struct Coord
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Coord[] Directions = {
        new Coord(0, 1),
        new Coord(1, 0),
        new Coord(0, -1),
        new Coord(-1, 0),
    };

    public static Coord[] Surrounding = {
        new Coord(0, 1),
        new Coord(1, 0),
        new Coord(0, -1),
        new Coord(-1, 0),
        new Coord(-1, 1),
        new Coord(1, -1),
        new Coord(-1, -1),
        new Coord(1, 1),
    };

    public static Coord operator +(Coord a, Coord b) => new Coord(a.x + b.x, a.y + b.y);
}

class Dungeon
{
    public bool[,] Tiles;

    public Dungeon(Coord size)
    {
        Tiles = new bool[size.x, size.y];
    }

    public bool Exists(Coord coord)
    {
        return coord.x >= 0 && coord.y >= 0 && coord.x < Tiles.GetLength(0) && coord.y < Tiles.GetLength(1);
    }

    public bool Occupied(Coord coord)
    {
        return Exists(coord) ? Tiles[coord.x, coord.y] : false;
    }

    public bool IsFree(Coord coord)
    {
        if (coord.x == 5 && coord.y == 8)
        {
            Debug.Log("Checking critical tile");
            Debug.Log(Tiles[5, 8]);
        }

        if (Occupied(coord) || !Exists(coord)) return false;

        foreach (var dir in Coord.Surrounding)
        {
            if (Occupied(coord + dir)) return false;
        }

        return true;
    }

    public void SetOccupied(Coord coord)
    {
        Tiles[coord.x, coord.y] = true;
    }

    public void SetOccupied(List<Coord> coords)
    {
        foreach (var coord in coords)
        {
            SetOccupied(coord);
        }
    }

    public List<Coord> GenerateRoomTiles(Coord position, int size)
    {
        if (!IsFree(position))
        {
            return null;
        }

        Queue<Coord> upcoming = new Queue<Coord>();
        List<Coord> tiles = new List<Coord>();
        upcoming.Enqueue(position);

        for (int i = 0; i < size; ++i)
        {
            var nextTile = GetNextValidTile(upcoming, tiles);
            if (nextTile == null) break;

            var adjacent = GetNumberOfAdjacentBlocks(nextTile.Value, tiles);
            Debug.Log(adjacent);
            if (Random.Range(0f, 1f) <= (adjacent / 3f) || tiles.Count < 2)
            {
                tiles.Add(nextTile.Value);
                foreach (var dir in Coord.Surrounding)
                {
                    upcoming.Enqueue(dir + nextTile.Value);
                }

            }
        }

        SetOccupied(tiles);
        return tiles;
    }

    private Coord? GetNextValidTile(Queue<Coord> queue, List<Coord> occupied)
    {
        if (queue.Count == 0) return null;
        var next = queue.Dequeue();

        while (!IsFree(next) || occupied.Contains(next))
        {
            if (queue.Count == 0) return null;
            next = queue.Dequeue();
        }

        return next;
    }

    private int GetNumberOfAdjacentBlocks(Coord coord, List<Coord> tiles)
    {
        return Coord.Surrounding.Select(dir => tiles.Contains(dir + coord) ? 1 : 0).Sum();
    }
}

public class DungeonGenerator : MonoBehaviour
{
    void Start()
    {
        Dungeon d = new Dungeon(new Coord(100, 100));

        for (int i = 5; i < 100; i += 10)
        {
            for (int j = 5; j < 100; j += 10)
            {
                var pos = new Coord(i, j);
                var tiles = d.GenerateRoomTiles(pos + new Coord(Random.Range(-10, 10), Random.Range(-10, 10)), Random.Range(20, 50));
                if (tiles == null) continue;
                InstantiateTiles(tiles, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));
            }
        }

        // var tiles =
        // InstantiateTiles(tiles, Color.blue);

        // tiles = d.GenerateRoomTiles(new Coord(5, 10), 20);
        // InstantiateTiles(tiles, Color.green);
    }

    // public List<Room> Rooms;

    // private Dungeon dungeon;

    // Start is called before the first frame update
    // void Start()
    // {
    //     dungeon = new Dungeon(100, 100);
    // var tiles = CreateRoomFromPoint(new Vector2(0, 0), 1);
    // Debug.Log("Setting tiles");
    // if (tiles != null)
    // {
    //     dungeon.SetTiles(tiles);
    //     InstantiateTiles(tiles, Color.red);
    // }

    // tiles = CreateRoomFromPoint(new Vector2(1, 0), 1);
    // if (tiles != null)
    // {
    //     dungeon.SetTiles(tiles);
    //     InstantiateTiles(tiles, Color.red);
    // }

    // tiles = CreateRoomFromPoint(new Vector2(1, 1), 1);
    // if (tiles != null)
    // {
    //     dungeon.SetTiles(tiles);
    //     InstantiateTiles(tiles, Color.red);
    // }

    // tiles = CreateRoomFromPoint(new Vector2(1, 2), 1);
    // if (tiles != null)
    // {
    //     dungeon.SetTiles(tiles);
    //     InstantiateTiles(tiles, Color.red);
    // }

    //     GenerateRooms();
    // }

    private void InstantiateTiles(IEnumerable<Coord> Tiles, Color color)
    {
        foreach (var tile in Tiles)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.localScale = new Vector3(1, 1, 0.1f);
            obj.transform.position = new Vector3(tile.x, tile.y, 0);
            obj.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
        }
    }

    // // Update is called once per frame
    // void Update()
    // {

    // }

    // private void GenerateRooms()
    // {
    //     Queue<Vector2> rooms = new Queue<Vector2>();
    //     rooms.Enqueue(new Vector2(0, 0));

    //     for (int i = 0; i < 10; ++i)
    //     {
    //         if (rooms.Count == 0) break;

    //         var next = rooms.Dequeue();
    //         if (!dungeon.IsFree(next)) continue; // ?

    //         var tiles = CreateRoomFromPoint(next, Random.Range(15, 20));
    //         if (tiles == null) continue; // Should we really?
    //         dungeon.SetTiles(tiles);
    //         InstantiateTiles(tiles, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));

    //         for (int j = 0; j < 10; ++j)
    //         {
    //             var pNext = next + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
    //             if (dungeon.IsFree(pNext)) rooms.Enqueue(pNext);
    //         }
    //     }
    // }

    // private List<Vector2> CreateRoomFromPoint(Vector2 StartPosition, int RoomSize)
    // {
    //     if (!dungeon.IsFree(StartPosition))
    //     {
    //         print("Can't create room on " + StartPosition);
    //         return null;
    //     }

    //     Queue<Vector2> Positions = new Queue<Vector2>();
    //     List<Vector2> Tiles = new List<Vector2>();
    //     Positions.Enqueue(StartPosition);

    //     for (int i = 0; i < RoomSize; ++i)
    //     {
    //         if (!GenerateAnotherRoomTile(ref Positions, ref Tiles)) break;
    //     }

    //     print("Overall got " + Tiles.Count + " tiles");
    //     return Tiles; // Tiles.Count > 10 ? Tiles : null;
    // }

    // // TODO: This is a nasty algorithm, maybe we should come up with something smarter
    // private bool GenerateAnotherRoomTile(ref Queue<Vector2> NextTiles, ref List<Vector2> Tiles)
    // {
    //     if (NextTiles.Count == 0) return false;

    //     var block = NextTiles.Dequeue();
    //     while (!dungeon.IsFree(block) || Tiles.Contains(block))
    //     {
    //         if (NextTiles.Count == 0) return false;
    //         block = NextTiles.Dequeue();
    //     }

    //     Tiles.Add(block);

    //     for (int i = -1; i <= 1; ++i)
    //     {
    //         for (int j = -1; j <= 1; ++j)
    //         {
    //             if (i == 0 && j == 0) continue;

    //             var nextBlock = new Vector2(i, j) + block;

    //             // TODO: Check that rooms have at least 1 free space tile between them
    //             if (Tiles.Contains(nextBlock)) continue;

    //             // Chances of enqueueing depend on the number of already existing adjacent blocks
    //             var adjacent = GetNumberOfAdjacentBlocks(nextBlock, Tiles);
    //             if (Random.Range(0f, 1f) <= (adjacent / 3f) || (adjacent != 0 && Tiles.Count < 2))
    //             {
    //                 NextTiles.Enqueue(nextBlock);
    //             }
    //         }
    //     }

    //     return true;
    // }

    // private int GetNumberOfAdjacentBlocks(Vector2 tile, List<Vector2> tiles)
    // {
    //     // int count = 0;
    //     // for (int i = -1; i <= 1; ++i)
    //     // {
    //     //     for (int j = -1; j <= 1; ++j)
    //     //     {
    //     //         if (i == 0 && j == 0) continue;
    //     //         if (tiles.Contains(new Vector2(i, j) + tile)) count++;
    //     //     }
    //     // }
    //     // return count;
    //     return tiles.Count(x => Vector2.Distance(x, tile) == 1);
    // }
}
