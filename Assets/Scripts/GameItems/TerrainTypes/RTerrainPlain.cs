using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainPlain : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.44f, 0.56f, 0.15f);
    string terraName = "Plain";
    bool isPassable = true;
    TerraGroup group = TerraGroup.LOWLAND;
    int terraID = 12;

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
