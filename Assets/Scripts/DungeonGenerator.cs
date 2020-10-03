using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class Room
{
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Doors { get; set; }
}

public class DungeonGenerator : MonoBehaviour
{
    public List<Room> Rooms;

    // Start is called before the first frame update
    void Start()
    {
        CreateRoomFromPoint(new Vector2(0, 0), 20);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateRoomFromPoint(Vector2 StartPosition, int RoomSize)
    {
        Queue<Vector2> Positions = new Queue<Vector2>();
        List<Vector2> Tiles = new List<Vector2>();
        Positions.Enqueue(StartPosition);

        for (int i = 0; i < RoomSize; ++i)
        {
            GenerateAnotherRoomTile(ref Positions, ref Tiles);
        }

        foreach (var tile in Tiles)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.localScale = new Vector3(1, 1, 0.1f);
            Instantiate(obj, new Vector3(tile.x, tile.y, 0), Quaternion.identity);
        }
    }

    // TODO: This is a nasty algorithm, maybe we should come up with something smarter
    private void GenerateAnotherRoomTile(ref Queue<Vector2> NextTiles, ref List<Vector2> Tiles)
    {
        var block = NextTiles.Dequeue();
        
        Tiles.Add(block);
        Debug.Log(block);
        
        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (i == 0 && j == 0) continue;

                var nextBlock = new Vector2(i, j) + block;
                if (!Tiles.Contains(nextBlock)) // !positions.Contains(nextBlock)
                {
                    // Chances of enqueueing 
                    if (Random.Range(0f, 1f) <= GetNumberOfSurroundingBlocks(nextBlock, Tiles) / 3f)
                    {
                        NextTiles.Enqueue(nextBlock);

                    }
                }
            }
        }
    }

    private int GetNumberOfSurroundingBlocks(Vector2 tile, List<Vector2> tiles)
    {
        // int count = 0;
        // for (int i = -1; i <= 1; ++i)
        // {
        //     for (int j = -1; j <= 1; ++j)
        //     {
        //         if (i == 0 && j == 0) continue;
        //         if (tiles.Contains(new Vector2(i, j) + tile)) count++;
        //     }
        // }
        // return count;
        return tiles.Count(x => Vector2.Distance(x, tile) == 1);
    }
}
