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
        HexTile.tileHover += OnNewHover;
        HexTile.tileSelected += OnNewSelection;
    }

    void OnNewHover(HexTile ht)
    {
        Vector3 temp = ht.transform.position;
        temp.x = temp.x - 1.75f;
        temp.y = ht.hexInfo.elevation * 2 + 0.1f;
        temp.z = temp.z + 1;
        HoverCircle.transform.position = temp;
    }

    void OnNewSelection(HexTile ht)
    {
        Vector3 temp = ht.transform.position;
        temp.x = temp.x - 1.5f;
        temp.y = 20 + ht.hexInfo.elevation * 2;
        temp.z = temp.z + 1f;
        SelectionBeam.transform.position = temp;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
