using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainSnow : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.8f, 0.8f, 0.9f);
    string terraName = "Snowy Mountain";
    bool isPassable = true;
    TerraGroup group = TerraGroup.MOUNTAIN;
    int terraID = 7;

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
