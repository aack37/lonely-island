using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public string structName;

    //spawn a new copy of the tempate into the world. other methods deal with the specifics
    public static Structure createNewCopy(Structure template)
    {
        return Instantiate(template);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
