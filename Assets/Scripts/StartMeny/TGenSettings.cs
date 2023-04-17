using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TGenSettings
{
    //BASIC settings
    public static int worldSeed = 0;
    public static float elevationStep = 0.5f;
    public static int mapSize = 2;
    public static bool featureNamesOn = false;

    //ADVANCED settings
    public static float mtnHeight = 7.0f;
    public static float snowHeight = 10.0f;
    public static float pineLine = 3.0f;
    public static bool[] enabledBiomes = { true, true, true, true };
    public static float forestDensity = 0.3f;


    //different sizes for grids
    public static (int, int, int) getMapSizeValues()
    {
        switch (mapSize)
        {
            case 0: return (20, 20, 8);
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

    public static bool verifyMtnSnowHeight(string input, string input2)
    {
        try
        {
            float.Parse(input);
            mtnHeight = float.Parse(input);
            float.Parse(input2);
            snowHeight = float.Parse(input2);

            return snowHeight >= mtnHeight; //only acceptable when snow height above mountain height
        }
        catch (System.FormatException)
        {
            mtnHeight = 7.0f;
            snowHeight = 10.0f;
            return false;
        }
    }

    public static bool verifyPineLine(string input)
    {
        try
        {
            float.Parse(input);
            pineLine = float.Parse(input);
            return true;
        }
        catch (System.FormatException)
        {
            pineLine = 3.0f;
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
