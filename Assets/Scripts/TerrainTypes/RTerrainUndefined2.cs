using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainUndefined2 : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.7f, 0, 0);
    string terraName = "Undefined";
    bool isPassable = false;
    TerraGroup group = TerraGroup.UNDEFINED;
    int terraID = 1;

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
