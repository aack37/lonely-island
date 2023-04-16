using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateRandomWorld : MonoBehaviour
{

    public void iWasClicked()
    {
        TGenSettings.worldSeed = Mathf.FloorToInt(Random.Range(0, 999999));
        SceneManager.LoadScene(1);
    }
}
