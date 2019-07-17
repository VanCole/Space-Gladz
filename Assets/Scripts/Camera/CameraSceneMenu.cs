using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSceneMenu : MonoBehaviour
{

    [SerializeField]
    GameObject[] virtualCam = new GameObject[2];

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.gameState == DataManager.GameState.mainMenu)
        {
            
            virtualCam[0].SetActive(true);
            virtualCam[1].SetActive(false);
            virtualCam[2].SetActive(false);
        }
        else if (DataManager.instance.gameState == DataManager.GameState.lobbyMenu)
        {
           
            virtualCam[0].SetActive(false);
            virtualCam[1].SetActive(false);
            virtualCam[2].SetActive(true);

        }
        else if (DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
           
            virtualCam[0].SetActive(false);
            virtualCam[1].SetActive(true);
            virtualCam[2].SetActive(false);
        }
    }
}
