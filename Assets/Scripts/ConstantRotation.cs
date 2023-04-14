using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public float speedInSeconds = 2;
    private float zAngle = 0;
    private float accumulatedTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        accumulatedTime += Time.deltaTime % speedInSeconds;
        zAngle = (accumulatedTime / speedInSeconds) * 360f % 360f;
        transform.rotation = Quaternion.Euler(0, zAngle, 0);
        //transform.localRotation = Quaternion.Euler(0, 0, zAngle);
        //transform.Rotate(Vector3.forward, zAngle);
        //transform.parent.Rotate(transform.position, zAngle);
        //transform.rotation = Quaternion.AngleAxis(zAngle, Vector3.up);
    }
}
