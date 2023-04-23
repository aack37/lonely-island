using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*this script runs directly after TerrainGen. It populates the island world with structures and units
 * eventually, there'll be a method to the madness. but for now, everything is random
 */
public class PopGen : MonoBehaviour
{
    public UnitPiece unitTemplates;
    public GameObject moveManager;
    private HexInfo[,] hexaGrid;

    //only run the script when TerrainGen is finished (TerrainGen will wake this script up as its final action.)
    void Awake()
    {
        hexaGrid = TerrainGen.hexGrid;
        spawnUnitsRandomly();
    }

    void spawnUnitsRandomly()
    {
        Faction defender = FactionManager.factions[0];
        Faction attacker = FactionManager.factions[1];
        for (int i = 0; i < 20; i++)
        {
            (int xC, int yC) coords = (Mathf.FloorToInt(Random.value * 10 + TerrainGen.gridWidth / 2),
                    Mathf.FloorToInt(Random.value * 10 + TerrainGen.gridWidth / 2));

            UnitPiece p = defender.spawnUnitPiece(unitTemplates, hexaGrid[coords.xC, coords.yC]);
        }

        for (int i = 0; i < 10; i++)
        {
            (int xC, int yC) coords = (Mathf.FloorToInt(Random.value * 10 + TerrainGen.gridWidth / 2),
                    Mathf.FloorToInt(Random.value * 10 + TerrainGen.gridWidth / 2));

            UnitPiece p = attacker.spawnUnitPiece(unitTemplates, hexaGrid[coords.xC, coords.yC]);
        }

        moveManager.SetActive(true);
    }

}
