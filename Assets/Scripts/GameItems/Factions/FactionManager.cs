using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public Material templateMat;
    public static List<Faction> factions;

    // Start is called before the first frame update
    void Start()
    {
        //0 is the defender. the other 6 slots are for attackers
        factions = new List<Faction>();

        //TODO: REMOVE ALL THIS, IT'S JUST TEMPORARY STUFF
        Color[] colsCo = { Color.blue, Color.red, Color.yellow, Color.green, Color.gray, Color.magenta };
        List<Color> cols = new List<Color>(colsCo);
        for(int i = 0; i < cols.Count; i++)
        {
            Material m = new Material(templateMat);
            m.color = cols[i];
            factions.Add(new Faction("Team " + i, m));
        }
        
    }
}
