using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
    Archer archer;

    float arrowVelocity = 25.0f; 
    float arrowLifeSpan = 4.0f;
    public float totalDamageDealt = 0.0f;

    [SerializeField]
    GameObject shotFX;


    override protected void Start()
    {        
        //Call start from mother class
        base.Start();

        StartCoroutine(DelayCanBestroy());
        StartCoroutine(DestroyArrow());

        archer = caster.GetComponent<Archer>();

        GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[caster.colorIndex]);

        GameObject obj = Instantiate(shotFX);
        obj.transform.position = transform.position;
        obj.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[caster.colorIndex]);
        obj.GetComponent<Animator>().Play("ProjectileFX", -1, 0);

        Destroy(gameObject, 10.0f);

    }


    override protected void Update()
    {
        //Call update from mother class
        base.Update();

        Travelling(arrowVelocity);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (archer.shadowActive)
        {
            if (other != null && other.gameObject != archer.gameObject && other.tag.Equals("Player"))
            {
                archer.arrowHit += 1;
                totalDamageDealt = archer.damageAA * archer.damageMultiplier;
                other.GetComponent<Player>().GetDamage(totalDamageDealt, true);
                //Debug.Log("Name : " + other.name + " /// Health : " + other.GetComponent<Player>().currentHealth);
                archer.GetComponent<Player>().damageDeals += totalDamageDealt;
                archer.GetComponent<Player>().totalDmgDealt += totalDamageDealt;


                Destroy(gameObject);
            }
        }
        else
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                archer.arrowHit += 1;
                totalDamageDealt = archer.damageAA * archer.damageMultiplier;
                other.GetComponent<Player>().GetDamage(totalDamageDealt, true);
                //Debug.Log("Name : " + other.name + " /// Health : " + other.GetComponent<Player>().currentHealth);
                archer.GetComponent<Player>().damageDeals += totalDamageDealt;
                archer.GetComponent<Player>().totalDmgDealt += totalDamageDealt;

                Destroy(gameObject);
            }
        }


        Tile tile = other.gameObject.GetComponentInParent<Tile>();
        if (tile != null)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Hexagon") || tile.Type == Tile.TileType.Baril || tile.Type == Tile.TileType.Turret)
            {
                Destroy(gameObject);
            }
        }

        if (other != null && other.gameObject != archer.gameObject && other.tag.Equals("Player"))
        {
            GiftBonused(other);
        }
    }

    IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(arrowLifeSpan);
        Destroy(gameObject);
    }

    public void GiftBonused(Collider other)
    {
        if (archer.GetComponent<Player>().canStun == true)
        {
            if (other.GetComponent<Player>().isShielded == true)
            {
                if (DataManager.instance.isMulti && isServer)
                {
                    other.GetComponent<Player>().RpcIsShielded(false);
                    archer.GetComponent<Player>().RpcCanStun(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().isShielded = false;
                    archer.GetComponent<Player>().canStun = false;
                }
            }
            else
            {
                if(DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().CmdPlaySound(1, 60, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    other.GetComponent<Player>().SoundCharacter(1, 60, 1.0f, AudioType.SFX);
                }

                if (DataManager.instance.isMulti && isServer)
                {
                   other.GetComponent<Player>().RpcStun(2.0f);
                   archer.GetComponent<Player>().RpcCanStun(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().Stun(2.0f);
                    archer.GetComponent<Player>().canStun = false;
                }
            }
        }
        else if (archer.GetComponent<Player>().canPanic == true)
        {
            if (other.GetComponent<Player>().isShielded == true)
            {
                if (DataManager.instance.isMulti && isServer)
                {
                    other.GetComponent<Player>().RpcIsShielded(false);
                    archer.GetComponent<Player>().RpcCanPanic(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().isShielded = false;
                    archer.GetComponent<Player>().canPanic = false;
                }
            }
            else
            {
                if (DataManager.instance.isMulti && isServer)
                {
                    other.GetComponent<Player>().RpcPanic(1.5f, archer.gameObject);
                    archer.GetComponent<Player>().RpcCanPanic(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().opponent = archer.transform;
                    other.GetComponent<Player>().Panic(1.5f);
                    archer.GetComponent<Player>().canPanic = false;
                }
            }
        }
        else if (archer.GetComponent<Player>().canSlow == true)
        {
            if (other.GetComponent<Player>().isShielded == true)
            {
                if (DataManager.instance.isMulti && isServer)
                {
                    other.GetComponent<Player>().RpcIsShielded(false);
                    archer.GetComponent<Player>().RpcCanSlow(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().isShielded = false;
                    archer.GetComponent<Player>().canSlow = false;
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
                if (DataManager.instance.isMulti && isServer)
                {
                    other.GetComponent<Player>().RpcSlow(0.3f, 1.0f, 4.0f, true);
                    archer.GetComponent<Player>().RpcCanSlow(false);
                }
                else if (!DataManager.instance.isMulti)
                {
                    other.GetComponent<Player>().Slow(0.3f, 1.0f, 4.0f, true);
                    archer.GetComponent<Player>().canSlow = false;
                }
            }
        }

        if (archer.GetComponent<Player>().canFlame)
        {
            if (DataManager.instance.isMulti && isServer)
                other.GetComponent<Player>().RpcAflame(2.5f);
            else if (!DataManager.instance.isMulti)
                other.GetComponent<Player>().Aflame(2.5f);
        }

        if (archer.GetComponent<Player>().canVamp)
        {
            if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isServer))
            {
                if (archer.GetComponent<Player>().currentHealth <= archer.GetComponent<Player>().healthMax)
                {
                    archer.GetComponent<Player>().currentHealth += archer.damageAA * archer.damageMultiplier;

                    if (archer.GetComponent<Player>().currentHealth > archer.GetComponent<Player>().healthMax)
                    {
                        archer.GetComponent<Player>().currentHealth = archer.GetComponent<Player>().healthMax;
                    }
                }
            }
        }
    }
}
