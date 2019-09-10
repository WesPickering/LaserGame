using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void OnActivation(int id);
    public static event OnActivation OnActivated;

    public delegate void OnDeactivation(int id);
    public static event OnDeactivation OnDeactivated;

    public delegate void HighlightImcomplete();
    public static event HighlightImcomplete Highlighted;

    public static void CallOnActivation(int id)
    {
        OnActivated?.Invoke(id);
    }

    public static void CallOnDeactivation(int id)
    {
        OnDeactivated?.Invoke(id);
    }

    public static void CallHighlightIncomplete()
    {
        Highlighted?.Invoke();
    }
}
