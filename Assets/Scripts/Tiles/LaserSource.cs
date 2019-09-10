using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSource : MonoBehaviour
{
    #region Line Rendering Variables
    LineRenderer laser;

    [SerializeField]
    [Tooltip("Width of the laser beam")]
    float laser_width;

    [SerializeField]
    float max_laser_offset;

    [SerializeField]
    float max_color_offset;

    [SerializeField]
    [Tooltip("Max Distance of the laser beam")]
    float max_dist;

    //unit Vector3 for storing the direction in which to shoot the laser
    Vector3 laser_dir;

    [SerializeField]
    [Tooltip("Offset Scalar for start point of the raycast")]
    float offset_scalar;

    [SerializeField]
    [Tooltip("Color of this laser source, check LaserColors.cs")]
    Color laser_color;

    //Layer for hittable objects
    LayerMask hittable_layer;
    LayerMask editable_layer;

    //Storage for an object hit by the Raycast2D
    RaycastHit2D hit2D;

    //Storage for current Port activated by laser beam
    Port port;

    [SerializeField]
    //port object that holds the particle systems
    GameObject output;
    #endregion

    bool isSetUp;

    bool isOn;

    int id;

    [SerializeField]
    GameObject id_particles;

    int active_count;

    public void SetUp(int _id)
    {
        //Set up Line Renderer variables
        laser = GetComponent<LineRenderer>();
        laser.startColor = laser_color;
        laser.endColor = laser_color;
        laser.startWidth = laser_width;
        laser.endWidth = laser_width;
        laser.SetPosition(0, transform.position);

        //Get access to Output port and calculate direction variable
        laser_dir = (transform.GetChild(0).position - transform.position).normalized;

        hittable_layer = LayerMask.NameToLayer("Hittable");
        editable_layer = LayerMask.NameToLayer("Editable");

        id = _id;

        isSetUp = true;

        isOn = true;

        if (id == 0)
        {
            id_particles.SetActive(false);
        }
        else
        {
            ParticleSystem ps = id_particles.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = IDManager.id_colors[id - 1];

            laser.enabled = false;
            isOn = false;
            output.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isSetUp)
        {
            return;
        }

        if (!isOn)
        {
            return;
        }

        laser.startWidth = Random.Range(laser_width - max_laser_offset, laser_width + max_laser_offset); ;
        laser.endWidth = laser.startWidth;

        laser.startColor = new Color(laser_color.r + Random.Range(-max_color_offset, max_color_offset),
                                     laser_color.g + Random.Range(-max_color_offset, max_color_offset),
                                     laser_color.b + Random.Range(-max_color_offset, max_color_offset));
        laser.endColor = laser.startColor;

        hit2D = Physics2D.Raycast(transform.position + offset_scalar * laser_dir, laser_dir, 5f, 1 << hittable_layer | 1 << editable_layer);
        if (hit2D.collider == null)
        {

            //short circuit if endpt of laser is the same as previous frame
            if (laser.GetPosition(1) == transform.position + max_dist * laser_dir)
            {
                return;
            }

            //otherwise set endpoint of laser to position + max distance * direction
            laser.SetPosition(1, transform.position + max_dist * laser_dir);

            //If a new port is found or no port is found, do something
            if (port != null)
            {
                ////Debug.Log("HELLO");

                port.Deactivate();
                port = null;
            }


        }
        else {

            ////short circuit if endpt of laser is the same as previous frame
            //if ((Vector2)laser.GetPosition(1) == hit2D.point)
            //{
            //    return;
            //}

            //otherwise set endpoint of laser to hit_point of raycast
            laser.SetPosition(1, hit2D.point);

            //If a new port is found or no port is found, do something
            if (hit2D.collider.GetComponent<Port>() != port)
            {
                ////Debug.Log("HELLO");
                //Deactivate current Port if existing
                if (port != null)
                {
                    ////Debug.Log(transform.name + " is deactivating " + port.transform.name);
                    port.Deactivate();
                }

                //Activate new Port if possible
                if ((port = hit2D.collider.GetComponent<Port>()) != null)
                {
                    ////Debug.Log(transform.name + " is activating " + port.transform.name);
                    port.Activate(laser_color);
                }
            }
        }
    }


    void TurnOnSource(int _id)
    {
        if (id == _id && _id != 0)
        {
            active_count++;
            laser.enabled = true;
            isOn = true;
            output.SetActive(true);
        }
    }

    void TurnOffSource(int _id)
    {
        if (id == _id && id != 0)
        {
            active_count--;
            if (active_count == 0)
            {
                laser.enabled = false;
                isOn = false;
                output.SetActive(false);

                if (port != null)
                {
                    port.Deactivate();
                    port = null;
                }
            }

        }
    }

    private void OnEnable()
    {
        EventManager.OnActivated += TurnOnSource;
        EventManager.OnDeactivated += TurnOffSource;

    }

    private void OnDisable()
    {
        EventManager.OnActivated -= TurnOnSource;
        EventManager.OnDeactivated -= TurnOffSource;
    }
}
