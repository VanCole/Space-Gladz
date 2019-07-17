using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectile : Projectile
{

    //public GameObject caster = null;

    float turretLaserVelocity = 20.0f;
    float lifeSpan = 1.0f;

    // Use this for initialization
    override protected void Start()
    {
        //Call start from mother class
        base.Start();

        StartCoroutine(DelayCanBestroy());
        StartCoroutine(DestroyProjectile());
    }

    override protected void Update()
    {
        //Call update from mother class
        base.Update();

        Travelling(turretLaserVelocity);
    }



    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        GameObject target = other.gameObject;

        if (target != null)
        {
            if (target.tag == "Player")
            {
                target.GetComponent<Player>().GetDamage(10.0f);
                //target.GetComponent<Player>().currentHealth -= 10;
                Destroy(gameObject);
            }
            else if (target.layer == LayerMask.NameToLayer("Hexagon"))
            {
                Destroy(gameObject);
            }
        }
    }

    IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(lifeSpan);
        Destroy(gameObject);
    }
}
