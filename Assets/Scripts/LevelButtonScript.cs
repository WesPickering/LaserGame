using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonScript : MonoBehaviour
{
    [SerializeField]
    Transform Laser_Source;

    private void Start()
    {
        Laser_Source = transform.GetChild(1);
        Laser_Source.gameObject.SetActive(false);
        GetComponent<Button>().interactable = false;
    }

    public void Activate_Button() {
        //////Debug.Log(Laser_Source.gameObject);
        Laser_Source.gameObject.SetActive(true);

        GetComponent<Button>().interactable = true;

        ////Debug.Log(GetComponent<Button>().interactable);


    }
}
