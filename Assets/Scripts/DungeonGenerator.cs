﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

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
    public static Coord Cross(Coord a, Coord b)
    {
        return new Coord(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
    }

    public Coord TurnLeft()
    {
        return Cross(this, new Coord(0, 1));
    }

    public Coord TurnRight()
    {
        return Cross(this, new Coord(0, 1));
    }
}

class Room
{
    public List<Coord> Tiles = new List<Coord>();
    public List<Coord> Entrances = new List<Coord>();
}

class Dungeon
{
    public bool[,] Tiles;

    List<Room> Rooms = new List<Room>();

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
        Rooms.Add(new Room
        {
            Tiles = tiles
        });
        return tiles;
    }

    // public class PathGenerator
    // {
    //     public Coord Latest;

    //     public Coord Direction;

    //     public Coord Advance()
    //     {
    //         var lot = Random.Range(0f, 1f);
    //         if (lot < .85f) { }
    //         else if (lot < .93)
    //         {
    //             Direction = Direction.TurnLeft();
    //         }
    //         else
    //         {
    //             Direction = Direction.TurnRight();
    //         }

    //         Latest = Latest + Direction;
    //         return Latest;
    //     }
    // }

    // public void GeneratePath()
    // {
    //     var gen = new PathGenerator { Direction = new Coord(1, 0) };
    // }

    public List<Coord> GeneratePath(Coord start, Coord direction)
    {
        while (Exists(start) && !(Occupied(start) && !Occupied(start + direction)))
        {
            start += direction;
        }

        if (!Exists(start)) return null; // ?

        start += direction;
        List<Coord> tiles = new List<Coord>();
        while (Exists(start) && !(Occupied(start)))
        {
            tiles.Add(start);
            start += direction;

            var lot = Random.Range(0f, 1f);
            if (lot >= .93f)
            {
                if (lot > .96f)
                {
                    direction = direction.TurnLeft();
                }
                else
                {
                    direction = direction.TurnRight();
                }
            }
        }

        if (!Exists(start)) return null;

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
                if (Random.Range(0f, 1f) >= .7f) continue;

                var pos = new Coord(i, j);
                var tiles = d.GenerateRoomTiles(pos + new Coord(Random.Range(-10, 10), Random.Range(-10, 10)), Random.Range(20, 50));
                if (tiles == null) continue;
                InstantiateTiles(tiles, new Color(.2f, .2f, Random.Range(.5f, 1f)));
            }
        }

        int wayCount = 0;
        for (int i = 0; i < 1500 && wayCount < 200; ++i)
        {
            var coord = new Coord(Random.Range(0, 100), Random.Range(0, 100));
            if (!d.IsFree(coord)) continue;

            var tiles = d.GeneratePath(coord, Coord.Directions[Random.Range(0, 4)]);
            if (tiles != null)
            {
                d.SetOccupied(tiles);
                InstantiateTiles(tiles, new Color(Random.Range(.5f, 1f), .2f, .2f));
                wayCount++;
            }
        }
    }
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
