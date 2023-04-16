using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MultiChoice : MonoBehaviour
{
    public Button[] choices;
    private Dictionary<Button, int> buttonMaps;
    private Image[] imageComps;
    public int currChosen;

    // Start is called before the first frame update
    void Start()
    {
        buttonMaps = new Dictionary<Button, int>();
        for(int i = 0; i < choices.Length; i++) {
            buttonMaps.Add(choices[i], i);
        }

        imageComps = new Image[choices.Length];
        for(int i = 0; i < imageComps.Length; i++)
        {
            imageComps[i] = choices[i].GetComponent<Image>();
        }

        currChosen = 0;
        choose(choices[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void choose(Button b)
    {
        int index = buttonMaps[b];

        imageComps[currChosen].color = Color.white;
        imageComps[index].color = Color.red;

        currChosen = index;
    }

    public void chooseMe(Button b)
    {
        choose(b);
    }
}
