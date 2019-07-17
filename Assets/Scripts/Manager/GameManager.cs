using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-9)]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int winnerIndex;
    public int nbrAlivePlayer;
    public string winnerName;

    public AudioSource source;

    float timerSpecialSound = 0.0f;
    float timerHealthAtTimeT = 0.0f;

    float[] currentHealthAtTimeT = new float[4];
    int counterNoDamage = 0;

    public bool gameInitialized = false;

    public List<bool> isPlayerInGame = new List<bool>();

    public bool serverOK = false;

    // Use this for initialization
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        gameInitialized = false;
        DataManager.instance.gameState = DataManager.GameState.inArena;
        StartCoroutine(InitGameCoroutine());
    }

    IEnumerator InitGameCoroutine()
    {
        Debug.Log("Start Initilization");
        // Wait for all player instantiation before init game
        yield return new WaitUntil(() =>
        {
            if (DataManager.instance.isMulti)
            {
                if (TerrainGenerator.instance)
                {
                    if (TerrainGenerator.instance.isServer)
                    {
                        return isPlayerInGame.Count >= DataManager.instance.currentNbrPlayer;
                    }
                    else
                    {
                        return serverOK;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return GameObject.FindGameObjectsWithTag("Player").Length >= DataManager.instance.currentNbrPlayer;
            }
        });

        TerrainGenerator.instance.LoadFinished();

        Debug.Log("Players ready");

        InitGame();
        Debug.Log("Game initialized");
        gameInitialized = true;

        yield return new WaitForSeconds(2.0f);

        DataManager.instance.isLoadingDone = true;
        Debug.Log("Start Game");
    }
    

    // Update is called once per frame
    void Update()
    {
        timerSpecialSound += Time.deltaTime;
        timerHealthAtTimeT += Time.deltaTime;

        if (!DataManager.instance.isMatchDone && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            SpecialSound();
        }

        //if (DataManager.instance.isMulti)
        //{
        //    //if (!ArePlayersConnected())
        //    //    Time.timeScale = 0;
        //    //else if (ArePlayersConnected() && Time.timeScale != 1)
        //    //    Time.timeScale = 1;

        //    if (!DataManager.instance.player[1])
        //        InitGame();
        //}

        if (gameInitialized)
        {
            WinCondition();
        }
    }

    bool ArePlayersConnected()
    {
        GameObject[] tempPlayer = GameObject.FindGameObjectsWithTag("Player");

        return tempPlayer.Length == DataManager.instance.player.Length;
    }

    void InitGame()
    {
        nbrAlivePlayer = DataManager.instance.currentNbrPlayer;

        DataManager.instance.gameState = DataManager.GameState.inArena;
        GameObject[] tempPlayer = GameObject.FindGameObjectsWithTag("Player");

        SoundManager.instance.PlaySound(0, 1.0f, AudioType.Ambient);
        SoundManager.instance.PlaySound(54, 1.0f, AudioType.Ambient);

        if (DataManager.instance.isMulti)
        {
            source = SoundManager.instance.PlaySound(53, 0.8f, true, AudioType.Ambient); // ambiance de la foule
            //DataManager.instance.player[0].CmdPlaySound(1, 53, 1.0f, 0, 0, 0.0f, 2, true);
        }
        else
        {
        source = SoundManager.instance.PlaySound(53, 1.0f, true, AudioType.Ambient); // ambiance de la foule
        }

        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            if (DataManager.instance.player != null)
            {
                DataManager.instance.player[i] = tempPlayer[i].GetComponent<Player>();
                DataManager.instance.isPlayerReady[i] = false;
            }
        }

        DataManager.instance.isMatchDone = false;

        StartCoroutine(StartMatch());
    }


    void WinCondition()
    {
        //DRAW
        if (nbrAlivePlayer == 0)
        {

            if (DataManager.instance.isMatchDone == false)
            {
                if (DataManager.instance.gameState == DataManager.GameState.inArena)
                {
                    SoundManager.instance.PlaySound(42, 1.0f, AudioType.Voice);
                    SoundManager.instance.PlaySound(0, 1.0f, AudioType.Ambient);
                    SoundManager.instance.StopSound(source);
                
                    SoundManager.instance.PlaySound(53, 1.0f, true, AudioType.Ambient);
                    
                    DataManager.instance.isMatchDone = true;
                }

                //Clear Map
                TerrainGenerator.instance.collapsing = false;
                TerrainGenerator.instance.mapRegeneration = false;

                //TerrainGenerator.instance.GenerateEmptyTerrain();

                //Stop all random event
                GameObject.Find("ArenaEventGenerator").GetComponent<ArenaEventManager>().StopEvent();

                //Center Camera on winner
                CameraManager.instance.SetDrawTrackPosition();

                //Delete all gift 
                foreach (Gift g in GameObject.FindObjectsOfType<Gift>())
                {
                    Destroy(g.gameObject);
                }
            }
        }

        //VICTORY
        if (nbrAlivePlayer == 1)
        {
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                if (DataManager.instance.player[i].GetComponent<Player>().currentHealth > 0)
                {
                    winnerName = DataManager.instance.player[i].name;

                if (DataManager.instance.isMatchDone == false)
                {
                    if (DataManager.instance.gameState == DataManager.GameState.inArena)
                    {
                        SoundManager.instance.PlaySound(42, 1.0f, AudioType.Voice);
                        SoundManager.instance.PlaySound(0, 1.0f, AudioType.Ambient);
                        SoundManager.instance.StopSound(source);

                        /*if(DataManager.instance.isMulti)
                        {
                            DataManager.instance.player[i].GetComponent<Player>().CmdPlaySound(1, 53, 1.0f, 0, 0, 0.0f, 2, true);
                        }
                        else
                        {*/
                        SoundManager.instance.PlaySound(53, 1.0f, true, AudioType.Ambient);
                        //}

                        DataManager.instance.player[i].PlayWinSound();
                        DataManager.instance.isMatchDone = true;
                        //SoundManager.instance.StopSound(SoundManager.instance.ambientSound); // ambiance de la foule
                    }
                    //Disable movement on player
                    DataManager.instance.player[i].GetComponent<InputController>().enabled = false;

                    //Winner face the south (face win camera)
                    GameObject.Find("WinnerLookAtTarget").transform.position = new Vector3(DataManager.instance.player[i].gameObject.transform.position.x,
                                                                                           DataManager.instance.player[i].gameObject.transform.position.y,
                                                                                           DataManager.instance.player[i].gameObject.transform.position.z - 5.0f);

                    //Clear Map
                    TerrainGenerator.instance.GenerateEmptyTerrain();

                    //ShowDirector focus on winner
                    ShowDirector.instance.StartCoroutineSwitchToVictoriousPlayer(DataManager.instance.player[i].transform.position, 15.0f, DataManager.instance.player[i].gameObject);

                    //Stop all random event
                    GameObject.Find("ArenaEventGenerator").GetComponent<ArenaEventManager>().StopEvent();

                    //Center Camera on winner
                    CameraManager.instance.SetVictoryTrackPosition(DataManager.instance.player[i].gameObject);
                    DataManager.instance.player[i].currentHealth = 1000.0f;

                    //Delete all gift 
                    foreach (Gift g in GameObject.FindObjectsOfType<Gift>())
                    {
                        Destroy(g.gameObject);
                    }
                }
                }
            }
        }
    }

    IEnumerator StartMatch()
    {
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            DataManager.instance.player[i].GetComponent<InputController>().enabled = false;
            //DataManager.instance.player[i].rb.isKinematic = true;

        }


        yield return new WaitForSeconds(7.0f);

        //SoundManager.instance.PlaySound(0, 1.0f, AudioType.Ambient);
        //SoundManager.instance.PlaySound(54, 1.0f, AudioType.Ambient);

        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            DataManager.instance.player[i].GetComponent<InputController>().enabled = true;
            //DataManager.instance.player[i].rb.isKinematic = false;
            DataManager.instance.player[i].canAttack = true;
            DataManager.instance.player[i].canMove = true;
            DataManager.instance.player[i].canDodge = true;
        }
    }

    void SpecialSound()
    {
        if (timerHealthAtTimeT <= 0.2f)
        {
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                currentHealthAtTimeT[i] = DataManager.instance.player[i].currentHealth; // PV a t0
            }
        }

        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            if (DataManager.instance.player[i].currentHealth < currentHealthAtTimeT[i]) // no damage recu depuis timerHealthAtTimeT
            {
                timerHealthAtTimeT = 0.0f;
                timerSpecialSound = 0.0f;
                counterNoDamage = 0;
            }
        }

        if (timerSpecialSound >= 20.0f)
        {
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                if (currentHealthAtTimeT[i] == DataManager.instance.player[i].currentHealth)
                {
                    counterNoDamage++;
                }
            }

            if (counterNoDamage != DataManager.instance.currentNbrPlayer) // si perso touché, remise à 0 des timer de sons
            {
                timerHealthAtTimeT = 0.0f;
                timerSpecialSound = 0.0f;
                counterNoDamage = 0;
            }
            else                                                          // si tous perso ont recu 0 damage
            {
                SoundManager.instance.PlaySound(49, 1.0f, AudioType.Voice);
                timerHealthAtTimeT = 0.0f;
                timerSpecialSound = 0.0f;
                counterNoDamage = 0;
            }
        }
    }
}
