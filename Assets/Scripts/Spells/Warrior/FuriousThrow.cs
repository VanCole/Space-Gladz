using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FuriousThrow : Projectile
{
    [SerializeField]
    GameObject target;

    public ParticleSystem explosionUltimate;
    public GameObject explosionUltimateObj;

    float furiousThrowVelocity = 55.0f;
    float projectileLifeSpan = 1.0f;
    float stunDuration = 0.5f;

    float damage;
    bool canDamage = true;
    float totalDamageDealt;
    bool particlePlayed = false;

    override protected void Start()
    {
        //Call start from mother class
        base.Start();
        StartCoroutine(DelayCanBestroy());
        totalDamageDealt = 0.0f;

        damage = caster.GetComponent<Warrior>().UltDamage;
    }


    // Update is called once per frame
    override protected void Update()
    {
        //Call update from mother class
        base.Update();
        StartCoroutine(Travelling());
    }


    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        target = other.gameObject;



        if (target != null && target.tag == "Player" && target.gameObject != caster.gameObject)
        {
            DamageApplier();
        }
    }


    void DamageApplier()
    {
        if (!target.GetComponent<Player>().isShielded)
        {
            caster.GetComponent<Player>().damageDeals += damage * caster.GetComponent<Player>().damageMultiplier;
            caster.GetComponent<Player>().totalDmgDealt += damage * caster.GetComponent<Player>().damageMultiplier;

        }

        target.GetComponent<Player>().GetDamage(damage * caster.GetComponent<Player>().damageMultiplier, false);
        totalDamageDealt += damage * caster.GetComponent<Player>().damageMultiplier;
        canDamage = false;

        if (DataManager.instance.isMulti && isServer)
            target.GetComponent<Player>().RpcStun(stunDuration);
        else if (!DataManager.instance.isMulti)
            target.GetComponent<Player>().Stun(stunDuration);
        //target.GetComponent<Player>().StartCoroutine(target.GetComponent<Player>().IsStunned(stunDuration));

        Destroy(gameObject);
    }

    public IEnumerator Travelling()
    {
        rb.velocity = transform.forward * furiousThrowVelocity;

        yield return new WaitForSeconds(projectileLifeSpan);

        if(DataManager.instance.isMulti)
        {
            caster.CmdPlaySound(1, 50, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Voice);
            caster.CmdPlaySound(1, 55, 0.4f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
        }
        else
        {
            SoundManager.instance.PlaySound(50, 1.0f, AudioType.Voice);
            SoundManager.instance.PlaySound(55, 0.4f, AudioType.Ambient);
        }

        if (DataManager.instance.isMulti)
            NetDestroy(gameObject);
        else
            Destroy(gameObject);
    }

    [ServerCallback]
    void NetDestroy(GameObject gm)
    {
        NetworkServer.Destroy(gm);
    }

    private void OnDestroy()
    {
        particlePlayed = false;
        GameObject obj = Instantiate(explosionUltimateObj);
        obj.transform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z) + transform.forward * 5.0f;


        if(particlePlayed == false)
        {
            explosionUltimate.Play();
            particlePlayed = true;
            explosionUltimate.Stop();
        }
       
        //MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        //if (mr != null)
        //{
        //    Color color = GetComponentInChildren<MeshRenderer>().material.GetColor("_EmissionColor");
        //    mr.material.SetColor("_EmissionColor", color);
        //}
    }
}
