using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Tile : NetworkBehaviour
{

    [SerializeField] GameObject flameParticle;
    [SerializeField] GameObject explosionParticle;
    [SerializeField] GameObject turretShot;
    [SerializeField] Mesh meshBaril, meshTurret, meshMine;
    [SerializeField] GameObject prefabBaril, prefabTurret, prefabLiquid, prefabHole;
    [SerializeField] float precision = 0.001f;

    public enum TileType { SafeZone = -1, Empty, Wall, Baril, Liquid, Hole, Turret, Mine }

    MeshRenderer mesh;
    MeshFilter meshFilter;
    [SyncVar] private TileType type = 0;
    bool typeChanged = false;
    [SyncVar] public bool isActive = false;
    Vector3 zero = Vector3.zero;
    [SyncVar] public Vector3 targetPosition;
    [SyncVar] public float timeToMove = 1.0f;
    [SyncVar] public Vector3 syncPosition;
    [SyncVar] private bool active = false;
    [SyncVar] private bool activeChanged = false;

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
        set
        {
            if (DataManager.instance.isMulti)
            {
                RpcSetPosition(value);
            }
            else
            {
                LocalSetPosition(value);
            }
        }
    }

    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            if (DataManager.instance.isMulti)
            {
                RpcSetActive(value);
            }
            else
            {
                LocalSetActive(value);
            }
        }
    }

    public Vector3 TargetPosition
    {
        get
        {
            return targetPosition;
        }
        set
        {
            targetPosition = value;
            heightChanged = true;
        }
    }

    [SyncVar] Vector3 offset;
    public float height = 0.0f;
    [SyncVar] bool heightChanged = false;

    [SyncVar] public bool isEnabled = true;
    [SyncVar] public bool isWarning = false;
    float warnTimer = 0.0f, warnSpeed = 3f;

    public Color baseColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color dangerColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    public Color wallColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    Color currentColor;

    Coroutine changing = null;

    public bool Enable
    {
        get { return isEnabled; }
        set
        {
            if (!value) // disabled
            {
                isActive = false;
                Height = 0.0f;
                Type = Tile.TileType.Empty;
                TargetPosition = new Vector3(0, -25.0f, 0);
                timeToMove = 0.5f;
            }

            isEnabled = value;
        }
    }


    Interactible item = null;

    public GameObject caster;
    Transform turretTarget = null;

    // Keep reference to the wall coroutine
    public Coroutine wallCoroutine = null;
    public Coroutine holeCoroutine = null;

    MeshRenderer mr;

    GameObject thisObject;
    TerrainGenerator terrain;

    public TileType Type
    {
        get
        {
            return type;
        }

        set
        {
            if (DataManager.instance.isMulti)
            {
                RpcSetType(value);
            }
            else
            {
                LocalSetType(value);
            }
        }
    }

    public float Height
    {
        get
        {
            return height;
        }
        set
        {
            if (DataManager.instance.isMulti)
            {
                RpcSetHeight(value);
            }
            else
            {
                LocalSetHeight(value);
            }
        }
    }

    private void Start()
    {
        terrain = TerrainGenerator.instance;
        mesh = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>();

        //TargetPosition = Vector3.zero;
        offset = Vector3.Scale(transform.position, new Vector3(1, 0, 1)) + height * Vector3.up;

        mr = GetComponentInChildren<MeshRenderer>();

        thisObject = gameObject;
        currentColor = baseColor;
        SetNeonColor(baseColor);
    }


    private void Update()
    {
        // Change the type
        // Not in a function to set the change only once in a frame and to get sure it's active
        if (typeChanged)
        {
            isActive = false;

            // Prevent persistent hole
            if (holeCoroutine != null)
            {
                Debug.Log("Hole coroutine stopped");
                StopCoroutine(holeCoroutine);
                if(isEnabled)
                {
                    TargetPosition = Vector3.zero;
                }
            }

            GameObject newItem = null;
            SetNeonColor(baseColor);
            SetDebugColor(Color.white);
            switch (type)
            {
                case TileType.SafeZone: // safe zone
                    SetDebugColor(Color.white);
                    break;
                case TileType.Wall: // Wall
                    SetDebugColor(Color.white);
                    SetNeonColor(wallColor);
                    break;
                case TileType.Baril: // Baril
                    newItem = prefabBaril;
                    break;
                case TileType.Liquid: // Flaming liquid
                    newItem = prefabLiquid;
                    break;
                case TileType.Hole: // Hole
                    SetDebugColor(Color.blue);
                    newItem = prefabHole;
                    break;
                case TileType.Turret: // Turret
                    newItem = prefabTurret;
                    break;
                case TileType.Mine: // Mine
                    SetDebugColor(Color.red);
                    break;
                default: // Other
                    break;
            }

            if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
            {
                if (changing != null)
                {
                    StopCoroutine(changing);
                }
                changing = StartCoroutine(ChangeItem(newItem));
            }

            typeChanged = false;
            isActive = false;
        }
        gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = active;
        if (item)
            item.gameObject.SetActive(active);

        // Deactivate the tile when position to low
        if (thisObject.activeInHierarchy && !isEnabled && thisObject.transform.position.y < -18.0f)
        {
            gameObject.SetActive(false);
        }

        // Smooth the height change for walls and holes
        if (heightChanged)
        {
            if ((transform.position - offset + targetPosition).sqrMagnitude < precision)
            {
                transform.position = offset + targetPosition;
                heightChanged = false;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, offset + targetPosition, ref zero, timeToMove);
            }
        }

        // Warn the player before collapsing=
        if (isWarning)
        {
            warnTimer += warnSpeed * Time.deltaTime;
            Color warnColor = Color.Lerp(currentColor, dangerColor, 0.5f + 0.5f * Mathf.Sin(warnTimer * (2 * Mathf.PI)));
            //SetNeonColor(warnColor);
            mr.material.SetColor("_EmissionColor", warnColor);
        }
    }

    void SetNeonColor(Color _c)
    {
        currentColor = _c;
        mr.material.SetColor("_EmissionColor", _c);
    }

    IEnumerator ChangeItem(GameObject _newItem)
    {
        if (item != null)
        {
            item.SetPosition(-2.0f * Vector3.up);
            item.CancelActivation();
            SoundManager.instance.StopSound(item.source);
            yield return new WaitForSeconds(0.2f);
            Destroy(item.gameObject);
            item = null;
        }
        if (_newItem != null)
        {
            if (DataManager.instance.isMulti)
            {
                if (!isServer)
                    yield break;
                item = Instantiate(_newItem, transform).GetComponent<Interactible>();
                NetworkServer.Spawn(item.gameObject);
                RpcChangeItem(item.gameObject, gameObject);
            }
            else
            {
                item = Instantiate(_newItem, transform).GetComponent<Interactible>();
                item.gameObject.SetActive(true);
                item.transform.localPosition = -2.0f * Vector3.up;
                item.SetPosition(Vector3.zero);
            }
        }
    }



    public void Activate(float _delay = 0.0f)
    {
        //GameObject obj;
        if (!isActive)
        {
            //isActive = true;
            switch (type)
            {
                case TileType.Baril:  // Baril
                case TileType.Turret: // Turret
                case TileType.Liquid: // Flaming liquid
                case TileType.Hole: // Hole
                    item.Activate(_delay);

                    break;
                default:
                    break;
            }


        }
    }

    public void Wall(float _duration = 2.0f, float _time = 1.0f)
    {
        if (gameObject.activeInHierarchy && (isActive == false || wallCoroutine != null))
        {
            if (wallCoroutine != null) StopCoroutine(wallCoroutine);
            isActive = true;
            wallCoroutine = StartCoroutine(WallBehaviour(_duration, _time));
        }
    }

    public void Hole(float _duration = 2.0f, float _time = 1.0f)
    {
        if (isActive == false || holeCoroutine != null)
        {
            if (holeCoroutine != null) StopCoroutine(holeCoroutine);
            isActive = true;
            holeCoroutine = StartCoroutine(HoleBehaviour(_duration, _time));
        }
    }

    public IEnumerator HoleBehaviour(float _duration = 2.0f, float _time = 1.0f)
    {
        timeToMove = _time;
        if (type == TileType.Hole && isEnabled)
        {
            TargetPosition = 10.0f * Vector3.down;
        }
        yield return new WaitForSeconds(_duration);
        if (isEnabled)
        {
            TargetPosition = Vector3.zero;
        }
        yield return new WaitForSeconds(3 * _time);
        isActive = false;
        holeCoroutine = null;
    }

    public IEnumerator WallBehaviour(float _duration = 2.0f, float _time = 1.0f)
    {
        timeToMove = _time;
        if (isEnabled)
        {
            TargetPosition = 2.0f * Vector3.up;
            //Type = Tile.TileType.Wall;
        }
        yield return new WaitForSeconds(_duration);
        if (isEnabled)
        {
            TargetPosition = Vector3.zero;
            //Type = Tile.TileType.Empty;
        }
        isActive = false;
    }

    public GameObject GetNearestPlayer(GameObject _exception = null)
    {
        GameObject player = null;
        float minDistance = float.MaxValue;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj != _exception && (obj.transform.position - transform.position).sqrMagnitude < minDistance)
            {
                player = obj;
                minDistance = (obj.transform.position - transform.position).sqrMagnitude;
            }
        }
        return player;
    }

    public List<Tile> GetNeighbourOfType(TileType type)
    {
        List<Tile> tiles = new List<Tile>();

        HexIndex index;
        terrain.GetHexIndex(gameObject, out index);
        List<HexIndex> neighbors = terrain.grid.GetRing(index, 1);
        foreach (HexIndex hex in neighbors)
        {
            GameObject obj;
            terrain.GetGameObject(hex, out obj);
            Tile t = obj.GetComponent<Tile>();
            if (t.Type == type)
            {
                tiles.Add(t);
            }
        }

        return tiles;
    }



    // ONLY TO VISUALIZE DIFFERENT TILE TYPE
    public void SetDebugColor(Color _c)
    {
        if (mesh != null)
        {
            mesh.material.color = _c;
        }
    }




    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    public void LocalSetType(TileType _type)
    {
        if (isEnabled)
        {
            type = _type;
            typeChanged = true;
        }
    }

    private void LocalSetHeight(float _height)
    {
        if (isEnabled)
        {
            height = _height;
            heightChanged = true;
            offset = Vector3.Scale(transform.position, new Vector3(1, 0, 1)) + height * Vector3.up;
        }
    }

    private void LocalSetPosition(Vector3 _pos)
    {
        syncPosition = _pos;
        gameObject.transform.position = syncPosition;
    }

    private void LocalSetActive(bool _active)
    {
        active = _active;
        activeChanged = true;
    }


    ///////////// PRIVATE NETWORK FUNCTION ///////////////
    [ClientRpc]
    public void RpcSetType(TileType _type)
    {
        LocalSetType(_type);
    }

    [ClientRpc]
    private void RpcSetHeight(float _height)
    {
        LocalSetHeight(_height);
    }

    [ClientRpc]
    private void RpcSetPosition(Vector3 _pos)
    {
        LocalSetPosition(_pos);
    }

    [ClientRpc]
    public void RpcSetActive(bool _active)
    {
        LocalSetActive(_active);
    }

    [ClientRpc]
    void RpcChangeItem(GameObject _item, GameObject _parent)
    {
        item = _item.GetComponent<Interactible>();
        item.transform.parent = _parent.transform;
        item.transform.localPosition = -2.0f * Vector3.up;
        item.GetComponent<Interactible>().SetPosition(Vector3.zero);
    }

}
