using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrthoCam : MonoBehaviour
{
    [SerializeField]
    private float xrotation;
    [SerializeField]
    private float yrotation;

    private float moveSpeed = 1f;
    public float sensitivity = 1;
    [SerializeField]
    bool strategicView = false;

    float minZoom = 10;
    float maxZoom = 50;
    float zoomFactor = 5;
    float currZoom = 40;
    private Camera cam;

    public TerrainGen terraGen;
    private Vector3 centerOfRotation;

    

    void Start()
    {
        //find center of the grid to base rotation around.
        int[] dims = terraGen.gridDimensions();
        float xCenter = (dims[0] * 3.5f / 2.0f); float zCenter = (dims[1] * 4 / 2.0f);
        centerOfRotation = new Vector3(xCenter, 1, zCenter);

        transform.parent.position = centerOfRotation;
        //transform.localPosition = new Vector3(0, 20, currZoom); //for the "in Place" camera

        cam = GetComponent<Camera>();
        cam.orthographicSize = minZoom;

        xrotation = 0; yrotation = 40;
    }

    float accumTime = 0; bool panup = false; Quaternion destination;
    float finishTime = 0.5f;

    void Update()
    {
        //old: camera movement
        if(Input.GetKeyDown(KeyCode.T))
        {
            yrotation = 40;
            xrotation = 0;
            if(!strategicView) //strategic view ON
            {
                accumTime = 0; panup = true;
                destination = Quaternion.Euler(90, 0, 0);
                maxZoom = 100;
            }
            else //strategic view OFF
            {
                accumTime = 0; panup = true;
                destination = Quaternion.Euler(40, 0, 0);
                transform.parent.position = centerOfRotation;
                maxZoom = 50;
                currZoom = 40;
            }
            strategicView = !strategicView;
        }
        if (panup)
        {
            accumTime += Time.deltaTime;
            transform.parent.rotation = Quaternion.Euler(Mathf.Lerp(transform.parent.rotation.eulerAngles.x, 
                destination.eulerAngles.x, accumTime / finishTime), 0, 0);

            if (accumTime > finishTime) panup = false;
        }


        if (Input.GetKey(KeyCode.W))
        {
            //transform.parent.Translate(new Vector3(0, 0, moveSpeed));
            Vector3 tempPos = transform.parent.position;
            tempPos.z = tempPos.z + moveSpeed;
            transform.parent.position = tempPos;
        }

        if (Input.GetKey(KeyCode.S))
        {
            //transform.parent.Translate(new Vector3(0, 0, -1 * moveSpeed));
            Vector3 tempPos = transform.parent.position;
            tempPos.z = tempPos.z - moveSpeed;
            transform.parent.position = tempPos;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //transform.parent.Translate(new Vector3(moveSpeed, 0, 0));
            Vector3 tempPos = transform.parent.position;
            tempPos.x = tempPos.x + moveSpeed;
            transform.parent.position = tempPos;
        }

        if (Input.GetKey(KeyCode.A))
        {
            //transform.parent.Translate(new Vector3(-1 * moveSpeed, 0, 0));
            Vector3 tempPos = transform.parent.position;
            tempPos.x = tempPos.x - moveSpeed;
            transform.parent.position = tempPos;
        }

        //reset to center of rotation
        if (Input.GetKeyDown(KeyCode.R) && !strategicView)
        {
            transform.parent.SetPositionAndRotation(centerOfRotation, Quaternion.Euler(0, 0, 0));
            //transform.localPosition = new Vector3(0, 20, currZoom); //default setting, can probably change
            //transform.localRotation = Quaternion.Euler(40, 0, 0);
            xrotation = 0; yrotation = 40;
            currZoom = 20;

            cam.orthographicSize = minZoom;
        }

        //zoom in or out
        if (Input.mouseScrollDelta.y != 0 /*&& inPlace*/)
        {
            float zoomAmount = -1 * Input.mouseScrollDelta.y * zoomFactor;
            if (currZoom + zoomAmount >= minZoom && currZoom + zoomAmount <= maxZoom)
            {
                currZoom += zoomAmount;

                //float newSize = Mathf.MoveTowards(cam.orthographicSize, currZoom, 1 * Time.deltaTime);
                cam.orthographicSize = currZoom;
            }
            //Debug.Log(GetComponent<Camera>().orthographicSize);
        }

        if (Input.GetMouseButton(2) && !strategicView)
        {
            float tempY = yrotation + Input.GetAxis("Mouse Y") * sensitivity;
            
            xrotation += Input.GetAxis("Mouse X") * sensitivity;
            xrotation %= 360f;
            if (tempY <= 50.1f && tempY >= 4.9f) //allowed angles of rotation in y terms
            {
                yrotation -= Input.GetAxis("Mouse Y") * sensitivity;
                yrotation %= 360f;
            }
            yrotation = Mathf.Clamp(yrotation, 5, 50);


            transform.parent.localRotation = Quaternion.Euler(yrotation, xrotation, 0);

        }

    }

    void resetLocalRotations()
    {
        yrotation = 40; xrotation = 0;
    }
}
