using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GiftSpawner : NetworkBehaviour 
{
    [SerializeField]
    GameObject[] gifts;

    [SerializeField]
    Light spotLight;

    public bool[] tabOccuped;
    float timerGift = 0.0f;
    int randomPosition;
    int randomChoice;
    int playerIndex;


    private void Start()
    {
        transform.parent = GameObject.Find("Managers").transform;
        tabOccuped = new bool[7];

        for (int i = 0; i < 7; i++)
        {
            tabOccuped[i] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        EnableSpotLight();

        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            playerIndex = DataManager.instance.player[i].colorIndex;

            if(DataManager.instance.player[i].pv_lost >= 300.0f)
            {
                SetGiftPosition();
                DataManager.instance.player[i].pv_lost -= 300.0f;
            }

            if (DataManager.instance.player[i].damageDeals >= 300.0f)
            {
                SetGiftPosition();
                DataManager.instance.player[i].damageDeals -= 300.0f;
            }
        }
    }


    void EnableSpotLight()
    {
        if (DataManager.instance.isMatchDone)
        {
            for (int i = 0; i < 7; i++)
            {
                if (tabOccuped[i] == false)
                {
                    spotLight.enabled = false;
                }
            }

            foreach (bool iSOccupied in tabOccuped)
            {
                if (iSOccupied)
                {
                    spotLight.enabled = true;
                }
            }
        }
        else
        {
            spotLight.enabled = false;
        }
    }

    void SetGiftPosition()
    {
        bool notFull = false;

        for (int i = 0; i < 7; i++)
        {
            if (tabOccuped[i] == false)
            {
                notFull = true;
            }
        }

        if (notFull == true)
        {
            GameObject gift = null;

            if (tabOccuped[0] == false)
            {
                SpawnGiftFromPlayer(gift, Vector3.zero, 0, playerIndex);
                tabOccuped[0] = true;
            }
            else
            {
                do
                {
                    randomPosition = Random.Range(0, 6);
                } while (tabOccuped[randomPosition + 1] == true);

                GameObject go;
                TerrainGenerator.instance.GetGameObject(HexIndex.direction[randomPosition], out go);
                tabOccuped[randomPosition + 1] = true;

                SpawnGiftFromPlayer(gift, go.transform.position, randomPosition + 1, playerIndex);
            }
        }
    }

    void SpawnGiftFromPlayer(GameObject giftTest, Vector3 position, int indexPosition, int playerIndex)
    {
        int randomGift = Random.Range(0, 3);

        if (DataManager.instance.isMulti && isServer)
        {
            int tempGift = (int)DataManager.instance.giftPlayer[playerIndex, randomGift];
            RpcSpawnGiftFromPlayer(giftTest, position, indexPosition, playerIndex, tempGift);
        }
        else if (!DataManager.instance.isMulti)
        {
            int tempGift = (int)DataManager.instance.player[playerIndex].gifts[randomGift];
            SpawnGift(giftTest, position, indexPosition, playerIndex, tempGift);
        }

        int randomSoundGift = Random.Range(0, 3);

        if (DataManager.instance.isMulti)
        {
            if (tabOccuped[0] == false)
            {
                switch (randomSoundGift)
                {
                    case 0:
                        CmdPlaySoundGS(39, 1.0f, AudioType.SFX);
                        break;

                    case 1:
                        CmdPlaySoundGS(40, 1.0f, AudioType.SFX);
                        break;

                    case 2:
                        CmdPlaySoundGS(41, 1.0f, AudioType.SFX);
                        break;
                }
            }
        }
        else
        {
            if (tabOccuped[0] == false)
            {
                switch (randomSoundGift)
                {
                    case 0:
                        SoundManager.instance.PlaySound(39, 1.0f, AudioType.Voice);
                        break;

                    case 1:
                        SoundManager.instance.PlaySound(40, 1.0f, AudioType.Voice);
                        break;

                    case 2:
                        SoundManager.instance.PlaySound(41, 1.0f, AudioType.Voice);
                        break;
                }
            }
        }
    }

    void SpawnGift(GameObject giftTest, Vector3 position, int indexPosition, int playerIndex, int tempGift)
    {

        if (!ShowDirector.instance.lookAtGift)
        {
            ShowDirector.instance.StartCoroutineSwitchToSpawnerPosition(15.0f, GameObject.Find("Arena").gameObject, 4.0f);
        }
        giftTest = Instantiate(gifts[tempGift - 1], position, Quaternion.identity);

        giftTest.GetComponent<Gift>().contourBox.material.SetColor("_EmissionColor", DataManager.instance.playerColor[playerIndex]);

        giftTest.GetComponent<Gift>().index = indexPosition;
        giftTest.GetComponent<Gift>().gs = this;
        giftTest.GetComponent<Gift>().playerIndex = playerIndex;
    }

    [ClientRpc]
    void RpcSpawnGiftFromPlayer(GameObject giftTest, Vector3 position, int indexPosition, int playerIndex, int tempGift)
    {
        ShowDirector.instance.StartCoroutineSwitchToSpawnerPosition(15.0f, GameObject.Find("Arena").gameObject, 4.0f);

        giftTest = Instantiate(gifts[tempGift - 1], position, Quaternion.identity);

        giftTest.GetComponent<Gift>().contourBox.material.SetColor("_EmissionColor", DataManager.instance.playerColor[playerIndex]);

        giftTest.GetComponent<Gift>().index = indexPosition;
        giftTest.GetComponent<Gift>().gs = this;
        giftTest.GetComponent<Gift>().playerIndex = playerIndex;
    }

    [Command]
    public void CmdPlaySoundGS(int soundNumber, float soundVolume, AudioType type)
    {
        RpcPlaySoundGS(soundNumber, soundVolume, type);
    }

    [ClientRpc]
    void RpcPlaySoundGS(int soundNumber, float soundVolume, AudioType type)
    {
        SoundManager.instance.PlaySound(soundNumber, soundVolume, type);
    }

}
