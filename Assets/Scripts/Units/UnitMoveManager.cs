using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*handles movement of all units in the game. sounds stupid, but it works because:
 - only one unit can move at a time anyway;
 - it's probably inefficient to keep getting the map from UnitStats/UnitPiece over and over again
        (note that neither class could accomodate storing the whole map on its own)
 - global movement factors can be implemented this way, such as rain making everything muddy*/
public class UnitMoveManager : MonoBehaviour
{
    private HexInfo[,] hexaGrid;
    private bool[,] marked;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /*A unit can move to all tiles where the following are true:
      - Cost to reach it is less than or equal to maxMovement
      - The elevation change between tiles is less than or equal to maxElevationScale
      - The target tile is empty*/
    List<HexInfo> getMovementOpportunities(UnitPiece unitMoving)
    {
        HexInfo startTile = unitMoving.stats.getCurrTile();

        Queue<HexInfo> queue = new Queue<HexInfo>();
        queue.Enqueue(startTile);
        System.Array.Clear(marked, 0, marked.Length);
        marked[startTile.GetX(), startTile.GetY()] = true;

        while (queue.Count > 0)
        {
            HexInfo currHex = queue.Dequeue();
            
            foreach(HexInfo n in currHex.GetNeighbors())
            {
                if(n != null && !marked[n.GetX(), n.GetY()]) {
                    marked[currHex.GetX(), currHex.GetY()] = true;

                    //TODO: dijkstra's algorithm for finding shortest path to a tile.
                }
            }
        }

        return null;
    }
}
