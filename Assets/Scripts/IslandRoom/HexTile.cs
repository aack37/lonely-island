using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

//the HexTile script is used for physical interactions with the tile.
//for instance: clicking on it
public class HexTile : MonoBehaviour
{
    public HexInfo hexInfo;

    /*with a new change, hexTiles now can have a "base" and "top", to simulate topsoil and underground.
     all hexTiles have a base by default.
     some land hexTiles which have foliage will have a "top" tile.*/
    private GameObject top;
    private GameObject bottom;

    Material topMat;
    Material bottomMat;

    private void Awake()
    {
        bottom = transform.GetChild(0).gameObject;
        top = null;

        bottomMat = bottom.GetComponent<MeshRenderer>().material;
        //TerrainGen.regenerateWorld += onRegen;
    }

    public async Task<HexInfo> readyToGo() //for async builds. build all tiles in the map as simultaneously as possible
    {
        await Task.Yield();
        if (hexInfo == null) throw new Exception("The tile stats were not yet specified");
        else
        {
            this.name = hexInfo.ToString();

            hexInfo.elevation = TerrainGen.getStep(hexInfo.elevation); //step elevation

            //if the elevation happens to be high enough, it can be a (snowy) mountain, or cliff depending on coastal status
            if (hexInfo.terrain.getTerrainID() == 4)
            {
                if(hexInfo.elevation >= 1.5f)
                {
                    hexInfo.terrain = TerrainGen.singles.getTerrain("CLIFF");
                }
            }
            else if (hexInfo.terrain.getTerrainID() != 5 && hexInfo.elevation >= TGenSettings.mtnHeight) //overwrite anything but lakes
            {
                if (hexInfo.elevation >= TGenSettings.snowHeight)
                {
                    hexInfo.terrain = TerrainGen.singles.getTerrain("SNOW");
                }
                else
                {
                    hexInfo.terrain = TerrainGen.singles.getTerrain("MOUNT");
                }
            }

            //land height determined by elevationStep (sea level is always 0.1)
            if (hexInfo.terrain.getTerrainID() != 2)
            {
                bottom.transform.localScale = new Vector3(1, 0.75f, hexInfo.elevation);
            }

            //set position, now that we have elevation figured out!
            transform.position = new Vector3(hexInfo.GetRealX(), hexInfo.elevation, hexInfo.GetRealY());
            //int zStarter = 0; if (hexInfo.GetX() % 2 == 0) zStarter = 2;
            //transform.position = new Vector3(hexInfo.GetX() * 3.5f, hexInfo.elevation, zStarter + hexInfo.GetY() * 4);

            //give topsoil, if applicable
            if (TerrainTypesSingletons.hasTopsoil(hexInfo.terrain)) {
                bottomMat.color = getBottomSoil(hexInfo.terrain);

                top = Instantiate(bottom, transform); //instantiate as a copy of bottom under same parent
                top.transform.localScale = new Vector3(1, 0.75f, 0.1f);

                //local pos
                Vector3 temp = top.transform.localPosition;
                temp.y = hexInfo.elevation;
                top.transform.localPosition = temp;

                topMat = top.GetComponent<MeshRenderer>().material;
                topMat.color = hexInfo.terrain.getColor(); //change material
            } else
            {
                bottomMat.color = hexInfo.terrain.getColor();
            }



            //adjust river height
            foreach (int riv in hexInfo.rivers)
            {
                float neighElev = TerrainGen.getStep(hexInfo.GetNeighbors()[riv].elevation);
                float diff = hexInfo.elevation - neighElev;
                Vector3 pos = Vector3.zero;

                //first, must find the actual correct river object
                string lookForRiv = "";
                switch(riv)
                {
                    case 0: lookForRiv = "N"; break;
                    case 1: lookForRiv = "NE"; break;
                    case 2: lookForRiv = "SE"; break;
                    case 3: lookForRiv = "S"; break;
                    case 4: lookForRiv = "SW"; break;
                    case 5: lookForRiv = "NW"; break;
                }

                int r = 0;
                for(r = 1; r < transform.childCount; r++)
                {
                    string temp = transform.GetChild(r).name;
                    int mindex = Mathf.Min(2, temp.Length);
                    temp = temp.Substring(0, mindex);
                    if (temp.Length > 1 && (temp[1] < 65 || temp[1] > 90)) temp = temp.Substring(0,1);

                    if (temp == lookForRiv)
                    {
                        //with it found, now you can change it
                        pos = transform.GetChild(r).localPosition;
                        if (diff > 0)
                        {
                            pos.y -= diff;
                        }
                        else
                        {
                            pos.y = 0;
                        }
                        break;
                    }
                }
                if (r == transform.childCount) Debug.Log("RIVER NAME NOT FOUND: " + lookForRiv + " at tile: " + hexInfo);
                else {
                    float minHeight = Mathf.Min(hexInfo.elevation, neighElev);
                    pos.y += minHeight / 2.0f;

                    // lastly, modify the "height" of the rivers
                    transform.GetChild(r).localPosition = pos;
                    Vector3 scaleMod = transform.GetChild(r).localScale;
                    scaleMod.z = minHeight;
                    transform.GetChild(r).localScale = scaleMod;
                }
                
            }
            /* // RIVERS DEBUG
            if (hexInfo.rivers.Count > 0)
            {
                if(topMat != null) topMat.color = Color.red;
                bottomMat.color = Color.red;
            }*/

            //the last step is making trees, if applicable.
            if (hexInfo.treeCover != 0)
            {
                GameObject newTrees;
                //trees no longer generate on mountains
                //if (hexInfo.elevation < TGenSettings.mtnHeight)
                //{
                    if (hexInfo.elevation > TGenSettings.pineLine)
                    {
                        hexInfo.treeCover = 2; //set to pine forest if above the pine line
                        newTrees = ForestTrees.instance.newPineForest();
                        newTrees.transform.position = new Vector3(hexInfo.GetRealX(), (hexInfo.elevation * 2f) + 0.5f, hexInfo.GetRealY());
                    }
                    else
                    {
                        newTrees = ForestTrees.instance.newTempForest();
                        newTrees.transform.position = new Vector3(hexInfo.GetRealX() + 0.5f, (hexInfo.elevation * 2f) + 0.5f,
                            hexInfo.GetRealY() - 1f);
                    }

                    //OPTIMIZE: perhaps no need to use getReals?
                    newTrees.transform.parent = transform;
                //}
            }

            await Task.Yield();
            return hexInfo;
        }
    }

    Color getBottomSoil(TerrainType topSoil)
    {
        int code = topSoil.getTerrainID();
        switch (code)
        {
            case 3: return new Color(0.31f, 0.27f, 0.23f); //grassland = light brown
            case 7: return new Color(0.5f, 0.5f, 0.5f); //snow = mountain
            case 8: return new Color(0.23f, 0.22f, 0.20f); //swamp = dark brown
            case 12: return new Color(0.65f, 0.56f, 0.44f); //plain = tinted brown
            default: return new Color(0.2f, 0.2f, 0.2f); //grayscale
        }
    }

    void onRegen(int _dontcare)
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

}
