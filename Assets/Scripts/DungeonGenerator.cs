using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public struct Coord
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
    public static Coord operator -(Coord a, Coord b) => new Coord(a.x - b.x, a.y - b.y);
    public static Coord operator -(Coord a) => new Coord(-a.x, -a.y);

    public int Distance(Coord other) => Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y);
    public bool Near(Coord other) => Distance(other) == 1;

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

public class Room
{
    public List<Coord> Tiles = new List<Coord>();
    public List<(Coord, Coord)> Entrances = new List<(Coord, Coord)>();
}

public class Path
{
    public List<Coord> Tiles = new List<Coord>();
}

public class Intersection
{
    // Add the coordinates of this intersection and the direction Coord object to obtain the path/intersection object in direction X
    public List<Coord> Connections = new List<Coord>();
}

class Dungeon
{
    public object[,] Tiles;

    List<Room> Rooms = new List<Room>();

    public Dungeon(Coord size)
    {
        Tiles = new object[size.x, size.y];
    }

    public bool Exists(Coord coord)
    {
        return coord.x >= 0 && coord.y >= 0 && coord.x < Tiles.GetLength(0) && coord.y < Tiles.GetLength(1);
    }

    public bool Occupied(Coord coord)
    {
        return Exists(coord) ? Tiles[coord.x, coord.y] != null : false;
    }

    public object GetObjectAt(Coord coord)
    {
        return Exists(coord) ? Tiles[coord.x, coord.y] : null;
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

    public void SetOccupied(Coord coord, object value)
    {
        Tiles[coord.x, coord.y] = value;
    }

    public void SetOccupied(List<Coord> coords, object value)
    {
        foreach (var coord in coords)
        {
            SetOccupied(coord, value);
        }
    }

    public Room GenerateRoomTiles(Coord position, int size)
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

        var room = new Room { Tiles = tiles };
        Rooms.Add(room);
        SetOccupied(tiles, room);
        return room;
    }

    public Path GeneratePath(Coord start, Coord direction)
    {
        while (Exists(start) && !(Occupied(start) && !Occupied(start + direction)))
        {
            start += direction;
        }

        if (!Exists(start)) return null; // ?

        MakeEffectOnDestination(start, direction);
        start += direction;

        List<Coord> tiles = new List<Coord>();
        while (Exists(start) && !(Occupied(start)))
        {
            tiles.Add(start);
            start += direction;

            var lot = Random.Range(0f, 1f);
            if (lot >= .93f)
            {
                if (lot > .96f) direction = direction.TurnLeft();
                else direction = direction.TurnRight();
            }
        }

        if (!Exists(start)) return null;

        MakeEffectOnDestination(start, -direction);

        var path = new Path { Tiles = tiles };
        SetOccupied(tiles, path);
        return path;
    }

    // Check for intersections

    private void MakeEffectOnDestination(Coord point, Coord direction)
    {
        var tile = GetObjectAt(point);

        if (tile is Path)
        {
            var intersectingPath = tile as Path;
            intersectingPath.Tiles.Remove(point);

            var intersection = new Intersection();
            SetOccupied(point, intersection);

            intersection.Connections.Add(direction);
            foreach (var otherPathTile in intersectingPath.Tiles.Where(x => x.Near(point)))
            {
                intersection.Connections.Add(otherPathTile - point);
            }
        }
        else if (tile is Intersection)
        {
            (tile as Intersection).Connections.Add(direction);
        }
        else if (tile is Room)
        {
            (tile as Room).Entrances.Add((point, direction));
        }
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
    Dungeon d = new Dungeon(new Coord(100, 100));

    void Start()
    {

        for (int i = 5; i < 100; i += 10)
        {
            for (int j = 5; j < 100; j += 10)
            {
                if (Random.Range(0f, 1f) >= .7f) continue;

                var pos = new Coord(i, j);
                var tiles = d.GenerateRoomTiles(pos + new Coord(Random.Range(-10, 10), Random.Range(-10, 10)), Random.Range(20, 50));
                if (tiles == null) continue;
                // InstantiateTiles(tiles.Tiles, new Color(.2f, .2f, Random.Range(.5f, 1f)));
            }
        }

        int wayCount = 0;
        for (int i = 0; i < 1500 && wayCount < 200; ++i)
        {
            var coord = new Coord(Random.Range(0, 100), Random.Range(0, 100));
            if (!d.IsFree(coord)) continue;

            var path = d.GeneratePath(coord, Coord.Directions[Random.Range(0, 4)]);
            if (path != null)
            {
                // InstantiateTiles(path.Tiles, new Color(Random.Range(.5f, 1f), .2f, .2f));
                wayCount++;
            }
        }

        for (int i = 0; i < 100; ++i)
        {
            for (int j = 0; j < 100; ++j)
            {
                InstantiateOn(new Coord(i, j));
            }
        }
    }

    [SerializeField] GameObject FloorRoom;
    [SerializeField] GameObject FloorPath;
    [SerializeField] GameObject FloorIntersection;

    void InstantiateOn(Coord c)
    {
        var obj = d.GetObjectAt(c);

        if (obj is Room)
        {
            Instantiate(FloorRoom, Map(c), Quaternion.identity);
        }
        else if (obj is Path)
        {
            Instantiate(FloorPath, Map(c), Quaternion.identity);
        }
        else if (obj is Intersection)
        {
            Instantiate(FloorIntersection, Map(c), Quaternion.identity);
        }
    }

    // private void InstantiateTiles(IEnumerable<Coord> Tiles, Color color)
    // {
    //     foreach (var tile in Tiles)
    //     {
    //         var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //         obj.transform.localScale = new Vector3(1, 1, 0.1f);
    //         obj.transform.position = new Vector3(tile.x, tile.y, 0);
    //         obj.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
    //     }
    // }

    private Vector3 Map(Coord coord)
    {
        return new Vector3(coord.x, 0, coord.y) * 8f;
    }
}
