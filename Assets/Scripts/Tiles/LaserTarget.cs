using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTarget : BasicActivatee
{

    #region Inspector Values
    [SerializeField]
    [Tooltip("Color of the Port when Activated")]
    Color Active_Color;

    [SerializeField]
    [Tooltip("Color of the Port when DeActivated")]
    Color Inactive_Color;

    [SerializeField]
    [Tooltip("Color of laser required to activate")]
    Color Required_Color;

    //Storage for easy access to Sprites of the Ports
    SpriteRenderer[] Port_Sprites;

    //Number of Ports, also number of activated ports needed to completely activate target
    int Num_Ports;

    //Current number of Ports activated
    int Active_Port_Count;

    //true for port indices that are activated
    bool[] Active_Ports;

    public BoardManager bm;

    [SerializeField]
    GameObject id_particles;

    AudioManager audio;
    #endregion

    [SerializeField]
    int id;

    public void SetUp(int _id)
    {
        id = _id;
        if (id == 0)
        {
            id_particles.SetActive(false);
        }
        else
        {
            ParticleSystem ps = id_particles.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = IDManager.id_colors[id - 1];
            id_particles.transform.rotation = Quaternion.Euler(0, 0, -transform.rotation.z);
        }

        int count = 0;
        Port_Sprites = new SpriteRenderer[transform.childCount];
        Active_Ports = new bool[transform.childCount];
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Port"))
            {
                child.GetComponent<Port>().Port_num = count;

                Port_Sprites[count] = child.GetComponent<SpriteRenderer>();
                Port_Sprites[count].color = Required_Color;
                Num_Ports++;
            }
            count++;
        }

        audio = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
    }

    //Called by a port when activated by a laser. Changes port color. If all ports activated, Target is fully complete
    override public void Activate(int port_index, Color activating_color)
    {
        //Debug.Log("activate:  " + port_index);
        if (Required_Color != Color.white && activating_color != Required_Color)
        {
            return;
        }

        Active_Ports[port_index] = true;
        //Port_Sprites[port_index].color = Active_Color;
        Active_Port_Count++;

        audio.PlayActivationClip();

        if (Active_Port_Count == Num_Ports)
        {
            //Debug.Log("FULLY ACTIVATED");
            EventManager.CallOnActivation(id);
            //bm.IncrementNumTargetsActivated(1);
        }
        ////Debug.Log("LaserTargetActivated");
    }

    //Called by a port when deactivated. returns port color to original.
    override public void Deactivate(int port_index)
    {
        //Debug.Log("Deactivate: " + port_index);
        if (!Active_Ports[port_index])
        {
            return;
        }
        ////Debug.Log("LaserTargetDectivated");
        //Port_Sprites[port_index].color = Inactive_Color;
        Active_Port_Count--;
        Active_Ports[port_index] = false;

        if (Active_Port_Count == Num_Ports - 1)
        {
            //Debug.Log("Not FUlly activated anymore");
            EventManager.CallOnDeactivation(id);
            //bm.IncrementNumTargetsActivated(-1);
        }
    }



}
