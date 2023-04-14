using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

//the HexTile script is used for physical interactions with the tile.
//for instance: clicking on it
public class HexTile : MonoBehaviour
{
    public HexInfo hexInfo;
    public static event Action<HexTile> tileHover;
    public static event Action<HexTile> tileSelected;

    Material mat;

    private void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        //TerrainGen.regenerateWorld += onRegen;
    }

    private void OnMouseDown()
    {
        tileSelected?.Invoke(this);
    }

    private void OnMouseEnter()
    {
        //int offset = 0; if (hexInfo.GetX() % 2 == 0) offset = 2;
        //selectionBeam.transform.position = new Vector3(hexInfo.GetX() * 3.5f,0,hexInfo.GetY() * 4 + offset);
        tileHover?.Invoke(this);
    }

    public async Task readyToGo() //for async builds. build all tiles in the map as simultaneously as possible
    {
        await Task.Yield();
        if (hexInfo == null) throw new Exception("The tile stats were not yet specified");
        else
        {
            this.name = hexInfo.ToString();

            hexInfo.elevation = TerrainGen.getStep(hexInfo.elevation); //step elevation

            //if the elevation happens to be high enough, it can be a mountain or even a snowy mountain
            if (hexInfo.elevation >= 7)
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
            mat.color = hexInfo.terrain.getColor();

            //land height determined by elevationStep (sea level is always 0.1)
            if (hexInfo.terrain.getTerrainID() != 2)
            {
                transform.localScale = new Vector3(1, 0.75f, hexInfo.elevation);
            }

            //set position, now that we have elevation figured out!
            int zStarter = 0; if (hexInfo.GetX() % 2 == 0) zStarter = 2;
            transform.position = new Vector3(hexInfo.GetX() * 3.5f, hexInfo.elevation, zStarter + hexInfo.GetY() * 4);

            //the last step is making trees, if applicable.
            if (hexInfo.treeCover)
            {
                GameObject newTrees;
                //if (hexInfo.terrain.getTerrainID() == 8) { newTrees = ForestTrees.instance.newTropForest(); }
                if (hexInfo.elevation > 3f) { newTrees = ForestTrees.instance.newPineForest(); }
                else { newTrees = ForestTrees.instance.newTempForest(); }

                //OPTIMIZE: perhaps no need to use getReals?
                newTrees.transform.position = new Vector3(hexInfo.GetRealX() - 1.75f, (hexInfo.elevation * 2f) + 0.5f, hexInfo.GetRealY() + 1f);
                newTrees.transform.parent = transform;
            }

            await Task.Yield();
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
