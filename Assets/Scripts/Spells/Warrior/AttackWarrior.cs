using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class AttackWarrior : NetworkBehaviour
{
    public GameObject caster;
    Transform tfCaster;

    [SerializeField]
    GameObject target;


    float delay;
    float delayMax = 0.2f;
    float speed = 25.0f;

    bool canDamage;
    float ultimateCDR = 5.0f;
    public float totalDamageDealt = 0.0f;


    // Use this for initialization
    void Start()
    {
        delay = 0.0f;
        canDamage = false;

        tfCaster = caster.GetComponent<Transform>();
    }


    private void OnTriggerEnter(Collider other)
    { 
        target = other.gameObject;

        if (target != null && target != caster && target.tag == "Player")
        {
            DamageApplier(target);

            GiftBonused(other);
        }
    }

    public IEnumerator AttackCast()
    {
        delay += Time.deltaTime * speed;
        transform.position = tfCaster.position+ Vector3.up + tfCaster.forward * delay;
        canDamage = true;

        yield return new WaitForSeconds(delayMax);

        canDamage = false;
        if (DataManager.instance.isMulti)
            NetDestroy(gameObject);
        else
            Destroy(gameObject);
    }


    public void DamageApplier(GameObject target)
    {
        if (canDamage)
        {
            totalDamageDealt = caster.GetComponent<Warrior>().currentAttackDamage * caster.GetComponent<Player>().damageMultiplier;
            target.GetComponent<Player>().GetDamage(totalDamageDealt, true);
            caster.GetComponent<Player>().damageDeals += totalDamageDealt;
            caster.GetComponent<Player>().totalDmgDealt += totalDamageDealt;

            caster.GetComponent<Warrior>().attackCount++;
            Destroy(gameObject);

            if (caster.GetComponent<Warrior>().isAttackBoosted)
            {
                if (DataManager.instance.isMulti)
                    caster.GetComponent<Warrior>().RpcUltCooldown(ultimateCDR);
                else
                    caster.GetComponent<Player>().GetSpell("FuriousThrow").timer += ultimateCDR;
            }
            if (DataManager.instance.isMulti)
                NetDestroy(gameObject);
            else
                Destroy(gameObject);
        }
    }

    public void GiftBonused(Collider other)
    {
        
        if (caster.GetComponent<Player>().canStun == true)
        {
            if (other.GetComponent<Player>().isShielded == true)
            {
                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().RpcIsShielded(false);
                    caster.GetComponent<Player>().RpcCanStun(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().isShielded = false;
                    caster.GetComponent<Player>().canStun = false;
                }
            }
            else
            {

                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().CmdPlaySound(1, 60, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    target.GetComponent<Player>().SoundCharacter(1, 60, 1.0f, AudioType.SFX);
                }

                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().RpcStun(2.0f);
                    caster.GetComponent<Player>().RpcCanStun(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().Stun(2.0f);
                    caster.GetComponent<Player>().canStun = false;
                }
            }
        }
        else if (caster.GetComponent<Player>().canPanic == true)
        {
            if (other.GetComponent<Player>().isShielded == true)
            {
                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().RpcIsShielded(false);
                    caster.GetComponent<Player>().RpcCanPanic(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().isShielded = false;
                    caster.GetComponent<Player>().canPanic = false;
                }
            }
            else
            {
                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().RpcPanic(1.5f, caster.gameObject);
                    caster.GetComponent<Player>().RpcCanPanic(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().opponent = caster.transform;
                    other.GetComponent<Player>().Panic(1.5f);
                    caster.GetComponent<Player>().canPanic = false;
                }
            }
        }
        else if (caster.GetComponent<Player>().canSlow == true)
        {
            if (other.GetComponent<Player>().isShielded == true)
            {
                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().RpcIsShielded(false);
                    caster.GetComponent<Player>().RpcCanSlow(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().isShielded = false;
                    caster.GetComponent<Player>().canSlow = false;
                }
            }
            else
            {
                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().CmdPlaySound(1, 63, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    other.GetComponent<Player>().SoundCharacter(1, 63, 1.0f, AudioType.SFX);
                }

                if (DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().RpcSlow(0.3f, 1.0f, 4.0f, true);
                    caster.GetComponent<Player>().RpcCanSlow(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().Slow(0.3f, 1.0f, 4.0f, true);
                    caster.GetComponent<Player>().canSlow = false;
                }
            }
        }
        if (caster.GetComponent<Player>().canFlame)
        {
            if (DataManager.instance.isMulti)
                other.GetComponent<Player>().RpcAflame(2.5f);
            else if (!DataManager.instance.isMulti)
                other.GetComponent<Player>().Aflame(2.5f);
        }

        if (caster.GetComponent<Player>().canVamp)
        {
            if (!DataManager.instance.isMulti || (DataManager.instance.isMulti))
            {
                if (caster.GetComponent<Player>().currentHealth <= caster.GetComponent<Player>().healthMax)
                {
                    caster.GetComponent<Player>().currentHealth += (caster.GetComponent<Warrior>().currentAttackDamage * caster.GetComponent<Player>().damageMultiplier);

                    if (caster.GetComponent<Player>().currentHealth > caster.GetComponent<Player>().healthMax)
                    {
                        caster.GetComponent<Player>().currentHealth = caster.GetComponent<Player>().healthMax;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(AttackCast());
    }

    [ServerCallback]
    void NetDestroy(GameObject gm)
    {
        NetworkServer.Destroy(gm);
    }
}