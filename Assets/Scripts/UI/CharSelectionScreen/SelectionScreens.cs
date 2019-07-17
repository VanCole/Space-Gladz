using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionScreens : MonoBehaviour
{
    public GameObject[] selectionScreen = new GameObject[4];


    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < selectionScreen.Length; i++)
        {
            selectionScreen[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
            if (!DataManager.instance.isMulti)
            {
                LocalMultiplayerDisplay();
            }
            else
            {
                MultiplayerDisplay();
            }
        }
    }


    void LocalMultiplayerDisplay()
    {
        for (int i = 0; i < selectionScreen.Length; i++)
        {
            if (selectionScreen[i].GetComponent<PlayerSelection>().playerIndex < DataManager.instance.currentNbrPlayer)
            {
                selectionScreen[i].SetActive(true);
            }
            else
            {
                selectionScreen[i].SetActive(false);
            }
        }
    }
    void MultiplayerDisplay()
    {
        for (int i = 0; i < selectionScreen.Length; i++)
        {
            if (selectionScreen[i].GetComponent<PlayerSelection>().playerIndex < DataManager.instance.currentNbrPlayer)
            {
                selectionScreen[i].SetActive(true);
            }
            else
            {
                selectionScreen[i].SetActive(false);
            }
        }
    }



    public void FadeAllCanvas()
    {
        for (int i = 0; i < selectionScreen.Length; i++)
        {
            StartCoroutine(FadeOutCanvas(i));
        }
    }


    public IEnumerator FadeOutCanvas(int playerIndex)
    {
        while (selectionScreen[playerIndex].GetComponent<CanvasGroup>().alpha > 0.0f)
        {
            selectionScreen[playerIndex].GetComponent<CanvasGroup>().alpha -= 0.1f;
            yield return new WaitForSeconds(0.01f);
        }

        selectionScreen[playerIndex].SetActive(false);
        DataManager.instance.gameState = DataManager.GameState.mainMenu;
    }
}
