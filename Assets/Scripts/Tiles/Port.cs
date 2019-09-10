using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Port : MonoBehaviour
{
    Transform manipulator;

    int port_num;


    private void Start()
    {
        manipulator = transform.parent;
    }

    public void Activate(Color activating_color) 
    {
        manipulator.GetComponent<BasicActivatee>().Activate(port_num, activating_color);
    }

    public void Deactivate()
    {
        manipulator.GetComponent<BasicActivatee>().Deactivate(port_num);
    }

    public int Port_num
    { 
        get { return port_num; }
        set { port_num = value; }
    }


}
