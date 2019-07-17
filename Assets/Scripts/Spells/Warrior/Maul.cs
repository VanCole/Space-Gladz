using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Maul : NetworkBehaviour
{
    public GameObject caster;
    Transform tfCaster;

    [SerializeField]
    GameObject target;

    float channelTime = 1.5f;
    float tickTimer = 0.25f;
    float ultimateCDR = 0.50f;
    bool hit = false;

    bool canDamage;
    public bool endChannel;

    float totalDamageDealt;

    // Use this for initialization
    void Start()
    {
        canDamage = true;
        endChannel = false;

        totalDamageDealt = 0.0f;
        caster.GetComponent<Warrior>().currentMaul = this;
        tfCaster = caster.GetComponent<Transform>();
    }


    private void OnTriggerStay(Collider other)
    {
        target = other.gameObject;

        if (target != null && target != caster && target.tag == "Player")
        {
            if (target.GetComponent<Player>().isShielded)
            {
                target.GetComponent<Player>().isShielded = false;
                endChannel = true;
                Destroy(gameObject);
            }
            else
            {
                if (canDamage)
                {
                    hit = true;
                    StartCoroutine(Channeled(target));
                }
            }
        }
    }

    public IEnumerator Channeled(GameObject target)
    {
        if (canDamage)
        {
            totalDamageDealt = caster.GetComponent<Warrior>().MaulDamagePerTick * caster.GetComponent<Player>().damageMultiplier;
            target.GetComponent<Player>().GetDamage(totalDamageDealt, false);
            caster.GetComponent<Player>().damageDeals += totalDamageDealt;
            caster.GetComponent<Player>().totalDmgDealt += totalDamageDealt;

            if (DataManager.instance.isMulti && isServer)
                caster.GetComponent<Warrior>().RpcUltCooldown(ultimateCDR);
            else if (!DataManager.instance.isMulti)
                caster.GetComponent<Player>().GetSpell("FuriousThrow").timer += ultimateCDR;

            totalDamageDealt += caster.GetComponent<Warrior>().MaulDamagePerTick * caster.GetComponent<Player>().damageMultiplier;
            canDamage = false;
        }

        yield return new WaitForSeconds(tickTimer);
        canDamage = true;
    }

    // Update is called once per frame
    void Update()
    {
        channelTime -= Time.deltaTime;

        EndMaul();

        //Hitbox is oriented by caster's sight
        transform.position = tfCaster.position + tfCaster.forward * 2.0f + Vector3.up;
    }


    void EndMaul()
    {
        //Cancel cast if stunned
        if (caster.GetComponent<Player>().isStunned)
        {
            endChannel = true;
            //Debug.Log("Maul total dmg " + totalDamageDealt);

            if (DataManager.instance.isMulti)
                NetDestroy(gameObject);
            else
                Destroy(gameObject);
        }

        //Cancel Cast when channel completed
        if (channelTime <= 0.0f)
        {
            endChannel = true;
            //Debug.Log("Maul total dmg " + totalDamageDealt);

            if (DataManager.instance.isMulti)
                NetDestroy(gameObject);
            else
                Destroy(gameObject);
        }
    }

    [ServerCallback]
    void NetDestroy(GameObject gm)
    {
        NetworkServer.Destroy(gm);
    }
}
