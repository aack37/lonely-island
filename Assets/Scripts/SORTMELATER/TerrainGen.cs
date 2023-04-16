using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    private static (int gridW, int gridH, int islandSize) mapSizeStats = TGenSettings.getMapSizeValues();
    public static int gridWidth = mapSizeStats.gridW;
    public static int gridHeight = mapSizeStats.gridH;

    public static float elevationStep = TGenSettings.elevationStep;

    public static event System.Action<int> regenerateWorld;

    //used in regenerating maps for debugging...
    bool DEBUG_GENERATING = false;
    private GameObject container;

    //used in generating initial grid...
    public GameObject hexTile;
    public HexInfo[,] hexGrid = new HexInfo[gridHeight, gridHeight];
    private bool[,] marked = new bool[gridWidth, gridHeight];

    private int worldSeed;

    public static TerrainTypesSingletons singles;

    //used in generating landforms, picking town locations...
    List<HexInfo>[] terraStorage;

    //used for storing natural features...
    List<HashSet<HexInfo>> naturalFeatureRegions; //stores the regions themselves as hashsets of tiles.
    Dictionary<int, TerrainType> naturalFeatureTypes; //maps (the index of each feature in above) to (the type of feature it is)
    public TextMeshProUGUI featureNametag;
    private List<GameObject> gennedNametags;


    void resetBigLists()
    {
        //terraStorage, for storing which terrain tiles are where
        //System.Array.Clear(terraStorage, 0, terraStorage.Length);

        for (int i = 0; i < singles.numTerrainTypes; i++)
        {
            terraStorage[i].Clear();
        }

        naturalFeatureRegions.Clear(); //natural feature stuff
        naturalFeatureTypes.Clear();

        int nametagNum = gennedNametags.Count;
        for (int i = 0; i < nametagNum; i++)
        {
            Destroy(gennedNametags[i]);
        }
        gennedNametags.Clear();
    }

    //give the tiles and terrain type of a region to add it as a natural feature on the map. returns index of the feature.
    int addNewNaturalFeature(HashSet<HexInfo> tiles, TerrainType terraType)
    {
        naturalFeatureRegions.Add(tiles);
        int index = naturalFeatureRegions.Count - 1;
        naturalFeatureTypes.Add(index, terraType);
        return index;
        //TODO: give natural features a name as well
    }

    void Start()
    {
        singles = FindObjectOfType<TerrainTypesSingletons>();
        container = new GameObject();

        System.Array.Clear(marked, 0, marked.Length); //use this line to reset the marked grid at any time.

        naturalFeatureRegions = new List<HashSet<HexInfo>>();
        naturalFeatureTypes = new Dictionary<int, TerrainType>();
        gennedNametags = new List<GameObject>();

        terraStorage = new List<HexInfo>[singles.numTerrainTypes];
        for (int i = 0; i < singles.numTerrainTypes; i++)
        {
            terraStorage[i] = new List<HexInfo>();
            //maybe do hash set instead of list?
        }

        //let's roll
        InitialGridSetup();
    }

    public int[] gridDimensions() { return new int[2] { gridWidth, gridHeight }; }
    public Canvas UI_system; //enable it when you're done generating everything.

    public int getSeed() { return worldSeed; }

    // ---------------------------
    //PHASE 1 - INITIAL SETUP
    // ---------------------------

    //prepare terrain generation by building a CONCEPTUAL grid.
    int zStarter = 0;
    async void InitialGridSetup()
    {
        //initialize each spot. (not random, always the same underlying grid)
        var tasks = new Task[gridWidth * gridHeight]; //build the tiles asyncally

        for (int xx = 0; xx < gridWidth; xx++)
        {
            for (int yy = 0; yy < gridHeight; yy++)
            {
                tasks[xx * gridHeight + yy] = createHexInfoGrid(xx, yy);
            }
        }

        await Task.WhenAll(tasks); //wait for all tiles to be done before proceeding

        //get the world seed.
        worldSeed = TGenSettings.worldSeed;
        Random.InitState(worldSeed);

        //initialize all neighbors lists. i know it's inefficient, IDRC at the moment.
        //the problem is how to get the neighbors in the middle of creation- early tiles' neighbors dont exist yet.
        bool xOdd;
        HexInfo hi;
        for (int xx = 0; xx < gridWidth; xx++)
        {
            xOdd = (xx % 2 == 1);
            for (int yy = 0; yy < gridHeight; yy++)
            {
                //initialize spot on grid with a new data object
                hi = hexGrid[xx, yy];
                if (yy != gridHeight - 1) hi.SetNewNeighbor(hexGrid[xx, yy + 1], 1); //N
                if (yy != 0) hi.SetNewNeighbor(hexGrid[xx, yy - 1], 4); //S

                if (xx != gridWidth - 1 && !(yy == gridHeight - 1 && !xOdd)) //NE
                {
                    if (xOdd) { hi.SetNewNeighbor(hexGrid[xx + 1, yy], 2); }
                    else { hi.SetNewNeighbor(hexGrid[xx + 1, yy + 1], 2); }
                }

                if (xx != gridWidth - 1 && !(yy == 0 && xOdd)) //SE
                {
                    if (xOdd) { hi.SetNewNeighbor(hexGrid[xx + 1, yy - 1], 3); }
                    else { hi.SetNewNeighbor(hexGrid[xx + 1, yy], 3); }
                }

                if (xx != 0 && !(yy == 0 && xOdd)) //SW
                {
                    if (xOdd) { hi.SetNewNeighbor(hexGrid[xx - 1, yy - 1], 5); }
                    else { hi.SetNewNeighbor(hexGrid[xx - 1, yy], 5); }
                }

                if (xx != 0 && !(yy == gridHeight - 1 && !xOdd)) //NW
                {
                    if (xOdd) { hi.SetNewNeighbor(hexGrid[xx - 1, yy], 6); }
                    else { hi.SetNewNeighbor(hexGrid[xx - 1, yy + 1], 6); }
                }
            }
        }



        //next, run all the different aspects of terrain generation.
        TerrainGeneration();

    }

    private async Task createHexInfoGrid(int xx, int yy)
    {

        hexGrid[xx, yy] = new HexInfo(xx, yy); //initialize spot on grid with a new data object
        hexGrid[xx, yy].terrain = singles.getTerrain("UNDF1");
        await Task.Yield();
    }

    // ---------------------------
    //PHASE 2 - TERRAIN GENERATION
    // ---------------------------
    void TerrainGeneration()
    {
        DEBUG_GENERATING = true;
        //clear all tiles from the last try
        //TODO: REMOVE ---

        for (int xx = 0; xx < gridWidth; xx++)
        {
            for (int yy = 0; yy < gridHeight; yy++)
            {
                hexGrid[xx, yy].resetForRegens(singles.getTerrain("UNDF1"));
            }
        }

        Destroy(container);
        container = new GameObject();
        container.name = "Container";

        resetBigLists();
        //TODO: REMOVE ABOVE ---

        //step 1: land and sea gen (make a blob of land to start)
        MultiStageGeneration(mapSizeStats.islandSize);

        //step 2: terrain polishing
        carveLakes(1, 2, 8); //add a couple big lakes...
        carveLakes(2, 5, 3); //and a few small ones...
        BayGen(2, 4, 13, true);
        BayGen(8, 20, 3, false);
        //PenGen(2, 4, 13);

        MountainBatchGeneration(3, 5, 7, 6);
        //we should add small hills later as well
        SwampGeneration(3, 5, 5);

        PlainGeneration(3, 5, 5, 0.2f);
        PlainGeneration(1, 1, 8, 0.4f);

        ForestyFinish(0.3f);

        TranslateToRealWorld();
        FinishingTouches();
        DEBUG_GENERATING = false;
    }

    //Good old multi-stage, the same one we used in GMS.
    void MultiStageGeneration(int maxPossibleDistAway)
    {
        //first, find the center of landmass
        const int searchDistForCenter = 2; //how far can each center be from the actual center of the grid?
        //maxPossibleDistAway represents how far we allow the island to get out from the center (usually doesn't get this far)

        int centerX = Mathf.FloorToInt(Random.value * searchDistForCenter + gridWidth / 2 - searchDistForCenter / 2);
        int centerY = Mathf.FloorToInt(Random.value * searchDistForCenter + gridHeight / 2 - searchDistForCenter / 2);

        //Debug.Log("Center of: " + centerX + "," + centerY);

        //next, probability of land decreases with dist from center.
        Queue<HexInfo> process = new Queue<HexInfo>();
        process.Enqueue(hexGrid[centerX, centerY]);
        HexInfo currTile;

        while (process.Count != 0)
        {
            currTile = process.Dequeue();
            int xx = currTile.GetX(); int yy = currTile.GetY();
            //use PythagBiased for a horizontal map
            int distFromCenter = PythagBiased(xx, yy, centerX, centerY);

            //how probability function works: if you're 0 away (at center) you are always land.
            //at 2 away, if MPDA is 5, you have a 40% chance of it being land
            if (Random.value > (float)distFromCenter / (float)maxPossibleDistAway)
            {
                //hexGrid[xx, yy].elevation = 1;
                hexGrid[xx, yy].terrain = singles.getTerrain("GRASS");
                foreach (HexInfo n in currTile.GetNeighbors())
                {
                    if (n != null && n.terrain == singles.getTerrain("UNDF1"))
                    {
                        process.Enqueue(n);
                        n.terrain = singles.getTerrain("UNDF2"); //make sure you don't add this tile again
                    }
                }
            }
            else
            {
                hexGrid[xx, yy].terrain = singles.getTerrain("BEACH");
                //hexGrid[xx, yy].isOceanCoastal = true;
            }


        }

        //cleanup loop, AKA coast refining: change all undefined tiles to ocean, mess with coastal tiles
        System.Array.Clear(marked, 0, marked.Length);
        for (int xx = 0; xx < gridWidth; xx++)
        {
            for (int yy = 0; yy < gridHeight; yy++)
            {
                currTile = hexGrid[xx, yy];
                if (singles.isUndefined(currTile.terrain)) //unvisited in generation, just make it ocean
                {
                    //hexGrid[xx, yy].terrain = singles.getTerrain("OCEAN");
                    //hexGrid[xx, yy].elevation = 0.1f;
                    if (!marked[xx, yy]) { LakeHunt(currTile, 12); } // identify lakes
                }
                else if (currTile.terrain == singles.getTerrain("BEACH")) //coastal
                {
                    int count = 0;
                    foreach (HexInfo n in currTile.GetNeighbors())
                    {
                        if (n != null)
                        {
                            if (singles.isInLakeHunting(n.terrain)) break;
                            else count++;
                        } //if a null n was found, it's on the edge, so it will stay coastal.
                    }
                    if (count == 6)
                    {
                        currTile.terrain = singles.getTerrain("GRASS");
                        //currTile.elevation = 1;
                        currTile.isOceanCoastal = false;
                    }
                    else //it stayed coastal after all, so...
                    {
                        currTile.isOceanCoastal = true;
                        currTile.elevation = 0.5f;
                    }

                }

                //at the end of the day, if it's grassy, give it a random elevation in a process we call elevation noise.
                if (currTile.terrain.getTerrainID() == 3)
                {
                    currTile.elevation = Random.Range(0.4f, 1.3f);
                    //currTile.elevation = 1f;
                }

                terraStorage[currTile.terrain.getTerrainID()].Add(currTile);
            }
        }

    }

    //find lakes that happened to generate in Multi Stage (we don't want small inland "oceans".
    void LakeHunt(HexInfo lakeStart, int threshold)
    {
        Queue<HexInfo> queue = new Queue<HexInfo>();
        HashSet<HexInfo> discovered = new HashSet<HexInfo>(); //for newly found tiles
        HashSet<HexInfo> currRegion = new HashSet<HexInfo>(); //for this region only

        int tilesFound = 0;

        queue.Enqueue(lakeStart);
        marked[lakeStart.GetX(), lakeStart.GetY()] = true;
        tilesFound++;
        discovered.Add(lakeStart);
        currRegion.Add(lakeStart);

        //visit nearby tiles
        while (queue.Count > 0 && tilesFound <= threshold)
        {
            HexInfo currTile = queue.Dequeue();

            foreach (HexInfo n in currTile.GetNeighbors())
            {
                if (n != null && singles.isInLakeHunting(n.terrain))
                {
                    if (marked[n.GetX(), n.GetY()] == false) //once we know a place is ocean/lake, we dont reset it anymore
                    {
                        marked[n.GetX(), n.GetY()] = true;
                        discovered.Add(n);
                    }
                    if (!currRegion.Contains(n)) //but we will allow adding any of them to our current list of explored tiles
                    {
                        queue.Enqueue(n);
                        tilesFound++;
                        currRegion.Add(n);
                    }

                }
            }
        }

        //either set the tile as a lake or ocean, depending on how many water neighbors were found.
        if (tilesFound <= threshold) //less than threshold tiles...so it's a LAKE!
        {
            int currFeature = addNewNaturalFeature(discovered, singles.getTerrain("LAKE")); //label it as such.

            foreach (HexInfo hi in discovered)
            {
                hi.inNaturalFeatures.Add(currFeature); //from now on we must start keeping track of which tiles are apart of features
                hi.terrain = singles.getTerrain("LAKE");
                hi.elevation = 0.1f;
                //additionally, coastal tiles aren't coastal anymore...sometimes...
                foreach (HexInfo n in hi.GetNeighbors())
                {
                    if (n != null && n.terrain == singles.getTerrain("BEACH"))
                    {
                        bool coastalConfirmed = false;
                        foreach (HexInfo n2 in n.GetNeighbors()) //don't overwrite tiles which are ACTUALLY coastal
                        {
                            if (n2 != null && singles.isInLakeHunting(n2.terrain))
                            {
                                n.terrain = singles.getTerrain("BEACH");
                                n.elevation = 0.5f;
                                n.isOceanCoastal = true;
                                n.isLakeCoastal = true;
                                coastalConfirmed = true;
                                break;
                            }
                        }
                        if (!coastalConfirmed)
                        {
                            n.terrain = singles.getTerrain("GRASS");
                            n.elevation = 0.5f; //elevChange when you have to restructure things
                            n.isOceanCoastal = false;
                            n.isLakeCoastal = true;
                        }
                    }
                }
            }
        }
        else //more than threshold tiles...so it's part of the OCEAN! (or a very large sea)
        {
            foreach (HexInfo hi in discovered)
            {
                hi.terrain = singles.getTerrain("OCEAN");
                hi.elevation = 0.1f;
            }
        }
    }

    //intentionally carve out a few lakes, which might become salty marshes instead.
    //why this caveat? because it's easy to tell when you're forcing a lake to stay a lake...
    void carveLakes(int minAmount, int maxAmount, int averageSize)
    {
        //how many to make?
        int amount = Mathf.RoundToInt(minAmount + (maxAmount - minAmount) * Random.value);
        for (int i = 0; i < amount; i++)
        {
            bool isMarshInstead = false; //if we hit seawater, everything becomes a marsh instead.

            //get a random land tile and start carving out the lake.
            //yes, the arrays aren't entirely accurate...so we have to double check, but it's not a big deal!
            HexInfo startOfLake; int indexOfSOL;
            do
            {
                indexOfSOL = Mathf.FloorToInt(Random.value * terraStorage[3].Count);
                startOfLake = terraStorage[3][indexOfSOL];
            } while (startOfLake.terrain.getTerrainID() != 3 || startOfLake.isLakeCoastal || startOfLake.isOceanCoastal);

            Queue<HexInfo> queue = new Queue<HexInfo>(); //typical queue stuff:
            System.Array.Clear(marked, 0, marked.Length); //create the queue, reset the marked grid

            queue.Enqueue(startOfLake); //basic starting stuff
            marked[startOfLake.GetX(), startOfLake.GetY()] = true;
            int count = 0;
            int sizeOfThisLake = Mathf.RoundToInt(averageSize * Random.Range(0.5f, 1.5f));

            HashSet<HexInfo> tilesInThisLake = new HashSet<HexInfo>(); //lastly, add it as a natural feature
            int soonToBeIndex = naturalFeatureRegions.Count;

            while (queue.Count > 0)
            {
                HexInfo currTile = queue.Dequeue(); //first, make the tile a lake
                currTile.terrain = singles.getTerrain("LAKE");
                if (currTile.isOceanCoastal) isMarshInstead = true;
                //remove this if you want to easily identify which lakes were genned in this algorithm:
                currTile.elevation = 0.1f;
                currTile.inNaturalFeatures.Add(soonToBeIndex); //the tile is apart of a natural feature
                tilesInThisLake.Add(currTile);
                terraStorage[3].Remove(currTile); //it's not land anymore, don't use it for future land stuff.

                foreach (HexInfo n in currTile.GetNeighbors())
                {
                    if (n != null && !marked[n.GetX(), n.GetY()])
                    {
                        marked[n.GetX(), n.GetY()] = true;
                        if (count < sizeOfThisLake && !n.isLakeCoastal)
                        {
                            queue.Enqueue(n);
                            count++;
                        }
                        else
                        {
                            n.isLakeCoastal = true;
                        }
                    }
                }
            }

            if (isMarshInstead) //convert everything to a bay if needed
            {
                foreach (HexInfo tile in tilesInThisLake)
                {
                    tile.terrain = singles.getTerrain("MARSH");
                    tile.elevation = 0.1f;

                    foreach (HexInfo n in tile.GetNeighbors())
                    {
                        if (n != null && n.isLakeCoastal)
                        {
                            n.isOceanCoastal = true;
                            n.isLakeCoastal = false;
                        }
                    }
                }
            }
            else
            {
                addNewNaturalFeature(tilesInThisLake, singles.getTerrain("LAKE")); //now add the natural feature
            }
        }
    }

    //Bays are places where the ocean cuts into the coastline, forming a nice circular stretch of coast
    void BayGen(int minAmount, int maxAmount, int averageSize, bool major)
    {
        int amount = Mathf.RoundToInt(minAmount + (maxAmount - minAmount) * Random.value);
        for (int i = 0; i < amount; i++)
        {
            //similar to what we did in carveLakes(). get a suitable coastal tile for the start of the bay.
            HexInfo startOfBay; int indexOfSOB;
            do
            {
                indexOfSOB = Mathf.FloorToInt(Random.value * terraStorage[4].Count);
                startOfBay = terraStorage[4][indexOfSOB];
            } while (startOfBay.terrain.getTerrainID() != 4 || !startOfBay.isOceanCoastal || startOfBay.isLakeCoastal);

            Queue<HexInfo> queue = new Queue<HexInfo>(); //typical queue stuff:
            System.Array.Clear(marked, 0, marked.Length); //create the queue, reset the marked grid

            queue.Enqueue(startOfBay); //basic starting stuff
            marked[startOfBay.GetX(), startOfBay.GetY()] = true;
            int count = 0;
            int sizeOfThisBay = Mathf.RoundToInt(averageSize * Random.Range(0.5f, 1.5f));

            HashSet<HexInfo> tilesInThisBay = new HashSet<HexInfo>(); //lastly, add it as a natural feature
            int soonToBeIndex = naturalFeatureRegions.Count;

            while (queue.Count > 0)
            {
                HexInfo currTile = queue.Dequeue(); //first, make the tile a lake
                currTile.terrain = singles.getTerrain("OCEAN");
                currTile.elevation = 0.1f;
                currTile.isOceanCoastal = false;

                tilesInThisBay.Add(currTile);
                currTile.inNaturalFeatures.Add(soonToBeIndex); //the tile is apart of a natural feature

                terraStorage[4].Remove(currTile); //it's not coastal anymore, don't use it for future land stuff.

                foreach (HexInfo n in currTile.GetNeighbors())
                {
                    if (n != null && !marked[n.GetX(), n.GetY()])
                    {
                        marked[n.GetX(), n.GetY()] = true;
                        if (n.terrain.getTerraGroup() != TerrainType.TerraGroup.WATER)
                        {
                            if (count < sizeOfThisBay && !n.isLakeCoastal)
                            {
                                queue.Enqueue(n);
                                count++;
                                //if (n.terrain.getTerrainID() == 3) terraStorage[3].Remove(n);
                            }
                            else
                            {
                                n.isOceanCoastal = true;
                                n.elevation = 0.5f;
                                n.terrain = singles.getTerrain("BEACH");
                                terraStorage[4].Add(currTile);
                            }
                        }
                    }
                }
            }

            if(major)
            {
                addNewNaturalFeature(tilesInThisBay, singles.getTerrain("OCEAN")); //now add the natural feature
            }
        }
    }

    //Peninsulas are the opposite: places where the land juts out into the ocean
    void PenGen(int minAmount, int maxAmount, int averageSize)
    {
        int amount = Mathf.RoundToInt(minAmount + (maxAmount - minAmount) * Random.value);
        for (int i = 0; i < amount; i++)
        {
            //similar to what we did in carveLakes(). get a suitable coastal tile for the start of the bay.
            HexInfo startOfPen; int indexOfSOP;
            do
            {
                indexOfSOP = Mathf.FloorToInt(Random.value * terraStorage[4].Count);
                startOfPen = terraStorage[4][indexOfSOP];
            } while (startOfPen.terrain.getTerrainID() != 4 || !startOfPen.isOceanCoastal);

            Queue<HexInfo> queue = new Queue<HexInfo>(); //typical queue stuff:
            System.Array.Clear(marked, 0, marked.Length); //create the queue, reset the marked grid

            queue.Enqueue(startOfPen); //basic starting stuff
            marked[startOfPen.GetX(), startOfPen.GetY()] = true;
            int count = 0;
            int sizeOfThisBay = Mathf.RoundToInt(averageSize * Random.Range(0.5f, 1.5f));

            HashSet<HexInfo> tilesInThisPen = new HashSet<HexInfo>(); //lastly, add it as a natural feature
            int soonToBeIndex = naturalFeatureRegions.Count;

            while (queue.Count > 0)
            {
                HexInfo currTile = queue.Dequeue(); //first, make the tile a lake
                currTile.terrain = singles.getTerrain("BEACH");
                currTile.elevation = 0.5f;
                currTile.updateCoastals();

                tilesInThisPen.Add(currTile);
                currTile.inNaturalFeatures.Add(soonToBeIndex); //the tile is apart of a natural feature

                foreach (HexInfo n in currTile.GetNeighbors())
                {
                    if (n != null && !marked[n.GetX(), n.GetY()])
                    {
                        marked[n.GetX(), n.GetY()] = true;
                        if (n.terrain.getTerraGroup() == TerrainType.TerraGroup.WATER)
                        {
                            if (count < sizeOfThisBay && !n.isLakeCoastal)
                            {
                                queue.Enqueue(n);
                                count++;
                            }
                            else
                            {
                                n.updateCoastals();
                            }
                        }
                    }
                }
            }

            addNewNaturalFeature(tilesInThisPen, singles.getTerrain("BEACH")); //now add the natural feature
        }
    }

    //Generate some Mountain batches around the world!
    //This one's a little different, instead of setting new terrain, we just change the elevation.
    //Terrain changes happen at the end when elevation has already finalized
    void MountainBatchGeneration(int minAmount, int maxAmount, int averageSize, int averageHeight)
    {
        int amount = Mathf.RoundToInt(minAmount + (maxAmount - minAmount) * Random.value);
        for (int i = 0; i < amount; i++)
        {
            //get a random land tile, this will be the "peak" of the mountain batch.
            //as usual, double check to see the land tile is something that we actually want
            HexInfo peak; int indexOfPeak;
            do
            {
                indexOfPeak = Mathf.FloorToInt(Random.value * terraStorage[3].Count);
                peak = terraStorage[3][indexOfPeak];
            } while (peak.terrain.getTerrainID() != 3);

            Queue<HexInfo> queue = new Queue<HexInfo>(); //typical queue stuff:
            System.Array.Clear(marked, 0, marked.Length); //create the queue, reset the marked grid

            queue.Enqueue(peak); //basic starting stuff
            marked[peak.GetX(), peak.GetY()] = true;
            int sizeOfMountain = Mathf.RoundToInt(averageSize * Random.Range(0.5f, 1.5f));
            int heightOfMountain = Mathf.RoundToInt(averageHeight * Random.Range(0.5f, 1.5f));

            HashSet<HexInfo> tilesInMountain = new HashSet<HexInfo>(); //lastly, add it as a natural feature
            int soonToBeIndex = naturalFeatureRegions.Count;

            while (queue.Count > 0)
            {
                HexInfo currTile = queue.Dequeue(); //small chance we make this actual mountain terrain later...?
                //currTile.terrain = singles.getTerrain("MOUNT");
                int distFromCenter = Pythag(currTile.GetX(), currTile.GetY(), peak.GetX(), peak.GetY());
                currTile.elevation += ((float)(sizeOfMountain - distFromCenter) / (float)sizeOfMountain) * heightOfMountain;
                currTile.inNaturalFeatures.Add(soonToBeIndex); //the tile is apart of a natural feature
                tilesInMountain.Add(currTile);
                //terraStorage[3].Remove(currTile); //we'll find other places for more features, thanks, thanks

                if (sizeOfMountain - distFromCenter > 1)
                {
                    foreach (HexInfo n in currTile.GetNeighbors())
                    {
                        if (n != null && !marked[n.GetX(), n.GetY()])
                        {
                            marked[n.GetX(), n.GetY()] = true;
                            if (n.terrain.getTerraGroup() != TerrainType.TerraGroup.WATER) //if it aint land, dont bother
                            {
                                queue.Enqueue(n);
                            }

                        }
                    }
                }

            }

            addNewNaturalFeature(tilesInMountain, singles.getTerrain("MOUNT")); //now add the natural feature
        }
    }

    //Generate swamps. Start from a center, probability decreases as you get further
    void SwampGeneration(int minAmount, int maxAmount, int averageSize)
    {
        int amount = Mathf.RoundToInt(minAmount + (maxAmount - minAmount) * Random.value);
        for (int i = 0; i < amount; i++)
        {
            //get a random land tile, this will be the "peak" of the mountain batch.
            //as usual, double check to see the land tile is something that we actually want
            HexInfo startOfSwamp; int indexOfSOF;
            do
            {
                indexOfSOF = Mathf.FloorToInt(Random.value * terraStorage[3].Count);
                startOfSwamp = terraStorage[3][indexOfSOF];
            } while (startOfSwamp.terrain.getTerrainID() != 3 || startOfSwamp.elevation >= 2);

            Queue<HexInfo> queue = new Queue<HexInfo>(); //typical queue stuff:
            System.Array.Clear(marked, 0, marked.Length); //create the queue, reset the marked grid

            queue.Enqueue(startOfSwamp); //basic starting stuff
            marked[startOfSwamp.GetX(), startOfSwamp.GetY()] = true;
            int sizeOfSwamp = Mathf.RoundToInt(averageSize * Random.Range(0.5f, 1.5f));

            HashSet<HexInfo> tilesInSwamp = new HashSet<HexInfo>(); //lastly, add it as a natural feature
            int soonToBeIndex = naturalFeatureRegions.Count;

            while (queue.Count > 0)
            {
                HexInfo currTile = queue.Dequeue(); //small chance we make this actual mountain terrain later...?
                currTile.terrain = singles.getTerrain("SWAMP");
                int distFromCenter = Pythag(currTile.GetX(), currTile.GetY(), startOfSwamp.GetX(), startOfSwamp.GetY());
                float odds = ((float)(sizeOfSwamp - distFromCenter) / (float)sizeOfSwamp);
                currTile.inNaturalFeatures.Add(soonToBeIndex); //the tile is apart of a natural feature
                tilesInSwamp.Add(currTile);
                terraStorage[3].Remove(currTile); //we'll find other places for more features, thanks, thanks

                if (sizeOfSwamp - distFromCenter > 1)
                {
                    foreach (HexInfo n in currTile.GetNeighbors())
                    {
                        if (n != null && !marked[n.GetX(), n.GetY()])
                        {
                            marked[n.GetX(), n.GetY()] = true;
                            if (n.terrain.getTerraGroup() != TerrainType.TerraGroup.WATER && (Random.value < odds)
                                && n.elevation <= 2f)
                            {
                                queue.Enqueue(n);
                            }

                        }
                    }
                }

            }

            addNewNaturalFeature(tilesInSwamp, singles.getTerrain("SWAMP")); //now add the natural feature
        }
    }


    //generates actual plains biomes (not just clearings of forest)
    void PlainGeneration(int minAmount, int maxAmount, int averageSize, float elevNoise)
    {
        int amount = Mathf.RoundToInt(minAmount + (maxAmount - minAmount) * Random.value);
        for (int i = 0; i < amount; i++)
        {
            //get a random land tile, this will be the "peak" of the mountain batch.
            //as usual, double check to see the land tile is something that we actually want
            HexInfo startOfPlain; int indexOfSOP;
            do
            {
                indexOfSOP = Mathf.FloorToInt(Random.value * terraStorage[3].Count);
                startOfPlain = terraStorage[3][indexOfSOP];
            } while (startOfPlain.terrain.getTerrainID() != 3 || startOfPlain.elevation >= 4f);

            Queue<HexInfo> queue = new Queue<HexInfo>(); //typical queue stuff:
            System.Array.Clear(marked, 0, marked.Length); //create the queue, reset the marked grid

            queue.Enqueue(startOfPlain); //basic starting stuff
            marked[startOfPlain.GetX(), startOfPlain.GetY()] = true;
            int sizeOfPlain = Mathf.RoundToInt(averageSize * Random.Range(0.5f, 1.5f));
            float plainElevation = startOfPlain.elevation; //plains have mostly consistent elevation thruout

            HashSet<HexInfo> tilesInPlain = new HashSet<HexInfo>(); //lastly, add it as a natural feature
            int soonToBeIndex = naturalFeatureRegions.Count;

            while (queue.Count > 0)
            {
                HexInfo currTile = queue.Dequeue(); //small chance we make this actual mountain terrain later...?
                currTile.terrain = singles.getTerrain("PLAIN");
                currTile.treeCover = false; //plains never have tree cover.
                currTile.elevation = Random.Range(plainElevation - elevNoise, plainElevation + elevNoise);

                int distFromCenter = Pythag(currTile.GetX(), currTile.GetY(), startOfPlain.GetX(), startOfPlain.GetY());
                float odds = ((float)(sizeOfPlain - distFromCenter) / (float)sizeOfPlain);
                currTile.inNaturalFeatures.Add(soonToBeIndex); //the tile is apart of a natural feature
                tilesInPlain.Add(currTile);
                terraStorage[3].Remove(currTile); //we'll find other places for more features, thanks, thanks

                if (sizeOfPlain - distFromCenter > 1)
                {
                    foreach (HexInfo n in currTile.GetNeighbors())
                    {
                        if (n != null && !marked[n.GetX(), n.GetY()])
                        {
                            marked[n.GetX(), n.GetY()] = true;
                            if (n.terrain.getTerrainID() == 3 && (Random.value < odds)
                                && n.elevation <= 4f)
                            {
                                queue.Enqueue(n);
                            }

                        }
                    }
                }

            }

            addNewNaturalFeature(tilesInPlain, singles.getTerrain("PLAIN")); //now add the natural feature
        }
    }

    //Every tile that hasn't already become some other natural feature has a chance of being a forest tile.
    //Later on, we label forests as "Temperate", "Pine", or "Tropical" depending on elevation and distance to coast.
    void ForestyFinish(float density)
    {
        while (terraStorage[3].Count > 0)
        {
            HexInfo currTile = terraStorage[3][0];
            terraStorage[3].RemoveAt(0);
            if (currTile.terrain.getTerraGroup() != TerrainType.TerraGroup.WATER && Random.value <= density)
            {
                //currTile.terrain = singles.getTerrain(9); //temperate forest
                currTile.treeCover = true;
            }
        }
    }

    // ---------------------------
    //PHASE 3 - BUILD THE WORLD
    // ---------------------------

    //use the information generated in Terrain Gen to build the physical map. NO RANDOMNESS HERE, that's already been done.
    async void TranslateToRealWorld()
    {
        //first, a random touchup: let's fix the elevation of lakes.
        lakeElevation();

        //now, we're ready to build the world.
        var tasks = new Task[gridWidth * gridHeight];
        for (int i = 0; i < gridWidth; i++)
        {
            //to make the hexes fit nicely, every other row gets offset by a little bit.
            if (i % 2 == 0) { zStarter = 2; } else { zStarter = 0; }
            for (int j = 0; j < gridHeight; j++)
            {
                tasks[i * gridHeight + j] = buildNewTile(i, j, zStarter);
            }
        }
        await Task.WhenAll(tasks);

        labelNaturalFeatures();

    }

    async Task buildNewTile(int i, int j, int zStarter)
    {
        if(hexGrid[i,j].terrain.getTerrainID() != 2) //don't bother creating ocean tiles anymore. that ship is sailed ;)
        {
            GameObject g = Instantiate(hexTile);
            //all new tiles are parents of the empty terrainGen object.
            g.transform.parent = container.transform;
            HexTile ht = g.GetComponent<HexTile>();

            ht.hexInfo = hexGrid[i, j];
            await ht.readyToGo();
        } else
        {
            await Task.Yield();
        }
        
    }

    void lakeElevation() //find elevation of all lakes on the map
    {
        int currIndex = 0;
        while (naturalFeatureTypes[currIndex].getTerrainID() == 5)
        {
            float minElevation = 100;
            foreach (HexInfo lakeTile in naturalFeatureRegions[currIndex])
            {
                foreach (HexInfo n in lakeTile.GetNeighbors())
                {
                    if (n != null && n.terrain.getTerraGroup() != TerrainType.TerraGroup.WATER)
                    {
                        minElevation = Mathf.Min(getStep(n.elevation), minElevation);
                    }
                }
            }

            foreach (HexInfo lakeTile in naturalFeatureRegions[currIndex])
            {
                lakeTile.elevation = minElevation - 0.1f;
            }
            currIndex++;
        }
    } //end this random method already.

    void FinishingTouches()
    {
        UI_system.gameObject.SetActive(true);
    }

    //for rounding off elevation.
    public static float getStep(float elev)
    {
        float roundDown = Mathf.Abs(Mathf.Floor(elev / elevationStep) * elevationStep - elev);
        float roundUp = Mathf.Abs(Mathf.Ceil(elev / elevationStep) * elevationStep - elev);
        float step;

        if (roundDown < roundUp) { step = Mathf.Floor(elev / elevationStep) * elevationStep; }
        else { step = Mathf.Ceil(elev / elevationStep) * elevationStep; }

        return Mathf.Max(step, elevationStep); //so that you never have negative or zero elevation
    }

    //calculate APPROXIMATE distance between (x1, y1) and (x2, y2)
    /*NOTE: NOT WHAT YOU THINK IT IS - it's just kind of unnecessary to calculate exact distances, so instead we make
     an approximation based on coordinates alone. Use the Pythagorean theorem for this.
     For ex, (0,0) can reach (10,7) by crossing as few as 12 tiles. The Pythagorean thm between these two points is 12.20.
     Very close, right? so just round down whatever you get.*/
    public int Pythag(int x1, int y1, int x2, int y2)
    {
        return Mathf.FloorToInt(Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2)));
    }

    //same as above, but biased towards horizontal expansion
    public int PythagBiased(int x1, int y1, int x2, int y2)
    {
        return Mathf.FloorToInt(Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2)));
    }

    void labelNaturalFeatures()
    {
        float xSum = 0, ySum = 0;
        for (int i = 0; i < naturalFeatureTypes.Count; i++)
        {
            HashSet<HexInfo> currFeature = naturalFeatureRegions[i];
            xSum = ySum = 0;
            float maxElev = 0;
            //first, get the "center" position of this natural feature.
            foreach (HexInfo tile in currFeature)
            {
                xSum += tile.GetRealX();
                ySum += tile.GetRealY();
                if (tile.elevation > maxElev) maxElev = tile.elevation;
            }
            (float centerX, float centerY) centerSpot = (xSum / currFeature.Count, ySum / currFeature.Count);
            //now, spawn a new nametag for it
            gennedNametags.Add(Instantiate(featureNametag.gameObject));
            gennedNametags[gennedNametags.Count - 1].name = naturalFeatureTypes[i] + " " + i;
            TextMeshProUGUI newTag = gennedNametags[gennedNametags.Count - 1].GetComponent<TextMeshProUGUI>();
            newTag.rectTransform.SetParent(featureNametag.rectTransform.parent);
            newTag.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
            newTag.rectTransform.localPosition = new Vector3(centerSpot.centerX, centerSpot.centerY, -2 * maxElev - 1.1f);
            newTag.text = naturalFeatureTypes[i] + " " + i;
        }
    }

    // Regeneration (debugging)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !DEBUG_GENERATING)
        {
            //Debug.Log("Regen start");
            worldSeed = Random.Range(0, 999999);
            Random.InitState(worldSeed);

            TerrainGeneration();
            regenerateWorld?.Invoke(worldSeed);
            //Debug.Log("Regen finish");
        }
    }
}

