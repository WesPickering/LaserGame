using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [SerializeField]
    Text Level_Text;

    GameManager gm;


    private void Start()
    {
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        Level_Text.text = "Level " + gm.curr_lvl;
    }
}
