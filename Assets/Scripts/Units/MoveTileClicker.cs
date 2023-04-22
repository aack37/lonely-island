using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveTileClicker : MonoBehaviour
{
    public static event Action<HexInfo> unitMoved;

    public HexInfo hexInfo;

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            unitMoved?.Invoke(hexInfo);
        }
    }
}
