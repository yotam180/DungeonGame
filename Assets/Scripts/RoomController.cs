using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public Dungeon Dungeon;
    public Coord MyCoord;

    // Start is called before the first frame update
    void Start()
    {
        // var room = Dungeon.GetObjectAt(MyCoord) as Room;
        // var (entrance, direction) = room.Entrances.Where((e) => e.Item1.Equals(MyCoord)).FirstOrDefault(null); // What happens if null?

        // foreach (var dir in Coord.Directions)
        // {
        //     if (dir.Equals(direction)) continue;

        //     // var wall = Instantiate()
        // }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
