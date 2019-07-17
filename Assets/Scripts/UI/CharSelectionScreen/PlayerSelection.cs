using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PlayerSelection : MonoBehaviour
{
    public int playerIndex;

    public GameObject canvasMenu;

    public GameObject[] classScreen = new GameObject[3];

    [SerializeField]
    GameObject[] helpButtons = new GameObject[2];

    [SerializeField]
    Text textIPserver;

    [SerializeField]
    Text playerName;

    [SerializeField]
    GameObject[] nextButtons = new GameObject[2];

    public GameObject classSelect;

    public GameObject championModel;

    [SerializeField]
    public GameObject informationPanel;

    public EventSystem playerEventSystem;
    public bool hasSelectedAButton = false;
    public bool giftsPreselected = false;
    public bool hasChangedScreen = false;

    [SerializeField] GameObject blockPanel;

    public enum TypeScreen
    {
        screenClass,
        screenGift
    }
    public TypeScreen typeScreen;


    public DataManager.TypeClass choosedClass;
    public DataManager.TypeGift[] selectedGifts = new DataManager.TypeGift[3];
    public int giftCount;

    public bool isToolTipsToggled;


    private void OnEnable()
    {
        StartCoroutine(FadeInCanvas());
    }

    // Use this for initialization
    private void Start()
    {
        if (DataManager.instance.isMulti)
            playerEventSystem = canvasMenu.GetComponent<CanvasMenu>().EventSystem[0];
        else
            playerEventSystem = canvasMenu.GetComponent<CanvasMenu>().EventSystem[playerIndex];

        giftCount = 0;

        choosedClass = DataManager.TypeClass.Warrior;
        DataManager.instance.player[playerIndex] = championModel.transform.GetChild((int)choosedClass).GetComponent<Player>();
        DataManager.instance.player[playerIndex].gameObject.SetActive(true);

        isToolTipsToggled = false;
        if (DataManager.instance.isMulti)
            typeScreen = TypeScreen.screenGift;
        else
            typeScreen = TypeScreen.screenClass;

        if (blockPanel) blockPanel.SetActive(!DataManager.instance.isMulti);
    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.isMulti)
        {
            playerName.text = GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[transform.GetSiblingIndex()].playerName;
            if (isToolTipsToggled)
                playerName.text = "";
        }

        if (textIPserver && DataManager.instance.isMulti && DataManager.instance.localPlayerIndex == 1 && !isToolTipsToggled)
        {
            textIPserver.text ="IP: " + GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[0].LocalIPAddress();
        }
        else if (textIPserver)
        {
            textIPserver.text = "";
        }

        if (DataManager.instance.isMulti && hasChangedScreen == false)
        {
            if (transform.GetSiblingIndex() == DataManager.instance.localPlayerIndex - 1)
            {
                typeScreen = TypeScreen.screenClass;
                hasChangedScreen = true;
            }
        }

        CharacterSelectionInput();

        //DisplayHelpButton();
        DisplayScreen();

        //Reinitialisation();

        DisplayInformationPanel();

        GiftPresetManager();

        ReadyButtonDisplay();
    }

    void DisplayInformationPanel()
    {
        if (DataManager.instance.gameState != DataManager.GameState.charSelection)
        {
            informationPanel.SetActive(false);
        }
        else
        {
            informationPanel.SetActive(true);
        }

        if (isToolTipsToggled)
        {
            informationPanel.GetComponent<CanvasGroup>().alpha = 1.0f;
        }
        else
        {
            informationPanel.GetComponent<CanvasGroup>().alpha = 0.0f;
        }
    }


    //Manage All Keyboard + controller input inside the Character Selection subScreen
    void CharacterSelectionInput()
    {
        if (DataManager.instance.isMulti)
        {
            if (playerIndex == DataManager.instance.localPlayerIndex - 1)
            {
                //Switch between class
                if (Input.GetKeyDown(KeyCode.E))
                {
                    DataManager.instance.isPlayerReady[playerIndex] = false;
                    classSelect.GetComponent<ClassSelection>().SwitchingClass(choosedClass, playerIndex, true, false);
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].SendNotReadyToBeginMessage();
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdChangeModel(DataManager.instance.localPlayerIndex - 1);
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdPlayerReady(DataManager.instance.localPlayerIndex - 1, false);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    DataManager.instance.isPlayerReady[playerIndex] = false;
                    classSelect.GetComponent<ClassSelection>().SwitchingClass(choosedClass, playerIndex, true, false);
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].SendNotReadyToBeginMessage();
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdChangeModel(DataManager.instance.localPlayerIndex - 1);
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdPlayerReady(DataManager.instance.localPlayerIndex - 1, false);
                }

                //Return
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (typeScreen == TypeScreen.screenGift)
                    {
                        nextButtons[1].GetComponent<Text>().color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
                        typeScreen = TypeScreen.screenClass;
                        StartCoroutine(SetEventSystemCursor(name + "/SkillsScreen/Skills/AutoAttack"));
                    }
                    else if (typeScreen == TypeScreen.screenClass)
                    {
                        ShowDirector.instance.hasSwitchPos = false;
                        GameObject.Find("Canvas MenuScene").GetComponent<CanvasMenu>().waitForDisconnected(1.0f);
                        GetComponentInParent<SelectionScreens>().FadeAllCanvas();
                    }
                }

                //Toggle Help 
                if (Input.GetKey(KeyCode.Tab))
                {
                    isToolTipsToggled = true;
                }
                else
                {
                    isToolTipsToggled = false;
                }
            }
        }
        else
        {
            //KEYBOARD 
            if (playerIndex == 0)
            {
                //Switch between class
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SoundManager.instance.PlaySound(78, 0.2f, AudioType.SFX);
                    choosedClass = classSelect.GetComponent<ClassSelection>().SwitchingClass(choosedClass, playerIndex);
                    DataManager.instance.isPlayerReady[playerIndex] = false;
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    SoundManager.instance.PlaySound(78, 0.2f, AudioType.SFX);
                    choosedClass = classSelect.GetComponent<ClassSelection>().SwitchingClass(choosedClass, playerIndex);
                    DataManager.instance.isPlayerReady[playerIndex] = false;
                }

                //Return
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (typeScreen == TypeScreen.screenGift)
                    {
                        nextButtons[1].GetComponent<Text>().color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
                        typeScreen = TypeScreen.screenClass;
                        StartCoroutine(SetEventSystemCursor(name + "/SkillsScreen/Skills/AutoAttack"));

                    }
                    else if (typeScreen == TypeScreen.screenClass)
                    {
                        ShowDirector.instance.hasSwitchPos = false;
                        GetComponentInParent<SelectionScreens>().FadeAllCanvas();
                    }
                }

                //Toggle Help 
                if (Input.GetKey(KeyCode.Tab))
                {
                    isToolTipsToggled = true;
                }
                else
                {
                    isToolTipsToggled = false;
                }
            }

            if (playerIndex != 0)
            {
                //CONTROLLER
                for (int i = 1; i < DataManager.instance.currentNbrPlayer; i++)
                {

                    string ControllerReturn = "Controller_" + i + "_ButtonB";
                    string ControllerRightButton = "Controller_" + i + "_RightButton";
                    string ControllerLeftButton = "Controller_" + i + "_LeftButton";
                    string ControllerToggleHelp = "Controller_" + i + "_ButtonY";

                    if (playerIndex == i)
                    {
                        //Switch between class
                        if (Input.GetButtonDown(ControllerRightButton))
                        {
                            SoundManager.instance.PlaySound(78, 0.2f, AudioType.SFX);
                            choosedClass = classSelect.GetComponent<ClassSelection>().SwitchingClass(choosedClass, playerIndex);
                            DataManager.instance.isPlayerReady[playerIndex] = false;                        }
                        if (Input.GetButtonDown(ControllerLeftButton))
                        {
                            SoundManager.instance.PlaySound(78, 0.2f, AudioType.SFX);
                            choosedClass = classSelect.GetComponent<ClassSelection>().SwitchingClass(choosedClass, playerIndex);
                            DataManager.instance.isPlayerReady[playerIndex] = false;
                        }

                        //Return
                        if (Input.GetButtonDown(ControllerReturn))
                        {
                            if (typeScreen == TypeScreen.screenGift)
                            {
                                nextButtons[1].GetComponent<Text>().color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
                                typeScreen = TypeScreen.screenClass;
                                StartCoroutine(SetEventSystemCursor(name + "/SkillsScreen/Skills/AutoAttack"));
                            }
                            else if (typeScreen == TypeScreen.screenClass)
                            {
                                ShowDirector.instance.hasSwitchPos = false;
                                GetComponentInParent<SelectionScreens>().FadeAllCanvas();
                            }
                        }

                        //Toggle Help 
                        if (Input.GetButton(ControllerToggleHelp))
                        {
                            isToolTipsToggled = true;
                        }
                        else
                        {
                            isToolTipsToggled = false;
                        }
                    }
                }
            }
        }
    }

    //Set player as READY and color ReadyButton accordingly
    public void NextButton()
    {
        //When player is in SkillMenu
        if (typeScreen == TypeScreen.screenClass)
        {
            SoundManager.instance.PlaySound(6, 1.0f, AudioType.SFX);

            typeScreen = TypeScreen.screenGift;

            StartCoroutine(SetEventSystemCursor(name + "/GiftScreen/Gifts/Gift 1"));
        }

        //When player is in GiftMenu
        else if (typeScreen == TypeScreen.screenGift)
        {
            SoundManager.instance.PlaySound(6, 1.0f, AudioType.SFX);
        
            if (!DataManager.instance.isPlayerReady[playerIndex])
            {
                if (choosedClass == DataManager.TypeClass.Warrior)
                {
                    SoundManager.instance.PlaySound(10, 1.0f, AudioType.Voice);
                }
                else if (choosedClass == DataManager.TypeClass.Archer)
                {
                    SoundManager.instance.PlaySound(25, 1.0f, AudioType.Voice);
                }
            }

            if (DataManager.instance.isMulti)
            {
                if (DataManager.instance.isPlayerReady[DataManager.instance.localPlayerIndex - 1])
                {
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdPlayerReady(DataManager.instance.localPlayerIndex - 1, false);
                    GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].SendNotReadyToBeginMessage();
                }
                else
                {
                    if (giftCount == 3)
                    {
                        GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdPlayerReady(DataManager.instance.localPlayerIndex - 1, true);
                        GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].SendReadyToBeginMessage();
                    }
                    else
                    {
                        transform.Find("GiftScreen").GetComponent<GiftsMenu>().StartCoroutineShowWarning();
                        nextButtons[1].GetComponent<Text>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                    }  
                }
            }
            else
            {
                if (giftCount == 3)
                {
                    DataManager.instance.isPlayerReady[playerIndex] = !DataManager.instance.isPlayerReady[playerIndex];
                }
                else
                {
                    transform.Find("GiftScreen").GetComponent<GiftsMenu>().StartCoroutineShowWarning();
                    nextButtons[1].GetComponent<Text>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                }
            }
        }
    }

    //Fonction that reinitialise the Class screen Panel when it is leaved
    public void Reinitialisation()
    {
        if (DataManager.instance.isMulti)
            typeScreen = TypeScreen.screenGift;
        else
            typeScreen = TypeScreen.screenClass;

        hasChangedScreen = false;

        //All player select warrior by default
        choosedClass = DataManager.TypeClass.Warrior;

        for (int i = 0; i < (int)DataManager.TypeClass.nbClass; i++)
        {
            championModel.transform.GetChild(i).gameObject.SetActive(false);
        }
        //DataManager.instance.player[playerIndex].gameObject.SetActive(false);
        DataManager.instance.player[playerIndex] = championModel.transform.GetChild((int)DataManager.TypeClass.Warrior).GetComponent<Player>();
        DataManager.instance.player[playerIndex].gameObject.SetActive(true);

        //Reset Gift
        if (giftCount > 0)
        {
            classScreen[(int)TypeScreen.screenGift].GetComponent<GiftsMenu>().ResetGift();
        }

        //Reset Ready button
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            DataManager.instance.isPlayerReady[i] = false;
            nextButtons[1].GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }


        if (typeScreen != TypeScreen.screenGift)
        {
            DataManager.instance.isPlayerReady[playerIndex] = false;
        }
    }

    //Fonction that copy selected gift from Gift selection screen into Datamanager gift player array
    void SaveGift()
    {
        for (int i = 0; i < selectedGifts.Length; i++)
        {
            if (DataManager.instance.isMulti)
            {
                GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].CmdSaveGifts(selectedGifts[i], DataManager.instance.localPlayerIndex - 1, i);
            }
            else
                DataManager.instance.giftPlayer[playerIndex, i] = selectedGifts[i];
        }
    }

    //Display multiple tooltips and icon showing how to interact with character selection screen
    //void DisplayHelpButton()
    //{
    //    if (isHelpToggled)
    //    {
    //        if (typeScreen == TypeScreen.screenClass)
    //        {
    //            helpButtons[0].SetActive(true);
    //            helpButtons[1].SetActive(true);
    //        }
    //        if (typeScreen == TypeScreen.screenGift)
    //        {
    //            helpButtons[0].SetActive(true);
    //            helpButtons[1].SetActive(false);
    //        }
    //    }
    //    else
    //    {
    //        if (typeScreen == TypeScreen.screenClass)
    //        {
    //            helpButtons[0].SetActive(false);
    //            helpButtons[1].SetActive(false);
    //        }
    //        if (typeScreen == TypeScreen.screenGift)
    //        {
    //            helpButtons[0].SetActive(false);
    //            helpButtons[1].SetActive(false);
    //        }
    //    }
    //}

    //Switch between skill menu and gift selection
    void DisplayScreen()
    {
        if (typeScreen == TypeScreen.screenClass)
        {
            classScreen[0].SetActive(true);
            classScreen[1].SetActive(false);

        }
        if (typeScreen == TypeScreen.screenGift)
        {
            classScreen[0].SetActive(false);
            classScreen[1].SetActive(true);
        }
    }

    void ReadyButtonDisplay()
    {
        if (typeScreen == TypeScreen.screenGift)
        {
            if (giftCount == 3)
            {
                //ColorBlock cb = new Color(0.0f, 1.0f, 0.0f, 1.0f);

                ColorBlock newColor = nextButtons[1].GetComponent<Button>().colors;
                newColor.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                nextButtons[1].GetComponent<Button>().colors = newColor;

                //nextButtons[1].GetComponent<ColorBlock>().pressedColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            }
            else
            {
                ColorBlock newColor = nextButtons[1].GetComponent<Button>().colors;
                newColor.pressedColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                nextButtons[1].GetComponent<Button>().colors = newColor;
                //nextButtons[1].GetComponent<ColorBlock>().pressedColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);

                //nextButtons[1].GetComponent<Button>().interactable = false;
            }
        }

        if (DataManager.instance.isPlayerReady[playerIndex] == true)
        {
            nextButtons[1].GetComponent<Text>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            if (DataManager.instance.isMulti)
            {
                if (playerIndex == DataManager.instance.localPlayerIndex - 1)
                    SaveGift();
            }
            else
                SaveGift();
        }
        else
        {
            nextButtons[1].GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }


    void GiftPresetManager()
    {
        //Setup the 3 first gift of the list
        if (!giftsPreselected && DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
            StartCoroutine(classScreen[(int)TypeScreen.screenGift].GetComponent<GiftsMenu>().SetGiftPreset(
            DataManager.TypeGift.Heal, DataManager.TypeGift.Immune, DataManager.TypeGift.PowerShield
            ));
        }
    }

    IEnumerator SetEventSystemCursor(string path)
    {
        yield return new WaitForSeconds(0.01f);

        playerEventSystem.SetSelectedGameObject(GameObject.Find(path));
        hasSelectedAButton = true;
    }
   

    IEnumerator FadeInCanvas()
    {
        GetComponent<CanvasGroup>().alpha = 0.0f;

        if (DataManager.instance.isMulti)
            playerEventSystem = canvasMenu.GetComponent<CanvasMenu>().EventSystem[0];
        else
            playerEventSystem = canvasMenu.GetComponent<CanvasMenu>().EventSystem[playerIndex];

        if (playerIndex < DataManager.instance.currentNbrPlayer)
        {
            Reinitialisation();         
        }

        yield return new WaitForSeconds(0.5f);

        while (GetComponent<CanvasGroup>().alpha < 1.0f)
        {
            GetComponent<CanvasGroup>().alpha += 0.1f;
            yield return new WaitForSeconds(0.03f);
        }
    }
}
