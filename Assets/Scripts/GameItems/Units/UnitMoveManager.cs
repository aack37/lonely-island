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
    private GameObject container1;
    public GameObject pathTracer;
    private GameObject container2;

    //used in dijkstra's
    static private HashSet<HexInfo> onceVisited;
    static private HashSet<HexInfo> lockedIn;
    static private HashSet<HexInfo> borderTiles; //border tiles that are locked in (important optimization for pathfinding)
    //locked in codes: 0 = empty, 1 = friendly unit (can swap), 2 = enemy unit (can attack)
    static private Dictionary<HexInfo, int> lockedInCodes;
    static private Dictionary<HexInfo, (AStarNode anode, HexInfo prev)> dijkstraInfo;
    static private MinHeapAStar<HexInfo> heap = new MinHeapAStar<HexInfo>(); //use a special minHeap for the job
    static private int nodeCounter = 0; //todo: reset on new selection

    // Start is called before the first frame update
    void Start()
    {
        //run BEFORE PopGen deletes TerrainGen!
        onceVisited = new HashSet<HexInfo>(); lockedIn = new HashSet<HexInfo>();
        borderTiles = new HashSet<HexInfo>();
        dijkstraInfo = new Dictionary<HexInfo, (AStarNode anode, HexInfo prev)>();
        lockedInCodes = new Dictionary<HexInfo, int>();

        //unit movement
        TileClicker.unitSelected += onUnitSelected;
        TileClicker.unitMoved += moveSelectedUnit;
        OceanHexFinder.oceanUnitSelected += onUnitSelected;
        OceanHexFinder.oceanUnitMoved += moveSelectedUnit;

        //pathfinding
        TileClicker.tileHover += tracePath;

        container1 = new GameObject();
        container2 = new GameObject();
    }

    //this actually just happens whenever you click on a tile. so why not just use the tileSelected event?
    //to make things easier with hexInfo for ocean tiles. that's literally it.
    private void onUnitSelected(HexInfo hi)
    {
        selectedUnit = hi.unit;
        if(selectedUnit != null)
        {
            showMoveOpportunities(hi);
        } else
        {
            
        }
        
    }

    //moves a unit from one location to another.
    void moveSelectedUnit(HexInfo dest)
    {
        //Debug.Log(dijkstraInfo[dest].prev);
        selectedUnit.setCurrTile(dest);
        resetMoveOpportunities();
        selectedUnit = null;
    }

    // ---------------------- //
    //  SHORTEST PATHFINDING  //
    // ---------------------- //

    /*get the shortest path from start to any other tile on the map. the path itself is stored in dijkstraInfo.
     return true if the path could be found, false if not.*/
    static bool getShortestPath(HexInfo dest)
    {
        //if we already found the path, just return that
        if (lockedIn.Contains(dest))
        {
            //Debug.Log("found shortcut path successfully, of length: " + dijkstraInfo[dest].anode.getGCost());
            return true;
        }

        //otherwise we gotta find it the hard way

        //reset structures
        onceVisited.UnionWith(lockedIn);
        //do NOT clear lockedInCodes - getShortestPath always runs after getMovementOpportunities
        heap.Clear();

        //get critical values
        Faction unitMovingFaction = selectedUnit.stats.faction;
        HexInfo start = selectedUnit.stats.currTile;
        float maxMovement = selectedUnit.stats.maxMovement;

        //BEGIN
        foreach (HexInfo tile in borderTiles)
        {
            (AStarNode a, HexInfo h) tileData = dijkstraInfo[tile];
            firstVisit(tileData.a.getGCost(), getDistanceApprox(start, tile), tile);
        }
        borderTiles.Clear();
        /*The kinda sorta Dijkstra process
         1. Find the tile with smallest F-cost, X, with the help of the minHeap
         2. if we DON'T already know the shortest path from start -> X,
         2. lock X in. we now the confirmed shortest path to X.
         3. for each of X's neighbors, n:
            a. calculate a new "tempDist" from X to n, factoring in elev && terrain
            b. if less than currently known min distance, update it as so.
                also update the "previous tile".
                updates change both our own dictionary, and the minheap.*/
        bool stillSearching = true;
        while (heap.getSize() > 0)
        {
            //extract min
            MinHeapNode<AStarNode, HexInfo> nextUp = heap.removeMin();
            HexInfo currHex = nextUp.getData();
            float finalDist = nextUp.getValue().getGCost();

            
            
            if (stillSearching)
            {
                Debug.Log("Locking in tile: " + currHex + ", with distance " + finalDist);
                lockedIn.Add(currHex);
                assignLockedInCode(currHex, unitMovingFaction);
            }

            //it so happens target tiles get locked in twice with the same distance each time- don't worry about it
            if (currHex == dest) stillSearching = false;

            foreach (HexInfo n in currHex.GetNeighbors())
            {
                if (n != null)
                {
                    //normal A-star search, if you are still searching for the shortest path.
                    if (stillSearching)
                    {
                        //if visited for the first time, add it to the minheap
                        if (!onceVisited.Contains(n))
                        {
                            firstVisit(1000, getDistanceApprox(start, n), n); //1000 is essentially a substitute for infinity
                        }

                        //if we've seen the tile but not yet locked it in...can we find a shorter path?
                        if (!lockedIn.Contains(n))
                        {
                            float tempDist = finalDist + getTrueMoveCost(currHex, n); //use special move calculator
                                                                                      //if we can find a shorter path AND it's elevation the unit can actually scale, update it.
                            if (Mathf.Abs(getElevationChange(currHex, n)) <= selectedUnit.stats.maxElevationScale
                                && tempDist < dijkstraInfo[n].anode.getGCost())
                            {
                                updateVisit(n, tempDist, currHex);
                            }
                        }
                    }

                    //else, wrap things up quickly but add all the border tiles.
                    else
                    {
                        borderTiles.Add(currHex);
                    }

                }
            }

            
        }

        if (!stillSearching)
        {
            //Debug.Log("found path successfully, of distance: " + dijkstraInfo[dest].anode.getGCost());
            return true;
        }
        else
        {
            Debug.Log("failed to find path");
            return false;
        }
    }

    //trace shortest path from the unit to a specified tile (whatever tile you're hovering over)
    void tracePath(HexTile hovering)
    {
        if (selectedUnit != null)
        {
            resetPathTracer();

            HexInfo dest = hovering.hexInfo;
            if (getShortestPath(dest))
            {
                HexInfo next = dest;
                do
                {
                    GameObject thing = Instantiate(pathTracer);
                    thing.transform.position = next.getOnTopOfCenter();
                    thing.transform.parent = container2.transform;

                    next = dijkstraInfo[next].prev;
                } while (next != null);
            } else
            {
                Debug.Log("ERR: Couldn't find a path to the tile");
            }
            
        } else
        {
            resetPathTracer();
        }
    }


    // ---------------------- //
    // MOVEMENT OPPORTUNITIES //
    // ---------------------- //
    //spawn the blue tiles that show where the current unit can move.
    void showMoveOpportunities(HexInfo start)
    {
        //delete everything when a new tile gets selected
        resetMoveOpportunities();

        //if there's a unit here, run the stuff
        if (start.unit != null)
        {
            HashSet<HexInfo> moves = getMovementOpportunities(start);
            foreach (HexInfo movable in moves)
            {
                GameObject curr = Instantiate(movementShowTemp);
                curr.transform.position = movable.getOnTopOfLC();
                curr.transform.parent = container1.transform;

                int code = lockedInCodes[movable];
                if (code > 0)
                {
                    //TODO: SWAP AND ATTACK MECHANICS!!!
                    if (code == 1)
                    {
                        curr.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.67f, 0.15f, 0.5f);
                        //Debug.Log("Friendly unit on " + movable);
                    }
                    else
                    {
                        curr.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.1f, 0.15f, 0.5f);
                        //Debug.Log("Enemy unit on " + movable);
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
        borderTiles.Clear();
        lockedInCodes.Clear();
        dijkstraInfo.Clear();
        heap.Clear();
        nodeCounter = 0;

        //get critical values
        //UnitPiece unitMoving = start.unit;
        //Faction unitMovingFaction = start.unit.stats.faction;
        //float maxMovement = unitMoving.stats.maxMovement;
        Faction unitMovingFaction = selectedUnit.stats.faction;
        float maxMovement = selectedUnit.stats.maxMovement;

        //BEGIN
        firstVisit(0, 0, start);
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
            MinHeapNode<AStarNode, HexInfo> nextUp = heap.removeMin();
            HexInfo currHex = nextUp.getData();
            float finalDist = nextUp.getValue().getGCost();

            //used to stop here, but now we build border tiles list over time
            if (finalDist > maxMovement)
            {
                borderTiles.Add(currHex); //we'll be needing this for pathfinding...
            } else
            {
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
                            firstVisit(1000, getDistanceApprox(start, n), n); //1000 is essentially a substitute for infinity
                        }

                        //if we've seen the tile but not yet locked it in...can we find a shorter path?
                        if (!lockedIn.Contains(n))
                        {
                            float tempDist = finalDist + getTrueMoveCost(currHex, n); //use special move calculator
                                                                                      //if we can find a shorter path AND it's elevation the unit can actually scale, update it.
                            if (Mathf.Abs(getElevationChange(currHex, n)) <= selectedUnit.stats.maxElevationScale
                                && tempDist < dijkstraInfo[n].anode.getGCost())
                            {
                                updateVisit(n, tempDist, currHex);
                            }
                        }
                    }
                }
            }
            
        }
        
        return lockedIn;
    }

    //return approximate distance between two tiles (used as the H-cost)
    private static int getDistanceApprox(HexInfo h1, HexInfo h2)
    {
        return Mathf.FloorToInt(Mathf.Sqrt(Mathf.Pow(h1.GetX() - h2.GetX(), 2) + Mathf.Pow(h1.GetY() - h2.GetY(), 2)));
    }

    private static void assignLockedInCode(HexInfo currHex, Faction f)
    {
        int code = 0;
        if (currHex.unit != null)
        {
            if (currHex.unit.stats.faction == f) { code = 1; }
            else { code = 2; }
        }
        if (!lockedInCodes.ContainsKey(currHex)) lockedInCodes.Add(currHex, code);
    }

    //add to dictionary and min heap on first time discovering a tile
    private static void firstVisit(float actualDist, float toGoApprox, HexInfo tile)
    {
        AStarNode anode = new AStarNode(actualDist, toGoApprox, nodeCounter++);
        heap.insert(anode, tile); //use 1000 instead of infinity, theyre practically the same
        onceVisited.Add(tile);
        if (!dijkstraInfo.ContainsKey(tile)) dijkstraInfo.Add(tile, (anode, null));

    }

    //update dictionary and min heap if shorter path is found
    private static void updateVisit(HexInfo NeighHex, float actualDist, HexInfo currHex)
    {
        heap.update(NeighHex, actualDist);
        dijkstraInfo[NeighHex].anode.setGCost(actualDist);
        dijkstraInfo[NeighHex] = (dijkstraInfo[NeighHex].anode, currHex);
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
        foreach (HexInfo movable in lockedIn) //reset all the hexInfo stuff
        {
            movable.withinRangeOfSelected = false;
        }

        Destroy(container1);
        container1 = new GameObject();
        container1.name = "MoveOppCont";
    }

    private void resetPathTracer()
    {
        Destroy(container2);
        container2 = new GameObject();
        container2.name = "PathTracerCont";
    }
}
