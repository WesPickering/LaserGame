using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    GameManager gm;


    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    public void LoadLevelSelect()
    {
        gm.LoadLevelSelect();
    }

    public void LoadCredits()
    {
        gm.LoadCredits();
    }
}
