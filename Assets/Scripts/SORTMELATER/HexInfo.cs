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
    private int tileSeed;

    private HexInfo[] Neighbors;
    public TerrainType terrain;
    //elevation measured in thousands of feet, so: 2000 feet = 2, sea level = 0. highest possible is 8.
    public float elevation = 0.1f;

    public bool isOceanCoastal = false; //set to -1 if not a coastal land tile, otherwise, the direction of any ocean water
    public bool isLakeCoastal = false; //same as above but for lakes
    public bool treeCover = false;

    public List<int> inNaturalFeatures;

    //basically empty constructor, does nothing
    public HexInfo(int xC, int yC)
    {
        xCoords = xC;
        yCoords = yC;

        tileSeed = Mathf.FloorToInt(Random.Range(0, 999999));
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

    public int getTileSeed()
    {
        return tileSeed;
    }

    //USE ONLY IN WORLD REGENS (DEBUGGING)
    public void resetForRegens(TerrainType resetTo)
    {
        elevation = 0.1f;
        terrain = resetTo;
        tileSeed = Mathf.FloorToInt(Random.Range(0, 999999));
        isOceanCoastal = false;
        isLakeCoastal = false;
        treeCover = false;
        inNaturalFeatures.Clear();
    }

    public int GetX() { return xCoords; }
    public int GetY() { return yCoords; }

    public float GetRealX() { return xCoords * 3.5f; }
    public float GetRealY() {
        if (xCoords % 2 == 0) return yCoords * 4 + 2;
        else return yCoords * 4;
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
}
