using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Reflection;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

//unity event
[System.Serializable]
public class OnInstantiation : UnityEvent<GameObject>
{
}


[DefaultExecutionOrder(-5)]
public class SpawnerManager : NetworkBehaviour
{
    public static SpawnerManager instance;

    bool instantitate = false;

    [SerializeField]
    GameObject[] spawner = new GameObject[4];

    [SerializeField]
    GameObject[] playableClass = new GameObject[4];

    [SerializeField]
    GameUI canvasUI;

    GameObject player;

    //[SerializeField] bool isNetwork = false;

    public OnInstantiation onInstantiation = new OnInstantiation();

    [Header("ONLY TO LAUNCH DIRECTLY FROM THE SCENE")]
    [SerializeField]
    bool noMenu = false;

    private void Awake()
    {
        if (DataManager.instance.isMulti)
        {
            int count = DataManager.instance.currentNbrPlayer;
            GameObject[] Pos = new GameObject[count];

            if (count == 2)
            {
                Pos[0] = Instantiate(new GameObject(), new Vector3(-17.32051f, -8.0f, 0.0f), Quaternion.identity);
                Pos[1] = Instantiate(new GameObject(), new Vector3(17.32051f, -8.0f, 0.0f), Quaternion.identity);
            }
            else if (count == 3)
            {
                Pos[0] = Instantiate(new GameObject(), new Vector3(-17.32051f, -8.0f, 0.0f), Quaternion.identity);
                Pos[1] = Instantiate(new GameObject(), new Vector3(8.660254f, -8.0f, -15.0f), Quaternion.identity);
                Pos[2] = Instantiate(new GameObject(), new Vector3(8.660254f, -8.0f, 15.0f), Quaternion.identity);
            }
            else if (count == 4)
            {
                Pos[0] = Instantiate(new GameObject(), new Vector3(-17.32051f, -8.0f, 0.0f), Quaternion.identity);
                Pos[1] = Instantiate(new GameObject(), new Vector3(17.32051f, -8.0f, 0.0f), Quaternion.identity);
                Pos[2] = Instantiate(new GameObject(), new Vector3(0, -8.0f, 18.0f), Quaternion.identity);
                Pos[3] = Instantiate(new GameObject(), new Vector3(0, -8.0f, -18.0f), Quaternion.identity);
            }

            for (int i = 0; i < count; i++)
            {
                Pos[i].name = "Spawn" + i;
                Pos[i].AddComponent<NetworkStartPosition>();
                Pos[i].transform.LookAt(new Vector3(0, -8.0f, 0));
            }
        }     
    }

    // Use this for initialization
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        if (!DataManager.instance.isMulti)
            InstantiateLocalPlayers();

    }


    [ClientRpc]
    void RpcSetSpawners(int _index, Vector3 _pos)
    {
        spawner[_index].transform.position = _pos;
    }

    //Generate player on the arena
    void InstantiateLocalPlayers()
    {
        // Set spawner positions to safezone centers
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            GameObject tile;
            TerrainGenerator.instance.GetGameObject(TerrainGenerator.instance.safeCenter[i + 1], out tile);
            spawner[i].transform.position = tile.transform.position;
        }

        // DECOMMENTEZ LES PARAGRAPHE SELON LA SCENE VOULUE !!!!!!!!!!!!!!!!!!!!!!



        for (int playerIndex = 0; playerIndex < DataManager.instance.currentNbrPlayer; playerIndex++)
        {
            Vector3 direction = Vector3.zero - spawner[playerIndex].GetComponent<Transform>().position;
            direction.y = 0.0f;
            Quaternion lookAtCenter = Quaternion.LookRotation(direction);
            GameObject classToInstantiate = null;

            if (noMenu) // when launched directly form the game scene
            {
                classToInstantiate = Instantiate(playableClass[(int)DataManager.instance.playerTest[playerIndex]], spawner[playerIndex].GetComponent<Transform>().position, lookAtCenter);
                DataManager.instance.player[playerIndex] = classToInstantiate.GetComponent<Player>();
                classToInstantiate.GetComponent<Player>().selectedClass = DataManager.instance.playerTest[playerIndex];

                // Set default gifts 1 2 3
                for (int i = 0; i < classToInstantiate.GetComponent<Player>().gifts.Length; i++)
                {
                    classToInstantiate.GetComponent<Player>().gifts[i] = (DataManager.TypeGift)i;
                }
            }
            else // when launched from the menu scene
            {
                classToInstantiate = Instantiate(playableClass[(int)DataManager.instance.player[playerIndex].selectedClass], spawner[playerIndex].GetComponent<Transform>().position, lookAtCenter);
                classToInstantiate.GetComponent<Player>().selectedClass = DataManager.instance.player[playerIndex].selectedClass;

                // Set the gifts
                for (int i = 0; i < classToInstantiate.GetComponent<Player>().gifts.Length; i++)
                {
                    classToInstantiate.GetComponent<Player>().gifts[i] = DataManager.instance.giftPlayer[playerIndex, i];
                }
            }
            
            // Set the controller
            classToInstantiate.GetComponent<InputController>().controller = (InputController.Controller)playerIndex;

            // Set the name and color
            classToInstantiate.name = "Challenger " + (playerIndex + 1).ToString();
            classToInstantiate.GetComponent<Player>().colorIndex = playerIndex;
        }
    }
}
