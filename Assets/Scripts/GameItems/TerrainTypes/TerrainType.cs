using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainType
{
    public enum TerraGroup { UNDEFINED, LOWLAND, WATER, FOREST, MOUNTAIN, VOLCANO };

    /*public float moveCost;
    public Color color;
    public string terraName;

    public abstract bool isPassable;
    public TerraGroup group;*/

    //methods here...
    public abstract float getMoveCost();

    public abstract Color getColor();

    public abstract string getTerrainName();

    public abstract bool getIsPassable();

    public abstract TerraGroup getTerraGroup();

    public abstract int getTerrainID();

    public override string ToString()
    {
        return getTerrainName();
    }
}
