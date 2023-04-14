using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTerrainTForest : TerrainType
{
    float moveCost = 1;
    Color color = new Color(0.3f, 0.5f, 0.3f);
    string terraName = "Temperate Forest";
    bool isPassable = true;
    TerraGroup group = TerraGroup.FOREST;
    int terraID = 9;

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
