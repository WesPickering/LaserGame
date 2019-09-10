using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TileColorTarget : BasicActivatee
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

    //Array for storing all laser components of children for easy access
    LineRenderer[] lasers;

    //boolean array that stores whether or not a port is activated by another laser
    int[] is_activated;

    //boolean array that stores which lasers are on
    bool[] is_turned_on;

    //Array storing the color of the laser that is activating index i
    Color[] incoming_laser_colors;

    Color[] outgoing_laser_colors;

    RaycastHit2D hit2D;
    Vector3 laser_dir;
    int num_ports;
    #endregion

    #region Target Variables
    [SerializeField]
    SpriteRenderer target_color_sprite;

    [SerializeField]
    SpriteRenderer current_color_sprite;

    [SerializeField]
    Color required_color;

    bool isActivated;
    #endregion

    #region Unity Functions
    private void Start()
    {
        Transform kid;

        hittable_layer = LayerMask.NameToLayer("Hittable");
        //editable_layer = LayerMask.NameToLayer("Editable");
        target_color_sprite.color = required_color;
        current_color_sprite.color = Color.white;

        //Count number of ports
        num_ports = 0;
        for (int i = 0; i < transform.childCount; i++) 
        {
            if (transform.GetChild(i).CompareTag("Port"))
            {
                num_ports++;
            }
        }

        objects_hit = new RaycastHit2D[num_ports];
        activated_ports = new Port[num_ports];
        lasers = new LineRenderer[num_ports];
        is_activated = new int[num_ports];
        is_turned_on = new bool[num_ports];
        incoming_laser_colors = new Color[num_ports];
        outgoing_laser_colors = new Color[num_ports];

        int count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            kid = transform.GetChild(i);

            if (kid.CompareTag("Port"))
            {
                ////Debug.Log(count);
                kid.GetComponent<Port>().Port_num = count;

                //set up laser component
                lasers[count] = kid.GetComponent<LineRenderer>();
                lasers[count].startWidth = laser_width;
                lasers[count].endWidth = laser_width;
                lasers[count].enabled = false;

                is_activated[count] = 0;
                is_turned_on[count] = false;

                count++;
            }

        }

    }

    //IDEALLY: want update function that can assume everything is taken care of in regards to turning on and off ports to avoid tedious checks
    //maybe move functionality for turning on and off lasers into other functions
    private void Update()
    {
        //Iterate through all ports (children)
        for (int i = 0; i < num_ports; i++)
        {
            //if a port is turned off, continue
            if (is_turned_on[i] == false)
            {
                continue;
            }

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
            }
            else
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
                        activated_ports[i].Activate(outgoing_laser_colors[i]);
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
        //When a port is activated, we want to turn on ONLY ONE other port (the opposite one) and set it to have the same color as incoming
        //Set activation variables to store info
        //Check if oppositee port is already activated
        // If so, do nothing, just return;
        // turn on opposite port with same color as 'activating_color'
        // Change color of the tile to incorporate 'activating_color'

        bool check = current_color_sprite.color == target_color_sprite.color;

        //set activated variable for corresponding port and set the correct color 
        is_activated[port_index] += 1;
        incoming_laser_colors[port_index] = activating_color;

        ////Debug.Log("is_activating[" + port_index + "] = " + is_activated[port_index]);

        //Check if opposite port is fueling this manipulator. if so, don't do anything further
        if (is_activated[(port_index + 2) % 4] > 0)
        {
            return;
        }
        //turn on opposite port with same color as 'activating color'
        outgoing_laser_colors[(port_index + 2) % 4] = activating_color;

        //Also, turn on opposite port
        TurnOnPort((port_index + 2) % 4);

        //Finally, Change color of the Tile 

        if (current_color_sprite.color == Color.white)
        {
            current_color_sprite.color = activating_color;
        }
        else
        {
            current_color_sprite.color += activating_color;
            current_color_sprite.color = new Color(Mathf.Clamp(current_color_sprite.color.r, 0, 133.0f / 255.0f),
                                                   Mathf.Clamp(current_color_sprite.color.g, 0, 133.0f / 255.0f),
                                                   Mathf.Clamp(current_color_sprite.color.b, 0, 133.0f / 255.0f), 1.0f);
        }


        //If activated, then another wrong added color, we need to deactivate
        if (check && current_color_sprite.color != target_color_sprite.color)
        {
            //isActivated = false;
            //StartCoroutine(CheckDeactivation());
            EventManager.CallOnDeactivation(-1);
        }

        //only activate if not previously activated AND is now activated
        if (!check && current_color_sprite.color == target_color_sprite.color)
        {
            //Debug.Log("+1 tile activated");
            //isActivated = true;
            //StartCoroutine(CheckActivation());
            EventManager.CallOnActivation(-1);

        }


    }

    //Called by Port/child object. Takes in port_index indicating the port that is no longer activated and turns off all outgoing ports
    override public void Deactivate(int port_index)
    {
        //check = 'if curreently activated'
        bool check = current_color_sprite.color == target_color_sprite.color;

        //set bool[port_index] to false
        if (is_activated[port_index] > 0)
        {
            is_activated[port_index] -= 1;
        }


        ////Debug.Log("is_activating[" + port_index + "] = " + is_activated[port_index]);
        if (is_activated[port_index] > 0)
        {
            return;
        }

        //Start Turning off components
        TurnOffPort((port_index + 2) % 4);

        //If we remove a color that is the same as the color along the other axis, we don't want to do anything
        if (incoming_laser_colors[port_index] != incoming_laser_colors[(port_index + 1) % 4]
         && incoming_laser_colors[port_index] != incoming_laser_colors[(port_index + 3) % 4])
        {
            current_color_sprite.color -= incoming_laser_colors[port_index];
            current_color_sprite.color = new Color(Mathf.Clamp(current_color_sprite.color.r, 0, 133.0f / 255.0f),
                                                   Mathf.Clamp(current_color_sprite.color.g, 0, 133.0f / 255.0f),
                                                   Mathf.Clamp(current_color_sprite.color.b, 0, 133.0f / 255.0f), 1.0f);
        }

        if (current_color_sprite.color == Color.black)
        {
            current_color_sprite.color = Color.white;
        }


        //check if opposite port is activated, in which case, start turning things back on
        if (is_activated[(port_index +2) % 4] > 0)
        {
            Debug.Log("Other port activated");
            outgoing_laser_colors[port_index] = incoming_laser_colors[(port_index + 2) % 4];
            TurnOnPort(port_index);

            if (current_color_sprite.color == Color.white)
            {
                current_color_sprite.color = incoming_laser_colors[(port_index + 2) % 4];
            }
            else
            {
                current_color_sprite.color += incoming_laser_colors[(port_index + 2) % 4];
                current_color_sprite.color = new Color(Mathf.Clamp(current_color_sprite.color.r, 0, 133.0f / 255.0f),
                                                   Mathf.Clamp(current_color_sprite.color.g, 0, 133.0f / 255.0f),
                                                   Mathf.Clamp(current_color_sprite.color.b, 0, 133.0f / 255.0f), 1.0f);
            }

        }

        incoming_laser_colors[port_index] = Color.black;


        // if used to be activated AND now not activated
        if (check && current_color_sprite.color != target_color_sprite.color)
        {
            EventManager.CallOnDeactivation(-1);
            //isActivated = false;
            //StartCoroutine(CheckDeactivation());
        }

        if (!check && current_color_sprite.color == target_color_sprite.color)
        {
            //Debug.Log("+1 tile activated");
            //isActivated = true;
            //StartCoroutine(CheckActivation());
            //Debug.Log("OnDeactivation, reactivated");
            EventManager.CallOnActivation(-1);
        }





    }
    #endregion

    IEnumerator CheckActivation ()
    {
        yield return new WaitForSeconds(.2f);
        if (isActivated)
        {
            EventManager.CallOnActivation(-1);
        }

    }

    IEnumerator CheckDeactivation()
    {
        yield return new WaitForSeconds(.2f);
        if (!isActivated)
        {
            EventManager.CallOnDeactivation(-1);
        }
    }

    #region Port Off/On Functions
    //Enable Line Renderer component, ensure easy access thru array
    void TurnOnPort(int port_index)
    {


        //Turn on this port
        is_turned_on[port_index] = true;
        lasers[port_index].enabled = true;
        lasers[port_index].SetPosition(0, transform.position);
        lasers[port_index].startColor = outgoing_laser_colors[port_index];
        lasers[port_index].endColor = outgoing_laser_colors[port_index];

        //Extend the activateed port laser to meet the activating one
        int opposite_port = (port_index + 2) % 4;
        Vector3 correction_vec = .1f * (transform.GetChild(opposite_port).position - transform.position).normalized;

        lasers[opposite_port].enabled = true;
        lasers[opposite_port].SetPosition(0, transform.position);
        lasers[opposite_port].SetPosition(1, transform.position + correction_vec);
        lasers[opposite_port].startColor = outgoing_laser_colors[port_index];
        lasers[opposite_port].endColor = outgoing_laser_colors[port_index];
        ////Debug.Log("Turning on " + port_index.ToString());
    }

    //Disable Line Renderer component, make sure to deactivate all ports hit by this port's laser
    void TurnOffPort(int port_index)
    {

        ////Debug.Log("Turn Off: " + port_index);

        //Deactivate all ports hit by port_index's laser
        if (activated_ports[port_index] != null)
        {
            activated_ports[port_index].Deactivate();
        }

        is_turned_on[port_index] = false;

        //Disable port_index's laser and reset info
        lasers[port_index].enabled = false;
        activated_ports[port_index] = null;
        objects_hit[port_index] = new RaycastHit2D();

        //Disable the makeshift opposite port laser
        lasers[(port_index + 2) % 4].enabled = false;

    }

    #endregion

}
