using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public AudioClip[] sounds;

    public void PlayRandomSound()
    {
        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        GameObject source = GameObject.Find("Main Camera");
        

        AudioSource.PlayClipAtPoint(clip, source.transform.position);
    }
}
