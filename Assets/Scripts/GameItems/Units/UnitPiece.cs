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
    //stores all unit stats.
    public UnitStats stats;
    Material[] mats;
    MeshRenderer mr;

    private void Start()
    {
        //Debug.Log("Done creating unit");
    }

    //so it's not in the start method...WHATEVER MAN
    public void forceStart()
    {
        mr = GetComponent<MeshRenderer>();
        mats = mr.materials;
    }

    //spawn a new copy of the tempate into the world. other methods deal with the specifics
    public static UnitPiece createNewCopy(UnitPiece template)
    {
        return Instantiate(template);
    }

    //set a new position for the unit. since hexInfo's store UnitPieces, not UnitStats, we deal with this here
    public void setCurrTile(HexInfo hi)
    {
        stats.setCurrTile(hi);
        hi.unit = this;
        resetWorldPos();
    }

    public void resetWorldPos() //set the unit's world position to its current set of coordinates
    {
        transform.position = new Vector3(stats.currTile.GetRealX(), stats.currTile.GetStanding(), stats.currTile.GetRealY());
    }

    public void resetWorldPos(int xC, int yC) //set the unit's world position to a new set of coordinates
    {
        if(yC % 2 == 0) { transform.position = new Vector3(xC * 3.5f, TerrainGen.hexGrid[xC, yC].elevation * 2, yC * 4f + 2); }
        else { transform.position = new Vector3(xC * 3.5f, TerrainGen.hexGrid[xC, yC].elevation * 2, yC * 4f); }
    }

    public override string ToString()
    {
        return stats.ToString();
    }

    public void assignFaction(Faction f)
    {
        stats.faction = f;
        Material[] found = f.getTones();
        for(int i = 0; i < 3; i++)
        {
            mats[i] = found[i];
        }

        mr.materials = mats;
    }
}
