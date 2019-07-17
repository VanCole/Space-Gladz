using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

using UnityEngine.UI;

public class ClassSelection : MonoBehaviour
{

    public PlayerSelection playerSelection;

    [SerializeField]
    SkillsMenu skillMenu;

    [SerializeField]
    GameObject[] nodeClass = new GameObject[2];
    public RectTransform[] rectTf = new RectTransform[2];

    GameObject championModel;

    int colorIndex;

    private void Start()
    {
        skillMenu.hasChanged = true;
        championModel = playerSelection.championModel;

        for (int i = 0; i < nodeClass.Length; i++)
        {
            rectTf[i] = nodeClass[i].GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        NodeAnimation();
        ChangePortraitEmissiveColor();
    }

    void ChangePortraitEmissiveColor()
    {
        colorIndex = playerSelection.championModel.GetComponentInChildren<Player>().colorIndex;

        for (int i = 0; i < nodeClass.Length; i++)
        {
            nodeClass[i].transform.GetChild(2).GetComponent<Image>().color = DataManager.instance.playerColor[colorIndex];
        }
    }


    DataManager.TypeClass SwitchClass(int playerIndex, DataManager.TypeClass desiredClass)
    {
        DataManager.TypeClass newClassSelected = desiredClass;

        DataManager.instance.player[playerIndex].gameObject.SetActive(false);
        DataManager.instance.player[playerIndex] = championModel.transform.GetChild((int)newClassSelected).GetComponent<Player>();
        DataManager.instance.player[playerIndex].gameObject.SetActive(true);
        skillMenu.hasChanged = true;

        DataManager.instance.player[playerIndex].selectedClass = newClassSelected;

        return newClassSelected;
    }


    void NodeAnimation()
    {
        if (playerSelection.choosedClass == DataManager.TypeClass.Warrior)
        {
            Vector2 tempEndSize1 = new Vector2(100.0f, 100.0f);
            rectTf[(int)DataManager.TypeClass.Warrior].sizeDelta = Vector2.Lerp(rectTf[(int)DataManager.TypeClass.Warrior].sizeDelta, tempEndSize1, 0.2f);

            Vector2 tempEndSize2 = new Vector2(50.0f, 50.0f);
            rectTf[(int)DataManager.TypeClass.Archer].sizeDelta = Vector2.Lerp(rectTf[(int)DataManager.TypeClass.Archer].sizeDelta, tempEndSize2, 0.2f);      
        }
        else if (playerSelection.choosedClass == DataManager.TypeClass.Archer)
        {
            Vector2 tempEndSize1 = new Vector2(100.0f, 100.0f);
            rectTf[(int)DataManager.TypeClass.Archer].sizeDelta = Vector2.Lerp(rectTf[(int)DataManager.TypeClass.Archer].sizeDelta, tempEndSize1, 0.2f);

            Vector2 tempEndSize2 = new Vector2(50.0f, 50.0f);
            rectTf[(int)DataManager.TypeClass.Warrior].sizeDelta = Vector2.Lerp(rectTf[(int)DataManager.TypeClass.Warrior].sizeDelta, tempEndSize2, 0.2f);
        }
    }


    public DataManager.TypeClass SwitchingClass(DataManager.TypeClass currentClassSelected, int playerIndex, bool changeActualPrefab = true , bool changeModel = true)
    {
        DataManager.TypeClass newClassSelected = currentClassSelected;

        switch (currentClassSelected)
        {
            case DataManager.TypeClass.Warrior:
                {             
                    if (changeModel)
                        newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Archer);
                    if (DataManager.instance.isMulti && changeActualPrefab)
                        GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].ClassPicked(1);
                }
                break;
            case DataManager.TypeClass.Archer:
                {
                    if (changeModel)
                        newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Warrior);
                    if (DataManager.instance.isMulti && changeActualPrefab)
                        GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].ClassPicked(0);
                }
                break;

            default:
                break;
        }
        return newClassSelected;
    }


    //LEGACY
    //public DataManager.TypeClass SlidingNodesRightToleft(DataManager.TypeClass currentClassSelected, int playerIndex)
    //{
    //    DataManager.TypeClass newClassSelected = currentClassSelected;

    //    anim.SetFloat("animSpeed", 1.0f);

    //    switch (currentClassSelected)
    //    {
    //        case DataManager.TypeClass.Warrior:
    //            {
    //                anim.Play("Node1SlideLeft", 0, 0.0f);

    //                newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Archer);
    //                playerSelection.canvasMenu.GetComponent<CanvasMenu>().lobbyManager.GetComponent<Prototype.NetworkLobby.LobbyManager>().gamePlayerPrefab = Resources.Load("Archer") as GameObject;

    //            }
    //            break;

    //        //case DataManager.TypeClass.Archer:
    //        //    {
    //        //        anim.Play("Node2SlideLeft", 0, 0.0f);

    //        //        newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Assassin);
    //        //        playerSelection.canvasMenu.GetComponent<CanvasMenu>().lobbyManager.GetComponent<Prototype.NetworkLobby.LobbyManager>().gamePlayerPrefab = Resources.Load("Assassin") as GameObject;
    //        //    }
    //        //    break;

    //        //case DataManager.TypeClass.Assassin:
    //        //    {
    //        //        anim.Play("Node3SlideLeft", 0, 0.0f);

    //        //        newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Warrior);
    //        //        playerSelection.canvasMenu.GetComponent<CanvasMenu>().lobbyManager.GetComponent<Prototype.NetworkLobby.LobbyManager>().gamePlayerPrefab = Resources.Load("Warrior") as GameObject;
    //        //    }
    //        //    break;
    //        default:
    //            break;
    //    }

    //    return newClassSelected;
    //}

    //public DataManager.TypeClass SlidingNodesLeftToRight(DataManager.TypeClass currentClassSelected, int playerIndex)
    //{
    //    DataManager.TypeClass newClassSelected = currentClassSelected;

    //    anim.SetFloat("animSpeed", -1.0f);

    //    switch (currentClassSelected)
    //    {
    //        case DataManager.TypeClass.Warrior:
    //            {
    //                anim.Play("Node3SlideLeft", 0, 1.0f);

    //                //newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Assassin);
    //                //playerSelection.canvasMenu.GetComponent<CanvasMenu>().lobbyManager.GetComponent<Prototype.NetworkLobby.LobbyManager>().gamePlayerPrefab = Resources.Load("Assassin") as GameObject;
    //            }
    //            break;

    //        case DataManager.TypeClass.Archer:
    //            {
    //                anim.Play("Node1SlideLeft", 0, 1.0f);

    //                newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Warrior);
    //                playerSelection.canvasMenu.GetComponent<CanvasMenu>().lobbyManager.GetComponent<Prototype.NetworkLobby.LobbyManager>().gamePlayerPrefab = Resources.Load("Warrior") as GameObject;
    //            }
    //            break;

    //        //case DataManager.TypeClass.Assassin:
    //        //    {
    //        //        anim.Play("Node2SlideLeft", 0, 1.0f);

    //        //        newClassSelected = SwitchClass(playerIndex, DataManager.TypeClass.Archer);
    //        //        playerSelection.canvasMenu.GetComponent<CanvasMenu>().lobbyManager.GetComponent<Prototype.NetworkLobby.LobbyManager>().gamePlayerPrefab = Resources.Load("Archer") as GameObject;       
    //        //    }
    //        //    break;
    //        default:
    //            break;

    //    }
    //    return newClassSelected;
    //}
}
