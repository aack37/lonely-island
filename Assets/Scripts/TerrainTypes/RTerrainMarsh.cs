using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainMarsh : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.72f, 0.55f, 0.31f);
    string terraName = "Marsh";
    bool isPassable = true;
    TerraGroup group = TerraGroup.LOWLAND;
    int terraID = 14;

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

