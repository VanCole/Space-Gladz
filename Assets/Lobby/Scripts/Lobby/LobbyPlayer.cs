using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace Prototype.NetworkLobby
{
    //Player entry in the lobby. Handle selecting color/setting name & getting ready for the game
    //Any LobbyHook can then grab it and pass those value to the game player prefab (see the Pong Example in the Samples Scenes)
    public class LobbyPlayer : NetworkLobbyPlayer
    {
        public GameObject[] selectionScreen = new GameObject[4];

        [HideInInspector]
        public enum CLASS
        {
            cWarrior,
            cArchere,
            cAssassin
        }
        public CLASS Class = CLASS.cWarrior;

        static Color[] Colors = new Color[] { Color.magenta, Color.red, Color.cyan, Color.blue, Color.green, Color.yellow };
        //used on server to avoid assigning the same color to two player
        static List<int> _colorInUse = new List<int>();

        public Button colorButton;
        public InputField nameInput;
        public Button readyButton;
        public Button waitingPlayerButton;
        public Button removePlayerButton;

        public GameObject localIcone;
        public GameObject remoteIcone;

        //OnMyName function will be invoked on clients when server change the value of playerName
        [SyncVar(hook = "OnMyName")]
        public string playerName = "";
        [SyncVar(hook = "OnMyColor")]
        public Color playerColor = Color.white;

        public Color OddRowColor = new Color(250.0f / 255.0f, 250.0f / 255.0f, 250.0f / 255.0f, 1.0f);
        public Color EvenRowColor = new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f, 1.0f);

        static Color JoinColor = new Color(255.0f/255.0f, 0.0f, 101.0f/255.0f,1.0f);
        static Color NotReadyColor = new Color(34.0f / 255.0f, 44 / 255.0f, 55.0f / 255.0f, 1.0f);
        static Color ReadyColor = new Color(0.0f, 204.0f / 255.0f, 204.0f / 255.0f, 1.0f);
        static Color TransparentColor = new Color(0, 0, 0, 0);

        //static Color OddRowColor = new Color(250.0f / 255.0f, 250.0f / 255.0f, 250.0f / 255.0f, 1.0f);
        //static Color EvenRowColor = new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f, 1.0f);


        public override void OnClientEnterLobby()
        {
            base.OnClientEnterLobby();

            if (LobbyManager.s_Singleton != null) LobbyManager.s_Singleton.OnPlayersNumberModified(1);

            LobbyPlayerList._instance.AddPlayer(this);
            LobbyPlayerList._instance.DisplayDirectServerWarning(isServer && LobbyManager.s_Singleton.matchMaker == null);
            selectionScreen = GameObject.Find("Canvas CharSelection").GetComponent<SelectionScreens>().selectionScreen;

            if (isLocalPlayer)
            {
                SetupLocalPlayer();
            }
            else
            {
                SetupOtherPlayer();
            }

            //setup the player data on UI. The value are SyncVar so the player
            //will be created with the right value currently on server            
            OnMyName(playerName);
            OnMyColor(playerColor);

        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            //if we return from a game, color of text can still be the one for "Ready"
            readyButton.transform.GetChild(0).GetComponent<Text>().color = Color.white;

           SetupLocalPlayer();
        }

        void ChangeReadyButtonColor(Color c)
        {
            ColorBlock b = readyButton.colors;
            b.normalColor = c;
            b.pressedColor = c;
            b.highlightedColor = c;
            b.disabledColor = c;
            readyButton.colors = b;
        }

        void SetupOtherPlayer()
        {
            nameInput.interactable = false;
            removePlayerButton.interactable = NetworkServer.active;

            ChangeReadyButtonColor(NotReadyColor);

            readyButton.transform.GetChild(0).GetComponent<Text>().text = "...";
            readyButton.interactable = false;

            OnClientReady(false);
        }

        void SetupLocalPlayer()
        {
            nameInput.interactable = true;
            remoteIcone.gameObject.SetActive(false);
            localIcone.gameObject.SetActive(true);

            CheckRemoveButton();

            if (playerColor == Color.white)
                CmdColorChange();

            ChangeReadyButtonColor(JoinColor);

            readyButton.transform.GetChild(0).GetComponent<Text>().text = "JOIN";
            readyButton.interactable = true;

            //have to use child count of player prefab already setup as "this.slot" is not set yet
            if (playerName == "")
                CmdNameChanged("Player" + (LobbyPlayerList._instance.playerListContentTransform.childCount-1));
            DataManager.instance.localPlayerIndex = DataManager.instance.currentNbrPlayer;
            CmdSynchronizePlayers();

            //we switch from simple name display to name input
            colorButton.interactable = true;
            nameInput.interactable = true;

            nameInput.onEndEdit.RemoveAllListeners();
            nameInput.onEndEdit.AddListener(OnNameChanged);

            colorButton.onClick.RemoveAllListeners();
            colorButton.onClick.AddListener(OnColorClicked);

            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(OnReadyClicked);

            playerName = DataManager.instance.localPlayerName;
            CmdSetName(DataManager.instance.localPlayerName, DataManager.instance.localPlayerIndex - 1);
            //when OnClientEnterLobby is called, the loval InputController is not yet created, so we need to redo that here to disable
            //the add button if we reach maxLocalPlayer. We pass 0, as it was already counted on OnClientEnterLobby
            if (LobbyManager.s_Singleton != null) LobbyManager.s_Singleton.OnPlayersNumberModified(0);
        }
        [Command]
        void CmdSetName(string _name, int _index)
        {
            RpcSetName(_name, _index);
        }

        [ClientRpc]
        void RpcSetName(string _name, int _index)
        {
            playerName = _name;
            DataManager.instance.PlayerNames[_index] = _name;
        }

        public void ClassPicked(int avIndex)
        {

            if (isServer)
                RpcClassPicked(avIndex);
            else
                CmdClassPicked(avIndex);
        }

        [ClientRpc]
        void RpcClassPicked(int avIndex)
        {
            CmdClassPicked(avIndex);
        }

        [Command]
        void CmdClassPicked(int avIndex)
        {
            LobbyManager.s_Singleton.SetPlayerTypeLobby(GetComponent<NetworkIdentity>().connectionToClient, avIndex);
        }

        [ClientRpc]
        void RpcChangeModel(int index)
        {
            selectionScreen[index].GetComponent<PlayerSelection>().choosedClass =
                selectionScreen[index].GetComponent<PlayerSelection>().classSelect.GetComponent<ClassSelection>().
                SwitchingClass(selectionScreen[index].GetComponent<PlayerSelection>().choosedClass, index, false);
        }

        [Command]
        public void CmdChangeModel(int index)
        {
            RpcChangeModel(index);
        }

        //This enable/disable the remove button depending on if that is the only local player or not
        public void CheckRemoveButton()
        {
            if (!isLocalPlayer)
                return;

            int localPlayerCount = 0;
            foreach (PlayerController p in ClientScene.localPlayers)
                localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

            removePlayerButton.interactable = localPlayerCount > 1;
        }

        public override void OnClientReady(bool readyState)
        {
            if (readyState)
            {
                ChangeReadyButtonColor(TransparentColor);

                Text textComponent = readyButton.transform.GetChild(0).GetComponent<Text>();
                textComponent.text = "READY";
                textComponent.color = ReadyColor;
                readyButton.interactable = false;
                colorButton.interactable = false;
                nameInput.interactable = false;
            }
            else
            {
                ChangeReadyButtonColor(isLocalPlayer ? JoinColor : NotReadyColor);

                Text textComponent = readyButton.transform.GetChild(0).GetComponent<Text>();
                textComponent.text = isLocalPlayer ? "JOIN" : "...";
                textComponent.color = Color.white;
                readyButton.interactable = isLocalPlayer;
                colorButton.interactable = isLocalPlayer;
                nameInput.interactable = isLocalPlayer;
            }
        }

        public void OnPlayerListChanged(int idx)
        { 
            GetComponent<Image>().color = (idx % 2 == 0) ? EvenRowColor : OddRowColor;
        }

        ///===== callback from sync var

        public void OnMyName(string newName)
        {
            playerName = newName;
            nameInput.text = playerName;
        }

        public void OnMyColor(Color newColor)
        {
            playerColor = newColor;
            colorButton.GetComponent<Image>().color = newColor;
        }

        //===== UI Handler

        //Note that those handler use Command function, as we need to change the value on the server not locally
        //so that all client get the new value throught syncvar
        public void OnColorClicked()
        {
            CmdColorChange();
        }

        public void OnReadyClicked()
        {
            SendReadyToBeginMessage();
        }

        public void OnNameChanged(string str)
        {
            CmdNameChanged(str);
        }

        public void OnRemovePlayerClick()
        {
            if (isLocalPlayer)
            {
                RemovePlayer();
            }
            else if (isServer)
            {
                LobbyManager.s_Singleton.KickPlayer(connectionToClient);
            }  
        }

        public void ToggleJoinButton(bool enabled)
        {
            readyButton.gameObject.SetActive(enabled);
            waitingPlayerButton.gameObject.SetActive(!enabled);
        }

        [ClientRpc]
        public void RpcUpdateCountdown(int countdown)
        {
            LobbyManager.s_Singleton.countdownPanel.UIText.text = "Match Starting in " + countdown;
            LobbyManager.s_Singleton.countdownPanel.gameObject.SetActive(countdown != 0);
        }

        [ClientRpc]
        public void RpcUpdateRemoveButton()
        {
            CheckRemoveButton();
        }

        [Command]
        public void CmdPlayerReady(int _index, bool _ready)
        {
            RpcPlayerReady(_index, _ready);
        }

        [ClientRpc]
       void RpcPlayerReady(int _index, bool _ready)
        {
            DataManager.instance.isPlayerReady[_index] = _ready;
        }


        [Command]
        public void CmdStartTheGame()
        {
            RpcStartTheGame();
        }

        [ClientRpc]
        void RpcStartTheGame()
        {
            SendReadyToBeginMessage();
        }


        //====== Server Command

        [Command]
        public void CmdColorChange()
        {
            int idx = System.Array.IndexOf(Colors, playerColor);

            int inUseIdx = _colorInUse.IndexOf(idx);

            if (idx < 0) idx = 0;

            idx = (idx + 1) % Colors.Length;

            bool alreadyInUse = false;

            do
            {
                alreadyInUse = false;
                for (int i = 0; i < _colorInUse.Count; ++i)
                {
                    if (_colorInUse[i] == idx)
                    {//that color is already in use
                        alreadyInUse = true;
                        idx = (idx + 1) % Colors.Length;
                    }
                }
            }
            while (alreadyInUse);

            if (inUseIdx >= 0)
            {//if we already add an entry in the colorTabs, we change it
                _colorInUse[inUseIdx] = idx;
            }
            else
            {//else we add it
                _colorInUse.Add(idx);
            }

            playerColor = Colors[idx];
        }

        [Command]
        public void CmdNameChanged(string name)
        {
            playerName = name;
        }



        //Cleanup thing when get destroy (which happen when client kick or disconnect)
        public void OnDestroy()
        {
            DataManager.instance.playerDCed = transform.GetSiblingIndex();
            LobbyPlayerList._instance.RemovePlayer(this);
            if (LobbyManager.s_Singleton != null) LobbyManager.s_Singleton.OnPlayersNumberModified(-1);

            int idx = System.Array.IndexOf(Colors, playerColor);

            if (idx < 0)
                return;

            for (int i = 0; i < _colorInUse.Count; ++i)
            {
                if (_colorInUse[i] == idx)
                {//that color is already in use
                    _colorInUse.RemoveAt(i);
                    break;
                }
            }
        }

        // ----------------- Custom ------------------

        public string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        [Command]
        public void CmdSynchronizePlayers()
        {
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                RpcSynchronizePlayers(i, (int)selectionScreen[i].GetComponent<PlayerSelection>().choosedClass);
            }
        }

        [ClientRpc]
        void RpcSynchronizePlayers(int _i, int _class)
        {
            if ((int)selectionScreen[_i].GetComponent<PlayerSelection>().choosedClass != _class)
            {
                selectionScreen[_i].GetComponent<PlayerSelection>().choosedClass =
                selectionScreen[_i].GetComponent<PlayerSelection>().classSelect.GetComponent<ClassSelection>().
                SwitchingClass(selectionScreen[_i].GetComponent<PlayerSelection>().choosedClass, _i, false);
            }
        }

        [Command]
        public void CmdDisconnectOnePlayer(int _indexPlayerDisconnected)
        {
            RpcDisconnectOnePlayer(_indexPlayerDisconnected);
        }


        [ClientRpc]
        void RpcDisconnectOnePlayer(int _indexPlayerDisconnected)
        {
            GameObject.Find("LobbyManager").GetComponent<LobbyManager>().GetComponentsInChildren<LobbyPlayer>()[DataManager.instance.localPlayerIndex - 1].SendNotReadyToBeginMessage();
            if (DataManager.instance.localPlayerIndex > _indexPlayerDisconnected)
            {
                DataManager.instance.localPlayerIndex--;
            }
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                DataManager.instance.isPlayerReady[i] = false;
                if (i >= _indexPlayerDisconnected)
                {                 
                    GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().choosedClass = GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().choosedClass;
                    GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().championModel.transform.GetChild(0).gameObject.SetActive(GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().championModel.transform.GetChild(0).gameObject.activeSelf);
                    GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().championModel.transform.GetChild(1).gameObject.SetActive(GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().championModel.transform.GetChild(1).gameObject.activeSelf);
                    GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().typeScreen = GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().typeScreen;
                    GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().hasChangedScreen = GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().hasChangedScreen;
                    GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().selectedGifts = GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().selectedGifts;
                    GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().giftCount = GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().giftCount;
                    for (int j = 0; j < GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().gameObject.GetComponentInChildren<GiftsMenu>().gifts.Length; j++)
                    {
                        GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().gameObject.GetComponentInChildren<GiftsMenu>().gifts[j].GetComponent<GiftUI>().isSelected
                        = GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().gameObject.GetComponentInChildren<GiftsMenu>().gifts[j].GetComponent<GiftUI>().isSelected;
                        GameObject.Find("Canvas CharSelection").transform.GetChild(i).GetComponent<PlayerSelection>().gameObject.GetComponentInChildren<GiftsMenu>().gifts[j].GetComponent<GiftUI>().arrayPos
                        = GameObject.Find("Canvas CharSelection").transform.GetChild(i + 1).GetComponent<PlayerSelection>().gameObject.GetComponentInChildren<GiftsMenu>().gifts[j].GetComponent<GiftUI>().arrayPos;
                    }
                }
            }
        }

        [Command]
        public void CmdSaveGifts(DataManager.TypeGift _gift, int _playerIndex, int _nbGift)
        {
            DataManager.instance.giftPlayer[_playerIndex, _nbGift] = _gift;
        }
    }
}
