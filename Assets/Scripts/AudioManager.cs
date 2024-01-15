using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [SerializeField]
    private AudioSource mainAudioSource;

    public static AudioManager Instance { get { return instance; } private set { } }
    public AudioSource MainAudioSource { get { return mainAudioSource; } private set { } }

    void Awake()
    {
        if(instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }
}
