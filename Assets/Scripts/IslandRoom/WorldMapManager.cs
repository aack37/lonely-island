using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    public HexTile selectedTile;
    public HexTile hoveringTile;

    public GameObject SelectionBeam;
    public GameObject HoverCircle;

    // Start is called before the first frame update
    void Start()
    {
        TileClicker.tileHover += OnNewHover;
        TileClicker.tileSelected += OnNewSelection;
        OceanHexFinder.oceanTileSelected += OnNewOceanSelection;
        OceanHexFinder.oceanTileHover += OnNewOceanHover;
    }

    void OnNewHover(HexTile ht)
    {
        Vector3 temp = ht.transform.position;
        temp.y = ht.hexInfo.elevation * 2 + 0.1f;
        HoverCircle.transform.position = temp;
    }

    void OnNewSelection(HexTile ht)
    {
        Vector3 temp = ht.transform.position;
        //temp.x = temp.x - 1.5f;
        temp.y = 20 + ht.hexInfo.elevation * 2;
        //temp.z = temp.z + 1f;
        SelectionBeam.transform.position = temp;

    }

    void OnNewOceanSelection(HexInfo hi)
    {
        Vector3 temp = new Vector3(hi.GetRealX(), 10f, hi.GetRealY());
        SelectionBeam.transform.position = temp;
    }

    void OnNewOceanHover(HexInfo hi)
    {
        Vector3 temp = hi.getOnTopOfCenter();
        HoverCircle.transform.position = temp;
    }

}
