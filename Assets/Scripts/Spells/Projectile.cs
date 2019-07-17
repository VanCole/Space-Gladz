using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    public Player caster;

    public Rigidbody rb;

    public bool canBeDestroy = false;
    public float velocity;

    public GameObject destroyFX = null;

    // Use this for initialization
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Travelling();
    }


    public void Travelling(float velocity)
    {
        rb.velocity = transform.forward * velocity;
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        //Destroy projectile upon colliding with a dome
        if (other.gameObject.tag.Equals("DefenseDome"))
        {
            if (canBeDestroy)
            {
                Destroy(gameObject);
            }
        }
    }



    public IEnumerator DelayCanBestroy()
    {
        yield return new WaitForSeconds(0.01f);
        canBeDestroy = true;
    }


    private void OnDestroy()
    {
        // Spawn FX
        if (destroyFX != null)
        {
            GameObject obj = Instantiate(destroyFX);
            obj.transform.position = transform.position;
            Animator anim = obj.GetComponent<Animator>();
            if(anim != null)
            {
                anim.Play("ProjectileFX", -1, 0);
            }
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if(mr != null)
            {
                Color color = GetComponentInChildren<MeshRenderer>().material.GetColor("_EmissionColor");
                mr.material.SetColor("_EmissionColor", color);
            }
        }
    }

}
