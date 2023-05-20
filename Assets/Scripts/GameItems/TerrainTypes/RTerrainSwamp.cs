using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainSwamp : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.05f, 0.2f, 0.05f);
    string terraName = "Swamp";
    bool isPassable = true;
    TerraGroup group = TerraGroup.LOWLAND;
    int terraID = 8;

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
