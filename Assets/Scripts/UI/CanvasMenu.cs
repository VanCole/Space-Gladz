using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CanvasMenu : MonoBehaviour
{
    [SerializeField]
    GameObject[] UiScreen;

    [SerializeField]
    GameObject gameChoice;
 
    [SerializeField]
    GameObject ModelScreen;

    public EventSystem[] EventSystem = new EventSystem[4];

    bool gameChoiceShown = false;

    public AudioSource source;

    //Button interaction
    public void MenuToCharSelection()
    {
        DataManager.instance.gameState = DataManager.GameState.charSelection;
    }

    public void MenuButton()
    {
        gameChoiceShown = !gameChoiceShown;
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
    }

    public void LocalButton()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        DataManager.instance.isMulti = false;
        DataManager.instance.gameState = DataManager.GameState.charSelection;
        StartCoroutine(SetEventSystemCursor());
    }

    public void MultiButton()
    {
        //ShowDirector.instance.StartCoroutineSwitchToMenuPosition(ShowDirector.instance.posMenu[1].position, 15.0f);

        ShowDirector.instance.hasSwitchPos = false;

        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        DataManager.instance.isMulti = true;
        DataManager.instance.gameState = DataManager.GameState.lobbyMenu;
        gameChoiceShown = false;


        //StartCoroutine(SetEventSystemCursor());
    }

    public void CharSelectionToMenu()
    {
        DataManager.instance.gameState = DataManager.GameState.mainMenu;

        //EventSystem.SetSelectedGameObject(GameObject.Find("Button Match"));
    }

    public void MenuToCredit()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        DataManager.instance.gameState = DataManager.GameState.credit;
    }

    public void CreditToMenu()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        DataManager.instance.gameState = DataManager.GameState.mainMenu;
    }

    public void MenuToOption()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        DataManager.instance.gameState = DataManager.GameState.option;

        //EventSystem.SetSelectedGameObject(GameObject.Find("Button Return"));
    }

    public void MenuToControl()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        DataManager.instance.gameState = DataManager.GameState.ControlScreen;
    }

    public void OptionToMenu()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        DataManager.instance.gameState = DataManager.GameState.mainMenu;

        //EventSystem.SetSelectedGameObject(GameObject.Find("Button Match"));
    }

    public void LobbyToMenu()
    {
        ShowDirector.instance.hasSwitchPos = false;

        DataManager.instance.gameState = DataManager.GameState.mainMenu;

        //EventSystem.SetSelectedGameObject(GameObject.Find("Button Match"));
    }

    public void QuitButton()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);
        Application.Quit();
    }

    public void LoadScene(int buildIndex)
    {
        //SoundManager.instance.StopSound(source);
        //SoundManager.instance.StopSound(SoundManager.instance.test);
        SceneManager.LoadScene(buildIndex);

        if (buildIndex == 0)
        {
            SoundManager.instance.StopSound(SoundManager.instance.ambientSound); // ambiance de la foule
        }
        /*if(buildIndex == 0)
        {
            source = SoundManager.instance.PlaySound(80, 0.2f, true);
        }*/
    }


    // Use this for initialization
    void Start()
    {

        Time.timeScale = 1.0f;
        gameChoiceShown = false;
        //DataManager.instance.gameState = DataManager.GameState.mainMenu;       
        //source = SoundManager.instance.PlaySound(80, 0.2f, true);
        SoundManager.instance.menuMusic = SoundManager.instance.PlaySound(80, 0.6f, true, AudioType.Music);

        Cursor.SetCursor(DataManager.instance.mouseCursor[0], Vector2.zero, CursorMode.Auto);


    }

    void DisplayGameChoiceButton()
    {
        if (gameChoiceShown)
        {
            gameChoice.SetActive(true);
        }
        else
        {
            gameChoice.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!DataManager.instance.isMulti && DataManager.instance.currentNbrPlayer > 1 && CanGameStart())
        {
            LoadScene(1);
        }

       // lobbyManager.GetComponent<Prototype.NetworkLobby.LobbyManager>().gamePlayerPrefab = Resources.Load("") as GameObject;

        DisplayGameChoiceButton();

        if (DataManager.instance.gameState == DataManager.GameState.mainMenu)
        {
            GameObject.Find("EventSystems").transform.GetChild(0).gameObject.SetActive(true);
            GameObject.Find("EventSystems").transform.GetChild(1).gameObject.SetActive(false);

            DataManager.instance.currentNbrPlayer = 1;

            UiScreen[(int)DataManager.GameState.mainMenu].SetActive(true);
            UiScreen[(int)DataManager.GameState.option].SetActive(false);
            UiScreen[(int)DataManager.GameState.credit].SetActive(false);
            UiScreen[(int)DataManager.GameState.ControlScreen].SetActive(false);


            for (int i = 0; i < DataManager.instance.player.Length; i++)
            {
                DataManager.instance.player[i] = null;
            }
        }
        else if (DataManager.instance.gameState == DataManager.GameState.lobbyMenu)
        {
            GameObject.Find("EventSystems").transform.GetChild(0).gameObject.SetActive(true);
            GameObject.Find("EventSystems").transform.GetChild(1).gameObject.SetActive(false);
            UiScreen[(int)DataManager.GameState.ControlScreen].SetActive(false);

        }
        else if (DataManager.instance.gameState == DataManager.GameState.option)
        {
            UiScreen[(int)DataManager.GameState.mainMenu].SetActive(false);
            UiScreen[(int)DataManager.GameState.option].SetActive(true);
            UiScreen[(int)DataManager.GameState.credit].SetActive(false);
            UiScreen[(int)DataManager.GameState.ControlScreen].SetActive(false);

            gameChoiceShown = false;

        }
        else if (DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
            GameObject.Find("EventSystems").transform.GetChild(0).gameObject.SetActive(false);
            GameObject.Find("EventSystems").transform.GetChild(1).gameObject.SetActive(true);

            UiScreen[(int)DataManager.GameState.mainMenu].SetActive(false);
            UiScreen[(int)DataManager.GameState.option].SetActive(false);
            UiScreen[(int)DataManager.GameState.credit].SetActive(false);
            UiScreen[(int)DataManager.GameState.ControlScreen].SetActive(false);

            gameChoiceShown = false;

        }
        else if (DataManager.instance.gameState == DataManager.GameState.credit)
        {
            UiScreen[(int)DataManager.GameState.mainMenu].SetActive(false);
            UiScreen[(int)DataManager.GameState.option].SetActive(false);
            UiScreen[(int)DataManager.GameState.credit].SetActive(true);
            UiScreen[(int)DataManager.GameState.ControlScreen].SetActive(false);

            gameChoiceShown = false;

        }
        else if (DataManager.instance.gameState == DataManager.GameState.ControlScreen)
        {
            UiScreen[(int)DataManager.GameState.mainMenu].SetActive(false);
            UiScreen[(int)DataManager.GameState.option].SetActive(false);
            UiScreen[(int)DataManager.GameState.credit].SetActive(false);
            UiScreen[(int)DataManager.GameState.ControlScreen].SetActive(true);

            gameChoiceShown = false;
        }

        MenuInput();

        //Debug.Log(EventSystem[1].currentSelectedGameObject);
    }


    //Check if all lpayer are ready
    private bool CanGameStart()
    {
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; ++i)
        {
            if (DataManager.instance.isPlayerReady[i] == false)
            {
                return false;
            }
        }
        return true;

    }


    void MenuInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (DataManager.instance.gameState == DataManager.GameState.option)
            {
                OptionToMenu();
            }

            if (DataManager.instance.gameState == DataManager.GameState.lobbyMenu)
            {
                LobbyToMenu();
            }

            //if (DataManager.instance.gameState == DataManager.GameState.charSelection)
            //{
            //    CharSelectionToMenu();
            //}
        }


        for (int i = 1; i < DataManager.instance.currentNbrPlayer; i++)
        {
            string ControllerCancel = "Controller_" + i + "_ButtonB";

            if (Input.GetButton(ControllerCancel))
            {
                if (DataManager.instance.gameState == DataManager.GameState.option)
                {
                    OptionToMenu();
                }
                //if (DataManager.instance.gameState == DataManager.GameState.charSelection)
                //{
                //    CharSelectionToMenu();
                //}
            }
        }      
    }



    public void waitForDisconnected(float timer)
    {
        StartCoroutine(cWaitForDisconnected(timer));
    }

    IEnumerator cWaitForDisconnected(float timer)
    {
        yield return new WaitForSeconds(timer);

        GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().backDelegate();
    }

    //Delay instantiated data recuperation (debug canvasUI <-> warrior skill set cooldown link)
    IEnumerator SetEventSystemCursor()
    {
        GameObject.Find("EventSystems").transform.GetChild(0).gameObject.SetActive(false);
        GameObject.Find("EventSystems").transform.GetChild(1).gameObject.SetActive(true);

        yield return new WaitForSeconds(0.01f);

        for (int i = 0; i < EventSystem.Length; i++)
        {
            string path = "Canvas CharSelection/CharSelect " + (i + 1) + "/SkillsScreen/Skills/AutoAttack";
            EventSystem[i].SetSelectedGameObject(GameObject.Find(path));
        }
    }
}
