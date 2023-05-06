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
    //the currently selected unit (by the player).
    public static UnitPiece selectedUnit;

    //funny enough, we don't need the hexaGrid for this class

    //these things are used in displaying tiles you can move to
    public GameObject movementShowTemp;
    private GameObject container;

    //used in dijkstra's
    static private HashSet<HexInfo> onceVisited;
    static private HashSet<HexInfo> lockedIn;
    //locked in codes: 0 = empty, 1 = friendly unit (can swap), 2 = enemy unit (can attack)
    static private Dictionary<HexInfo, int> lockedInCodes;
    static private Dictionary<HexInfo, (float dist, HexInfo prev)> dijkstraInfo;
    static private MinHeap<float, HexInfo> heap = new MinHeap<float, HexInfo>();

    // Start is called before the first frame update
    void Start()
    {
        //run BEFORE PopGen deletes TerrainGen!
        onceVisited = new HashSet<HexInfo>(); lockedIn = new HashSet<HexInfo>();
        dijkstraInfo = new Dictionary<HexInfo, (float dist, HexInfo prev)>();
        lockedInCodes = new Dictionary<HexInfo, int>();

        TileClicker.unitSelected += showMoveOpportunities;
        TileClicker.unitMoved += moveSelectedUnit;
        OceanHexFinder.oceanUnitSelected += showMoveOpportunities;
        OceanHexFinder.oceanUnitMoved += moveSelectedUnit;

        container = new GameObject();
    }

    //moves a unit from one location to another.
    void moveSelectedUnit(HexInfo dest)
    {
        //Debug.Log(dijkstraInfo[dest].prev);
        selectedUnit.setCurrTile(dest);
        resetMoveOpportunities();
    }

    void genMoveArrows(HexInfo dest)
    {
        while(dest != null)
        {
            dest = dijkstraInfo[dest].prev;

        }
    }

    //spawn the blue tiles that show where the current unit can move.
    void showMoveOpportunities(HexInfo start)
    {
        //delete everything when a new tile gets selected
        resetMoveOpportunities();

        //if there's a unit here, run the stuff
        if (start.unit != null)
        {
            HashSet<HexInfo> moves = getMovementOpportunities(start);
            foreach(HexInfo movable in moves)
            {
                GameObject curr = Instantiate(movementShowTemp);
                curr.transform.position = movable.getOnTopOfLC();
                curr.transform.parent = container.transform;

                int code = lockedInCodes[movable];
                if (code > 0)
                {
                    //TODO: SWAP AND ATTACK MECHANICS!!!
                    if(code == 1)
                    {
                        curr.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.67f, 0.15f, 0.5f);
                        Debug.Log("Friendly unit on " + movable);
                    } else
                    {
                        curr.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.1f, 0.15f, 0.5f);
                        Debug.Log("Enemy unit on " + movable);
                    }
                }
                else
                {

                    movable.withinRangeOfSelected = true; //tell the tile that it can now be moved to...
                }
                curr.SetActive(true);
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
        lockedInCodes.Clear();
        dijkstraInfo.Clear();
        heap.Clear();

        //get critical values
        UnitPiece unitMoving = start.unit;
        Faction unitMovingFaction = start.unit.stats.faction;
        selectedUnit = unitMoving;
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
            assignLockedInCode(currHex, unitMovingFaction);
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

    private static void assignLockedInCode(HexInfo currHex, Faction f)
    {
        int code = 0;
        if (currHex.unit != null)
        {
            if(currHex.unit.stats.faction == f) { code = 1; }
            else { code = 2; }
        }
        lockedInCodes.Add(currHex, code);
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

    //get rid of all the blue hexes showing where you can move
    private void resetMoveOpportunities()
    {
        foreach(HexInfo movable in lockedIn) //reset all the hexInfo stuff
        {
            movable.withinRangeOfSelected = false;
        }

        Destroy(container);
        container = new GameObject();
        container.name = "MoveOppCont";
    }
}
