using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GenTailoredWorld : MonoBehaviour
{
    public Image initial;
    public Image settings;
    public TMP_InputField seedInput;
    public TMP_InputField elevInput;
    public MultiChoice mapSizePicker;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void goToTerrainGenSettings()
    {
        initial.gameObject.SetActive(false);
        settings.gameObject.SetActive(true);
    }

    public void startGeneration()
    {
        if (!TGenSettings.verifyValidSeed(seedInput.text))
        {
            Debug.Log("Bad seed input, using 0 instead");
        } if (!TGenSettings.verifyElevation(elevInput.text))
        {
            Debug.Log("Bad elevation input, using 0.5 instead");
        }
        TGenSettings.setMapSize(mapSizePicker.currChosen);
        SceneManager.LoadScene(1);
    }
}
