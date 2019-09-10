using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    GameManager gm;

    [SerializeField]
    GameObject[] level_buttons;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();


    }

    public void LoadButtons( int max_level_unlocked)
    {
        for (int i = 0; i < max_level_unlocked; i++)
        {
            ////Debug.Log(level_buttons[i].GetComponent<LevelButtonScript>());
            level_buttons[i].GetComponent<LevelButtonScript>().Activate_Button();
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void LoadLevel(int x)
    {
        Debug.Log(x);
        if (x == 1)
        {
            gm.LoadLevel(x, true);
        }
        else if (x == 21)
        {
            gm.LoadLevel(x, true);
        }
        else
        {
            gm.LoadLevel(x, false);
        }

    }
}
