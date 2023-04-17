using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*a unit is a military fighting force that can move, stands on one tile at a time, and uses resources
IT IS DIFFERENT FROM A STRUCTURE, which is a building / fortification that can't move
a unit and a structure can be on the same tile at the same time, but you can't have two units on one tile

 furthermore, an actual in-game unit will have two scripts attached to it: UnitPiece and UnitStats.
 it's a similar setup to HexTile and HexInfo.
 UnitPiece controls physical properties, while UnitStats controls conceptual stats and calculations*/

public class UnitPiece : MonoBehaviour
{
    private HexInfo currTile;
    //public UnitStats stats;

    public void setCurrTile(HexInfo hi)
    {
        currTile = hi;
    }

    public HexInfo getCurrTile()
    {
        return currTile;
    }

    public void resetWorldPos() //set the unit's world position to its current set of coordinates
    {
        transform.position = new Vector3(currTile.GetRealX(), currTile.elevation + 0.2f, currTile.GetRealY());
    }

    public void resetWorldPos(int xC, int yC) //set the unit's world position to a new set of coordinates
    {
        if(yC % 2 == 0) { transform.position = new Vector3(xC * 3.5f, TerrainGen.hexGrid[xC, yC].elevation, yC * 4f + 2); }
        else { transform.position = new Vector3(xC * 3.5f, TerrainGen.hexGrid[xC, yC].elevation, yC * 4f); }
    }
}
