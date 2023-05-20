using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//put this script on the flat ocean object. it finds what hex tile you WOULD be interacting with.
public class OceanHexFinder : MonoBehaviour
{
    public static event Action<HexInfo> oceanTileHover;
    public static event Action<HexInfo> oceanTileSelected;
    public static event Action<HexInfo> oceanUnitSelected;
    public static event Action<HexInfo> oceanUnitMoved;

    private void OnMouseDown()
    {
        (int xC, int yC) coords = getOceanCoordinates();
        if (coords.xC != -1)
        {
            HexInfo tile = TerrainGen.hexGrid[coords.xC, coords.yC];
            oceanTileSelected?.Invoke(tile);
            oceanUnitSelected?.Invoke(tile);
        }
    }

    /*private void OnMouseEnter()
    {
        oceanTileHover?.Invoke(null);
    }*/

    private void OnMouseOver()
    {
        (int xC, int yC) coords = getOceanCoordinates();
        if (coords.xC != -1)
        {
            HexInfo tile = TerrainGen.hexGrid[coords.xC, coords.yC];
            if (Input.GetMouseButtonDown(1))
            {
                if (tile.withinRangeOfSelected)
                {
                    oceanUnitMoved?.Invoke(tile);
                }
            }
            else
            {
                oceanTileHover?.Invoke(tile);
            }

        }
    }

    //get coordinates of the hex you would have clicked
    private (int, int) getOceanCoordinates()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPos = Vector3.zero;
        
        if(Physics.Raycast(ray, out RaycastHit hitData))
        {
            worldPos = hitData.point;
        }

        if (worldPos.x > 0 && worldPos.z > 0) //min bounds
        {
            if(worldPos.x < TerrainGen.gridWidth * 3.5 && worldPos.z < TerrainGen.gridHeight * 4) //max bounds
            {
                //then figure out which hex we are actually hovering / clicking.

                if((worldPos.x - 1) % 3.5f >= 1.5f) //rectangular section of hexagon: easy
                {
                    if((worldPos.x + 1) % 7 < 3.5f) //even column, offset UP by 2
                    {
                        return (Mathf.FloorToInt((worldPos.x + 1) / 3.5f), Mathf.CeilToInt((worldPos.z - 4) / 4));
                    } else //odd column, so no offset
                    {
                        return (Mathf.FloorToInt((worldPos.x + 1) / 3.5f), Mathf.CeilToInt((worldPos.z - 2) / 4));
                    }
                }

                else //triangular sections. harder. the slope of hexagons is currently 4/3
                {
                    float onLine = ((worldPos.x - 1) % 3.5f) * (4f / 3f); //will always be in between 0-1.5
                    (int xC, int yC) coords = (Mathf.FloorToInt((worldPos.x + 1 - 1.6f) / 3.5f),
                                                       Mathf.CeilToInt((worldPos.z - 4) / 4));

                    if ((worldPos.x + 1) % 7 <= 3.5f) //even column on left
                    {
                        if((worldPos.z - 2) % 4 < 2) //DOWN triangle
                        {
                            onLine = onLine * -1 + 2;
                            if (worldPos.z % 2 > onLine) //right case. if on left, return as is
                            {
                                coords.xC += 1; coords.yC += 1;
                            }
                        }
                        else //UP triangle
                        {
                            if (worldPos.z % 2 < onLine) //right case. if on left, return as is
                            {
                                coords.xC += 1;
                            }
                        }
                        
                    }

                    else //odd column on left
                    {
                        if (worldPos.z % 4 > 2) //UP triangle
                        {
                            if (worldPos.z % 2 < onLine) //right case. if on left, return as is
                            {
                                coords.xC += 1; coords.yC -= 1;
                            }
                            coords.yC += 1;
                        }
                        else //DOWN triangle
                        {
                            onLine = onLine * -1 + 2;
                            if (worldPos.z % 2 > onLine) //right case. if on left, return as is
                            {
                                coords.xC += 1;
                            }
                        }
                    }

                    return coords;
                }
            }
        }

        Debug.Log("you didn't click in bounds");
        return (-1, -1);
    }

}
