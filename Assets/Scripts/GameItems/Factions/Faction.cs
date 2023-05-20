using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    private Material countryTone;
    private Material skinTone;
    private Material uniformTone;

    private Material[] toneList;

    private string factionName;

    private GameObject armyContainer;

    public Faction(string facName, Material countryTone)
    {
        //basic stuff
        factionName = facName;

        //country col
        this.countryTone = countryTone;

        //uniform tone is 30% darker, skin tone is 30% brighter
        Color countryCol = countryTone.color;
        uniformTone = new Material(countryTone);
        uniformTone.color = new Color(countryCol.r * 0.7f, countryCol.g * 0.7f, countryCol.b * 0.7f);
        skinTone = new Material(countryTone);
        skinTone.color = new Color(countryCol.r * 1.3f, countryCol.g * 1.3f, countryCol.b * 1.3f);
        toneList = new Material[] { countryTone, uniformTone, skinTone };

        armyContainer = new GameObject();
    }

    public Material[] getTones()
    {
        return toneList;
    }

    public UnitPiece spawnUnitPiece(UnitPiece template, HexInfo spawnTile)
    {
        if (spawnTile.unit == null) //do not spawn the piece if something is already on the tile.
        {
            UnitPiece piece = UnitPiece.createNewCopy(template);
            piece.setCurrTile(spawnTile);
            piece.transform.parent = armyContainer.transform;

            piece.forceStart();
            assignMaterials(piece);
            return piece;
        }
        else
        {
            return null;
        }

    }

    private void assignMaterials(UnitPiece unit)
    {
        unit.assignFaction(this);
    }
}
