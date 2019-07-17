using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class DataManager : MonoBehaviour
{
    public int currentNbrPlayer = 4;

    public Texture2D[] mouseCursor;

    [SerializeField]
    GameObject prefabLobbyManager;

    public bool networkFromGame = false;
    bool destructionConnection = false;

    [SerializeField]
    public TypeClass[] playerTest = new TypeClass[4];
    public Player[] player = new Player[4];
    [HideInInspector]
    public int playerDCed;
    public TypeGift[,] giftPlayer = new TypeGift[4, 3];
    public bool[] isPlayerReady = new bool[4];

    public int[] PlayerIndexes = new int[4];

    public string[] PlayerNames = new string[4];

    public string localPlayerName;
    
    public Color[] playerColor = new Color[4]
    {
        Color.red,
        Color.red,
        Color.yellow,
        Color.green
    };

   
    public static DataManager instance;

    public bool isMatchDone;
    public bool isLoadingDone;

    public GameState gameState;

    public Config config;
    [SerializeField] public AudioMixer audioMixer;

    // Network
    public GameObject localPlayer;
    public bool isMulti = false;
    public int localPlayerIndex = -1;

    public enum TypeClass
    {
        Warrior,
        Archer,
        nbClass
    }

    public enum TypeGift
    {
        none,
        Heal,
        Immune,
        PowerShield,
        BonusMoveSpeed,
        ReducCD,
        BoostDamage,
        StunBoost,
        SlowBoost,
        PanicBoost,
        Regeneration,
        FlameShot,
        VampShot
    }

    public enum GameState
    {
        mainMenu,
        option,
        charSelection,
        credit,
        ControlScreen,
        inArena,
        lobbyMenu,  
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (!Config.ReadConfig("config.cfg", out config))
            {
                Debug.LogWarning("Unable to load configuration file \"config.cfg\".");
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        if (this == instance)
        {
            // Set volumes in audio mixer
            audioMixer.SetFloat("Master", config.masterVolume == 0 ? -80.0f : 20 * Mathf.Log10(config.masterVolume));
            audioMixer.SetFloat("Music", config.musicVolume == 0 ? -80.0f : 20 * Mathf.Log10(config.musicVolume));
            audioMixer.SetFloat("SFX", config.sfxVolume == 0 ? -80.0f : 20 * Mathf.Log10(config.sfxVolume));
            audioMixer.SetFloat("Ambient", config.ambientVolume == 0 ? -80.0f : 20 * Mathf.Log10(config.ambientVolume));
            audioMixer.SetFloat("Voice", config.voicesVolume == 0 ? -80.0f : 20 * Mathf.Log10(config.voicesVolume));
        }
    }

    // Update is called once per frame
    void Update()
    {   
        if (isMulti && playerDCed > 0)
        {
            GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().GetComponentsInChildren<Prototype.NetworkLobby.LobbyPlayer>()[0].CmdDisconnectOnePlayer(playerDCed);
            playerDCed = 0;
        }

        if (networkFromGame)
        {      
            Destroy(GameObject.Find("DataManager"));
            Destroy(GameObject.Find("SoundManager"));
            GameObject.Find("LobbyManager").GetComponent<Prototype.NetworkLobby.LobbyManager>().StopHostClbk();
            networkFromGame = false;
        }

    }

    private void OnDestroy()
    {
        // Save configuration on application quit
        if(instance == this)
        {
            if(!Config.WriteConfig("config.cfg", config))
            {
                Debug.LogWarning("Unable to save configuration file \"config.cfg\".");
            }
        }
    }
}
