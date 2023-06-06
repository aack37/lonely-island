using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    public static HexInfo selectedTile = null;
    public static HexInfo hoveringTile = null;

    public GameObject SelectionBeam;
    public GameObject HoverCircle;

    public static event Action deselect; //when a tile is clicked twice

    // Start is called before the first frame update
    void Start()
    {
        TileClicker.tileHover += OnNewHover;
        TileClicker.tileSelected += OnNewSelection;
        OceanHexFinder.oceanTileSelected += OnNewOceanSelection;
        OceanHexFinder.oceanTileHover += OnNewOceanHover;

        deselect += resetSelectionStuff;
    }

    public static void deselectTile()
    {
        deselect.Invoke();
    }

    //if it looks stupid to have 2 methods structured this way, just remember, no one asked
    void resetSelectionStuff()
    {
        selectedTile = null;
        SelectionBeam.transform.position = Vector3.zero;
    }

    void OnNewHover(HexTile ht)
    {
        hoveringTile = ht.hexInfo;
        Vector3 temp = ht.transform.position;
        temp.y = ht.hexInfo.elevation * 2 + 0.1f;
        HoverCircle.transform.position = temp;
    }

    void OnNewSelection(HexTile ht)
    {
        if(selectedTile != ht.hexInfo)
        {
            selectedTile = ht.hexInfo;
            Vector3 temp = ht.transform.position;
            temp.y = 20 + ht.hexInfo.elevation * 2;
            SelectionBeam.transform.position = temp;
        } else //double selection, so deselect it.
        {
            deselectTile();
        }

    }

    void OnNewOceanSelection(HexInfo hi)
    {
        if(selectedTile != hi)
        {
            selectedTile = hi;
            Vector3 temp = new Vector3(hi.GetRealX(), 10f, hi.GetRealY());
            SelectionBeam.transform.position = temp;
        }
        else //double selection, so deselect it.
        {
            deselectTile();
        }
        
    }

    void OnNewOceanHover(HexInfo hi)
    {
        hoveringTile = hi;
        Vector3 temp = hi.getOnTopOfCenter();
        HoverCircle.transform.position = temp;
        
    }

}
