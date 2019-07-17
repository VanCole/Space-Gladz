using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Shadow : NetworkBehaviour
{
    [SerializeField]
    GameObject arrow;

    public Archer caster;

    // Variables AA
    bool AALaunched;
    int charge = 8;
    float timerRafale = 0.0f;
    float velocityBetweenTwoArrow = 0.2f;

    Animator animatorShadow;
    // Variables Passive
    public int arrowHit = 0;
    int stackArrowHit = 0;
    float timerPassive = 5.0f;
    public float cooldownAA = 1.5f;
    public float timerResetpassive = 0.0f;
    public float damageAA = 5.0f;

    GameObject enemy = null;

    private void Start()
    {
        animatorShadow = GetComponent<Animator>();
        StartCoroutine(ShadowUpdateTarget());

        Color color = DataManager.instance.playerColor[caster.colorIndex] * 0.5f;
        color.a = 1.0f;

        SetColor(color);
    }

    public void SetColor(Color _c)
    {
        foreach (SkinnedMeshRenderer mr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            foreach (Material m in mr.materials)
            {
                m.SetColor("_EmissionColor", _c);
            }
        }
    }

    private void OnDestroy()
    {
        if(DataManager.instance.isMulti)
        {
            caster.CmdPlaySound(1, 73, 0.5f, 0, 0, 0.0f, 2, false, AudioType.SFX);
        }
        else
        {
            SoundManager.instance.PlaySound(73, 0.5f, AudioType.SFX);
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        // Timers Reset Charge
        if (charge <= 7)
        {
            timerRafale += Time.deltaTime;
        }

        if (timerRafale >= cooldownAA)
        {
            charge = 8;
            timerRafale = 0.0f;
        }

        // Passive timers
        if (arrowHit != 0)
        {
            timerResetpassive += Time.deltaTime;
        }

        if (timerResetpassive >= cooldownAA)
        {
            arrowHit = 0;
            timerResetpassive = 0.0f;
        }

        timerPassive -= Time.deltaTime;

        if (arrowHit == 3)
        {
            arrowHit = 0;
            stackArrowHit += 1;
            timerPassive = 5.0f;

            if (stackArrowHit > 3)
            {
                stackArrowHit = 3;
            }
        }

        if (timerPassive <= 0.0f)
        {
            stackArrowHit = 0;
        }
    }

    // Coroutine Auto-attack
    IEnumerator CoroutineAA()
    {
        animatorShadow.SetBool("AutoAttack", true);

        while (AALaunched)
        {
            velocityBetweenTwoArrow = caster.velocityBetweenTwoArrow;
            ArrowLaunched();


            yield return new WaitForSeconds(velocityBetweenTwoArrow);

            AALaunched = caster.AALaunched;


        }
        animatorShadow.SetBool("AutoAttack", false);

        yield return 0;

    }

    // Auto-attack StartCoroutine function
    public int AA_Range()
    {
        AALaunched = true;

        StartCoroutine(CoroutineAA());

        return 0;
    }

    void ArrowLaunched() // Auto-attack : rafale de 3 flèches
    {
        //animatorShadow.SetBool("AutoAttack", true);

        charge--;

        if (charge >= 0)
        {
            if(DataManager.instance.isMulti)
            {
                CmdArrowLaunched();
            }
            else
            {
                LocalArrowLaunched();
            }
        }

        if (charge < 0)
        {
            charge = 0;
        }
    }

    void Passive() // augmentation attack speed quand rafale complète réussie
    {
        switch (stackArrowHit)
        {
            case 0:
                velocityBetweenTwoArrow = 0.2f;
                cooldownAA = 1.5f;
                break;
            case 1:
                velocityBetweenTwoArrow = 0.15f;
                cooldownAA = 1.2f;
                break;
            case 2:
                velocityBetweenTwoArrow = 0.1f;
                cooldownAA = 1.0f;
                break;
            case 3:
                velocityBetweenTwoArrow = 0.05f;
                cooldownAA = 0.8f;
                break;
        }
    }

    public void ShadowArrowLaunched()
    {
        if(DataManager.instance.isMulti)
        {
            CmdShadowRotate();
        }
        else
        {
            LocalShadowRotate();
        }

        if(!AALaunched)
        {
            AA_Range();
        }
        //ArrowLaunched();
    }



    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    void LocalArrowLaunched()
    {
        GameObject go = Instantiate(arrow);
        //SoundManager.instance.PlaySound(7,0.5f);

        if (DataManager.instance.isMulti)
        {
            NetworkServer.Spawn(go);
            RpcAttackObjectOnce(go);
        }
        else
        {
            LocalAttackObjectOnce(go);
        }
    }
    
    public void LocalAttackObjectOnce(GameObject attack)
    {
        //attack.GetComponent<Arrow>().archer = caster;

        //if (attack.GetComponent<Rigidbody>() != null)
        //    attack.GetComponent<Rigidbody>().velocity = transform.forward * 25.0f;

        //Quaternion q = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        //attack.transform.position = transform.position + transform.forward * 1.5f;
        //attack.transform.rotation = transform.rotation * q;

        Vector3 tempPosition = transform.position + transform.forward * 1.5f;
        //GameObject attack = Instantiate(arrow, tempPosition, transform.rotation);
        attack.transform.position = tempPosition + Vector3.up;
        attack.transform.rotation = transform.rotation;
        attack.GetComponent<Projectile>().caster = caster.GetComponent<Archer>();
        SoundManager.instance.PlaySound(7, 0.5f, AudioType.SFX);


        //GameObject go = Instantiate(arrow);
        //go.GetComponent<Projectile>().caster = caster.GetComponent<Archer>();
        //Quaternion q = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        //go.transform.position = transform.position + transform.forward * 1.5f;
        //go.transform.rotation = transform.rotation * q;

        //Rigidbody rb = go.GetComponent<Rigidbody>();
        //if (rb != null)
        //    rb.velocity = transform.forward * 25.0f;
        //}
    }
    
    public void LocalShadowRotate()
    {
        Debug.Log(caster + " caster +" + enemy + " enemy");
        float distance_Archer_Ennemi = (caster.transform.position - enemy.transform.position).magnitude;
        Vector3 targetPosition = distance_Archer_Ennemi * caster.pc.direction + caster.transform.position;
        Vector3 shadowDirection = (targetPosition - transform.position).normalized;

        transform.LookAt(enemy.transform.position);
    }

    public GameObject GetNearestPlayer(GameObject _exception = null)
    {
        GameObject player = null;
        float minDistance = float.MaxValue;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj != _exception && (obj.transform.position - transform.position).sqrMagnitude < minDistance)
            {
                player = obj;
                minDistance = (obj.transform.position - transform.position).sqrMagnitude;
            }
        }
        return player;
    }

    IEnumerator ShadowUpdateTarget()
    {
        while (caster.shadowActive)
        {
            GameObject player = GetNearestPlayer(caster.gameObject);

            if (player != null)
            {
                enemy = player;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    ///////////// PRIVATE NETWORK FUNCTIONS //////////////

    [Command]
    void CmdArrowLaunched()
    {
        LocalArrowLaunched();
    }

    [ClientRpc]
    public void RpcAttackObjectOnce(GameObject attack)
    {
        LocalAttackObjectOnce(attack);
    }

    [Command]
    public void CmdShadowRotate()
    {
        RpcShadowRotate();
    }


    [ClientRpc]
    public void RpcShadowRotate()
    {
        LocalShadowRotate();
    }
}

