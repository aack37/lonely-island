using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//it's just an ordinary min heap ya know
public class MinHeap<TValue, TData> where TValue : IComparable
{
    protected List<MinHeapNode<TValue, TData>> arr = new List<MinHeapNode<TValue, TData>>();
    protected int size = 0;
    protected Dictionary<TData, int> heapMap = new Dictionary<TData, int>(); //maps each element in the heap to its index in the heap

    //add to minHeap
    public void insert(TValue priority, TData element)
    {
        size++;
        arr.Add(new MinHeapNode<TValue,TData>(priority, element));

        int curr = size - 1;

        //sift up the tree (starting at the very last element). if curr is less than its parent, it goes up.
        while (Comparer<TValue>.Default.Compare(arr[getParent(curr)].getValue(), arr[curr].getValue()) > 0)
        {
            //swap curr and its parent if applicable.
            MinHeapNode<TValue, TData> temp = arr[getParent(curr)];
            arr[getParent(curr)] = arr[curr];
            arr[curr] = temp;

            curr = getParent(curr); //get ready for the next round.
        }
        if(element != null) heapMap.Add(element, curr); //add it into the heapMap
    }

    //remove + return the mininum element of the heap
    public MinHeapNode<TValue, TData> removeMin()
    {
        if (size > 1)
        {
            MinHeapNode<TValue, TData> min = getMin();

            //swap the bottom and root.
            MinHeapNode<TValue, TData> last = arr[size - 1];
            arr[0] = last;

            //remove the bottom, which was formerly the root
            size--;
            arr.RemoveAt(size);

            //call heapify() to maintain the minHeap
            TData swapThis = arr[0].getData();
            int newIndex = heapify(0); //heapify fixes the heap, even if heapMap doesn't get updated!
            if (swapThis != null) { heapMap[swapThis] = newIndex; }
            return min;
        } else if(size == 1)
        {
            MinHeapNode<TValue, TData> min = arr[0];
            Clear();
            return min;
        }
        return null;
    }

    //rearrange the heap at specified index. also, return the NEW index of that element.
    private int heapify(int index)
    {
        if (size > 1)
        {
            int left = getLChild(index);
            int right = getRChild(index);

            //get smallest element of subtree
            int smallest = index;

            //if Lchild exists, and it's smaller than the current element, swap it
            if (left < size && Comparer<TValue>.Default.Compare(arr[left].getValue(), arr[index].getValue()) < 0)
                smallest = left;

            //if Rchild exists, and it's smaller than the current element, swap it
            if (right < size && Comparer<TValue>.Default.Compare(arr[right].getValue(), arr[smallest].getValue()) < 0)
                smallest = right;

            //swap the current element with the smallest child if need be
            if (smallest != index)
            {
                heapMap[arr[smallest].getData()] = index;
                MinHeapNode<TValue, TData> temp = arr[index];
                arr[index] = arr[smallest];
                arr[smallest] = temp;
                return heapify(smallest);
            } else
            {
                return index;
            }
        }
        return 0;
    }

    //update the value of the specified element
    public int update(TData updateThis, TValue newValue)
    {
        if(updateThis != null)
        {
            int index = heapMap[updateThis];
            TValue old = arr[index].getValue();
            arr[index].setValue(newValue);
            //TODO: move accordingly
            if(old.CompareTo(newValue) > 0) //it got smaller, so move it up the heap
            {
                int parent = getParent(index);
                //sift up the tree (starting at the very last element). if curr is less than its parent, it goes up.
                while (Comparer<TValue>.Default.Compare(arr[getParent(index)].getValue(), arr[index].getValue()) > 0)
                {
                    //swap curr and its parent if applicable.
                    heapMap[arr[parent].getData()] = index;
                    MinHeapNode<TValue, TData> temp = arr[parent];
                    arr[parent] = arr[index];
                    arr[index] = temp;

                    index = getParent(index); //get ready for the next round.
                    parent = getParent(index);
                }
                heapMap[updateThis] = index; //update its entry in the heapMap

            } else if(old.CompareTo(newValue) < 0) //it got bigger, so move it down
            {
                heapMap[updateThis] = heapify(index);
            }
            return heapMap[updateThis];
        }
        return -1;
    }



    //return index of the specified item in heap
    public int getIndexInHeap(TData findMe)
    {
        return heapMap[findMe];
    }

    //clear the heap
    public void Clear()
    {
        arr.Clear();
        heapMap.Clear();
        size = 0;
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

    MinHeapNode<TValue,TData> getMin()
    {
        return arr[0];
    }

    public int getSize()
    {
        return size;
    }

    public override string ToString()
    {
        string temp = "";
        foreach(MinHeapNode<TValue,TData> thing in arr)
        {
            temp = temp + thing.ToString() + ",";
        }
        return temp;
    }
}
