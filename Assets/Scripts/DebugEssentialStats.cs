using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugEssentialStats : MonoBehaviour
{
    public TerrainGen terraGen;

    public TextMeshProUGUI seed;
    public TextMeshProUGUI hover;
    public TextMeshProUGUI select;
    public TextMeshProUGUI selectedFullStats;
    
    void Awake()
    {
        seed.text = "World Seed: " + terraGen.getSeed();
        hover.text = "Hovering: " + "NONE";
        select.text = "Selected: " + "NONE";

        TerrainGen.regenerateWorld += GUINewWorld;
        HexTile.tileHover += GUINewHoverTile;
        HexTile.tileSelected += GUINewSelectedTile;
        HexTile.tileSelected += DispSelectedTileStats;
    }

    void GUINewWorld(int newWorldSeed)
    {
        seed.text = "World Seed: " + newWorldSeed;
    }

    void GUINewHoverTile(HexTile ht)
    {
        hover.text = "Hovering: " + ht.hexInfo;
    }

    void GUINewSelectedTile(HexTile ht)
    {
        select.text = "Selected: " + ht.hexInfo;
    }

    void DispSelectedTileStats(HexTile ht)
    {
        string tempStr = ""; HexInfo hs = ht.hexInfo;
        tempStr = tempStr + "SELECTED ---> " + hs + "\n";
        tempStr = tempStr + "Terrain: " + hs.terrain + "\n";
        tempStr = tempStr + "Tile seed: " + hs.getTileSeed() + "\n";
        tempStr = tempStr + "Tree Cover: " + hs.treeCover + "\n";
        tempStr = tempStr + "Elevation: " + hs.elevation + "\n";
        tempStr = tempStr + "Ocean Coastal: " + hs.isOceanCoastal + "\n";
        tempStr = tempStr + "Lake Coastal: " + hs.isLakeCoastal + "\n";
        tempStr = tempStr + "Natural Features: ";
        foreach(int feat in hs.inNaturalFeatures)
        {
            tempStr = tempStr + feat + ",";
        }
        selectedFullStats.text = tempStr;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
