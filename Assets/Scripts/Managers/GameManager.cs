using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public int curr_lvl;

    public int max_lvl_unlocked;

    int tutorial_num;

    //True - can start loading a scene, false - cannot start loading a scene
    [SerializeField]
    bool load_scene_lock;


    /* Board Creation Guide
     * TILES:   
     * 1 - Basic Empty Tile
     * 2 - Obstacle Tile
     * 3 - Laser Source
     * 4 - Laser Target 1 Port
     * 5 - Laser Target 2 Port, L - shape
     * 6 - Laser Target 2 Port, 180 angle
     * 7 - Laser Target 3 Port
     * 8 - Laser Target 4 Port
     * 
     * DIrection:
     * 1 - North
     * 2 - West
     * 3 - South
     * 4 - East
     * 
     * A Single tile is represented by an Integer which specifies Type and Rotation.
     * The Ones digit specifies the Direction that the tile is facing.
     * the tens digit specifies the type of tile
     * Ex. 33 -> 3: Laser Source + 3: South
     *      this would instantiate a Laser Source facing South   
    */

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        curr_lvl = -1;

        load_scene_lock = true;
    }

    #region Scene Selection
    public void LoadLevelSelect()
    {
        if (load_scene_lock)
        {
            load_scene_lock = false;
            StartCoroutine(LoadAsyncScene("LevelSelect"));
            curr_lvl = -1;
            ////Debug.Log(load_scene_lock);
        }
    }

    public void LoadCredits()
    {
        if (load_scene_lock)
        {
            load_scene_lock = false;
            StartCoroutine(LoadAsyncScene("Credits"));
            curr_lvl = -1;
            ////Debug.Log(load_scene_lock);
        }
    }

    public void LoadTutorial(int x)
    {
        if (load_scene_lock)
        {
            load_scene_lock = false;
            tutorial_num = x;
            StartCoroutine(LoadAsyncScene("Tutorial"));
            ////Debug.Log(load_scene_lock);
        }
    }


    public void LoadLevel(int level, bool overridden)
    {
        if (load_scene_lock)
        {
            curr_lvl = level;
            Debug.Log(curr_lvl);
            if (level == 1 && overridden)
            {
                curr_lvl = level;
                LoadTutorial(0);
            }
            else if (level == 21 && overridden)
            {
                curr_lvl = level;
                LoadTutorial(1);
            }
            else
            {
                load_scene_lock = false;
                StartCoroutine(LoadAsyncScene("Main"));
                curr_lvl = level;
            }

        }
        else
        {
            return;
        }
    }

    public void ReloadScene() 
    {
        LoadLevel(curr_lvl, false); 
    }


    IEnumerator LoadAsyncScene(string SceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        ////Debug.Log("Done loading");
        load_scene_lock = true;
        ////Debug.Log(load_scene_lock);
        if (SceneName == "Main")
        {
            Debug.Log(curr_lvl);
            GameObject.FindWithTag("BoardManager").GetComponent<BoardManager>().CreateLevel(curr_lvl);
        }
        else if (SceneName == "LevelSelect")
        {
            GameObject.FindWithTag("LevelSelectManager").GetComponent<LevelSelect>().LoadButtons(max_lvl_unlocked);
        }
        else if (SceneName == "Tutorial")
        {
            GameObject.FindWithTag("TutorialManager").GetComponent<TutorialManager>().SetUp(tutorial_num, curr_lvl);
        }
    }
    #endregion





}
