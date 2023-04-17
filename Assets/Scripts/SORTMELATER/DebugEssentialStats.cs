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
        seed.text = "World Seed: " + TGenSettings.worldSeed;
        hover.text = "Hovering: " + "NONE";
        select.text = "Selected: " + "NONE";

        TerrainGen.regenerateWorld += GUINewWorld;
        TileClicker.tileHover += GUINewHoverTile;
        TileClicker.tileSelected += GUINewSelectedTile;
        TileClicker.tileSelected += DispSelectedTileStats;
        OceanHexFinder.oceanTileSelected += GUINewSelectedTile;
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

    void GUINewSelectedTile(HexInfo ht)
    {
        select.text = "Selected: " + ht;
    }

    void DispSelectedTileStats(HexTile ht)
    {
        string tempStr = ""; HexInfo hs = ht.hexInfo;
        tempStr = tempStr + "SELECTED ---> " + hs + "\n";
        tempStr = tempStr + "Terrain: " + hs.terrain + "\n";
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
