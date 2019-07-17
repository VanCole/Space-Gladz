using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameUI : MonoBehaviour
{
    [SerializeField]
    GameObject playersUI;

    [SerializeField]
    GameObject networkPlayerUI;

    [SerializeField]
    GameObject endScreen;

    [SerializeField]
    GameObject pauseScreen;

    [SerializeField]
    GameObject[] player = new GameObject[4];

    [SerializeField]
    GameObject[] buttonsStatScreen;

    [SerializeField]
    GameObject[] buttonsPauseScreen;

    [SerializeField]
    GameObject loadingScreen;

    public AudioSource sourceUI; // ambiance de la foule    bool hasFade = false;

    bool hasFade = false;

    bool showPauseMenu = false;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < player.Length; i++)
        {
            player[i].SetActive(false);
        }
        endScreen.SetActive(false);


        StartCoroutine(FadeInCanvas(playersUI, 5.0f));

        if (DataManager.instance.isMulti)
        {
            StartCoroutine(FadeInCanvas(networkPlayerUI, 5.0f));
        }
        else
        {
            StartCoroutine(FadeInCanvas(playersUI, 5.0f));
        }
        endScreen.GetComponent<CanvasGroup>().alpha = 0.0f;


        //Set Red Circle cursor
        Cursor.SetCursor(DataManager.instance.mouseCursor[1],
                         new Vector2(DataManager.instance.mouseCursor[1].width / 2.0f + 2.0f, DataManager.instance.mouseCursor[1].height / 2.0f + 5.0f),
                         CursorMode.ForceSoftware);
    }

    private void Update()
    {
        DisplayEndMatchScreen();

        DisplayPlayerUI();

        PauseMenu();

        LoadingScreen();

        if (Input.GetKeyDown(KeyCode.Escape) && !DataManager.instance.isMatchDone)
        {
            showPauseMenu = !showPauseMenu;
        }

        if (DataManager.instance.isMatchDone && !hasFade)
        {
            hasFade = true;
            StartCoroutine(FadeInCanvas(endScreen, 5.0f));

            if (DataManager.instance.isMulti)
            {
                StartCoroutine(FadeOutCanvas(networkPlayerUI));
            }
            else
            {
                StartCoroutine(FadeOutCanvas(playersUI));
            }

        }
    }

    void PauseMenu()
    {
        if (!DataManager.instance.isMatchDone)
        {
            if (DataManager.instance.isMulti)
            {
                buttonsPauseScreen[0].SetActive(false);
                buttonsPauseScreen[1].SetActive(true);
            }
            else
            {
                buttonsPauseScreen[0].SetActive(true);
                buttonsPauseScreen[1].SetActive(false);
            }

            if (showPauseMenu)
            {
                //Set default cursor
                Cursor.SetCursor(DataManager.instance.mouseCursor[0], Vector2.zero, CursorMode.Auto);

                pauseScreen.SetActive(true);
                if (!DataManager.instance.isMulti)
                {
                    Time.timeScale = 0;
                }
                if (DataManager.instance.isMatchDone)
                {
                    showPauseMenu = false;
                }
            }
            else
            {
                //Set Red Circle cursor
                Cursor.SetCursor(DataManager.instance.mouseCursor[1],
                                 new Vector2(DataManager.instance.mouseCursor[1].width / 2.0f + 2.0f, DataManager.instance.mouseCursor[1].height / 2.0f + 5.0f),
                                 CursorMode.ForceSoftware);

                pauseScreen.SetActive(false);
                Time.timeScale = 1.0f;
            }
        }       
    }

    void DisplayPlayerUI()
    {
        if (!DataManager.instance.isMatchDone)
        {
            if (DataManager.instance.isMulti)
            {
                networkPlayerUI.SetActive(true);
                playersUI.SetActive(false);
            }
            else
            {
                networkPlayerUI.SetActive(false);
                playersUI.SetActive(true);
                for (int i = 0; i < player.Length; i++)
                {
                    if (player[i].GetComponent<PlayerUI>().playerIndex < DataManager.instance.currentNbrPlayer)
                    {
                        player[i].SetActive(true);
                    }
                    else
                    {
                        player[i].SetActive(false);
                    }
                }
            }
        }
    }

    void LoadingScreen()
    {
        if (DataManager.instance.isMulti)
        {
            if (!DataManager.instance.isLoadingDone)
            {
                loadingScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
            }
            else
            {
                StartCoroutine(FadeOutCanvas(loadingScreen));
            }
        }
        else
        {
            loadingScreen.SetActive(false);
        }
    }



    void DisplayEndMatchScreen()
    {
        if (DataManager.instance.isMatchDone)
        {
            //Set Red Circle cursor
            Cursor.SetCursor(DataManager.instance.mouseCursor[0], Vector2.zero, CursorMode.Auto);


            //playersUI.SetActive(false);
            //networkPlayerUI.SetActive(false);

            if (DataManager.instance.isMulti)
            {
                buttonsStatScreen[0].SetActive(false);
                buttonsStatScreen[1].SetActive(true);
            }
            else
            {
                buttonsStatScreen[0].SetActive(true);
                buttonsStatScreen[1].SetActive(false);
            }

            endScreen.SetActive(true);
        }

        //if (DataManager.instance.isMatchDone)
        //{
        //    string s = GameManager.instance.winnerName;
        //    endScreen.transform.Find("Victory Name").GetComponent<Text>().text = s + " is victorious!";
        //}
    }

    public void LoadScene(int buildIndex)
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);

        sourceUI = GameManager.instance.source; // ambiance de la foule
        SoundManager.instance.StopSound(sourceUI);

        if (DataManager.instance.isMulti)
        {
            StartCoroutine(GameObject.Find("ArenaEventGenerator").GetComponent<ArenaEventManager>().disconnect());
        }
        else
        {
            DataManager.instance.gameState = DataManager.GameState.mainMenu;
            DataManager.instance.isMatchDone = false;
            SceneManager.LoadScene(buildIndex);
        }
    }


    public void BackToCharacterSelection()
    {
        SoundManager.instance.PlaySound(5, 0.5f, AudioType.SFX);

        sourceUI = GameManager.instance.source; // ambiance de la foule
        SoundManager.instance.StopSound(sourceUI);

        if (DataManager.instance.isMulti)
        {
            StartCoroutine(GameObject.Find("ArenaEventGenerator").GetComponent<ArenaEventManager>().disconnect());
        }
        else
        {
            DataManager.instance.gameState = DataManager.GameState.charSelection;
            DataManager.instance.isMatchDone = false;
            SceneManager.LoadScene(0);
        }
    }

    public void LeavePauseButton()
    {
        showPauseMenu = false;
        Time.timeScale = 1.0f;
    }



    public IEnumerator FadeOutCanvas(GameObject canvas)
    {
        while (canvas.GetComponent<CanvasGroup>().alpha > 0.0f)
        {
            canvas.GetComponent<CanvasGroup>().alpha -= 0.1f;
            yield return new WaitForSeconds(0.03f);
        }
        canvas.SetActive(false);
    }

    public IEnumerator FadeInCanvas(GameObject canvas, float delay)
    {
        canvas.GetComponent<CanvasGroup>().alpha = 0.0f;
        canvas.SetActive(true);

        yield return new WaitForSeconds(delay);

        while (canvas.GetComponent<CanvasGroup>().alpha < 1.0f)
        {
            canvas.GetComponent<CanvasGroup>().alpha += 0.1f;
            yield return new WaitForSeconds(0.03f);
        }
    }
}
