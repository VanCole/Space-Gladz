using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Harpoon : NetworkBehaviour
{
    public GameObject caster;
    Transform posWarrior;

    [SerializeField]
    GameObject target;

    Rigidbody rb;
    float velocity = 35.0f;  // 35.0f

    Vector3 distanceFromCaster;
    float currentDistance;
    float distanceMax = 15.0f;

    Vector3 startPos;

    bool isReturning;
    public bool isStuck;


    float stunDuration = 0.1f;
    float debuffDuration = 2.0f;
    float speedDebuffMultiplier = 0.4f;
    float totalDamageDealt = 0.0f;

    public bool endHarpoon;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        posWarrior = caster.GetComponent<Transform>();

        caster.GetComponent<Warrior>().currentHarpoon = this;

        isReturning = false;
        isStuck = false;
        endHarpoon = false;


        startPos = new Vector3(posWarrior.position.x, posWarrior.position.y + 1.0f, posWarrior.position.z);
        GetComponent<LineRenderer>().SetPosition(0, startPos);
        GetComponent<LineRenderer>().SetPosition(1, startPos);

        GetComponent<LineRenderer>().startWidth = 0.1f;
        GetComponent<LineRenderer>().startColor = caster.GetComponent<Player>().color;
        GetComponent<LineRenderer>().endColor = caster.GetComponent<Player>().color;

        //Debug.Log(caster.GetComponent<Player>().color);

        //transform.Rotate(90.0f, 0.0f, 0.0f);

        GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[caster.GetComponent<Player>().colorIndex]);


    }

    // Update is called once per frame
    void Update()
    {
        Travelling();
        HarpoonBehavior();
        DisjoinctmentProtection();

        LineRendererManager();
    }

    void LineRendererManager()
    {
        startPos = new Vector3(posWarrior.position.x, posWarrior.position.y + 1.0f, posWarrior.position.z);

        GetComponent<LineRenderer>().SetPosition(0, transform.position);
        GetComponent<LineRenderer>().SetPosition(1, startPos + transform.right * 0.42f + transform.up * 0.7f);
    }


    void HarpoonBehavior()
    {
        //If harpoon sget stuck in a wall, caster is pull to it destination
        if (isStuck) 
        {
            velocity = 0.0f;
            caster.GetComponent<Rigidbody>().velocity = transform.up * 25.0f;
            caster.GetComponent<Warrior>().GetSpell("Harpoon").timer = caster.GetComponent<Warrior>().harpoonCD / 2.0f;
        }

        if (currentDistance >= distanceMax)
        {
            isReturning = true;
        }

        if (currentDistance <= 2.0f && (isReturning || isStuck))
        {
            endHarpoon = true;

            //If harpoon successfully pull a target, target is slowed upon arriving at caster location
            if (target != null && target != caster && target.tag == "Player")
            {
                if (DataManager.instance.isMulti && isServer)
                    target.GetComponent<Player>().RpcSlow(speedDebuffMultiplier, 1.0f, debuffDuration, true);
                else if (!DataManager.instance.isMulti)
                    target.GetComponent<Player>().Slow(speedDebuffMultiplier, 1.0f, debuffDuration, true);
                target.GetComponent<Player>().harponned = null;
            }
            if (DataManager.instance.isMulti)
                NetDestroy(gameObject);
            else
                Destroy(gameObject);
        }
    }

    void Travelling()
    {
        distanceFromCaster = posWarrior.position + Vector3.up - transform.position;
        currentDistance = Vector3.Scale(distanceFromCaster, new Vector3(1.0f, 0.0f, 1.0f)).magnitude;

        if (!isReturning)
        {
            rb.velocity = transform.up * velocity;
        }
        else
        {
            rb.velocity = -transform.up * velocity;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        target = other.gameObject;

        if (target != null && target != caster)
        {
            if (target.tag == "Player") //Harpoon hook the target
            {
                if(target.GetComponent<Player>().isShielded)
                {
                    if (DataManager.instance.isMulti && isServer )
                        target.GetComponent<Player>().RpcIsShielded(false);
                    else if (!DataManager.instance.isMulti)
                        target.GetComponent<Player>().isShielded = false;
                    endHarpoon = true;
                    if (DataManager.instance.isMulti)
                        CmdNetDestroy(gameObject);
                    else
                        Destroy(gameObject);
                }
                else
                {
                    caster.GetComponent<Warrior>().harpoonSoundPlayed = true;

                    if (DataManager.instance.isMulti && isServer)
                        target.GetComponent<Player>().RpcStun(stunDuration);
                    else if (!DataManager.instance.isMulti)
                        target.GetComponent<Player>().Stun(stunDuration);

                    if (DataManager.instance.isMulti && target.GetComponent<Player>().isLocalPlayer)
                        target.GetComponent<Player>().harponned = gameObject;
                    else
                        other.transform.position = transform.position - Vector3.up;
                    isReturning = true;

                   
                }
            }
            else if (target.layer == LayerMask.NameToLayer("Hexagon") && !isReturning) // Harpoon get stuck in the wall
            {
                isStuck = true;
            }
        }
    }

    //Delete Harpoon if player is hard controlled or player is mispositionned 
    void DisjoinctmentProtection()
    {
        if (distanceFromCaster.y <= -1.0f)
        {
            endHarpoon = true;

            Destroy(gameObject);

        }

        if (caster.GetComponent<Player>().isStunned)
        {
            endHarpoon = true;

            Destroy(gameObject);
        }

        RaycastManager();
    }

    void RaycastManager()
    {
        RaycastHit hitsHexagon;

        if (Physics.Raycast(caster.transform.position, Vector3.Normalize(transform.position - caster.transform.position),
            out hitsHexagon, (transform.position - caster.transform.position).magnitude / 2.0f, LayerMask.GetMask("Hexagon")))
        {
            if (hitsHexagon.transform.gameObject.layer == LayerMask.NameToLayer("Hexagon"))
            {
                endHarpoon = true;

                Destroy(gameObject);
                Debug.Log("DestroyRayCast");
            }         
        }
    }

    [ClientRpc]
    void RpcToHarpoon()
    {
        target.GetComponent<Player>().harponned = gameObject;
    }

    [Command]
    void CmdNetDestroy(GameObject gm)
    {
        NetworkServer.Destroy(gm);
    }

    [ServerCallback]
    void NetDestroy(GameObject gm)
    {
        NetworkServer.Destroy(gm);
    }
}
