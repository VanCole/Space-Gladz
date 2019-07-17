using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Turret : Interactible
{
    [SerializeField] GameObject turretShot;
    Transform target = null;
    Color defaultColor = new Color(0.5f, 0.5f, 0.5f);

    Vector3 direction;

    Animator shotFX;

    override protected void Start()
    {
        base.Start();
        offset = Vector3.zero;
        SetColor(defaultColor);
        shotFX = transform.Find("ShotFX").GetComponent<Animator>();
    }

    override protected void Update()
    {
        base.Update();

        // Turn the turret to nearby player when activated
        if (tile.isActive)
        {
            if (target != null)
            {
                direction = Vector3.Slerp(transform.forward, Vector3.Scale(target.position - transform.position, new Vector3(1, 0, 1)).normalized, 0.2f);
                Quaternion rot = Quaternion.LookRotation(direction);
                if (DataManager.instance.isMulti)
                    CmdSetRot(rot);
                else
                    transform.rotation = rot;
            }
        }
    }

    [Command]
    public void CmdSetRot(Quaternion _rot)
    {
        RpcSetRot(_rot);
    }

    [ClientRpc]
    public void RpcSetRot(Quaternion _rot)
    {
        transform.rotation = _rot;
    }

    protected override void Behaviour()
    {
        base.Behaviour();
        StartCoroutine(TurretBehaviour(0.2f, 10));
    }


    public IEnumerator TurretUpdateTarget()
    {
        while (tile.isActive)
        {
            GameObject player = tile.GetNearestPlayer(tile.caster);

            if (player != null)
            {
                target = player.transform;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator TurretBehaviour(float _cooldown = 1.0f, int _nbShot = 5)
    {
        StartCoroutine(TurretUpdateTarget());
        SetColor(tile.caster.GetComponent<Player>().color);
        for (int i = 0; i < _nbShot && tile.isEnabled && tile.Type == Tile.TileType.Turret; i++)
        {
            yield return new WaitForSeconds(_cooldown);
            if (DataManager.instance.isMulti)
            {
                CmdShot();
            }
            else
            {
                GameObject obj = Instantiate(turretShot);
                LocalShot(obj);
            }
        }
        yield return new WaitForSeconds(0.5f);
        tile.isActive = false;
        SetColor(defaultColor);
    }




    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    public void LocalShot(GameObject _obj)
    {
        _obj.transform.position = transform.position + offset + 1.0f * Vector3.up + 0.5f * transform.forward;
        _obj.transform.rotation = transform.rotation;
        //_obj.GetComponent<TurretProjectile>().caster = tile.caster;
        _obj.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", tile.caster.GetComponent<Player>().color);
        shotFX.Play("ProjectileFX", -1, 0);
        SoundManager.instance.PlaySound(4, 0.5f, AudioType.SFX);
    }

    ///////////// PRIVATE NETWORK FUNCTIONS //////////////

    [Command]
    void CmdShot()
    {
        GameObject _obj = Instantiate(turretShot);
        _obj.transform.position = transform.position + offset + 1.0f * Vector3.up + 0.5f * transform.forward;
        _obj.transform.rotation = transform.rotation;
        //_obj.GetComponent<TurretProjectile>().caster = tile.caster;
        _obj.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", tile.caster.GetComponent<Player>().color);
        NetworkServer.Spawn(_obj);
        RpcShot(_obj);
    }

    [ClientRpc]
    void RpcShot(GameObject obj)
    {
        LocalShot(obj);
    }
}
