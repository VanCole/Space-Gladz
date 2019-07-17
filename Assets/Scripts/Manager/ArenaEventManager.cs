using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class ArenaEventManager : NetworkBehaviour
{
    [SerializeField]
    VerticalBeamGenerator verticalBeam;

    [SerializeField]
    HorizontalLaserGenerator HorizontalBeam;

    [SerializeField]
    GameObject defenseDome;

    public GameObject activeDome;

    public bool launchVerticalBeam;
    public bool launchHorizontalBeam;

    public bool isEventActive;

    enum TypeEvent
    {
        VLaser,
        Dome,
        HLaser
    }
    TypeEvent typeEvent;

    private void Awake()
    {
        name = "ArenaEventGenerator";
    }


    // Use this for initialization
    void Start()
    {
        launchVerticalBeam = false;
        launchHorizontalBeam = false;
        isEventActive = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (DataManager.instance.isLoadingDone)
        {
            if (!isEventActive && (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer)))
            {
                StartCoroutine(GenerateEvent());
            }
        }
        

#if __DEBUG
        if (Input.GetKeyDown(KeyCode.I))
        {
            launchVerticalBeam = true;
            if (DataManager.instance.isMulti)
                RpcLaunchVerticalBeam();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            launchHorizontalBeam = true;
            if (DataManager.instance.isMulti)
            {
                int tempRngNumber = Random.Range(0, 3);
                int[] tempRngLaser = new int[3];
                for (int i = 0; i < tempRngNumber; i++)
                {
                    tempRngLaser[i] = Random.Range(0, 6);
                }
                RpcLaunchHorizontalBeam(tempRngNumber, tempRngLaser);
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnDefenseDome();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            StopEvent();
        }
#endif
    }


    public void StopEvent()
    {
        StopAllCoroutines();
        Destroy(activeDome);
        verticalBeam.StopVerticalLaser();
        HorizontalBeam.StopHorizontalLaser();
    }


    IEnumerator GenerateEvent()
    {
        float timerRandom = Random.Range(20, 30);
        isEventActive = true;

        yield return new WaitForSeconds(timerRandom);

        int randomEvent;

        if (TerrainGenerator.instance.currentRadius <= 16)
        {
            randomEvent = Random.Range((int)TypeEvent.VLaser, (int)TypeEvent.HLaser + 1); //HLaser included
        }
        else
        {
            randomEvent = Random.Range((int)TypeEvent.VLaser, (int)TypeEvent.HLaser);  //HLaser excluded, all event but HLaser can spawn
        }

        switch (randomEvent)
        {
            case (int)TypeEvent.VLaser:
                {
                    launchVerticalBeam = true;
                    if (DataManager.instance.isMulti)
                        RpcLaunchVerticalBeam();
                    break;
                }
            case (int)TypeEvent.Dome:
                {
                    SpawnDefenseDome();
                    break;
                }

            case (int)TypeEvent.HLaser:
                {
                    launchHorizontalBeam = true;
                    if (DataManager.instance.isMulti)
                    {
                        int tempRngNumber = Random.Range(0, 3);
                        int[] tempRngLaser = new int[3];
                        for (int i = 0; i < tempRngNumber; i++)
                        {
                            tempRngLaser[i] = Random.Range(0, 6);
                        }
                        RpcLaunchHorizontalBeam(tempRngNumber,tempRngLaser);
                    }
                    break;
                }

            default:
                break;
        }
    }

    [ClientRpc]
    void RpcLaunchVerticalBeam()
    {
        launchVerticalBeam = true;
    }

    [ClientRpc]
    void RpcLaunchHorizontalBeam(int _rngNumber, int[] _rngLaser)
    {
        GetComponentInChildren<HorizontalLaserGenerator>().rngNumberNet = _rngNumber;
        GetComponentInChildren<HorizontalLaserGenerator>().rngLaserNet = _rngLaser;
        
        launchHorizontalBeam = true;
    }

    void SpawnDefenseDome()
    {
        PlayRandomEventSound();

        //Unique
        int indexTile = -1;
        List<HexIndex> randomList = TerrainGenerator.instance.grid.GetRandom();
        GameObject tile;
        do
        {
            indexTile++;

        } while (HexIndex.Distance(HexIndex.origin, randomList[indexTile]) > 15); //TerrainGenerator.instance.currentRadius

        TerrainGenerator.instance.GetGameObject(randomList[indexTile], out tile);
        if (DataManager.instance.isMulti)
            RpcSpawnDefenseDome(tile.transform.position);
        else
        {
            GameObject dome;
            dome = Instantiate(defenseDome, tile.transform.position, Quaternion.identity);
            dome.transform.parent = transform;
        }

        //int nbrDome = Random.Range(0, 7);

        //int indexTile = -1;
        //List<HexIndex> randomList = TerrainGenerator.instance.grid.GetRandom();

        //for (int i = 0; i <= nbrDome; i++)
        //{
        //    GameObject tile;

        //    do
        //    {
        //        indexTile++;

        //    } while (HexIndex.Distance(HexIndex.origin, randomList[indexTile]) > 15); //TerrainGenerator.instance.currentRadius

        //    TerrainGenerator.instance.GetGameObject(randomList[indexTile], out tile);

        //    GameObject dome = Instantiate(giftDome, tile.transform.position, Quaternion.identity);
        //}

    }
    [ClientRpc]
    void RpcSpawnDefenseDome(Vector3 _pos)
    {
        GameObject dome = Instantiate(defenseDome, _pos, Quaternion.identity);
    }

    public void PlayRandomEventSound()
    {
        int randomSoundEvent = Random.Range(0, 2);

        if (randomSoundEvent == 0)
        {
            SoundManager.instance.PlaySound(43, 1.0f, AudioType.Voice);
        }
        else
        {
            SoundManager.instance.PlaySound(44, 1.0f, AudioType.Voice);
        }
    }


    //----------------------------Custom functions used by others things, but it's the only spawned manager so it's easier to use them here ---------------------


    public IEnumerator disconnect()
    {
        if(isServer)
            RpcDisconnectFromServer(0);

        if (isClient)
        {
            DataManager.instance.networkFromGame = true;
            Prototype.NetworkLobby.LobbyManager.s_Singleton.StopClientClbk();
        }

        yield return new WaitForSeconds(1);

        if(isServer)
            RpcDisconnectFromServer(1);
    }

    [ClientRpc]
    void RpcDisconnectFromServer(int _step)
    {
        if (_step == 0)
        {
            DataManager.instance.networkFromGame = true;
            if (isClient)
                Prototype.NetworkLobby.LobbyManager.s_Singleton.StopClientClbk();
        }
        if (_step == 1)
            Prototype.NetworkLobby.LobbyManager.s_Singleton.StopHostClbk();
    }

    [Command]
    public void CmdReturnToLoby()
    {
        StartCoroutine(ReturnToLoby());
    }

    IEnumerator ReturnToLoby()
    {
        RpcReturnToLoby();
        yield return new WaitForSeconds(0.3f);
        Prototype.NetworkLobby.LobbyManager.s_Singleton.ServerReturnToLobby();
    }

    [ClientRpc]
    void RpcReturnToLoby()
    {
        DataManager.instance.networkFromGame = true;
       // DataManager.instance.isMatchDone = false;
    }

    public IEnumerator charSelection()
    {
        if (isServer)
        {
            RpcReturnToCharSelection(0);
            Prototype.NetworkLobby.LobbyManager.s_Singleton.ServerReturnToLobby();
        }

        yield return new WaitForSeconds(1);

        RpcReturnToCharSelection(1);

    }

    [ClientRpc]
    void RpcReturnToCharSelection(int _step)
    {
        if (_step == 0)
        {
            Destroy(GameObject.Find("DataManager"));
            Destroy(GameObject.Find("SoundManager"));
        }
        if (_step == 1)
        {
            Prototype.NetworkLobby.LobbyManager.s_Singleton.initCharSelection();
        }
    }

    public void setStatsNames()
    {
        if(isServer && DataManager.instance.isMulti)
            RpcSetStatsNames(DataManager.instance.PlayerNames);
        else if (!DataManager.instance.isMulti)
            LocalSetStatsNames(DataManager.instance.PlayerNames);
    }

    [ClientRpc]
    void RpcSetStatsNames(string[] _names)
    {
        StartCoroutine(WaitforSetStatsNames(_names));
    }

    IEnumerator WaitforSetStatsNames(string[] _names)
    {
        yield return new WaitUntil(() => DataManager.instance.isLoadingDone );

        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            GameObject.Find("CanvasUI").transform.GetComponentInChildren<PersonalStatPanel>().transform.parent.transform.GetChild(i).gameObject.
                GetComponent<PersonalStatPanel>().setName(_names[i]);
        }
    }


    void LocalSetStatsNames(string[] _names)
    {
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            GameObject.Find("CanvasUI").transform.GetComponentInChildren<PersonalStatPanel>().transform.parent.transform.GetChild(i).gameObject.
                GetComponent<PersonalStatPanel>().setName(_names[i]);
        }
    }

}
