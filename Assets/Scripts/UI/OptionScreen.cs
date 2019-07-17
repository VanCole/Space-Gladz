using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionScreen : MonoBehaviour {

    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider masterSlider, musicSlider, sfxSlider, ambientSlider, voiceSlider;

    // Use this for initialization
    void Start()
    {
        LoadConfig();

        masterSlider.onValueChanged.AddListener(MasterUpdate);
        musicSlider.onValueChanged.AddListener(MusicUpdate);
        sfxSlider.onValueChanged.AddListener(SoundFXUpdate);
        ambientSlider.onValueChanged.AddListener(AmbientUpdate);
        voiceSlider.onValueChanged.AddListener(VoiceUpdate);
    }

    // Update is called once per frame
    void Update()
    {
        //float masterVolume = masterSlider.value == 0 ? -80.0f : 20 * Mathf.Log10(masterSlider.value);
        //float musicVolume = musicSlider.value == 0 ? -80.0f : 20 * Mathf.Log10(musicSlider.value);
        //float sfxVolume = sfxSlider.value == 0 ? -80.0f : 20 * Mathf.Log10(sfxSlider.value);
        //float ambientVolume = ambientSlider.value == 0 ? -80.0f : 20 * Mathf.Log10(ambientSlider.value);
        //float voiceVolume = voiceSlider.value == 0 ? -80.0f : 20 * Mathf.Log10(voiceSlider.value);

        //DataManager.instance.audioMixer.SetFloat("Master", masterVolume);
        //DataManager.instance.audioMixer.SetFloat("Music", musicVolume);
        //DataManager.instance.audioMixer.SetFloat("SFX", sfxVolume);
        //DataManager.instance.audioMixer.SetFloat("Ambient", ambientVolume);
        //DataManager.instance.audioMixer.SetFloat("Voice", voiceVolume);

        //UpdateConfig();
    }


    void MasterUpdate(float value)
    {
        DataManager.instance.audioMixer.SetFloat("Master", value == 0 ? -80.0f : 20 * Mathf.Log10(value));
        DataManager.instance.config.masterVolume = value;
    }

    void MusicUpdate(float value)
    {
        DataManager.instance.audioMixer.SetFloat("Music", value == 0 ? -80.0f : 20 * Mathf.Log10(value));
        DataManager.instance.config.musicVolume = value;
    }

    void SoundFXUpdate(float value)
    {
        DataManager.instance.audioMixer.SetFloat("SFX", value == 0 ? -80.0f : 20 * Mathf.Log10(value));
        DataManager.instance.config.sfxVolume = value;
    }

    void AmbientUpdate(float value)
    {
        DataManager.instance.audioMixer.SetFloat("Ambient", value == 0 ? -80.0f : 20 * Mathf.Log10(value));
        DataManager.instance.config.ambientVolume = value;
    }

    void VoiceUpdate(float value)
    {
        DataManager.instance.audioMixer.SetFloat("Voice", value == 0 ? -80.0f : 20 * Mathf.Log10(value));
        DataManager.instance.config.voicesVolume = value;
    }



    void UpdateConfig()
    {
        DataManager.instance.config.masterVolume = masterSlider.value;
        DataManager.instance.config.musicVolume = musicSlider.value;
        DataManager.instance.config.sfxVolume = sfxSlider.value;
        DataManager.instance.config.ambientVolume = ambientSlider.value;
        DataManager.instance.config.voicesVolume = voiceSlider.value;
    }

    void LoadConfig()
    {
        masterSlider.value = DataManager.instance.config.masterVolume;
        musicSlider.value = DataManager.instance.config.musicVolume;
        sfxSlider.value = DataManager.instance.config.sfxVolume;
        ambientSlider.value = DataManager.instance.config.ambientVolume;
        voiceSlider.value = DataManager.instance.config.voicesVolume;
    }
}
