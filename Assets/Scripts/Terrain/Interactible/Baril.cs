using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Baril : Interactible
{
    //[SerializeField] GameObject explosionParticle;

    ParticleSystem explosion;
    MeshRenderer mr;
    SphereCollider explosionTrigger;
    CapsuleCollider barilCollider;
    GameObject explosionObj;
    bool consumed = false;

    protected override void Start()
    {
        base.Start();
        mr = GetComponent<MeshRenderer>();
        barilCollider = GetComponent<CapsuleCollider>();
        explosionObj = transform.GetChild(0).gameObject;
        explosion = explosionObj.GetComponent<ParticleSystem>();
        explosionTrigger = explosionObj.GetComponent<SphereCollider>();
    }

    protected override void Behaviour()
    {
        base.Behaviour();

        if(DataManager.instance.isMulti)
        {
            CmdExpansionBehaviour();
        }
        else
        {
            LocalExpansionBehaviour();
        }
        StartCoroutine(BarilBehaviour());
    }

    public IEnumerator BarilBehaviour(float _duration = 0.2f)
    {
        if (!consumed)
        {
            consumed = true;
            mr.enabled = false;
            barilCollider.enabled = false;
            explosionObj.SetActive(true);
            explosionTrigger.enabled = true;
            explosion.Play();
            SoundManager.instance.PlaySound(3, 1.0f, AudioType.SFX);
            CameraManager.instance.camera.Shake(true, 0.2f, 0.5f, 0.0f, 1.0f);
            yield return new WaitForSeconds(_duration);
            explosionTrigger.enabled = false;
            yield return new WaitUntil(() => { return explosion.isStopped && explosion.transform.GetChild(0).GetComponent<ParticleSystem>().isStopped; });
            tile.Type = Tile.TileType.Empty;
            tile.isActive = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.GetMask("Attack"))
        {
            tile.Activate();
        }
        if (collision.collider.gameObject.CompareTag("Explosion"))
        {
            tile.Activate(0.2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            tile.Activate();
        }
        if (other.gameObject.CompareTag("Explosion"))
        {
            tile.Activate(0.2f);
        }
    }

    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    private void LocalExpansionBehaviour()
    {
        foreach (Tile t in tile.GetNeighbourOfType(Tile.TileType.Liquid))
        {
            if (DataManager.instance.isMulti)
            {
                RpcBehaviour(t.gameObject);
            }
            else
            {
                LocalActivationBehaviour(t.gameObject);
            }
        }
    }

    private void LocalActivationBehaviour(GameObject _t)
    {
        if (!_t.GetComponent<Tile>().isActive)
        {
            _t.GetComponent<Tile>().Activate(0.2f);
        }
    }


    ///////////// PRIVATE NETWORK FUNCTIONS //////////////

    [Command]
    private void CmdExpansionBehaviour()
    {
        LocalExpansionBehaviour();
    }

    [ClientRpc]
    private void RpcBehaviour(GameObject _t)
    {
        LocalActivationBehaviour(_t);
    }
}
