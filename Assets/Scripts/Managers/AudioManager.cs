using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioClip activation_clip;

    [SerializeField]
    AudioClip win_clip;

    //Audio source out of which to play the clips
    AudioSource audio;

    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }


    public void PlayActivationClip ()
    {
        audio.clip = activation_clip;
        audio.Play();
    }

    public void PlayWinClip ()
    {
        audio.clip = win_clip;
        audio.Play();
    }


}
