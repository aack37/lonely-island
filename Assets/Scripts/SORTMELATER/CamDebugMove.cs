using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamDebugMove : MonoBehaviour
{
    [SerializeField]
    private float xrotation;
    [SerializeField]
    private float yrotation;

    private float moveSpeed = 0.4f;
    public float sensitivity = 1;
    [SerializeField]
    bool inPlace = true;

    float minZoom = -20;
    float maxZoom = -100;
    float zoomFactor = -2;
    float currZoom = -50;
    private Camera cam;

    public TerrainGen terraGen;
    private Vector3 centerOfRotation;

    

    // Start is called before the first frame update
    void Start()
    {
        //find center of the grid to base rotation around.
        int[] dims = terraGen.gridDimensions();
        float xCenter = (dims[0] * 3.5f / 2) + 1.75f; float zCenter = (dims[1] * 4 / 2) + 2;
        centerOfRotation = new Vector3(xCenter, 1, zCenter);

        transform.parent.position = centerOfRotation;
        transform.localPosition = new Vector3(0, 20, currZoom); //for the "in Place" camera

        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //old: camera movement
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            if (inPlace)
            {
                inPlace = false;
                resetLocalRotations();
            }
        }

        if (Input.GetKey(KeyCode.W)) {
            transform.Translate(new Vector3(0, 0, moveSpeed));
        }

        if (Input.GetKey(KeyCode.S)) {
            transform.Translate(new Vector3(0, 0, -1 * moveSpeed));
        }

        if (Input.GetKey(KeyCode.D)) {
            transform.Translate(new Vector3(moveSpeed, 0, 0));
        }

        if (Input.GetKey(KeyCode.A)) {
            transform.Translate(new Vector3(-1 * moveSpeed, 0, 0));
        }

        //reset to center of rotation
        if (Input.GetKeyDown(KeyCode.R))
        {
            inPlace = true;
            transform.parent.SetPositionAndRotation(centerOfRotation, Quaternion.Euler(0, 0, 0));
            transform.localPosition = new Vector3(0, 20, currZoom); //default setting, can probably change
            transform.localRotation = Quaternion.Euler(40, 0, 0);
            xrotation = 0; yrotation = 0;
            currZoom = -50;

            cam.fieldOfView = 60;
        }

        //zoom in or out
        if (Input.mouseScrollDelta.y != 0 && inPlace)
        {
            float zoomAmount = Input.mouseScrollDelta.y * zoomFactor;
            if (currZoom + zoomAmount < minZoom && currZoom + zoomAmount > maxZoom)
            {
                currZoom += zoomAmount;
                cam.fieldOfView += zoomAmount;
                //float newSize = Mathf.MoveTowards(cam.orthographicSize, currZoom, 1 * Time.deltaTime);
                //cam.orthographicSize = newSize;
            }
            //Debug.Log(GetComponent<Camera>().orthographicSize);
        }

        if (Input.GetMouseButton(1))
        {
            float tempY = yrotation + Input.GetAxis("Mouse Y") * sensitivity;
            
            xrotation += Input.GetAxis("Mouse X") * sensitivity;
            xrotation %= 360f;
            if (tempY < 20 && tempY > -50)
            {
                yrotation += Input.GetAxis("Mouse Y") * sensitivity;
                yrotation %= 360f;
            }
            //float yTemp = Input.GetAxis("Mouse Y") * sensitivity;
            //if (yrotation + yTemp <= 30 && yrotation + yTemp >= -90) { yrotation += yTemp; }
            //yrotation += Input.GetAxis("Mouse Y") * sensitivity;
            //transform.parent.localRotation = Quaternion.AngleAxis(xrotation, Vector3.up);
            if (inPlace)
            {
                transform.parent.localRotation = Quaternion.Euler(-yrotation, xrotation, 0);
            } else
            {
                transform.localRotation = Quaternion.Euler(-yrotation, xrotation, 0);
            }
            
        }

    }

    void resetLocalRotations()
    {
        yrotation = -40; xrotation = 0;
    }
}
