using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the hexInfo is used for storing its data in one place.
//for instance: its terrain type, factional occupier, and elevation
public class HexInfo
{
    //coordinates
    private int xCoords;
    private int yCoords;

    private HexInfo[] Neighbors;
    public TerrainType terrain;
    //TODO: elevation measured in thousands of feet, so: 2000 feet = 2, sea level = 0. highest possible is 8.
    public float elevation = 0.1f;

    public bool isOceanCoastal = false; //set to -1 if not a coastal land tile, otherwise, the direction of any ocean water
    public bool isLakeCoastal = false; //same as above but for lakes
    public int treeCover = 0; //0 = none, 1 = temperate, 2 = pine, 3 = tropical

    public List<int> inNaturalFeatures; //which natural features are you in, if any

    //UNIT STUFF
    public UnitPiece unit;
    public bool withinRangeOfSelected = false; //within range of selected unit?

    //STRUCTURE / BUILDINGS
    public Structure structure;
    public HashSet<int> roads = new HashSet<int>();
    public HashSet<int> rivers = new HashSet<int>();

    //basically empty constructor, does nothing
    public HexInfo(int xC, int yC)
    {
        xCoords = xC;
        yCoords = yC;

        Neighbors = new HexInfo[6] { null, null, null, null, null, null };

        inNaturalFeatures = new List<int>();
    }

    public override string ToString()
    {
        return "(" + xCoords + "," + yCoords + ")";
    }

    //Gives Neighbors in the following order:
    //(1) North, (2) NorthEast, (3) SouthEast, (4) South, (5) SouthWest, (6) NorthWest
    public void SetNewNeighbor(HexInfo tile, int index)
    {
        Neighbors[index-1] = tile;
    }

    public HexInfo[] GetNeighbors() //return all, even the nulls. might change later
    {
        return Neighbors;
    }

    //USE ONLY IN WORLD REGENS (DEBUGGING)
    public void resetForRegens(TerrainType resetTo)
    {
        elevation = 0.1f;
        terrain = resetTo;
        isOceanCoastal = false;
        isLakeCoastal = false;
        treeCover = 0;
        inNaturalFeatures.Clear();

        //structure = null; unit = null;
        rivers.Clear(); //roads.Clear();
    }

    //start using this from now on, to get the position that would put an object ON TOP OF a tile...with left corner coords.
    public Vector3 getOnTopOfLC()
    {
        return new Vector3(GetRealX() + 1.75f, GetStanding(), GetRealY() - 1);
    }

    //to get the position that would put an object ON TOP OF a tile...with centered coords
    public Vector3 getOnTopOfCenter()
    {
        return new Vector3(GetRealX(), GetStanding(), GetRealY());
    }

    public int GetX() { return xCoords; }
    public int GetY() { return yCoords; }

    public float GetRealX() { return xCoords * 3.5f; }
    public float GetRealY() {
        if (xCoords % 2 == 0) return yCoords * 4 + 2;
        else return yCoords * 4;
    }
    public float GetStanding() //return the point at which a unit / structure would stand on this tile
    {
        if (TerrainTypesSingletons.hasTopsoil(terrain))
        {
            return elevation * 2 + 0.2f;
        }
        else return elevation * 2;
    }

    public float GetElevWTopsoil() //return elevation, plus topsoil if it exists
    {
        if (TerrainTypesSingletons.hasTopsoil(terrain))
        {
            return elevation + 0.1f;
        }
        else return elevation;
    }

    public void updateCoastals()
    {
        isOceanCoastal = isLakeCoastal = false;

        foreach(HexInfo n in Neighbors)
        {
            if (n.terrain.getTerrainID() == 2)
            {
                isOceanCoastal = true;
            }
            else if(n.terrain.getTerrainID() == 5)
            {
                isLakeCoastal = true;
            }
        }
    }

    public float getDistanceApprox(HexInfo other)
    {
        return Mathf.FloorToInt(Mathf.Sqrt(Mathf.Pow(xCoords - other.xCoords, 2) + Mathf.Pow(yCoords - other.yCoords, 2)));
    }
}
