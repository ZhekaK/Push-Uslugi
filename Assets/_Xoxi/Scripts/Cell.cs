using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cell
{
    public int[] line = new int[5];
    public Transform[] spawnPointsCell = new Transform[5];
    public GameObject[] figure = new GameObject[5];
}
