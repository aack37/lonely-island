using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainBeach : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.9f, 0.8f, 0.2f);
    string terraName = "Beach";
    bool isPassable = true;
    TerraGroup group = TerraGroup.LOWLAND;
    int terraID = 4;

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
