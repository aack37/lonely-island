using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//used in generating actual roads, and the arrows used in displaying pathfinding
public class RoadGenerator : MonoBehaviour
{
    public GameObject[] roadModels;
    public GameObject[] climbers;
    private static float theta = 60.2551f;
    private static float aDiag = 53.13f;

    //generate road given a path.
    public GameObject generateRoadPath(List<HexInfo> pathToTake)
    {
        GameObject container = new GameObject();

        /*GameObject road0 = addRoad(pathToTake[0], pathToTake[1]); //get path from this tile to the next
        road0.transform.position = pathToTake[0].getOnTopOfCenter();
        road0.transform.parent = container.transform;*/

        for (int i = 1; i < pathToTake.Count - 1; i++)
        {
            HexInfo currHex = pathToTake[i - 1];
            HexInfo nextHex = pathToTake[i];
            GameObject road = addRoad(currHex, nextHex); //get path from this tile to the next
            road.transform.position = currHex.getOnTopOfCenter();
            road.transform.parent = container.transform;
            GameObject climb = climberSection(road, currHex, nextHex); //add vertical section to the road, if elevation changed from previous tile to this one
            if(climb != null)
            {
                climb.transform.parent = container.transform;
            }
        }

        return container;
    }

    //adds a vertical section to the road
    private GameObject climberSection(GameObject container, HexInfo currHex, HexInfo nextHex)
    {
        float change = currHex.GetElevWTopsoil() - nextHex.GetElevWTopsoil();
        if (change != 0)
        {
            if (change < 0)
            {
                return climberSection(container, nextHex, currHex);
            }
            int neighborIndex = findIndexInNeighbors(currHex, nextHex);

            int chosenIndex = neighborIndex % 3; //that worked out nicely

            GameObject climber = Instantiate(climbers[chosenIndex]);
            climber.transform.position = currHex.getOnTopOfCenter();

            //set climber scale
            Vector3 sc = climber.transform.localScale;
            sc.y = change;
            climber.transform.localScale = sc;

            //set climber rotation
            Quaternion rot = new Quaternion();
            switch (chosenIndex)
            {
                case 0: //STRAIGHT
                    if (neighborIndex == 0) { rot = Quaternion.Euler(0, 0, 180); } //north
                    else { rot = Quaternion.Euler(0, 180, 180); } //south
                    break;
                case 1:
                    if (neighborIndex == 1) { rot = Quaternion.Euler(0, -1 * (180 - aDiag), 180); } //
                    else { rot = Quaternion.Euler(0, aDiag, 180); } //
                    break;
                case 2:
                    if (neighborIndex == 2) { rot = Quaternion.Euler(0, -1 * aDiag, 180); } //
                    else { rot = Quaternion.Euler(0, (180 - aDiag), 180); } //
                    break;
            }
            climber.transform.rotation = rot;

            return climber;
        }
        else return null;
    }

    //adds a road on "target" tile, leading towards "dest" tile. assume the two tiles are neighbors.
    private GameObject addRoad(HexInfo target, HexInfo dest)
    {
        int indexInNeighbors = findIndexInNeighbors(target, dest);
        target.roads.Add(indexInNeighbors);
        dest.roads.Add(findIndexInNeighbors(dest, target));

        return getNewRoadModel(target);
    }

    //find the index in target's neighbors list that dest resides in.
    //this tells us which direction dest is, in relation to target.
    private int findIndexInNeighbors(HexInfo target, HexInfo dest)
    {
        for (int i = 0; i < 6; i++)
        {
            if (dest == target.GetNeighbors()[i])
            {
                return i;
            }
        }
        Debug.Log("Couldn't find neighbor in road building");
        return -1;
    }

    private bool hasRoadAt(HexInfo tile, int dest)
    {
        return tile.roads.Contains(dest);
    }

    private bool hasRoadAt(HexInfo tile, params int[] dests)
    {
        foreach (int i in dests)
        {
            if (!tile.roads.Contains(i)) return false;
        }
        return true;
    }

    //choose the "road model" for this tile based off its road data.
    private GameObject getNewRoadModel(HexInfo target)
    {
        int numRoads = target.roads.Count;
        int chosenModel = -1; //the index of the road model we decide on
        float xOrient = 0; //the Xrotation of the model
        float zOrient = 0; //the Zrotation of the model

        //the yandere dev way of coding (i seriously don't think there is a better option for this)
        switch (numRoads)
        {
            case 0:
                return null;

            //1-PIECERS
            case 1:
                if (hasRoadAt(target, 0)) { //North
                    chosenModel = 0;
                    xOrient = -90;
                    zOrient = 0;
                }
                else if (hasRoadAt(target, 1)) //NorthEast
                {
                    chosenModel = 1;
                    xOrient = 90;
                    zOrient = 180 - theta;
                }
                else if (hasRoadAt(target, 2)) //SouthEast
                {
                    chosenModel = 1;
                    xOrient = -90;
                    zOrient = 180 - theta;
                }
                else if (hasRoadAt(target, 3)) //South
                {
                    chosenModel = 0;
                    xOrient = -90;
                    zOrient = 180;
                }
                else if (hasRoadAt(target, 4)) //SouthWest
                {
                    chosenModel = 1;
                    xOrient = 90;
                    zOrient = -1 * theta;
                }
                else if (hasRoadAt(target, 5)) //NorthWest
                {
                    chosenModel = 1;
                    xOrient = -90;
                    zOrient = -1 * theta;
                }
                break;


            //2-PIECERS
            case 2:
                if (hasRoadAt(target, 3, 2))
                {
                    chosenModel = 5;
                    xOrient = -90;
                    zOrient = -1 * theta;
                }
                else if (hasRoadAt(target, 3, 1))
                {
                    chosenModel = 4;
                    xOrient = 90;
                    zOrient = 180;
                }
                else if (hasRoadAt(target, 3, 0))
                {
                    chosenModel = 2;
                    xOrient = -90;
                    zOrient = 0;
                }
                else if (hasRoadAt(target, 3, 5))
                {
                    chosenModel = 4;
                    xOrient = -90;
                    zOrient = 0;
                }
                else if (hasRoadAt(target, 3, 4))
                {
                    chosenModel = 5;
                    xOrient = 90;
                    zOrient = 180 - theta;
                }
                else if (hasRoadAt(target, 2, 1))
                {
                    chosenModel = 6;
                    xOrient = -90;
                    zOrient = -1 * (180 - theta);
                }
                else if (hasRoadAt(target, 2, 0))
                {
                    chosenModel = 4;
                    xOrient = -90;
                    zOrient = 180;
                }
                else if (hasRoadAt(target, 2, 5))
                {
                    chosenModel = 3;
                    xOrient = -90;
                    zOrient = -1 * theta;
                }
                else if (hasRoadAt(target, 2, 4))
                {
                    chosenModel = 7;
                    xOrient = -90;
                    zOrient = theta;
                }
                else if (hasRoadAt(target, 1, 0))
                {
                    chosenModel = 5;
                    xOrient = 90;
                    zOrient = -1 * theta;
                }
                else if (hasRoadAt(target, 1, 5))
                {
                    chosenModel = 7;
                    xOrient = -90;
                    zOrient = -1 * (180 - theta);
                }
                else if (hasRoadAt(target, 1, 4))
                {
                    chosenModel = 3;
                    xOrient = 90;
                    zOrient = -1 * theta;
                }
                else if (hasRoadAt(target, 0, 5))
                {
                    chosenModel = 5;
                    xOrient = -90;
                    zOrient = 180 - theta;
                }
                else if (hasRoadAt(target, 0, 4))
                {
                    chosenModel = 4;
                    xOrient = 90;
                    zOrient = 0;
                }
                else if (hasRoadAt(target, 5, 4))
                {
                    chosenModel = 6;
                    xOrient = -90;
                    zOrient = theta;
                }
                else
                {
                    Debug.Log("No suitable model found for tile " + target /*+ ". Its roads are: " + dummy(target.roads)*/);
                }
                break;
            /*case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;*/
            default:
                Debug.Log("Road gen inconclusive, found " + numRoads + " roads at tile " + target);
                return null;
        }

        GameObject newRoad = Instantiate(roadModels[chosenModel]);
        newRoad.transform.rotation = Quaternion.Euler(xOrient, 0, zOrient);
        return newRoad;
    }

    /*string dummy(HashSet<int> x)
    {
        string str = "";
        foreach(int i in x)
        {
            str = str + i + ",";
        }
        return str;
    }*/
}
