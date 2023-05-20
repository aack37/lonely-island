using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AStarNode : IComparable
{
    //in our pathfinding:
    float gCost; // "G COST" is the distance you've travelled so far from the start tile
    float hCost; // "H COST" is the approximation of how far you still have left to go
    float fCost; // "F COST" is Gcost + Hcost, serves as "priority" in searching.
    int identifier;

    public AStarNode(float g, float h, int id)
    {
        gCost = g;
        hCost = h;
        fCost = g + h;
        identifier = id;
    }

    public void setCost(float g, float h)
    {
        gCost = g;
        hCost = h;
        fCost = g + h;
    }

    public void setGCost(float g)
    {
        gCost = g;
        fCost = g + hCost;
    }

    public int CompareTo(object obj)
    {
        AStarNode other = obj as AStarNode;
        if (fCost > other.getFCost()) return 1;
        else if (fCost < other.getFCost()) return -1;
        else {
            if (gCost > other.getGCost()) return 1;
            else if (gCost < other.getGCost()) return -1;
            else return 0;
        }
    }

    public override bool Equals(object obj)
    {
        AStarNode other = obj as AStarNode;
        return identifier == other.identifier;
    }

    public override int GetHashCode()
    {
        return identifier;
    }

    public float getFCost() { return fCost; }
    public float getGCost() { return gCost; }
    public float getHCost() { return hCost; }
}
