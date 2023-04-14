using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTypesSingletons : MonoBehaviour
{
    //this class instantiates the singletons that represent terrain types.
    public int numTerrainTypes = 12; // UPDATE AS YOU ADD MORE TERRAIN TYPES
    private List<TerrainType> terrainSingletons;

    // Start is called before the first frame update
    void Start()
    {
        terrainSingletons = new List<TerrainType>();
        terrainSingletons.Add(new RTerrainUndefined1());
        terrainSingletons.Add(new RTerrainUndefined2());
        terrainSingletons.Add(new RTerrainOcean());
        terrainSingletons.Add(new RTerrainGrassland());
        terrainSingletons.Add(new RTerrainBeach());
        terrainSingletons.Add(new RTerrainLake());
        terrainSingletons.Add(new RTerrainMountain());
        terrainSingletons.Add(new RTerrainSnow());
        terrainSingletons.Add(new RTerrainSwamp()); 
        terrainSingletons.Add(new RTerrainTForest()); //TODO: remove generic forest later
        terrainSingletons.Add(new RTerrainTForest());
        terrainSingletons.Add(new RTerrainTForest());
        terrainSingletons.Add(new RTerrainPlain());
    }

    public TerrainType getTerrain(int code)
    {
        return terrainSingletons[code];
    }

    private int convertStrToCode(string inp)
    {
        int code = -1;
        switch (inp)
        {
            case "UNDF1": code = 0; break;
            case "UNDF2": code = 1; break;
            case "OCEAN": code = 2; break;
            case "GRASS": code = 3; break;
            case "BEACH": code = 4; break;
            case "LAKE": code = 5; break;
            case "MOUNT": code = 6; break;
            case "SNOW": code = 7; break;
            case "SWAMP": code = 8; break;
            case "TFOREST": code = 9; break;
            case "PFOREST": code = 10; break;
            case "RFOREST": code = 11; break;
            case "PLAIN": code = 12; break;
            default: code = -1; Debug.Log("Something went wrong"); break;
        }
        return code;
    }

    public TerrainType getTerrain(string inp)
    {
        int code = convertStrToCode(inp);
        return getTerrain(code);
    }

    private int findCode(TerrainType t)
    {
        return terrainSingletons.IndexOf(t);
    }

    public bool isUndefined(TerrainType t)
    {
        int code = findCode(t);
        return code <= 1;
    }

    public bool isInLakeHunting(TerrainType t)
    {
        int code = findCode(t);
        return code <= 2;
    }

}
