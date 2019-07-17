using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class GiftUI : MonoBehaviour/*, ISelectHandler, IDeselectHandler*/
{
    [SerializeField]
    PlayerSelection playerSelect;

    public DataManager.TypeGift typeGift;

    public bool isSelected;
    public int arrayPos; //where that gift is stock in playerSelect.selectedGift[]


   
    Color selectedColor;
    Color deselectedgColor;


    private void Start()
    {
        isSelected = false;
        arrayPos = 0;
    }

    //Sprite
    public Sprite icon;
    public string name;
    public string description;

    private void Update()
    {
        //Reinitialisation();

        if (playerSelect.playerEventSystem.currentSelectedGameObject == gameObject)
        {
            GetComponent<CanvasGroup>().alpha = 1.0f;
        }
        else
        {
            GetComponent<CanvasGroup>().alpha = 0.5f;
        }


        if (isSelected)
        {
            GetComponent<Image>().color = new Color(0.0f, 0.7f, 0.0f);
        }
        else
        {
            GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        }
        transform.GetChild(0).GetComponent<Image>().color = GetComponent<Image>().color;
    }

    public void GiftSelect(int index)
    {
        //Selection
        if (!isSelected)
        {
            for (int i = 0; i < playerSelect.selectedGifts.Length; i++)
            {
                //Stock gift in array if there is a free space
                if (playerSelect.selectedGifts[i] == DataManager.TypeGift.none)
                {
                    SoundManager.instance.PlaySound(79, 0.5f, AudioType.SFX);

                    playerSelect.selectedGifts[i] = (DataManager.TypeGift)index;
                    arrayPos = i + 1;
                    isSelected = true;
                    playerSelect.giftCount++;
                    break;
                }
            }
        }

        //Deselection
        else
        {
            if (DataManager.instance.isMulti)
            {
                GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdPlayerReady(DataManager.instance.localPlayerIndex - 1, false);
                GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].SendNotReadyToBeginMessage();
            }

            DataManager.instance.isPlayerReady[playerSelect.playerIndex] = false;

            //free current position in playerSelect.selectedGift[]
            if (playerSelect.giftCount != 0)
            {
                SoundManager.instance.PlaySound(79, 1.0f, AudioType.SFX);

                playerSelect.selectedGifts[arrayPos - 1] = DataManager.TypeGift.none;
                arrayPos = 0;
                isSelected = false;
                playerSelect.giftCount--;
            }
        }
    }
}

