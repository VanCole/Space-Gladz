using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class Message : NetworkBehaviour {

    [SerializeField] Text textBox;

    bool[] trackPlayerAlive = { true, true, true, true };
    int nbrPlayerAlive = 0;

    private void Awake()
    {
        NetworkUIManager container = FindObjectOfType<NetworkUIManager>();
        if(container)
        {
            transform.SetParent(container.transform, false);
        }
    }

    // Use this for initialization
    void Start () {
        nbrPlayerAlive = GameManager.instance.nbrAlivePlayer;
        textBox.text = "";
        //Debug.Log("<color=#FF8800>MESSAGES WORKING</color>");
        //Debug.Log("<color=#FFFF00>Local Player is server : " + isServer + "</color>");

        //StartCoroutine(DisplayMessage("Message Test", 5.0f));


        RectTransform rect = GetComponent<RectTransform>();
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // Update is called once per frame
    void Update () {
        if (!isServer) return;

        // Detect death of a player
        if (nbrPlayerAlive != GameManager.instance.nbrAlivePlayer)
        {
            //Debug.Log("<color=#FF8800>Player death</color>");
            for (int i = 0; i < 4; i++)
            {
                if (DataManager.instance.player[i] != null)
                {
                    if (trackPlayerAlive[i] && !DataManager.instance.player[i].isAlive)
                    {
                        //Debug.Log("<color=#FF8800>Player " + DataManager.instance.PlayerNames[i] + "</color>");
                        trackPlayerAlive[i] = false;
                        Send("<b>" + DataManager.instance.PlayerNames[i] + "</b> is dead !");
                    }
                }
            }
        }

        nbrPlayerAlive = GameManager.instance.nbrAlivePlayer;
	}


    public void Send(string message)
    {
       // Debug.Log("SEND Message : " + isServer + " | " + DataManager.instance.isMulti);
        if (isServer && DataManager.instance.isMulti)
        {
            //Debug.Log("SEND : " + message);
            RpcSend(message);
        }
    }

    [ClientRpc]
    private void RpcSend(string message)
    {
        //Debug.Log(message);
        //textBox.text = message;
        //Debug.Log("RPC : " + message);
        StartCoroutine(DisplayMessage(message));
    }


    IEnumerator DisplayMessage(string message, float _duration = 2.0f)
    {
        textBox.text = message;
        yield return new WaitForSeconds(_duration);
        textBox.text = "";
    }
}
