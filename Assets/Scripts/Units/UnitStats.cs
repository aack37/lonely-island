using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    //what tile are you standing on?
    public HexInfo currTile;

    //movement stats
    public float maxElevationScale;
    public float maxMovement;

    //other stats
    public Faction faction;
    public string unitName = "Unit";

    //when going to a new position, the unit moves from one tile to another.
    public void setCurrTile(HexInfo hi)
    {
        if (currTile != null) { currTile.unit = null; }
        currTile = hi;
    }

    public Faction getFaction()
    {
        return faction;
    }

    //return tile you're standing on
    public HexInfo getCurrTile()
    {
        return currTile;
    }

    public override string ToString()
    {
        return unitName;
    }
}
