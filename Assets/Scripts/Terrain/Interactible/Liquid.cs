using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Liquid : Interactible
{
    ParticleSystem flames;
    ParticleSystem smoke;
    SphereCollider colliderGas, colliderFlames;
    bool consumed = false;

    protected override void Start()
    {
        base.Start();
        smoke = transform.GetChild(0).GetComponent<ParticleSystem>();
        flames = transform.GetChild(1).GetComponent<ParticleSystem>();
        smoke.Play();
        colliderGas = GetComponent<SphereCollider>();
        colliderFlames = transform.GetChild(1).GetComponent<SphereCollider>();
        offset = 0.05f * Vector3.up;
    }

    protected override void Update()
    {
        base.Update();

        if (!smoke.isPlaying && !consumed)
            smoke.Play();
    }

    protected override void Behaviour()
    {
        if (!consumed)
        {
            base.Behaviour();

            if (DataManager.instance.isMulti)
            {
                CmdBehaviour();
            }
            else
            {
                LocalBehaviour();
            }

            StartCoroutine(LiquidBehaviour());
            consumed = true;
        }
    }


    public IEnumerator LiquidBehaviour(float _duration = 5.0f)
    {
        smoke.Stop();
        flames.Play();
        source = SoundManager.instance.PlaySound(56, 0.05f, true, AudioType.SFX);
        colliderGas.enabled = false;
        colliderFlames.enabled = true;
        yield return new WaitForSeconds(_duration);
        flames.Stop();
        colliderFlames.center -= 3*Vector3.up;
        tile.SetDebugColor(Color.white);
        yield return new WaitUntil(() => { return flames.particleCount <= 0; });
        colliderFlames.enabled = false;
        tile.isActive = false;
        SoundManager.instance.StopSound(source);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!consumed && !tile.isActive)
        {
            if(other.CompareTag("Player") && other.GetComponent<Player>().isOnFire)
            {
                tile.Activate();
            }
        }
    }



    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    void LocalBehaviour()
    {
        foreach (Tile t in tile.GetNeighbourOfType(Tile.TileType.Liquid))
        {
            if (DataManager.instance.isMulti)
                RpcBehaviourTile(t.gameObject, 0.2f);
            else
                LocalBehaviourTile(t.gameObject, 0.2f);

        }

        foreach (Tile t in tile.GetNeighbourOfType(Tile.TileType.Baril))
        {
            if (DataManager.instance.isMulti)
                RpcBehaviourTile(t.gameObject, 3.0f);
            else
                LocalBehaviourTile(t.gameObject, 3.0f);
        }
    }
    
    void LocalBehaviourTile(GameObject _t, float _time)
    {
        if (!_t.GetComponent<Tile>().isActive)
        {
            _t.GetComponent<Tile>().Activate(_time);
        }
    }

    ///////////// PRIVATE NETWORK FUNCTIONS //////////////
    [Command]
    void CmdBehaviour()
    {
        LocalBehaviour();
    }

    [ClientRpc]
    void RpcBehaviourTile(GameObject _t, float _time)
    {
        LocalBehaviourTile(_t, _time);
    }
}
