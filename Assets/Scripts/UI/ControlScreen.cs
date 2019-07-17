using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlScreen : MonoBehaviour
{
    [SerializeField]
    GameObject[] Screens;

    bool controllerScreen = false;

    public void GoToMenu()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        controllerScreen = false;
        DataManager.instance.gameState = DataManager.GameState.mainMenu;
    }

    public void SwitchControlScreen()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        controllerScreen = !controllerScreen;      
    }


    // Use this for initialization
    void Start()
    {
        Screens[0].SetActive(true);
        Screens[1].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!controllerScreen)
        {
            Screens[0].SetActive(true);
            Screens[1].SetActive(false);
        }
        else
        {
            Screens[0].SetActive(false);
            Screens[1].SetActive(true);
        }
    }
}
