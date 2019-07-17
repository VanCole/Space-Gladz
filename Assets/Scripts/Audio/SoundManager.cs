using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public enum AudioType
{
    Music,
    Ambient,
    SFX,
    Voice,
    NbType
}

[DefaultExecutionOrder(-7)]
public class SoundManager : MonoBehaviour
{

    public static SoundManager instance = null;
    private List<AudioSource>[] audioPool;

    [SerializeField] AudioMixer audioMixer;
    // Modify these values in inspector directly
    [SerializeField] public int musicPoolSize = 1;
    [SerializeField] public int sfxPoolSize = 1;
    [SerializeField] public int voicePoolSize = 1;
    [SerializeField] public int ambientPoolSize = 1;

    [SerializeField]
    public AudioClip[] sounds;

    public AudioSource menuMusic;
    public AudioSource ambientSound;


    // Use this for initialization
    void Start ()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            //audioPool = GetComponentsInChildren<AudioSource>();

            CreatePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void CreatePools()
    {
        audioPool = new List<AudioSource>[(int)AudioType.NbType];

        GameObject sourceObject = null;
        GameObject sourceContainer = null;
        AudioSource source = null;

        int[] poolSize = { musicPoolSize, ambientPoolSize, sfxPoolSize, voicePoolSize };

        for (int i = 0; i < audioPool.Length; i++)
        {
            // name of the pool
            string type = ((AudioType)i).ToString();

            // Create an empty game object to contain audioSources
            sourceContainer = new GameObject(type + " Pool");
            sourceContainer.transform.SetParent(transform, false);

            // Create the pool
            audioPool[i] = new List<AudioSource>();
            for (int j = 0; j < poolSize[i]; j++)
            {
                // Create the source object
                sourceObject = new GameObject("Audio Source");
                sourceObject.transform.SetParent(sourceContainer.transform, false);

                // Create AudioSource and set the mixer group
                source = sourceObject.AddComponent<AudioSource>();
                if (audioMixer)
                {
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups(type)[0];
                }
                else
                {
                    Debug.LogWarning("No AudioMixer set in SoundManager");
                }

                // Add object to the pool
                audioPool[i].Add(sourceObject.GetComponent<AudioSource>());
            }
        }
        
    }

    // Update is called once per frame
    void Update ()
    {
    }

    public AudioSource PlaySound(int index, float m_volume, AudioType type = AudioType.SFX)
    {
        return PlaySound(index, m_volume, false, type);
    }

    public AudioSource PlaySound(int index, float m_volume, bool isLoop, AudioType type = AudioType.SFX) // Volume is set by a value between 0 and 1 (0 min 1 max)
    {
        //Debug.Log("Sound play " + index);
        if (index >= 0 && index < sounds.Length)
        {
            AudioSource source = FindSource(type);
            if (source != null)
            {
                //Debug.Log("Play Sound " + index);
                source.clip = sounds[index];
                source.Play();
                source.volume = m_volume;
                source.loop = isLoop;

                return source;
            }
        }

        return null;
    }

    // Stop all sound in a specific pool type or in all pools if type is NbType
    public void StopAllSound(AudioType type = AudioType.NbType)
    {
        if(type == AudioType.NbType)
        {
            for(int i = 0; i < audioPool.Length; i++)
            {
                foreach(AudioSource source in audioPool[i])
                {
                    StopSound(source);
                }
            }
        }
        else
        {
            foreach (AudioSource source in audioPool[(int)type])
            {
                StopSound(source);
            }
        }
    }

    public void StopSound(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
        }
    }

    public void StopAmbientSound(AudioClip[] sounds)
    {
        for(int i = 0; i < sounds.Length; i++)
        {
            if(sounds[i] != null)
            {
                sounds[i].UnloadAudioData();
            }
        }
    }

    // Return audio source if one is avalaible
    // Return Null if type is not valid or no audio source available
    private AudioSource FindSource(AudioType type)
    {
        AudioSource source = null;
        int i = 0;

        // check if type is valid
        int pool = (int)type;
        if (pool >= 0 && pool < (int)AudioType.NbType)
        {
            do
            {
                if (!audioPool[pool][i].isPlaying)
                {
                    source = audioPool[pool][i];
                }
                i++;
            } while (source == null && i < audioPool[pool].Count);
        }

        return source;
    }

    private void OnLevelWasLoaded(int level)
    {
        // Stop main menu music when entering game
        if (level == 1)
        {
            StopSound(menuMusic);
        }

        if(level == 0)
        {
            //SoundManager.instance.StopAmbientSound(SoundManager.instance.sounds);
            //if (DataManager.instance.isMulti)
                StopAmbientSound(sounds);
            //StopSound(ambientSound);
            /*for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i] != null)
                {
                    sounds[i].UnloadAudioData();
                }
            }*/
        }
    }
}
