using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Manipulator : BasicActivatee
{
    #region Line Rendering Variables
    [SerializeField]
    [Tooltip("Width of the laser beam")]
    float laser_width;

    [SerializeField]
    [Tooltip("Max Distance of the laser beam")]
    float max_dist;

    [SerializeField]
    [Tooltip("Offset Scalar for start point of the raycast")]
    float offset_scalar;

    //Layers for objects hit that block laser
    LayerMask hittable_layer;
    LayerMask editable_layer;

    //Storage for all objects hit by the Raycast2D
    RaycastHit2D[] objects_hit;

    //Storage for current Ports activated by laser beam
    Port[] activated_ports;

    //Index of the Port that is currently "Fueling" this manipulator
    int curr_activating_port_index;

    //Array for storing all laser components of children for easy access
    LineRenderer[] lasers;

    //Array for storing laser trail particle systems of ports
    ParticleSystem[] laser_trails;

    //Array for storing laser glow particle systems of ports
    ParticleSystem[] laser_glows;

    //boolean array that stores whether or not a port is activated by another laser
    int[] is_activated;

    //boolean array that stores which lasers are on
    bool[] is_turned_on;

    //Array storing the color of the laser that is activating index i
    Color[] incoming_laser_colors;

    //Color of the first activating laser and also all outgoing lasers
    Color curr_outgoing_color;

    [SerializeField]
    float max_laser_offset;

    [SerializeField]
    float max_color_offset;


    RaycastHit2D hit2D;
    Vector3 laser_dir;
    #endregion

    #region UI Variables


    Collider2D coll;

    Collider2D last_coll;

    public UIItemScript UI_Item;
    #endregion

    #region Drag and Drop variables
    SpriteRenderer obj_spr;

    Vector3 mouse_world_pos;

    int x;
    int y;
    BoardManager.tile t;
    #endregion

    #region Unity Functions
    private void Start()
    {
        //Initialize curr port index to -1, indicating not active
        curr_activating_port_index = -1;


        //Retrieve layers
        hittable_layer = LayerMask.NameToLayer("Hittable");
        editable_layer = LayerMask.NameToLayer("Editable");

        //Initialize arrays
        objects_hit = new RaycastHit2D[transform.childCount];
        activated_ports = new Port[transform.childCount];
        lasers = new LineRenderer[transform.childCount];
        is_activated = new int[transform.childCount];
        is_turned_on = new bool[transform.childCount];
        incoming_laser_colors = new Color[transform.childCount];
        laser_glows = new ParticleSystem[transform.childCount];
        laser_trails = new ParticleSystem[transform.childCount];


        //Retrieve Components of this gameobject
        GetComponent<Collider2D>().enabled = false;

        obj_spr = GetComponent<SpriteRenderer>();


        Transform kid;

        //Iterate through all children and if it is a port, instantiate the correct array entries
        for (int i = 0; i < transform.childCount; i++)
        {
            kid = transform.GetChild(i);

            if (kid.CompareTag("Port"))
            {
                kid.GetComponent<Collider2D>().enabled = false;

                kid.GetComponent<Port>().Port_num = i;

                //set up laser component
                lasers[i] = kid.GetComponent<LineRenderer>();
                lasers[i].startWidth = laser_width;
                lasers[i].endWidth = laser_width;
                lasers[i].enabled = false;

                //set up laser trail array and disable component for now
                laser_trails[i] = kid.GetChild(0).GetComponent<ParticleSystem>();
                laser_trails[i].Stop();

                //Set up laser glow array
                laser_glows[i] = kid.GetChild(1).GetComponent<ParticleSystem>();
                laser_glows[i].Stop();

                is_activated[i] = 0;
                is_turned_on[i] = false;
            }

        }

    }

    //IDEALLY: want update function that can assume everything is taken care of in regards to turning on and off ports to avoid tedious checks
    //maybe move functionality for turning on and off lasers into other functions
    private void Update()
    {
        //Iterate through all ports (children)
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).CompareTag("Port"))
            {
                continue;
            }
            //if a port is turned off, continue
            if (is_turned_on[i] == false)
            {
                continue;
            }

            lasers[i].startWidth = Random.Range(laser_width - max_laser_offset, laser_width + max_laser_offset); ;
            lasers[i].endWidth = lasers[i].startWidth;

            lasers[i].startColor = new Color(curr_outgoing_color.r + Random.Range(-max_color_offset, max_color_offset),
                                         curr_outgoing_color.g + Random.Range(-max_color_offset, max_color_offset),
                                         curr_outgoing_color.b + Random.Range(-max_color_offset, max_color_offset));
            lasers[i].endColor = lasers[i].startColor;

            //Otherwise
            laser_dir = (transform.GetChild(i).position - transform.position).normalized;
            hit2D = Physics2D.Raycast(transform.position + offset_scalar * laser_dir, laser_dir, 5f, 1 << hittable_layer | 1 << editable_layer);
            if (hit2D.collider == null)
            {
                //short circuit if endpt of laser is the same as previous frame
                if (lasers[i].GetPosition(1) == transform.position + max_dist * laser_dir)
                {
                    continue;
                }

                //otherwise set endpoint of laser to position + max distance * direction
                lasers[i].SetPosition(1, transform.position + max_dist * laser_dir);

                //If a new port is found or no port is found, do something
                if (activated_ports[i] != null)
                {
                    activated_ports[i].Deactivate();
                    activated_ports[i] = null;
                }
            } else
            {

                ////short circuit if endpt of laser is the same as previous frame
                //if ((Vector2)lasers[i].GetPosition(1) == hit2D.point)
                //{
                //    return;
                //}

                //otherwise set endpoint of laser to hit_point of raycast
                lasers[i].SetPosition(1, hit2D.point);

                //If a new port is found or no port is found, do something
                if (hit2D.collider.GetComponent<Port>() != activated_ports[i])
                {
                    //////Debug.Log("HElloooo");
                    //Deactivate current Port if existing
                    if (activated_ports[i] != null)
                    {
                        ////Debug.Log(transform.name + " is deactivating " + activated_ports[i].transform.name);
                        activated_ports[i].Deactivate();
                    }

                    //Activate new Port if possible
                    if ((activated_ports[i] = hit2D.collider.GetComponent<Port>()) != null)
                    {
                        ////Debug.Log(transform.name + " is activating " + activated_ports[i].transform.name);
                        activated_ports[i].Activate(curr_outgoing_color);
                    }
                }
            }
        }
    }
    #endregion


    #region Port Caller Functions

    //Called by Port/child object. Takes in port_index indicating the input port and turns on all outgoing ports if no current activated port
    override public void Activate(int port_index, Color activating_color)
    {


        //set bool[port_index] to true and set the correct color 
        is_activated[port_index] += 1;
        incoming_laser_colors[port_index] = activating_color;

        ////Debug.Log("is_activating[" + port_index + "] = " + is_activated[port_index]);

        //Check if another port is fueling this manipulator. if so, don't do anything further
        if (curr_activating_port_index != -1 || curr_activating_port_index == port_index)
        {
            return;
        }
        //Now, we must know that this is the first port activating the manipulator
        //Debug.Log(curr_activating_port_index);
        curr_activating_port_index = port_index;
        //Debug.Log(curr_activating_port_index);

        //Set current outgoing color to incoming color
        curr_outgoing_color = activating_color;

        //Also, turn on every port except this one
        for (int j = 0; j < is_turned_on.Length; j++)
        {
            if (j != port_index)
            {
                TurnOnPort(j);
                is_turned_on[j] = true;
            }
        }

        //Finally, Turn on all particle systems and assign them the correct color
        TurnOnParticleSystems(activating_color);




    }

    //Called by Port/child object. Takes in port_index indicating the port that is no longer activated and turns off all outgoing ports
    override public void Deactivate(int port_index)
    {

        //Set port color back to original
        //transform.GetChild(port_index).GetComponent<SpriteRenderer>().color = Color.red;

        //set bool[port_index] to false
        if (is_activated[port_index] > 0)
        {
            is_activated[port_index] -= 1;
        }


        if (is_activated[port_index] > 0)
        {

            return;
        }
        //Check if this is the port that is currently fueling the manipulator. if not, don't need to do anything extra
        if (curr_activating_port_index != port_index)
        {
            return;
        }

        //Return color array[port_index] to null
        int another_activated_port = -1;
        //check array for any other true values
        for (int i = 0; i < is_activated.Length; i++)
        {
            is_turned_on[i] = false;
            TurnOffPort(i);

            //Found another activated port
            if (is_activated[i] > 0)
            {
                another_activated_port = i;
            }
        }

        //if another activating port, turn stuff back on
        if (another_activated_port >= 0)
        {
            curr_activating_port_index = another_activated_port;

            //Set curr outgoing color to whichever color is activating this port
            curr_outgoing_color = incoming_laser_colors[another_activated_port];

            //Turn on particle systems with new color
            TurnOnParticleSystems(curr_outgoing_color);

            //if we find another port that is activated, turn on all ports except the activated one
            for (int j = 0; j < is_turned_on.Length; j++)
            {
                if (j != another_activated_port)
                {
                    TurnOnPort(j);
                    is_turned_on[j] = true;
                }
            }
        }
        //if there are no more activated ports, all ports should be off
        else
        {
            curr_activating_port_index = -1;

            //Turn off particle systems
            TurnOffParticleSystems();
        }




    }
    #endregion

    #region Port Off/On Functions
    //Enable Line Renderer component, ensure easy access thru array
    void TurnOnPort(int port_index)
    {
        if (!transform.GetChild(port_index).CompareTag("Port"))
        {
            return;
        }
        lasers[port_index].enabled = true;
        lasers[port_index].SetPosition(0, transform.position);
        lasers[port_index].startColor = curr_outgoing_color;
        lasers[port_index].endColor = curr_outgoing_color;
        ////Debug.Log("Turning on " + port_index.ToString());
    }

    //Disable Line Renderer component, make sure to deactivate all ports hit by this port's laser
    void TurnOffPort(int port_index)
    {

        if (!transform.GetChild(port_index).CompareTag("Port"))
        {
            return;
        }

        ////Debug.Log("Turn Off: " + port_index);

        //Deactivate all ports hit by port_index's laser
        if (activated_ports[port_index] != null)
        {
            activated_ports[port_index].Deactivate();
        }

        //lasers[port_index].SetPosition(1, transform.GetChild(port_index).position);


        lasers[port_index].enabled = false;
        activated_ports[port_index] = null;
        objects_hit[port_index] = new RaycastHit2D();


    }

    #endregion



    //Turns on particle systems and sets their color to 'col'
    void TurnOnParticleSystems(Color col)
    {
        int count = 0;
        ParticleSystem.MainModule main;
        ParticleSystem.ColorOverLifetimeModule col_life;
        while (count < transform.childCount)
        {
            if (laser_glows[count] == null)
            {
                count++;
                continue;
                
            }
            //First do Laser Glow : Set Start Color to col
            main = laser_glows[count].main;
            main.startColor = col;
            laser_glows[count].Play();

            //Next do Laser Trail: Set Color over lifetime initial color to col
            col_life = laser_trails[count].colorOverLifetime;
            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(col, 0), new GradientColorKey(Color.white, 1) },
                          new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0), new GradientAlphaKey(.3f, 1) });
            col_life.color = grad;
            laser_trails[count].Play();

            count++;
        }
    }

    //Turns off particle systems
    void TurnOffParticleSystems()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (laser_glows[i] == null)
            {
                continue;
            }

            //Turn off laser_glows and laser_trails
            laser_glows[i].Stop();
            laser_trails[i].Stop();
        }
    }

    #region UI Functions
    public void Rotate_Right() 
    {
        Reset_Manipulator();
        transform.rotation *= Quaternion.Euler(0, 0, -90);
    }

    //Button for removing
    public void Delete()
    {
        //Deactivate all outgoing ports
        foreach (Port p in activated_ports)
        {
            if (p != null)
            {
                p.Deactivate();
            }
        }

        //Increment number of uses for this type by 1
        UI_Item.IncrementUseCount(1);

        //Remove data from the board 
        x = Mathf.FloorToInt((transform.position.x - BoardManager.static_start_loc.x) * 100);
        x = Mathf.FloorToInt(x / (BoardManager.offset.x * 100));
        y = Mathf.FloorToInt((transform.position.y - BoardManager.static_start_loc.y) * -100);
        y = Mathf.FloorToInt(y / (BoardManager.offset.y * 100));

        BoardManager.board[y, x].available = true;
        BoardManager.board[y, x].tile_obj = null;

        //Destroy game object
        Destroy(this.gameObject);
    }
    #endregion

    #region Drag Manipulator Functions
    public void StartDrag ()
    {
        Reset_Manipulator();

        //Step 2
        Deactivate_Manipulator();

        //Step 3
        x = Mathf.FloorToInt((transform.position.x - BoardManager.static_start_loc.x) * 100);
        x = Mathf.FloorToInt(x / (BoardManager.offset.x * 100));
        y = Mathf.FloorToInt((transform.position.y - BoardManager.static_start_loc.y) * -100);
        y = Mathf.FloorToInt(y / (BoardManager.offset.y * 100));

        BoardManager.board[y, x].available = true;
        BoardManager.board[y, x].tile_obj = null;

    }

    public void Drag ()
    {
        mouse_world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        x = Mathf.FloorToInt((Camera.main.ScreenToWorldPoint(Input.mousePosition).x - BoardManager.static_start_loc.x) * 100);
        x = Mathf.FloorToInt(x / (BoardManager.offset.x * 100));
        y = Mathf.FloorToInt((Camera.main.ScreenToWorldPoint(Input.mousePosition).y - BoardManager.static_start_loc.y) * -100);
        y = Mathf.FloorToInt(y / (BoardManager.offset.y * 100));

        if (x < 0 || x > BoardManager.board.GetLength(1) - 1 || y < 0 || y > BoardManager.board.GetLength(0) - 1)
        {
            return;
        }
        t = BoardManager.board[y, x];
        transform.position = t.center;

        if (t.available)
        {
            obj_spr.color = Color.green;
        }
        else if (!t.available)
        {
            obj_spr.color = Color.red;
        }
    }

    public void EndDrag()
    {
        obj_spr.color = Color.white;

        if (x < 0 || x > BoardManager.board.GetLength(1) - 1 || y < 0 || y > BoardManager.board.GetLength(0) - 1 || !t.available)
        {
            UI_Item.IncrementUseCount(1);
            Destroy(gameObject);
        }

        else if (!t.available)
        {
            UI_Item.IncrementUseCount(1);
            Destroy(gameObject);
        }
        else
        {
            BoardManager.board[y, x].tile_obj = gameObject;
            BoardManager.board[y, x].available = false;
            gameObject.layer = hittable_layer;
            Activate_Manipulator();
        }
    }


    //Called by Drag Handler to "Turn On" the manipulator, activating colliders
    public void Activate_Manipulator()
    {
        ////Debug.Log("Activating manipulator");
        GetComponent<Collider2D>().enabled = true;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("Port"))
            {
                transform.GetChild(i).GetComponent<Collider2D>().enabled = true;
            }

        }
    }

    //Called by dragging to turn off the manipulator, deactivating colliders
    void Deactivate_Manipulator()
    {
        GetComponent<Collider2D>().enabled = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).CompareTag("Port"))
            {
                transform.GetChild(i).GetComponent<Collider2D>().enabled = false;
            }
        }

    }

    //Resets entire manipulator
    void Reset_Manipulator()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).CompareTag("Port"))
            {
                continue;
            }

            //First turn off the port
            TurnOffPort(i);
            is_turned_on[i] = false;
        }

        curr_activating_port_index = -1;

        TurnOffParticleSystems();
    }
    #endregion

}
