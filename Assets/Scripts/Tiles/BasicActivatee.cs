using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicActivatee : MonoBehaviour
{
    virtual public void Activate(int port_index, Color activating_color)
    {
        ////Debug.Log("ACTIVATE ACTIVATE ACTIVATE");
    }

    virtual public void Deactivate(int port_index)
    {
        ////Debug.Log("DEACTIVATE DEACTIVATE DEACTIVATE");
    }
}
