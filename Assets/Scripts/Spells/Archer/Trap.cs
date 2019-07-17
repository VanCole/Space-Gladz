using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Trap : NetworkBehaviour
{

    public Archer archer;
    public Player playerTrapped;
    public bool trapTaken = false;
    public Tile tile;

    private ParticleSystem placedFX;
    private GameObject circleFX;
    private Animator anim;

    [SerializeField] Material trapMaterial;

    public float trapSlow = 0.5f;
    public float totalDamageDealt = 0.0f;
    public bool placed = false;
    private bool prevPlaced = false;

    private void Start()
    {
        placedFX = transform.Find("PlacedFX").GetComponent<ParticleSystem>();
        circleFX = transform.Find("PlacedCircleFX").gameObject;
        anim = GetComponent<Animator>();
        GetComponentInChildren<MeshRenderer>().material.color = DataManager.instance.playerColor[archer.colorIndex] + new Color(0.5f, 0.5f, 0.5f);
        //foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        //{
        //    mr.material.SetColor("_EmissionColor", DataManager.instance.playerColor[archer.colorIndex]);
        //}
        //foreach (ParticleSystemRenderer psr in GetComponentsInChildren<ParticleSystemRenderer>())
        //{
        //    psr.material.SetColor("_EmissionColor", DataManager.instance.playerColor[archer.colorIndex]);
        //}
    }

    private void Update()
    {
        if (tile == null)
        {
            
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, 10000, LayerMask.GetMask("Hexagon")))
            {
                tile = hit.collider.transform.parent.gameObject.GetComponent<Tile>();
            }
            Debug.Log("tile : " + tile);
        }

        if (tile != null && (!tile.isEnabled || tile.Type == Tile.TileType.Wall || tile.Type == Tile.TileType.Turret || tile.Type == Tile.TileType.Baril || tile.Type == Tile.TileType.Hole))
        {
            if (DataManager.instance.isMulti)
            {
                RpcTrapDecrease();
                archer.nbTrapPlaced--;
                NetDestroy(gameObject);
            }
            else
            {
                archer.nbTrapPlaced--;
                Destroy(gameObject);
            }
        }

        if (!prevPlaced && placed)
        {
            GetComponentInChildren<MeshRenderer>().material = trapMaterial;
            GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[archer.colorIndex]);
            //placedFX.Play();
            circleFX.GetComponent<QuadTextureAnimation>().Play();
            anim.Play("TrapPlacedFX", -1, 0);
        }
        prevPlaced = placed;

        if (trapTaken)
        {
            transform.position = playerTrapped.transform.position;
        }
    }

    IEnumerator Trapped()
    {
        trapTaken = true;
        playerTrapped.Slow(trapSlow, trapSlow, archer.timerTrapped, true);
        playerTrapped.canDodge = false;
        totalDamageDealt = archer.damageSpell2 * archer.damageMultiplier;
        playerTrapped.GetDamage(totalDamageDealt, false);
        //archer.GetComponent<Player>().AddAttackInTab(totalDamageDealt);

        if (!playerTrapped.GetComponent<Player>().isShielded)
        {
            archer.GetComponent<Player>().damageDeals += totalDamageDealt;
            archer.GetComponent<Player>().totalDmgDealt += totalDamageDealt;
        }

        yield return new WaitForSeconds(archer.timerTrapped);

        trapTaken = false;
        playerTrapped.canDodge = true;
        if (DataManager.instance.isMulti)
        {
            RpcTrapDecrease();
            archer.nbTrapPlaced--;
            NetDestroy(gameObject);
        }
        else
        {
            archer.nbTrapPlaced--;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject != archer.gameObject && other.tag.Equals("Player"))
        {
            if (DataManager.instance.isMulti)
            {
                other.GetComponent<Player>().CmdPlaySound(1, 70, 1.0f, 0, 0, 0.0f, 2, false, AudioType.SFX);
            }
            else
            {
                SoundManager.instance.PlaySound(70, 1.0f, AudioType.SFX);
            }

            if (other.gameObject.GetComponent<Player>().isShielded)
            {
                other.gameObject.GetComponent<Player>().isShielded = false;
                if (DataManager.instance.isMulti)
                {
                    RpcTrapDecrease();
                    archer.nbTrapPlaced--;
                    NetDestroy(gameObject);
                }
                else
                {
                    archer.nbTrapPlaced--;
                    Destroy(gameObject);
                }
            }
            else
            {
                playerTrapped = other.GetComponent<Player>();
                StartCoroutine(Trapped());
            }
        }
    }

    void NetDestroy(GameObject gm)
    {
        NetworkServer.Destroy(gm);
    }

    [ClientRpc]
    void RpcTrapDecrease()
    {
        archer.nbTrapPlaced--;
    }
}
