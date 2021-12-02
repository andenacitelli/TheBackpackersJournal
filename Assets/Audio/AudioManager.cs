using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    public static AudioManager instance;

    public AudioSource aSrc;

    void Awake()
    {

        /*has one and only one AudioManager throughout the game*/
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        /*load audio source for each sound*/
        aSrc = gameObject.GetComponent<AudioSource>();

        
    }

    private void Start()
    {
        Assign3DSource(aSrc, "MainMenuBackground");
        Play("MainMenuBackground");
    }

    // Update is called once per frame
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!(When asked to play)");
            return;
        }

        s.source.Play();

    }

    public void Assign3DSource(AudioSource newSrc, String name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!(When asked to play)");
            return;
        } else if (s.source == null) {
            Debug.Log("AudioSource newly assigned.");
        } else
        {
            Debug.Log("AudioSource replaced");
        }
        s.source = newSrc;
        Debug.Log($"{newSrc.gameObject.name} playing sound.\nAudio source comparison\nPassed: {newSrc}\nUsing: {s.source}");
        s.source.clip = s.clip;
        s.source.outputAudioMixerGroup = s.group;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;

    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!(When asked to stop)");
            return;
        }
        s.source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!(When asked to pause)");
            return;
        }
        s.source.Pause();
    }

    private void Update()
    {
        /*foreach (Sound s in sounds)
        {
            if (PauseMenu.isPaused)
            {
                s.source.volume = .1f;
            }
            else
            {
                s.source.volume = .5f;

            }
        }*/
    }
}
