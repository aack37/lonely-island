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
    public Checkbox featureNamesEnabled;

    public TMP_InputField mtnHeight;
    public TMP_InputField snowHeight;
    public TMP_InputField pineLine;

    public Checkbox[] enabledBiomes;
    public Slider forestDensity;

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
        //seed and elevation inputs
        if (!TGenSettings.verifyValidSeed(seedInput.text))
        {
            Debug.Log("Bad seed input, using 0 instead");
        } if (!TGenSettings.verifyElevation(elevInput.text))
        {
            Debug.Log("Bad elevation input, using 0.5 instead");
        } if (!TGenSettings.verifyMtnSnowHeight(mtnHeight.text, snowHeight.text))
        {
            Debug.Log("Bad input for mountain/snow height, using defaults");
        } if (!TGenSettings.verifyPineLine(pineLine.text))
        {
            Debug.Log("Using 3.0f for the pine line");
        }

        //world size, feature options
        TGenSettings.setMapSize(mapSizePicker.currChosen);
        TGenSettings.featureNamesOn = featureNamesEnabled.getStatus();
        for(int i = 0; i < enabledBiomes.Length; i++)
        {
            TGenSettings.enabledBiomes[i] = enabledBiomes[i].getStatus();
        }
        TGenSettings.forestDensity = forestDensity.value;

        SceneManager.LoadScene(1);
    }
}
