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

    public async Task readyToGo() //for async builds. build all tiles in the map as simultaneously as possible
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
            else if (hexInfo.elevation >= 7)
            {
                if (hexInfo.elevation >= 10)
                {
                    hexInfo.terrain = TerrainGen.singles.getTerrain("SNOW");
                }
                else
                {
                    hexInfo.terrain = TerrainGen.singles.getTerrain("MOUNT");
                }
            }

            //if(ht.hexInfo.isLakeCoastal) mat.color = Color.red; //DEBUGGING
            //else mat.color = ht.hexInfo.terrain.getColor(); //DEBUGGING
            //mat.color = hexInfo.terrain.getColor();

            //land height determined by elevationStep (sea level is always 0.1)
            if (hexInfo.terrain.getTerrainID() != 2)
            {
                bottom.transform.localScale = new Vector3(1, 0.75f, hexInfo.elevation);
            }

            //set position, now that we have elevation figured out!
            int zStarter = 0; if (hexInfo.GetX() % 2 == 0) zStarter = 2;
            transform.position = new Vector3(hexInfo.GetX() * 3.5f, hexInfo.elevation, zStarter + hexInfo.GetY() * 4);

            //give topsoil, if applicable
            if (TerrainGen.singles.hasTopsoil(hexInfo.terrain)) {
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

            //the last step is making trees, if applicable.
            if (hexInfo.treeCover)
            {
                GameObject newTrees;
                //if (hexInfo.terrain.getTerrainID() == 8) { newTrees = ForestTrees.instance.newTropForest(); }
                if (hexInfo.elevation > 3f) { newTrees = ForestTrees.instance.newPineForest(); }
                else { newTrees = ForestTrees.instance.newTempForest(); }

                //OPTIMIZE: perhaps no need to use getReals?
                newTrees.transform.position = new Vector3(hexInfo.GetRealX(), (hexInfo.elevation * 2f) + 0.5f, hexInfo.GetRealY());
                newTrees.transform.parent = transform;
            }

            await Task.Yield();
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
