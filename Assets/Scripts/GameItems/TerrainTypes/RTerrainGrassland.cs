using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainGrassland : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0, 0.5f, 0);
    string terraName = "Grassland";
    bool isPassable = true;
    TerraGroup group = TerraGroup.LOWLAND;
    int terraID = 3;

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
