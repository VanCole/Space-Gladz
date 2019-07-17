using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Class of a terrain
[DefaultExecutionOrder(-6)]
public class TerrainGenerator : NetworkBehaviour
{
    static public TerrainGenerator instance = null; // Singleton

    [SerializeField] GameObject giftSpawnerPrefab;
    [SerializeField] GameObject arenaEventManagerPrefab;
    [SerializeField] GameObject canvasMessagesPrefab;

    [SerializeField] int arenaRadius = 5;
    [SerializeField] float tileRadius = 1;
    [SerializeField] GameObject tile;
    [SerializeField] float beforeAppearing = 5.0f, timeToAppear = 0.1f, safeZoneDuration = 2.0f;
    // Attributes for map generation
    [SerializeField] int nbZone = 20;
    [SerializeField] int zoneRadius = 2;
    [SerializeField] int minRadius = 0;
    [SerializeField] int maxRadius = 0;

    [SerializeField] int nbBarils = 0;
    [SerializeField] int nbHole = 0;
    [SerializeField] int holeRadius = 0;
    [SerializeField] int nbTurret = 0;

    public bool collapsing = true;
    public bool mapRegeneration = true;

    bool firstFall = false;
    int counterFall = 0;

    public AudioSource source;

    // The grid that store all hex index
    public HexGrid grid;

    // Dictionaries to links hex index and game object in bidirectional
    Dictionary<HexIndex, GameObject> hexesByKey = new Dictionary<HexIndex, GameObject>();
    Dictionary<GameObject, HexIndex> hexesByValue = new Dictionary<GameObject, HexIndex>();


    float SQRT3 = Mathf.Sqrt(3);
    enum Pattern { Spiral, Linear, Circle, Random }

    // store the generation coroutine to stop it when we regenerate the terrain
    Coroutine generation = null;

    List<HexIndex> safeZones = new List<HexIndex>();
    public List<HexIndex> safeCenter = new List<HexIndex>();

    public int currentRadius = -1;

    [SerializeField] bool isNetwork = false;

    private void Awake()
    {
        if ((isNetwork && !DataManager.instance.isMulti)
            || (!isNetwork && DataManager.instance.isMulti))
        {
            Destroy(gameObject);

            //if((DataManager.instance.isMulti && isServer) || !DataManager.instance.isMulti)
            //{

            //}
            //CmdPlaySound(1, 48, 1.0f, 0, 0, 0.0f, 2);

        }

        //SoundManager.instance.PlaySound(48, 1.0f);
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
            GameObject giftSpawner = Instantiate(giftSpawnerPrefab, Vector3.zero, Quaternion.identity);
            GameObject arenaEventManager = Instantiate(arenaEventManagerPrefab, Vector3.zero, Quaternion.identity);
            giftSpawner.name = "GiftSpawner";
            arenaEventManager.name = "ArenaEventGenerator";

            SoundManager.instance.PlaySound(48, 1.0f, AudioType.Voice);

            if (DataManager.instance.isMulti)
            {
                if (isServer)
                {

                    GameObject canvasMessages = Instantiate(canvasMessagesPrefab);

                    //Debug.Log("<color=#FFFF00>Spawn on clients</color>");
                    NetworkServer.Spawn(giftSpawner);
                    NetworkServer.Spawn(arenaEventManager);
                    NetworkServer.Spawn(canvasMessages);
                    //RpcSetParent(canvasMessages);
                }
                SoundManager.instance.StopSound(source);
            }

            instance = this;
            currentRadius = arenaRadius;
            grid = new HexGrid(arenaRadius);
            if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
            {
                GenerateGrid();
            }

            int distanceFromCenter = 10;

            // Central safezone for gifts
            safeCenter.Add(HexIndex.origin);

            switch (DataManager.instance.currentNbrPlayer)
            {
                case 2:
                    safeCenter.Add(distanceFromCenter * HexIndex.direction[3]);
                    safeCenter.Add(distanceFromCenter * HexIndex.direction[0]);
                    break;
                case 3:
                    safeCenter.Add(distanceFromCenter * HexIndex.direction[3]);
                    safeCenter.Add(distanceFromCenter * HexIndex.direction[1]);
                    safeCenter.Add(distanceFromCenter * HexIndex.direction[5]);
                    break;
                case 4:
                    int d = distanceFromCenter + (int)(distanceFromCenter * 0.3f);
                    safeCenter.Add(distanceFromCenter * HexIndex.direction[3]);
                    safeCenter.Add(distanceFromCenter * HexIndex.direction[0]);
                    safeCenter.Add((int)(d * 0.5f) * HexIndex.direction[4] + (int)((d - 1) * 0.5f) * HexIndex.direction[5]);
                    safeCenter.Add((int)(d * 0.5f) * HexIndex.direction[1] + (int)((d - 1) * 0.5f) * HexIndex.direction[2]);
                    break;
                default: break;
            }

            for (int i = 0; i < safeCenter.Count; i++)
            {
                safeZones.AddRange(grid.GetCircle(safeCenter[i], 1, true));
            }


            // Add safe zones for player entry
            foreach (HexIndex index in safeZones)
            {
                if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
                {
                    hexesByKey[index].GetComponent<Tile>().Type = Tile.TileType.SafeZone;
                    hexesByKey[index].GetComponent<Tile>().Height = 0;
                }
            }
            if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
            {
                StartCoroutine(GenerateMap());
                StartCoroutine(TerrainAppearing());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ////////////////////////////////////////
        //          TO REMOVE LATER
        ////////////////////////////////////////
        // Generate terrain on button pressed
        //if (Input.GetKeyDown(KeyCode.F8))
        //{
        //    if (generation != null)
        //    {
        //        StopCoroutine(generation);
        //    }
        //    generation = StartCoroutine(GenerateMap());
        //}
#if __DEBUG
        if (Input.GetKeyDown(KeyCode.F9))
        {
            GenerateEmptyTerrain();
        }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            GenerateTrailerTerrain();
        }
#endif
    }


    public void GenerateEmptyTerrain()
    {
        // Attributes for map generation
        nbZone = 0;
        nbBarils = 0;
        nbHole = 0;
        nbTurret = 0;
        collapsing = false;
        mapRegeneration = false;

        if (generation != null)
        {
            StopCoroutine(generation);
        }
        generation = StartCoroutine(GenerateMap());

    }

    public void GenerateTrailerTerrain()
    {
        // Attributes for map generation
        nbZone = 10;
        nbBarils = 10;
        nbHole = 5;
        nbTurret = 5;
        collapsing = false;
        mapRegeneration = false;

        if (generation != null)
        {
            StopCoroutine(generation);
        }
        generation = StartCoroutine(GenerateMap());

    }

    IEnumerator TerrainRegeneration(float _cooldown)
    {
        while (!DataManager.instance.isMatchDone)
        {
            yield return new WaitForSeconds(_cooldown);
            if (mapRegeneration)
            {
                StartCoroutine(GenerateMap());
            }
        }
    }

    IEnumerator TerrainCollapsing()
    {
        currentRadius = arenaRadius;

        while (currentRadius > 3 && !DataManager.instance.isMatchDone)
        {
            List<HexIndex> border = grid.GetRing(HexIndex.origin, currentRadius);

            yield return new WaitForSeconds(7.0f);
            if (collapsing)
            {
                foreach (HexIndex index in border)
                {
                    GameObject obj;
                    GetGameObject(index, out obj);
                    obj.GetComponent<Tile>().isWarning = true;
                }
            }
            yield return new WaitForSeconds(3.0f);
            if (collapsing)
            {
                foreach (HexIndex index in border)
                {
                    GameObject obj;
                    GetGameObject(index, out obj);
                    obj.GetComponent<Tile>().Enable = false;
                }
                currentRadius--;
                SoundManager.instance.PlaySound(51, 0.5f, AudioType.SFX);

                if (!DataManager.instance.isMatchDone)
                {
                    TerrainFallSound();
                }
            }
        }
    }


    IEnumerator TerrainAppearing()
    {
        foreach (HexIndex index in safeZones)
        {
            hexesByKey[index].GetComponent<Tile>().Active = true;
            hexesByKey[index].GetComponent<Tile>().timeToMove = safeZoneDuration;
            //hexesByKey[index].transform.position -= 20.0f * Vector3.up;
            hexesByKey[index].GetComponent<Tile>().Position -= 20.0f * Vector3.up;
            hexesByKey[index].GetComponent<Tile>().TargetPosition = -20.0f * Vector3.up;
        }

        yield return new WaitUntil(() => DataManager.instance.isLoadingDone);
        yield return new WaitForSeconds(1.0f);

        foreach (HexIndex index in safeZones)
        {
            hexesByKey[index].GetComponent<Tile>().TargetPosition = Vector3.zero;
        }

        if(DataManager.instance.isMulti)
        {
            CmdPlaySoundTG(1, 1.0f, AudioType.SFX);
        }
        else
        {
            SoundManager.instance.PlaySound(1, 1.0f, AudioType.SFX);
        }

        yield return new WaitForSeconds(beforeAppearing);

        List<HexIndex> circle;
        for (int i = 0; i <= arenaRadius; i++)
        {

            circle = grid.GetCircle(HexIndex.origin, i);
            foreach (HexIndex index in circle)
            {
                if (hexesByKey[index].GetComponent<Tile>().Type >= 0)
                {
                    hexesByKey[index].GetComponent<Tile>().Active = true;
                    hexesByKey[index].GetComponent<Tile>().Position -= 10.0f * Vector3.up;
                    hexesByKey[index].GetComponent<Tile>().timeToMove = 0.2f;
                    hexesByKey[index].GetComponent<Tile>().TargetPosition = Vector3.zero;
                }
            }

            yield return new WaitForSeconds(timeToAppear);
            SoundManager.instance.PlaySound(51, 1.0f, AudioType.SFX);
        }

        StartCoroutine(TerrainCollapsing());
        StartCoroutine(TerrainRegeneration(30.0f));
    }

    // Return the HexIndex associated with the game object provided
    public bool GetHexIndex(GameObject _tile, out HexIndex _index)
    {
        return hexesByValue.TryGetValue(_tile, out _index);
    }

    // Return the game object assosiated with the HexIndex provided
    public bool GetGameObject(HexIndex _index, out GameObject _tile)
    {
        return hexesByKey.TryGetValue(_index, out _tile);
    }

    // Generate the grid with the game objects
    void GenerateGrid()
    {

        HashSet<HexIndex> indexes = grid.GetAll();
        foreach (HexIndex index in indexes)
        {
            GameObject obj = GameObject.Instantiate(tile, transform);
            hexesByKey.Add(index, obj);
            obj.transform.Rotate(Vector3.up, 90.0f);
            obj.transform.position = new Vector3(
                (index.x + index.z * 0.5f) * tileRadius * SQRT3,
                0,
                (index.z) * tileRadius * 1.5f);
            if (DataManager.instance.isMulti)
                NetworkServer.Spawn(obj);
            hexesByValue.Add(obj, index);
            obj.GetComponent<Tile>().Active = false;

        }
    }

    public IEnumerator GenerateMap()
    {
        // Reset Terrain
        for (int i = 0; i < arenaRadius + 1; i++)
        {
            foreach (HexIndex index in grid.GetRing(HexIndex.origin, i))
            {
                Tile tile = hexesByKey[index].GetComponent<Tile>();
                if ((int)tile.Type > 0 && tile.isEnabled)
                {
                    if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
                    {
                        tile.Type = Tile.TileType.Empty;
                        tile.Height = 0.0f;
                    }
                    tile.timeToMove = 0.2f;
                    tile.TargetPosition = Vector3.zero;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }

        // Shuffle the indexe list
        List<HexIndex> randomList = grid.GetRandom();
        int indexRandom = 0;



        // Generate Walls
        for (int i = 0; i < nbZone; i++)
        {
            // Choose random index in the terrain
            HexIndex randomIndex;
            do
            {
                randomIndex = randomList[indexRandom];
                indexRandom++;
            }
            while (indexRandom < randomList.Count
                && (!IndexInZone(randomIndex, minRadius, maxRadius)
                || (hexesByKey[randomIndex].GetComponent<Tile>().Type != Tile.TileType.Empty)));

            // place walls
            int prob = 100;
            foreach (HexIndex index in grid.GetSpiral(randomIndex, zoneRadius))
            {
                int random = Random.Range(0, 100);
                if (random < prob && hexesByKey[index].GetComponent<Tile>().Type == Tile.TileType.Empty)
                {
                    if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
                    {
                        hexesByKey[index].GetComponent<Tile>().Type = Tile.TileType.Wall;
                        hexesByKey[index].GetComponent<Tile>().Height = Random.Range(1.5f, 2.5f);
                    }
                }
                prob -= 5;
            }
            yield return 0;
        }

        // Generate hole
        for (int i = 0; i < nbHole; i++)
        {
            // Choose random index in the terrain
            HexIndex randomIndex;
            do
            {
                randomIndex = randomList[indexRandom];
                indexRandom++;
            }
            while (indexRandom < randomList.Count
                && (!IndexInZone(randomIndex, minRadius, maxRadius)
                || (hexesByKey[randomIndex].GetComponent<Tile>().Type != Tile.TileType.Empty)));

            // place holes
            int prob = 100;
            foreach (HexIndex index in grid.GetSpiral(randomIndex, holeRadius))
            {
                int random = Random.Range(0, 100);
                if (random < prob && hexesByKey[index].GetComponent<Tile>().Type == Tile.TileType.Empty)
                {
                    if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
                        hexesByKey[index].GetComponent<Tile>().Type = Tile.TileType.Hole;
                }
                prob -= 5;
            }
            yield return 0;

        }

        // Generates barils
        for (int i = 0; i < nbBarils; i++)
        {
            // Choose random index in the terrain
            HexIndex randomIndex;
            do
            {
                randomIndex = randomList[indexRandom];
                indexRandom++;
            }
            while (indexRandom < randomList.Count
                && (!IndexInZone(randomIndex, minRadius, maxRadius)
                || (hexesByKey[randomIndex].GetComponent<Tile>().Type != Tile.TileType.Empty)));


            // Place the baril
            if (Random.Range(0, 2) == 0 ? true : false)
            {
                if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
                    hexesByKey[randomIndex].GetComponent<Tile>().Type = Tile.TileType.Baril;
            }

            // Generate surrounding flaming liquid
            if (Random.Range(0, 2) == 0 ? true : false)
            {
                int prob = 100;
                foreach (HexIndex index in grid.GetSpiral(randomIndex, zoneRadius))
                {
                    int random = Random.Range(0, 100);
                    if (random < prob && hexesByKey[index].GetComponent<Tile>().Type == Tile.TileType.Empty)
                    {
                        if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
                            hexesByKey[index].GetComponent<Tile>().Type = Tile.TileType.Liquid;
                    }
                    prob -= 5;
                }
                yield return 0;
            }

        }

        // Generate turrets
        for (int i = 0; i < nbTurret; i++)
        {
            // Choose random index in the terrain
            HexIndex randomIndex;
            do
            {
                randomIndex = randomList[indexRandom];
                indexRandom++;
            }
            while (indexRandom < randomList.Count
                && !IndexInZone(randomIndex, minRadius, maxRadius)
                || (hexesByKey[randomIndex].GetComponent<Tile>().Type != Tile.TileType.Empty));


            // Place the turret
            if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
                hexesByKey[randomIndex].GetComponent<Tile>().Type = Tile.TileType.Turret;

            yield return 0;
        }

        generation = null;
    }

    void TerrainFallSound()
    {
        if (!firstFall)
        {
            if(DataManager.instance.isMulti)
            {
                CmdPlaySoundTG(45, 1.0f, AudioType.Voice);
            }
            else
            {
                SoundManager.instance.PlaySound(45, 1.0f, AudioType.Voice);
            }

            firstFall = true;
        }
        else
        {
            counterFall++;

            if (counterFall == 4 || counterFall == 12)
            {
                if (DataManager.instance.isMulti)
                {
                    CmdPlaySoundTG(47, 1.0f, AudioType.Voice);
                }
                else
                {
                    SoundManager.instance.PlaySound(47, 1.0f, AudioType.Voice);
                }
            }
            else if (counterFall == 8 || counterFall == 16)
            {
                if (DataManager.instance.isMulti)
                {
                    CmdPlaySoundTG(46, 1.0f, AudioType.Voice);
                }
                else
                {
                    SoundManager.instance.PlaySound(46, 1.0f, AudioType.Voice);
                }
            }
        }
    }


    public bool IndexInZone(HexIndex _index, int _innerRadius, int _outerRadius)
	{
        return !(((_index.x < -_outerRadius || _index.x > _outerRadius)
                || (_index.y < -_outerRadius || _index.y > _outerRadius)
                || (_index.z < -_outerRadius || _index.z > _outerRadius))
                || ((_index.x > -_innerRadius && _index.x < _innerRadius)
                && (_index.y > -_innerRadius && _index.y < _innerRadius)
                && (_index.z > -_innerRadius && _index.z < _innerRadius)));
    }

    [Command]
    public void CmdPlaySoundTG(int soundNumber, float soundVolume, AudioType type)
    {
        RpcPlaySoundTG(soundNumber, soundVolume, type);
    }

    [ClientRpc]
    void RpcPlaySoundTG(int soundNumber, float soundVolume, AudioType type)
    {
        SoundManager.instance.PlaySound(soundNumber, soundVolume, type);
    }

    //###############################################################################################
    //###############################################################################################
    //###############################################################################################
    //###############################################################################################

    public void LoadFinished()
    {
        if(DataManager.instance.isMulti && isServer)
        {
            RpcLoadFinished();
        }
    }

    [ClientRpc]
    private void RpcLoadFinished()
    {
        GameManager.instance.serverOK = true;
    }

}


#if _TRUC
[CustomEditor(typeof(TerrainGenerator))]
[CanEditMultipleObjects]
public class TerrainGeneratorEditor : Editor
{
    SerializedProperty arenaRadius;
    SerializedProperty tileRadius;
    SerializedProperty tile;
    SerializedProperty pattern;
    SerializedProperty randomPattern;
    // Map Generation
    SerializedProperty nbZone;
    SerializedProperty zoneRadius;
    SerializedProperty minRadius;
    SerializedProperty maxRadius;

    private void OnEnable()
    {
        arenaRadius = serializedObject.FindProperty("arenaRadius");
        tileRadius = serializedObject.FindProperty("tileRadius");
        tile = serializedObject.FindProperty("tile");
        pattern = serializedObject.FindProperty("pattern");
        randomPattern = serializedObject.FindProperty("randomPattern");
        nbZone = serializedObject.FindProperty("nbZone");
        zoneRadius = serializedObject.FindProperty("zoneRadius");
        minRadius = serializedObject.FindProperty("minRadius");
        maxRadius = serializedObject.FindProperty("maxRadius");
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();

        GUILayout.Label("General Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(arenaRadius);
        EditorGUILayout.PropertyField(tileRadius);
        EditorGUILayout.PropertyField(tile);

        randomPattern.boolValue = EditorGUILayout.Toggle("Random Pattern", randomPattern.boolValue);

        if (!randomPattern.boolValue)
        {
            //randomPattern.boolValue = EditorGUILayout.BeginToggleGroup("Random Pattern", randomPattern.boolValue);
            EditorGUILayout.PropertyField(pattern);
            //EditorGUILayout.EndToggleGroup();
        }
        //pattern. = !randomPattern.boolValue;


        GUILayout.Label("Map Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(nbZone);
        EditorGUILayout.PropertyField(zoneRadius);
        EditorGUILayout.PropertyField(minRadius);
        EditorGUILayout.PropertyField(maxRadius);


        serializedObject.ApplyModifiedProperties();
    }
}

#endif