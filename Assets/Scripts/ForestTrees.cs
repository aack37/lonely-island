using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestTrees : MonoBehaviour
{
    public static ForestTrees instance { get; private set; } //start singleton stuff

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    } //end singleton stuff

    public GameObject tempTree;
    public GameObject pineTree;
    public GameObject tropTree;

    public GameObject newTempForest()
    {
        return Instantiate(tempTree);
    }

    public GameObject newPineForest()
    {
        return Instantiate(pineTree);
    }

    public GameObject newTropForest()
    {
        return Instantiate(tropTree);
    }
}
