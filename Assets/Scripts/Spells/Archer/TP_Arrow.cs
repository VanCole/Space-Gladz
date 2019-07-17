using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TP_Arrow : Projectile
{
    Archer archer;
    float TPArrowLifeSpan = 1.5f;
    public bool arrowInSomething = false;
    public float totalDamageDealt = 0.0f;
    public bool hit = false;

    float tpArrowVelocity = 10.0f;


    [SerializeField]
    GameObject prefabTPArrowDestroyedFX;

    // Use this for initialization
    override protected void Start()
    {
        //Call start from mother class
        base.Start();

        archer = caster.GetComponent<Archer>();

        StartCoroutine(DelayCanBestroy());
        StartCoroutine(DestroyArrow());
    }

    private void OnDestroy()
    {

        if (prefabTPArrowDestroyedFX)
        {
            GameObject fx = Instantiate(prefabTPArrowDestroyedFX);
            fx.transform.position = transform.position;
            fx.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[archer.colorIndex]);
            Destroy(fx, 1.0f);
        }
    }


    override protected void Update()
    {
        //Call update from mother class
        base.Update();

        Travelling(tpArrowVelocity);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.tag.Equals("DefenseDome"))
        {
            arrowInSomething = true;
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player") && other.gameObject != caster.gameObject)
        {
            if (other.gameObject.GetComponent<Player>().isShielded)
            {
                other.gameObject.GetComponent<Player>().isShielded = false;
                archer.tp_arrow_instantiated = null;
                Destroy(gameObject);
            }
            else
            {
                if (!other.GetComponent<Player>().isShielded)
                {
                    archer.GetComponent<Player>().damageDeals += archer.damageSpell3 * archer.damageMultiplier;
                    archer.GetComponent<Player>().totalDmgDealt += archer.damageSpell3 * archer.damageMultiplier;


                }

                totalDamageDealt += archer.damageSpell3 * archer.damageMultiplier;
                other.GetComponent<Player>().GetDamage(totalDamageDealt, false);
                Debug.Log("Name : " + other.name + " /// Health : " + other.GetComponent<Player>().currentHealth);

                hit = true;
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        arrowInSomething = false;
    }

    IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(TPArrowLifeSpan);
        archer.tp_arrow_instantiated = null;
        if (DataManager.instance.isMulti)
        {
            NetDestroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    [ServerCallback]
    void NetDestroy(GameObject gm)
    {
        NetworkServer.Destroy(gm);
    }
}
