using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//this script handles when a tile (hexTile) is actually clicked.
public class TileClicker : MonoBehaviour
{
    public static event Action<HexTile> tileHover;
    public static event Action<HexTile> tileSelected;
    public static event Action<HexInfo> unitSelected;
    public static event Action<HexInfo> unitMoved;

    private HexTile hexTile;

    private void Start()
    {
        hexTile = transform.parent.GetComponent<HexTile>();
    }

    private void OnMouseDown()
    {
        tileSelected?.Invoke(hexTile);
        unitSelected?.Invoke(hexTile.hexInfo);
    }

    private void OnMouseEnter()
    {
        //int offset = 0; if (hexInfo.GetX() % 2 == 0) offset = 2;
        //selectionBeam.transform.position = new Vector3(hexInfo.GetX() * 3.5f,0,hexInfo.GetY() * 4 + offset);
        tileHover?.Invoke(hexTile);
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1) && hexTile.hexInfo.withinRangeOfSelected)
        {
            unitMoved?.Invoke(hexTile.hexInfo);
        }
    }
}
