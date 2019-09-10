using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDManager : MonoBehaviour
{
    [SerializeField]
    public static Color[] id_colors;

    private void Start()
    {
        id_colors = new Color[6];
        id_colors[0] = Color.red;
        id_colors[1] = Color.green;
        id_colors[2] = Color.blue;
        id_colors[3] = Color.yellow;
        id_colors[4] = Color.magenta;
        id_colors[5] = Color.cyan;
    }
}
