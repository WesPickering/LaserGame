using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelInfo
{
    //What things constitutes a level
    //1. A map : int[] of 7 x 10 
    //2. A list of manipulators and corresponding number of uses
    //3. a level number

    public int[] sources;

    public int[] targets;

    public int[] obstacles;

    public int[] checkers;

    public int width;

    public int height;

    public int[] manipulator_index;

    public int[] manipulator_max;

    public int level_num;

}