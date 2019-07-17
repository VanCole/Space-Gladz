using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


[DefaultExecutionOrder(-2)]
public class CameraManager : MonoBehaviour
{
    static public CameraManager instance = null;

    [SerializeField] GameObject cameraEnding;
    [SerializeField] CameraSmooth cameraGame;
    [SerializeField] CameraNetwork cameraNetwork;
    [HideInInspector] public new CameraCore camera = null;

    [SerializeField] GameObject victoryDollyTrack;
    [SerializeField] CinemachineDollyCart dollyCartVictory;

    public GameObject cameraDraw;
    [SerializeField] GameObject drawDollyTrack;
    [SerializeField] CinemachineDollyCart dollyCartDraw;

    // Use this for initialization
    IEnumerator Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        cameraEnding.SetActive(false);
        cameraDraw.SetActive(false);
        dollyCartDraw.m_Speed = 0.0f;


        //yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => GameManager.instance.gameInitialized);

        if (!DataManager.instance.isMulti)
        {
            cameraGame.gameObject.SetActive(true);
            cameraNetwork.gameObject.SetActive(false);
            camera = cameraGame;
            // Add all players to camera target list
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                if (DataManager.instance.player[i] != null)
                {
                    cameraGame.targets.Add(DataManager.instance.player[i].gameObject);
                }
            }
        }

    }


    public void SetVictoryTrackPosition(GameObject target)
    {

        Vector3 posTrack = new Vector3(target.transform.position.x, target.transform.position.y + 1.0f, target.transform.position.z);
        victoryDollyTrack.transform.position = posTrack;


        victoryDollyTrack.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>().LookAt = victoryDollyTrack.transform;
        dollyCartVictory.m_Speed = -0.8f;
    }

    public void SetDrawTrackPosition()
    {
        victoryDollyTrack.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>().LookAt = victoryDollyTrack.transform;

        cameraDraw.SetActive(true);

        dollyCartDraw.m_Speed = 3.0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.isMulti && !cameraNetwork.target)
        {
            cameraGame.gameObject.SetActive(false);
            cameraNetwork.gameObject.SetActive(true);
            // Change the 0 by the player localIndex
            cameraNetwork.target = DataManager.instance.localPlayer;
            camera = cameraNetwork;
        }

        // Switch to end view camera when match done
        if (DataManager.instance.isMatchDone)
        {
            cameraEnding.SetActive(true);
        }

        // Remove player from camera target list if he dies
        for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
        {
            if (DataManager.instance.player[i] != null)
            {
                if (!DataManager.instance.player[i].isAlive)
                {
                    cameraGame.targets.Remove(DataManager.instance.player[i].gameObject);
                }
            }
        }
    }


}
