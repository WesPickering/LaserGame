using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public struct tile
    {
        //Center of the tile
        public Vector2 center;

        //Whether or not the tile is available
        public bool available;

        //Pointer to the object currently occupying this tile
        public GameObject tile_obj;
         
    }

    #region Board Creation Variables
    public bool test;
    public int testlevel;

    [SerializeField]
    [Tooltip("Array of tile types used for creating board")]
    GameObject[] tile_types;

    //2D Array representing board and its current state
    public static tile[,] board;

    [SerializeField]
    [Tooltip("Start Location for upper left Tile")]
    Vector2 start_location;

    [SerializeField]
    [Tooltip("Offset in X direction")]
    float x_offset;

    [SerializeField]
    [Tooltip("Offset in Y direction")]
    float y_offset;

    [SerializeField]
    [Tooltip("UI Item that represents a type of manipulator and can be dragged into the scene")]
    GameObject UI_Item;

    [SerializeField]
    [Tooltip("This is the Panel under which UI Items will be instantiated")]
    Transform Bottom_Panel;

    [SerializeField]
    Text Level_Text;

    [SerializeField]
    [Tooltip("Different Manipulators, sorted by least to most ports")]
    GameObject[] Manipulators;


    public static Vector2 static_start_loc;
    public static Vector2 offset;
    #endregion

    #region Game State Variables
    int num_targets_to_win;
    int curr_num_targets;
    int curr_lvl;

    [HideInInspector]
    public bool gameover;

    [SerializeField]
    GameObject[] disable_on_win;

    [SerializeField]
    LineRenderer[] laser_show_lasers;

    [SerializeField]
    [Tooltip("UI For win condition")]
    GameObject Win_UI;

    [SerializeField]
    GameObject Blocking_Layer;
    #endregion

    #region Unity Functions
    private void Start()
    {
        static_start_loc = start_location;
        offset = new Vector2(x_offset, y_offset);

        Win_UI.SetActive(false);
        Blocking_Layer.SetActive(false);

        foreach (LineRenderer laser in laser_show_lasers)
        {
            laser.enabled = (false);
        }

        curr_num_targets = 0;
        if (test)
        {
            StartCoroutine(TestLevel(testlevel));

        }

    }
    #endregion

    #region Board Functions
    /* Board Creation Guide
     * TILES:   
     * 0 - Basic Empty Tile
     * 1 - Obstacle Tile
     * 2 - Source - Red
     * 3 - Source - Green
     * 4 - Source - Blue
     * 5 - Target - Red
     * 6 - Target - Green
     * 7 - Target - Blue
     * 8 - Laser Target 2 Port, 90 angle
     * 9 - Laser Target 2 Port, 180 angle
     * 10 - Laser Target 3 Port
     * 11 - Laser Target 4 Port
     * 
     * Type:
     * 0 - Obstacle
     * 1 - Source
     * 2 - Target
     * 
     * Ports:
     * 0 - 1 Port
     * 1 - 2 Port, 90 angle
     * 2 - 2 Port, 180 angle
     * 3 - 3 Port
     * 4 - 4 Port
     * 
     * Color:
     * 0 - White
     * 1 - Red
     * 2 - Green
     * 3 - Blue
     * 
     * Directions:
     * 0 - North
     * 1 - West
     * 2 - South
     * 3 - East
     * 
     * Location:   
     * (x,y) - > 10*x + y
     * 
     * A single tile is represented by an integer which specifies type, rotation, and location.
     * The ones digit specifies the direction that the tile is facing
     * the tens digit specifies the color of the tile
     * the 100s digit specifies the ports type of the tile
     * the 1000s digit specifies y component
     * the 10000s digit specifies x component
     * direction = value % 10
     * color = (value / 10) % 10
     * ports = (value / 100) % 10
     * y = (value / 1000) % 10
     * x = (value / 10000) % 10
     * Ex. 33 -> 3: Laser Source + 3: South
     * this would instantiate a Laser Source facing South   
    */

    static int TileMap(int type, int ports, int color)
    {
        switch (type)
        {
            case 0: return 1;
            case 1:
                if (color > 3 || color == 0) return 0;
                return 1 + color;
            case 2:
                if (color > 3 || color == 0 || ports > 4) return 0;
                return color * 5 + ports;
            case 3:
                if (color > 6 || color == 0) return 0;
                return 19 + color;
        }
        return 0;
    }

    IEnumerator TestLevel(int level_num) 
    {
        yield return new WaitForSeconds(1);
        CreateLevel(level_num);
    }

    //Instantiates a board based on the given bitmap
    public void CreateLevel(int level_num)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "level" + level_num.ToString() + ".json");

        if (File.Exists(path))
        {

            //Read in level info from json file "level_.json"
            string json = File.ReadAllText(path);

            LevelInfo level = JsonUtility.FromJson<LevelInfo>(json);
            curr_lvl = level.level_num;

            gameover = false;

            //Create the physical board
            InitializeBoard(level);
            Debug.Log("num to win " + num_targets_to_win);

            //Set up UI elements
            //1. Level Title
            Level_Text.text = "Level " + level_num;

            //2. Create bottom panel UI by iterating thru the manipulators
            int count = 0;
            GameObject UI_obj;

            while (count < level.manipulator_index.Length)
            {
                UI_obj = Instantiate(UI_Item, Bottom_Panel);
                UI_obj.GetComponent<UIItemScript>().InitializeUI(Manipulators[level.manipulator_index[count]], level.manipulator_max[count]);
                count++;
            }

        }
        else
        {
            //Debug.LogError(path + ": file not available");
        }
    }


    void InitializeBoard(LevelInfo level)
    {
        //initialize board
        Transform board_parent = GameObject.Find("Board").transform;
        board = new tile[level.height, level.width];
        tile t;
        int direction;
        int ports;
        int color;
        int y;
        int x;
        //Iterate through bitmap - rows
        for (int i = 0; i < level.height; i++)
        {
            //Iterate through bitmap - columns
            for (int j = 0; j < level.width; j++)
            {
                t = new tile();
                t.center = start_location + new Vector2(j * x_offset + x_offset / 2.0f, i * -y_offset - y_offset / 2.0f);
                Instantiate(tile_types[0], t.center, transform.rotation, board_parent);
                t.available = true;
                board[i, j] = t;
            }
        }

        int id;
        if (level.obstacles != null)
            foreach (int value in level.obstacles)
            {
                y = value % 10;
                x = (value / 10) % 10;
                id = (value / 100) % 10;

                board[y, x].tile_obj = Instantiate(tile_types[TileMap(0, 0, 0)], board[y, x].center, Quaternion.Euler(0f, 0f, 0f), board_parent);
                board[y, x].tile_obj.GetComponent<Obstacle>().SetUp(id);
                board[y, x].available = false;
            }

        if (level.checkers != null)
            foreach (int value in level.checkers)
            {
                color = value % 10;
                y = (value / 10) % 10;
                x = (value / 100) % 10;

                board[y, x].tile_obj = Instantiate(tile_types[TileMap(3, 0, color)], board[y, x].center, Quaternion.Euler(0f, 0f, 0f), board_parent);
                board[y, x].available = false;
                num_targets_to_win += 1;
            }

        if (level.targets != null)
            foreach (int value in level.targets)
            {
                direction = value % 10;
                color = (value / 10) % 10;
                ports = (value / 100) % 10;
                y = (value / 1000) % 10;
                x = (value / 10000) % 10;
                id = (value / 100000) % 10;


                board[y, x].tile_obj = Instantiate(tile_types[TileMap(2, ports, color)], board[y, x].center, Quaternion.Euler(0f, 0f, direction * 90), board_parent);
                board[y, x].tile_obj.GetComponent<LaserTarget>().SetUp(id);
                board[y, x].available = false;
                num_targets_to_win += 1;
            }

        if (level.sources != null)
            foreach (int value in level.sources)
            {
                direction = value % 10;
                color = (value / 10) % 10;
                ports = (value / 100) % 10;
                y = (value / 1000) % 10;
                x = (value / 10000) % 10;
                id = (value / 100000) % 10;

                board[y, x].tile_obj = Instantiate(tile_types[TileMap(1, 0, color)], board[y, x].center, Quaternion.Euler(0f, 0f, direction * 90), board_parent);
                board[y, x].tile_obj.GetComponent<LaserSource>().SetUp(id);
                board[y, x].available = false;
            }

        //Debug.Log(num_targets_to_win);
    }
 
    #endregion

    void IncrementNumTargetsActivated(int x)
    {
        curr_num_targets += 1;
        Debug.Log(curr_num_targets);
        if (curr_num_targets == num_targets_to_win)
        {
            StartCoroutine(WinGame());

        }
    }

    void DecrementNumTargetsActivated(int x)
    {
        curr_num_targets -= 1;
        Debug.Log(curr_num_targets);
    }

    public void GoNextLevel() {
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().LoadLevel(curr_lvl + 1, true);
    }

    IEnumerator WinGame()
    {
        yield return new WaitForSeconds(.2f);

        if (curr_num_targets != num_targets_to_win)
        {
            yield break;
        }
        //Turn on layer blocking touch input
        Blocking_Layer.SetActive(true);

        //Turn off all other UI elements
        foreach (GameObject go in disable_on_win)
        {
            go.SetActive(false);
        }
        gameover = true;

        //Cue sound effects
        GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>().PlayWinClip();

        //Do Laser show
        //Step 1: Initialize stationary end points
        float timer = 0;
        LineRenderer left = laser_show_lasers[0];
        LineRenderer right = laser_show_lasers[1];
        LineRenderer topright = laser_show_lasers[2];
        LineRenderer topleft = laser_show_lasers[3];

        left.enabled = true;
        right.enabled = true;
        topright.enabled = true;
        topleft.enabled = true;
        bool rightturn = true;
        while (timer <= 2)
        {
            if (rightturn)
            {
                //right.enabled = true;
                //topright.enabled = true;
                //left.enabled = false;
                //topleft.enabled = false;

                right.SetPosition(1, right.GetPosition(0) + Quaternion.Euler(0, 0, -120 * timer / 2) * (10 * Vector2.left));
                right.startColor = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
                right.endColor = right.startColor;

                topright.SetPosition(1, topright.GetPosition(0) + Quaternion.Euler(0, 0, 120 * timer / 2) * (10 * Vector2.left));
                topright.startColor = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
                topright.endColor = topright.startColor;
            }
            else
            {
                //right.enabled = false;
                //topright.enabled = false;
                //left.enabled = true;
                //topleft.enabled = true;

                left.SetPosition(1, left.GetPosition(0) + Quaternion.Euler(0, 0, 120 * timer / 2) * (10 * Vector2.right));
                left.startColor = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
                left.endColor = left.startColor;

                topleft.SetPosition(1, topleft.GetPosition(0) + Quaternion.Euler(0, 0, -120 * timer / 2) * (10 * Vector2.right));
                topleft.startColor = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
                topleft.endColor = topright.startColor;
            }
            rightturn = !rightturn;

            timer += Time.deltaTime;
            yield return null;
        }
        Time.timeScale = 1;
        Win_UI.SetActive(true);
    }

    private void OnEnable()
    {
        EventManager.OnActivated += IncrementNumTargetsActivated;
        EventManager.OnDeactivated += DecrementNumTargetsActivated;
    }

    private void OnDisable()
    {
        EventManager.OnActivated -= IncrementNumTargetsActivated;
        EventManager.OnDeactivated -= DecrementNumTargetsActivated;
    }

}