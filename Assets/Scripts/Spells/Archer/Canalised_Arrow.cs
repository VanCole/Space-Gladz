using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canalised_Arrow : Projectile
{
    Archer archer;
    float canalisedArrowLifeSpan = 4.0f;
    public float totalDamageDealt = 0.0f;
    public bool charged = false;
    public bool launched = false;
    private bool endCharging = false;

    private Animator anim;
    private ParticleSystem chargingFX, chargeEndFX, chargedFX;

    override protected void Start()
    {
        base.Start();

        anim = GetComponent<Animator>();

        archer = caster.GetComponent<Archer>();
        velocity = 0.0f;

        chargingFX = transform.Find("ChargingFX").GetComponent<ParticleSystem>();
        chargeEndFX = transform.Find("ChargeEndFX").GetComponent<ParticleSystem>();
        chargedFX = transform.Find("ChargedFX").GetComponent<ParticleSystem>();
        chargingFX.Play();

        chargingFX.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[caster.colorIndex]);
        GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[caster.colorIndex]);
    }


    // Use this for initialization
    override protected void Update()
    {
        //Call update from mother class
        base.Update();

        if (launched)
        {
            StartCoroutine(DelayCanBestroy());
            StartCoroutine(DestroyArrow());
            chargedFX.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            chargeEndFX.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            chargingFX.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else
        {
            if (charged)
            {
                if (!endCharging)
                {
                    chargingFX.Stop();
                    chargeEndFX.Play();
                    //Debug.Log("Play chargeEnd : " + chargeEndFX.isPlaying);
                }

                endCharging = true;

                if(!chargeEndFX.isPlaying)
                {
                    chargedFX.Play();
                }
            }
        }
    }

  
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);


        if (archer.dmgArrow)
        {
            if (other != null && other.gameObject != archer.gameObject && other.tag.Equals("Player"))
            {
                if (!other.GetComponent<Player>().isShielded)
                {
                    archer.GetComponent<Player>().damageDeals += archer.damageSpell1_charged * archer.damageMultiplier;
                    archer.GetComponent<Player>().totalDmgDealt += archer.damageSpell1_charged * archer.damageMultiplier;

                }

                totalDamageDealt = archer.damageSpell1_charged * archer.damageMultiplier;
                other.GetComponent<Player>().GetDamage(totalDamageDealt, false);
                Debug.Log("Name : " + other.name + " /// Health : " + other.GetComponent<Player>().currentHealth);

                Destroy(gameObject);
            }
        }
        else
        {
            if (other != null && other.gameObject != archer.gameObject && other.tag.Equals("Player"))
            {
                if (other.gameObject.GetComponent<Player>().isShielded)
                {
                    other.gameObject.GetComponent<Player>().isShielded = false;
                }
                else
                {
                    if (!other.GetComponent<Player>().isShielded)
                    {
                        archer.GetComponent<Player>().damageDeals += archer.damageSpell1 * archer.damageMultiplier;
                        archer.GetComponent<Player>().totalDmgDealt += archer.damageSpell1_charged * archer.damageMultiplier;

                    }

                    totalDamageDealt = archer.damageSpell1 * archer.damageMultiplier;
                    other.GetComponent<Player>().GetDamage(totalDamageDealt, false);
                    other.GetComponent<Player>().opponent = archer.transform;
                    other.GetComponent<Player>().KnockBack(0.1f);


                    Debug.Log("Name : " + other.name + " /// Health : " + other.GetComponent<Player>().currentHealth);
                }

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
    }

    IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(canalisedArrowLifeSpan);
        Destroy(gameObject);
    }
}
