using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Checkbox : MonoBehaviour
{
    public Button checkboxButton;
    private bool isChecked;
    public bool defaultSetting = false;
    private Image buttonImage;

    // Start is called before the first frame update
    void Start()
    {
        isChecked = defaultSetting;

        buttonImage = checkboxButton.GetComponent<Image>();

        if (isChecked)
        {
            buttonImage.color = Color.red;
        }
    }

    public void checkMe()
    {
        isChecked = !isChecked;
        if (isChecked)
        {
            buttonImage.color = Color.red;
        }
        else
        {
            buttonImage.color = Color.white;
        }
    }

    public bool getStatus()
    {
        return isChecked;
    }
}
