using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TGenSettings
{
    public static int worldSeed = 0;
    public static float elevationStep = 0.5f;
    public static int mapSize = 2;
    public static bool featureNamesOn = false;

    //different sizes for grids
    public static (int, int, int) getMapSizeValues()
    {
        switch (mapSize)
        {
            case 0: return (20, 20, 10);
            case 1: return (40, 40, 20);
            case 2: return (60, 60, 30);
            case 3: return (80, 80, 40);
            default: return (40, 40, 20);
        }
    }

    public static bool verifyValidSeed(string input)
    {
        try
        {
            int.Parse(input);
            worldSeed = int.Parse(input);
            return true;
        } catch (System.FormatException)
        {
            worldSeed = makeItASeed(input);
            return false;
        }
    }

    public static bool verifyElevation(string input)
    {
        try
        {
            float.Parse(input);
            elevationStep = float.Parse(input);
            return true;
        }
        catch (System.FormatException)
        {
            elevationStep = 0.5f;
            return false;
        }
    }

    public static void setMapSize(int size)
    {
        mapSize = size;
    }

    //TODO: convert text input to integers
    public static int makeItASeed(string input)
    {
        return 0;
    }
}
