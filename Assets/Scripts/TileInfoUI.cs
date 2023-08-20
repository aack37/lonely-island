using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileInfoUI : MonoBehaviour
{
    public TextMeshProUGUI terrainStats;
    public TextMeshProUGUI elevStats;
    public TextMeshProUGUI roadStats;
    public TextMeshProUGUI riverStats;
    public TextMeshProUGUI trenchLevel;
    public TextMeshProUGUI aaLevel;
    public TextMeshProUGUI bunkerLevel;
    public TextMeshProUGUI structureStats;

    public Image[] subRiverSprites;

    public Image terrainIcon;
    public Image treeCover;
    public Sprite[] terraTypeIcons;
    public Sprite[] treeIcons;

    void Start()
    {
        transform.gameObject.SetActive(false);

        //when tile is clicked on, update the stats
        TileClicker.tileSelected += GUINewSelectedTile;
        OceanHexFinder.oceanTileSelected += GUINewSelectedTile;


        WorldMapManager.deselect += onDeselection;
    }

    //TODO: replace with sliding off screen
    void clearStatPanel()
    {
        terrainIcon.sprite = terraTypeIcons[0];
        treeCover.sprite = treeIcons[0];
        terrainStats.text = "[terrain]";
        elevStats.text = "[elev]";
        roadStats.text = "[roads]";
        riverStats.text = "[rivers]";
        trenchLevel.text = "[trench]";
        aaLevel.text = "[aa]";
        bunkerLevel.text = "[bunker]";

        structureStats.text = "[structure]";
    }

    void onDeselection()
    {
        transform.gameObject.SetActive(false);
        clearStatPanel();
    }

    void GUINewSelectedTile(HexTile ht)
    {
        GUINewSelectedTile(ht.hexInfo);
    }

    void GUINewSelectedTile(HexInfo ht)
    {
        transform.gameObject.SetActive(true);

        terrainIcon.sprite = terraTypeIcons[ht.terrain.getTerrainID()];
        terrainStats.text = ht.terrain.getTerrainName();
        treeCover.sprite = treeIcons[ht.treeCover];
        elevStats.text = ht.elevation + " m";
        roadStats.text = (ht.roads.Count != 0).ToString();
        //riverStats.text = (ht.rivers.Count).ToString();
        trenchLevel.text = "[trench]";
        aaLevel.text = "[aa]";
        bunkerLevel.text = "[bunker]";

        //river stuff
        if(ht.rivers.Count == 0)
        {
            foreach(Image i in subRiverSprites)
            {
                i.enabled = false;
            }
            riverStats.text = "NONE";
        } else
        {
            //NORTH
            if (ht.rivers.Contains(0)) subRiverSprites[0].enabled = true;
            else subRiverSprites[0].enabled = false;
            //NORTHEAST
            if (ht.rivers.Contains(1)) subRiverSprites[1].enabled = true;
            else subRiverSprites[1].enabled = false;
            //SOUTHEAST
            if (ht.rivers.Contains(2)) subRiverSprites[2].enabled = true;
            else subRiverSprites[2].enabled = false;
            //SOUTH
            if (ht.rivers.Contains(3)) subRiverSprites[3].enabled = true;
            else subRiverSprites[3].enabled = false;
            //SOUTHWEST
            if (ht.rivers.Contains(4)) subRiverSprites[4].enabled = true;
            else subRiverSprites[4].enabled = false;
            //NORTHWEST
            if (ht.rivers.Contains(5)) subRiverSprites[5].enabled = true;
            else subRiverSprites[5].enabled = false;
        }

        //structure stuff
        if (ht.structure != null)
        {
            structureStats.text = ht.structure.structName;
        } else
        {
            structureStats.text = "[structure]";
        }
    }
}
