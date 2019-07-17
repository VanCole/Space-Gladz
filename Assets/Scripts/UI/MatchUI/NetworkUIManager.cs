using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkUIManager : MonoBehaviour {

    [SerializeField] GameObject mainUI;
    [SerializeField] GameObject spectatorUI;

    // Use this for initialization
    void Start () {
        mainUI.SetActive(true);
        spectatorUI.SetActive(false);

    }
	
	// Update is called once per frame
	void Update () {
        if (DataManager.instance.localPlayer)
        {
            if (DataManager.instance.isMulti && !DataManager.instance.isMatchDone && !DataManager.instance.localPlayer.GetComponent<Player>().isAlive)
            {
                mainUI.SetActive(false);
                spectatorUI.SetActive(true);
            }
        }
	}
}
