using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*this script runs directly after TerrainGen. It populates the island world with structures and units
 * eventually, there'll be a method to the madness. but for now, everything is random
 */
public class PopGen : MonoBehaviour
{
    public UnitPiece[] unitTemplates;
    public Structure[] structTemplates;
    public GameObject moveManager;
    private HexInfo[,] hexaGrid;

    //only run the script when TerrainGen is finished (TerrainGen will wake this script up as its final action.)
    void Awake()
    {
        hexaGrid = TerrainGen.hexGrid;
        spawnStructuresRandomly();
        spawnUnitsRandomly();
        
    }

    void spawnUnitsRandomly()
    {
        for(int f = 0; f < 6; f++)
        {
            Faction fact = FactionManager.factions[f];
            for (int i = 0; i < 5; i++)
            {
                (int xC, int yC) coords = (Mathf.FloorToInt(Random.value * 10 + TerrainGen.gridWidth / 2),
                        Mathf.FloorToInt(Random.value * 10 + TerrainGen.gridWidth / 2));

                UnitPiece p = fact.spawnUnitPiece(unitTemplates[0], hexaGrid[coords.xC, coords.yC]);
            }
        }

        

        moveManager.SetActive(true);
    }

    void spawnStructuresRandomly()
    {
        GameObject container = new GameObject();
        container.name = "StructureModels";
        for (int f = 0; f < 6; f++)
        {
            Faction fact = FactionManager.factions[f];
            for (int i = 0; i < 4; i++)
            {
                (int xC, int yC) coords = (Mathf.FloorToInt(Random.Range(0, TerrainGen.gridWidth - 1)),
                        Mathf.FloorToInt(Random.Range(0, TerrainGen.gridHeight - 1)));

                HexInfo hi = hexaGrid[coords.xC, coords.yC];
                Structure s = createStructure(hi, i);
                s.transform.parent = container.transform;
            }
        }
    }

    Structure createStructure(HexInfo hi, int index)
    {
        Structure s = Structure.createNewCopy(structTemplates[index]);
        
        s.transform.position = hi.getOnTopOfCenter();
        hi.structure = s;

        return s;
    }
}
