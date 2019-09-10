using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ReloadScene()
    {
        gm.ReloadScene(); 
    }

    public void LoadLevel(int x)
    {
        gm.LoadLevel(x, true);
    }

    public void LoadNextLevel(int x)
    {
        gm.LoadLevel(x+1, true);
    }

    public void LoadlevelSelect()
    {
        gm.LoadLevelSelect();
    }
}
