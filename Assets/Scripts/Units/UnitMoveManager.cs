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
    //funny enough, we don't need the hexaGrid for this class

    //these things are used in displaying tiles you can move to
    public GameObject movementShowTemp;
    private GameObject container;

    //used in dijkstra's
    static private HashSet<HexInfo> onceVisited;
    static private HashSet<HexInfo> lockedIn;
    static private Dictionary<HexInfo, (float dist, HexInfo tile)> dijkstraInfo;
    static private MinHeap<float, HexInfo> heap = new MinHeap<float, HexInfo>();

    // Start is called before the first frame update
    void Start()
    {
        //run BEFORE PopGen deletes TerrainGen!
        onceVisited = new HashSet<HexInfo>(); lockedIn = new HashSet<HexInfo>();
        dijkstraInfo = new Dictionary<HexInfo, (float dist, HexInfo tile)>();
        TileClicker.unitSelected += showMoveOpportunities;

        container = new GameObject();
    }

    //spawn the blue tiles that show where the current unit can move.
    void showMoveOpportunities(HexInfo start)
    {
        //delete everything when a new tile gets selected
        Destroy(container);
        container = new GameObject();
        container.name = "MoveOppCont";

        //if there's a unit here, run the stuff
        if (start.unit != null)
        {
            HashSet<HexInfo> moves = getMovementOpportunities(start);
            foreach(HexInfo movable in moves)
            {
                GameObject curr = Instantiate(movementShowTemp);
                curr.transform.position = movable.getOnTopOfLC();
                curr.transform.parent = container.transform;
            }
        }
    }
    /*When a tile gets clicked on, this code runs. if there's a unit on the tile, it finds all available tiles it can move to
     * A unit can move to all tiles where the following are true:
      - Cost to reach it is less than or equal to maxMovement
      - The elevation change between tiles is less than or equal to maxElevationScale
      - The target tile is empty*/
    static HashSet<HexInfo> getMovementOpportunities(HexInfo start)
    {
        //reset structures
        onceVisited.Clear(); lockedIn.Clear();
        dijkstraInfo.Clear();
        heap.Clear();

        //get critical values
        UnitPiece unitMoving = start.unit;
        float maxMovement = unitMoving.stats.maxMovement;

        //BEGIN
        firstVisit(0, start);
        /*The kinda sorta Dijkstra process
         1. Find the closest known tile, X, with the help of the minHeap
         2. lock X in. we now the confirmed shortest path to X.
         3. for each of X's neighbors, n:
            a. calculate a new "tempDist" from X to n, factoring in elev && terrain
            b. if less than currently known min distance, update it as so.
                also update the "previous tile".
                updates change both our own dictionary, and the minheap.*/
        while (heap.getSize() > 0)
        {
            //extract min
            MinHeapNode<float, HexInfo> nextUp = heap.removeMin();
            HexInfo currHex = nextUp.getData();
            float finalDist = nextUp.getValue();

            //we can stop when we find even the minimum possible tile is out of movement range.
            if (finalDist > maxMovement) break;

            //otherwise, lock this tile in, and do the neighbors thing
            lockedIn.Add(currHex);
            foreach (HexInfo n in currHex.GetNeighbors())
            {
                if (n != null)
                {
                    //if visited for the first time, add it to the minheap
                    if (!onceVisited.Contains(n))
                    {
                        firstVisit(1000, n); //1000 is essentially a substitute for infinity
                    }

                    //if we've seen the tile but not yet locked it in...can we find a shorter path?
                    if (!lockedIn.Contains(n))
                    {
                        float tempDist = finalDist + getTrueMoveCost(currHex, n); //use special move calculator
                        //if we can find a shorter path AND it's elevation the unit can actually scale, update it.
                        if (Mathf.Abs(getElevationChange(currHex, n)) <= unitMoving.stats.maxElevationScale
                            && tempDist < dijkstraInfo[n].dist)
                        {
                            updateVisit(n, tempDist, currHex);
                        }
                    }
                }
            }
        }

        return lockedIn;
    }

    //add to dictionary and min heap on first time discovering a tile
    private static void firstVisit(float theDist, HexInfo tile)
    {
        heap.insert(theDist, tile); //use 1000 instead of infinity, theyre practically the same
        onceVisited.Add(tile);
        dijkstraInfo.Add(tile, (theDist, null));
    }

    //update dictionary and min heap if shorter path is found
    private static void updateVisit(HexInfo NeighHex, float theDist, HexInfo currHex)
    {
        heap.update(NeighHex, theDist);
        dijkstraInfo[NeighHex] = (theDist, currHex);
    }

    //get the change in elevation between two tiles (assumed to be neighbors)
    private static float getElevationChange(HexInfo start, HexInfo finish)
    {
        return finish.elevation - start.elevation;
    }

    //movement between two tiles factors in elevation and terrain
    private static float getTrueMoveCost(HexInfo start, HexInfo finish)
    {
        return finish.terrain.getMoveCost() * Mathf.Max(1 + getElevationChange(start, finish), 0.75f);
    }
}
