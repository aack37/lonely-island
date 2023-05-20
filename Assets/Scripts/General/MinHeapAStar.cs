using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//it's just an ordinary min heap ya know
public class MinHeapAStar<TData> : MinHeap<AStarNode, TData>
{

    private int heapify(int index)
    {
        if (size > 1)
        {
            int left = getLChild(index);
            int right = getRChild(index);

            //get smallest element of subtree
            int smallest = index;

            //if Lchild exists, and it's smaller than the current element, swap it
            if (left < size && arr[left].getValue().CompareTo(arr[index].getValue()) < 0)
                smallest = left;

            //if Rchild exists, and it's smaller than the current element, swap it
            if (right < size && arr[right].getValue().CompareTo(arr[smallest].getValue()) < 0)
                smallest = right;

            //swap the current element with the smallest child if need be
            if (smallest != index)
            {
                heapMap[arr[smallest].getData()] = index;
                MinHeapNode<AStarNode, TData> temp = arr[index];
                arr[index] = arr[smallest];
                arr[smallest] = temp;
                return heapify(smallest);
            }
            else
            {
                return index;
            }
        }
        return 0;
    }

    //update the value of the specified element
    public virtual int update(TData updateThis, float newGCost)
    {

        if (updateThis != null)
        {
            int index = heapMap[updateThis];
            float old = arr[index].getValue().getGCost();
            arr[index].getValue().setGCost(newGCost);

            //TODO: move accordingly
            if(old.CompareTo(newGCost) > 0) //it got smaller, so move it up the heap
            {
                int parent = getParent(index);
                //sift up the tree (starting at the very last element). if curr is less than its parent, it goes up.
                while (arr[getParent(index)].getValue().CompareTo(arr[index].getValue()) > 0)
                {
                    //swap curr and its parent if applicable.
                    heapMap[arr[parent].getData()] = index;
                    MinHeapNode<AStarNode, TData> temp = arr[parent];
                    arr[parent] = arr[index];
                    arr[index] = temp;

                    index = getParent(index); //get ready for the next round.
                    parent = getParent(index);
                }
                heapMap[updateThis] = index; //update its entry in the heapMap

            } else if(old.CompareTo(newGCost) < 0) //it got bigger, so move it down
            {
                heapMap[updateThis] = heapify(index);
            }
            return heapMap[updateThis];
        }
        return -1;
    }

    //the basics
    int getParent(int i)
    {
        return (i - 1) / 2;
    }

    int getLChild(int i)
    {
        return (2 * i + 1);
    }

    int getRChild(int i)
    {
        return (2 * i + 2);
    }
}
