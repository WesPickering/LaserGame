using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] steps;

    int next_level;

    public void LoadLevel()
    {
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().LoadLevel(next_level, false);
    }

    public void SetUp(int step, int next)
    {
        Debug.Log("step : " + step);
        Debug.Log("next :" + next);
        next_level = next;

        for (int i = 0; i < steps.Length; i++)
        {
            if (i == step)
            {
                steps[i].SetActive(true);
            }
            else
            {
                steps[i].SetActive(false);
            }
        }
    }


    public void LoadMenu ()
    {
        SceneManager.LoadScene("Menu");
    }

}
