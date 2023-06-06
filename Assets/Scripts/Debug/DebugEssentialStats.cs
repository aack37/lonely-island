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

        //top left GUI - new world seed
        TerrainGen.regenerateWorld += GUINewWorld;

        //top left GUI - new tile hovered or selected
        TileClicker.tileHover += GUINewHoverTile;
        TileClicker.tileSelected += GUINewSelectedTile;
        OceanHexFinder.oceanTileSelected += GUINewSelectedTile;
        OceanHexFinder.oceanTileHover += GUINewHoverTile;

        //bottom left GUI - display tile information
        TileClicker.tileSelected += DispSelectedTileStats;
        OceanHexFinder.oceanTileSelected += DispSelectedTileStats;
        WorldMapManager.deselect += onDeselection;
    }

    void onDeselection()
    {
        select.text = "Selected: NONE";
        selectedFullStats.text = "No tile selected";
    }

    void GUINewWorld(int newWorldSeed)
    {
        seed.text = "World Seed: " + newWorldSeed;
    }

    void GUINewHoverTile(HexTile ht)
    {
        hover.text = "Hovering: " + ht.hexInfo;
    }

    void GUINewHoverTile(HexInfo ht)
    {
        hover.text = "Hovering: " + ht;
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
        DispSelectedTileStats(ht.hexInfo);
    }

    void DispSelectedTileStats(HexInfo hs)
    {
        string tempStr = "";
        tempStr = tempStr + "SELECTED ---> " + hs + "\n";
        tempStr = tempStr + "Terrain: " + hs.terrain + "\n";
        tempStr = tempStr + "Tree Cover: " + hs.treeCover + "\n";
        tempStr = tempStr + "Elevation: " + hs.elevation + "\n";
        tempStr = tempStr + "Ocean Coastal: " + hs.isOceanCoastal + "\n";
        tempStr = tempStr + "Lake Coastal: " + hs.isLakeCoastal + "\n";
        tempStr = tempStr + "Natural Features: ";
        foreach (int feat in hs.inNaturalFeatures)
        {
            tempStr = tempStr + feat + ",";
        }
        tempStr = tempStr + "\n" + "Unit: " + hs.unit;
        selectedFullStats.text = tempStr;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
