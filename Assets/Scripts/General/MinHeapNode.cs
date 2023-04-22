using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//nodes used specifically by a min heap.
public class MinHeapNode<TValue, TData> : IComparable
    where TValue : IComparable
{
    //think of it as a priority queue. values are priority, data is whatever else tags along
    private TValue value;
    private TData data;

    public MinHeapNode(TValue val, TData dat) {
        value = val;
        data = dat;
    }

    public TValue getValue()
    {
        return value;
    }

    public TData getData()
    {
        return data;
    }

    public void setValue(TValue v)
    {
        value = v;
    }

    public void setData(TData d)
    {
        data = d;
    }

    //comparing two nodes in the heap only means comparing their values
    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        MinHeapNode<TValue, TData> otherNode = obj as MinHeapNode<TValue, TData>;
        if (otherNode != null)
            return this.value.CompareTo(otherNode.value);
        else
            throw new ArgumentException("Failed to compare two minHeapNodes");
    }

    public override string ToString()
    {
        return data + " (" + value + ")";
    }
}
