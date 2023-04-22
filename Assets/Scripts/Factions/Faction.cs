using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    private Material countryTone;
    private Material skinTone;
    private Material uniformTone;

    private string factionName;

    private GameObject armyContainer;

    public Faction(string facName, Material countryTone)
    {
        //basic stuff
        factionName = facName;

        //country col
        this.countryTone = countryTone;

        //uniform tone is 30% brighter, skin tone is 60% brighter
        Color countryCol = countryTone.color;
        uniformTone = new Material(countryTone);
        uniformTone.color = new Color(countryCol.r * 1.3f, countryCol.g * 1.3f, countryCol.b * 1.3f);
        skinTone = new Material(countryTone);
        skinTone.color = new Color(countryCol.r * 1.6f, countryCol.g * 1.6f, countryCol.b * 1.6f);

        armyContainer = new GameObject();
    }

    public Material[] getTones()
    {
        return new Material[] {countryTone, uniformTone, skinTone};
    }

    public UnitPiece spawnUnitPiece(UnitPiece template, HexInfo spawnTile)
    {
        if (spawnTile.unit == null) //do not spawn the piece if something is already on the tile.
        {
            UnitPiece piece = Instantiate(template);
            piece.setCurrTile(spawnTile);
            piece.transform.parent = armyContainer.transform;
            return piece;
        }
        else
        {
            return null;
        }

    }

    private void assignMaterials(UnitPiece unit, Faction f)
    {
        Material[] found = f.getTones();
        //unit.countryTone = found[0];
        uniformTone = found[1];
        skinTone = found[2];
    }
}
