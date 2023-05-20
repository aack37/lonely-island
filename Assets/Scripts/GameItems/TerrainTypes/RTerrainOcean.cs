using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainOcean : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.10f, 0.25f, 0.35f);
    string terraName = "Ocean";
    bool isPassable = false;
    TerraGroup group = TerraGroup.WATER;
    int terraID = 2;

    public override Color getColor()
    {
        return color;
    }

    public override bool getIsPassable()
    {
        return isPassable;
    }

    public override float getMoveCost()
    {
        return moveCost;
    }

    public override TerraGroup getTerraGroup()
    {
        return group;
    }

    public override string getTerrainName()
    {
        return terraName;
    }

    public override int getTerrainID()
    {
        return terraID;
    }
}
