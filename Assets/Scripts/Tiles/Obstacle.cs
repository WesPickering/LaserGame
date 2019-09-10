using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{

    Collider2D coll;

    SpriteRenderer spr;

    [SerializeField]
    int id;

    int active_count;

    [SerializeField]
    GameObject id_particles;

    public void SetUp(int id_value)
    {
        coll = GetComponent<Collider2D>();

        spr = GetComponent<SpriteRenderer>();

        id = id_value;

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
    }

    private void OnEnable()
    {
        EventManager.OnActivated += TurnOffObstacle;
        EventManager.OnDeactivated += TurnOnObstacle;
    }

    private void OnDisable()
    {
        EventManager.OnActivated -= TurnOffObstacle;
        EventManager.OnDeactivated -= TurnOnObstacle;
    }


    void TurnOffObstacle(int x)
    {
        if (id == x && x != 0)
        {
            active_count++;
            coll.enabled = false;
            spr.enabled = false;

        }
    }

    void TurnOnObstacle(int x)
    {
        if (id == x && x != 0)
        {
            active_count--;
            if (active_count == 0)
            {
                coll.enabled = true;
                spr.enabled = true;
            }
        }
    }
}