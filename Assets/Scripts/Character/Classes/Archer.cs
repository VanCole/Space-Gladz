using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Archer : Player
{
    [SerializeField]
    GameObject arrow;

    [SerializeField]
    GameObject canalizedArrow;

    [SerializeField]
    GameObject trap;

    [SerializeField]
    GameObject TP_Arrow;

    [SerializeField]
    GameObject shadow;

    [SerializeField]
    GameObject TPFX;

    [SerializeField]
    GameObject prefabTPStartPointFX;

    // Variables AA
    public bool AALaunched;
    int charge = 3;
    float timerRafale = 0.0f;
    public float velocityBetweenTwoArrow = 0.2f;
    //public float AAvelocity = 25.0f;

    // Variables Passive
    public int arrowHit = 0;
    int stackArrowHit = 0;
    float timerPassive = 5.0f;
    public float timerResetpassive = 0.0f;

    // Variables Spell1
    public bool Spell1_Launched;
    public bool dmgArrow;
    float canalisedSlow = 0.3f;
    bool isCanalised;

    // Variables Spell2 Trapped_Arrow
    Tile tile = null;
    bool TrapLaunched;
    int rangeTrap = 10;
    public float timerTrapped = 3.0f;
    bool isPut;
    int counterTrap;
    public int nbTrapPlaced = 0;

    // Variables Spell3 TP_Arrow
    public bool TPArrowIsLaunched;
    public GameObject tp_arrow_instantiated;
    public float timerTP_Arrow = 0.0f;

    // Variables ultimate Shadow_Archer
    public Shadow objectShadow;
    GameObject shadow_instantiated;
    public bool shadowActive = false;
    public float timerShadow = 20.0f;

    Coroutine getIndexCoroutine = null;

    // Variables de damage
    [HideInInspector] public float damageAA;
    [HideInInspector] public float damageSpell1;
    [HideInInspector] public float damageSpell1_charged;
    [HideInInspector] public float damageSpell2;
    [HideInInspector] public float damageSpell3;

    // Variables de cooldown
    [HideInInspector] public float cooldownAA;
    float cooldownSpell_1 = 6.0f;
    float cooldownSpell_2 = 6.0f;
    float cooldownSpell_3 = 10.0f;
    float cooldownUlti = 40.0f;

    // Variables sonores
    int randomCanalised = 0;
    bool canalisedSound = false;

    // Variables ponctuelles
    public bool playWithGamepad = false;


    // Timer before anim Win start
    float timerWin;
    AudioSource source;

    HexIndex tempHex = new HexIndex();
    bool waitForTempHex = false;

    // Use this for initialization
    override protected void Start()
    {
        base.Start();

        timerFlaming = 0.0f;
        timerWin = 0.0f;
        damageAA = 5.0f;
        damageSpell1 = 20.0f;
        damageSpell1_charged = 100.0f;
        damageSpell2 = 50.0f;
        damageSpell3 = 10.0f;

        cooldownAA = 0.0f;

        counterTrap = -1;

        isCanalised = false;
        isPut = false;

        if (pc.controller != InputController.Controller.Keyboard)
        {
            playWithGamepad = true;
        }

        AddSpell("AutoAttack_Range", cooldownAA).AddBehaviour(AA_Range);
        AddSpell("Canalised_Arrow", cooldownSpell_1);
        AddSpell("Trapped_Arrow", cooldownSpell_2);
        AddSpell("TP_Arrow", cooldownSpell_3);
        AddSpell("Shadow_Archer", cooldownUlti);


        pc.autoAttack.AddListener(() =>
        {
            if (!isStunned && canAttack && isAlive && !isAttacking)
            {
                CastSpell("AutoAttack_Range");
                AALaunched = true;
            }
        });

        pc.AAReleased.AddListener(() =>
        {
            AALaunched = false;
        });

        pc.spell1.AddListener(() =>
        {
            if (!isStunned && canAttack && isAlive && !isAttacking)
            {

                if (DataManager.instance.isMulti)
                {
                    CmdSpell1_lauched(false);
                }
                else
                {
                    LocalSpell1_Launched(false);
                }

                if (GetSpell("Canalised_Arrow").IsOffCooldown)
                {
                    isCanalised = true;

                    if (DataManager.instance.isMulti)
                    {
                        CmdCanalisedArrow();
                    }
                    else
                    {
                        LocalCanalisedArrow();
                    }

                    source = SoundManager.instance.PlaySound(75, 1.0f, AudioType.SFX);

                    randomCanalised = UnityEngine.Random.Range(0, 6);

                    if (randomCanalised == 0)
                    {
                        if (DataManager.instance.isMulti)
                        {
                            CmdPlaySound(1, 33, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Voice);
                        }
                        else
                        {
                            SoundManager.instance.PlaySound(33, 1.0f, AudioType.Voice);
                        }

                        canalisedSound = true;
                    }
                }
            }
        });

        pc.spell2.AddListener(() =>
        {
            if (!isStunned && canAttack && isAlive && !isAttacking)
            {
                TrapLaunched = false;

                if (counterTrap > 0)
                {
                    //Debug.Log("counterTrap " + counterTrap );

                    isPut = true;

                    if(nbTrapPlaced < 5)
                    {
                        //Debug.Log("nbTrapPlaced " + nbTrapPlaced);
                        StartCoroutine(CoroutineTA());
                    }
                }
            }
        });

        pc.spell3.AddListener(() =>
        {
            if (!isStunned && canAttack && isAlive && !isAttacking)
            {
                if (GetSpell("TP_Arrow").IsOffCooldown)
                {
                    if (TPArrowIsLaunched == false)
                    {
                        TP_Arrow_Launched();

                        if(DataManager.instance.isMulti)
                        {
                            CmdPlaySound(1, 71, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                        }
                        else
                        {
                            SoundCharacter(1, 71, 1.0f, AudioType.SFX);
                        }
                       
                        TPArrowIsLaunched = true;
                    }
                    else
                    {
                        if (tp_arrow_instantiated != null && !tp_arrow_instantiated.GetComponent<TP_Arrow>().arrowInSomething)
                        {
                            TPArrowIsLaunched = false;
                            TP_Arrow_TP();

                            if (DataManager.instance.isMulti)
                            {
                                CmdPlaySound(1, 72, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                                CmdPlaySound(6, 38, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
                            }
                            else
                            {
                                SoundCharacter(1, 72, 1.0f, AudioType.SFX);
                                SoundCharacter(6, 38, 1.0f, AudioType.Voice);
                            }
                                
                            CooldownSpell3();
                        }
                    }
                }
            }
        });

        pc.ultimate.AddListener(() =>
        {
            if (!isStunned && canAttack && isAlive && !isAttacking)
            {

                if (GetSpell("Shadow_Archer").IsOffCooldown)
                {
                    if (shadowActive == false)
                    {
                        Debug.Log("Ultimate Launched");
                        Shadow_Archer();

                        if (DataManager.instance.isMulti)
                        {
                            CmdPlaySound(1, 76, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                            CmdPlaySound(1, 29, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
                        }
                        else
                        {
                            SoundCharacter(1, 76, 1.0f, AudioType.SFX);
                            SoundCharacter(1, 29, 1.0f, AudioType.Voice);
                        }
                        
                        shadowActive = true;
                        CooldownUltimate();
                    }
                }
            }
        });

        pc.S1Released.AddListener(() =>
        {
            SoundManager.instance.StopSound(source);

            if (isCanalised)
            {
                CastSpell("Canalised_Arrow");

                if (DataManager.instance.isMulti)
                {
                    CmdPlaySound(1, 74, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    SoundCharacter(1, 74, 1.0f, AudioType.SFX);
                }

                Spell1_Launched = true;
                currentSpeed = SpeedMax;
                isCanalised = false;


                if (DataManager.instance.isMulti)
                {
                    CmdSpell1_lauched(true);
                }
                else
                {
                    LocalSpell1_Launched(true);
                }

                if(canalisedSound)
                {
                    if (DataManager.instance.isMulti)
                    {
                        CmdPlaySound(1, 34, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
                    }
                    else
                    {
                        SoundCharacter(1, 34, 1.0f, AudioType.Voice);
                    }

                    canalisedSound = false;
                }
                
            }

        });

        pc.S2Released.AddListener(() =>
        {
            if (isPut)
            {
                TrapLaunched = true;
                currentSpeed = SpeedMax;
                currentRotateSpeed = RotateSpeedMax;
                isPut = false;
            }
        });

        ArcherSkillSet();
    }

    // Update is called once per frame
    override protected void Update()
    {
        if(counterTrap < 2 && GetSpell("Trapped_Arrow").IsOffCooldown)
        {
            counterTrap++;
            CastSpell("Trapped_Arrow");
        }

        // Timers Reset Charge
        if (shadowActive)
        {
            if (charge <= 7)
            {
                timerRafale += Time.deltaTime;
            }
        }
        else
        {
            if (charge <= 2)
            {
                timerRafale += Time.deltaTime;
            }
        }
        
        if (timerRafale >= cooldownAA)
        {
            if(shadowActive)
            {
                charge = 8;
            }
            else
            {
                charge = 3;
            }
            
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

        // TP_Arrow timer to TP
        if (TPArrowIsLaunched)
        {
            timerTP_Arrow += Time.deltaTime;

            if (timerTP_Arrow >= 1.5f)
            {
                /*if(tp_arrow_instantiated.GetComponent<TP_Arrow>().hit)
                {
                    GetComponent<Player>().AddAttackInTab(tp_arrow_instantiated.GetComponent<TP_Arrow>().totalDamageDealt);
                }
                else
                {
                    GetComponent<Player>().AddAttackInTab(-1.0f);
                }*/

                Destroy(tp_arrow_instantiated);
                tp_arrow_instantiated = null;
                timerTP_Arrow = 0.0f;
                TPArrowIsLaunched = false;
                CooldownSpell3();
            }
        }

        // Shadow timer
        if (shadowActive)
        {
            timerShadow -= Time.deltaTime;

            if (timerShadow <= 0.0f)
            {
                Destroy(shadow_instantiated);
                shadowActive = false;
                timerShadow = 20.0f;
            }
        }

        timerFlaming += Time.deltaTime;

        if (isOnFire == true && timerFlaming >= 10.0f)
        {
            PlayAflameSound();
            timerFlaming = 0.0f;
        }

        if(!isAlive)
        {
            animator.SetTrigger("Dead");
            AALaunched = false;
        }

        if (DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
            animator.SetBool("stanceMenu", true);
        }

        if (DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (DataManager.instance.isMatchDone == true)
            {
                LaunchWinAnimation();
            }
            else
            {
                animator.SetBool("Win", false);
                animator.SetBool("stanceMenu", false);
            }
        }

        //Call update from mother class
        base.Update();
    }

    void LaunchWinAnimation()
    {

        timerWin += Time.deltaTime;

        // Stop any other animations
        animator.SetFloat("X", 0.0f);
        animator.SetFloat("Y", 0.0f);
        animator.SetBool("AutoAttack", false);
        animator.SetBool("CanalizedArrow", false);
        animator.SetBool("TPArrow", false);
        animator.SetBool("Shadow", false);
        animator.SetBool("Stuned", false);
        AALaunched = false;

        //Debug.Log(timerWin);
        if (timerWin >= 2.0f)
        {
            animator.SetBool("Win", true);

        }
        if (timerWin >= 5.0f)
        {

            animator.SetTrigger("WinLoop");

        }
    }

    public override void PlayAflameSound()
    {
        if (isAlive)
        {
            if (DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 32, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
            }
            else
            {
                SoundCharacter(1, 32, 1.0f, AudioType.Voice);
            }
        }

        base.PlayAflameSound();
    }

    public override void PlayWinSound()
    {
        /*if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 30, 1.0f, 2, 31, 1.0f, 1);
        }
        else
        {*/
            MultipleSoundCharacter(1, 30, 1.0f, 2, 31, 1.0f, AudioType.Voice);
        //}

        base.PlayWinSound();
    }

    public override void PlayDeathSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 26, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
            CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
            CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
            //CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);
            //CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);
        }
        else
        {
            SoundCharacter(1, 26, 1.0f, AudioType.Voice);
            //SoundCharacter(1, 0, 1.0f, AudioType.Ambient);
            //SoundCharacter(1, 54, 1.0f, AudioType.Ambient);
            SoundManager.instance.PlaySound(0, 1.0f, AudioType.Ambient);
            SoundManager.instance.PlaySound(54, 1.0f, AudioType.Ambient);
        }

        base.PlayDeathSound();
    }

    public override void PlayFallSound()
    {
        if (isAlive)
        {
            if (DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 27, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
                //CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);
                //CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);
                CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
                CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
            }
            else
            {
                SoundCharacter(1, 27, 1.0f, AudioType.Voice);
                //SoundCharacter(1, 0, 1.0f, AudioType.Ambient);
                //SoundCharacter(1, 54, 1.0f, AudioType.Ambient);
                SoundManager.instance.PlaySound(0, 1.0f, AudioType.Ambient);
                SoundManager.instance.PlaySound(54, 1.0f, AudioType.Ambient);
            }
        }

        base.PlayFallSound();
    }

    public override void PlayChosenSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 25, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
        }
        else
        {
            SoundCharacter(1, 25, 1.0f, AudioType.Voice);
        }

        base.PlayChosenSound();
    }

    public override void PlayDodgeSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(6, 35, 1.0f, 2, 36, 1.0f, 1, false, AudioType.Voice);
        }
        else
        {
            MultipleSoundCharacter(6, 35, 1.0f, 2, 36, 1.0f, AudioType.Voice);
        }

        base.PlayDodgeSound();
    }

    public override void PlayHitSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(6, 37, 1.0f, 2, 52, 1.0f, 1, false, AudioType.Voice);
        }
        else
        {
            MultipleSoundCharacter(6, 37, 1.0f, 2, 52, 1.0f, AudioType.Voice);
        }

        base.PlayHitSound();
    }

    public override void PlayPanicSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 59, 0.7f, 0, 0, 0.0f, 0, false, AudioType.SFX);
        }
        else
        {
            SoundCharacter(1, 59, 0.7f, AudioType.SFX);
        }

        base.PlayPanicSound();
    }

    // Coroutine Auto-attack
    IEnumerator CoroutineAA()
    {
        while (AALaunched)
        {
            animator.SetBool("AutoAttack", true);
            Slow(0.7f, 0.7f, () => { return AALaunched == false; });

            if (objectShadow != null)
            {
                objectShadow.ShadowArrowLaunched();
            }


            ArrowLaunched();
            Passive();

            yield return new WaitForSeconds(velocityBetweenTwoArrow);
        }
        animator.SetBool("AutoAttack", false);

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
        charge--;

        if (charge >= 0)
        {
            //GameObject go = Instantiate(arrow);
            //go.GetComponent<Arrow>().archer = this;
            //Quaternion q = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            //go.transform.position = transform.position + transform.forward * 1.5f;
            //go.transform.rotation = transform.rotation * q;
            //SoundManager.instance.PlaySound(7,0.5f);
            //Rigidbody rb = go.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    rb.velocity = transform.forward * 25.0f;
            //}
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

    // Coroutine Spell1
    IEnumerator CoroutineSpell1()
    {
        isAttacking = true;
        float timerCanalisation = 0.0f;
        if (!DataManager.instance.isMulti)
            animator.SetBool("CanalizedArrow", true);
        GameObject go = Instantiate(canalizedArrow);

        if (DataManager.instance.isMulti)
        {
            CanalizedObjectOnce(go);
            NetworkServer.Spawn(go);
            RpcCanalizedObjectOnce(go);
        }
        else
            CanalizedObjectOnce(go);


        yield return new WaitUntil(() =>
        {
            timerCanalisation += Time.deltaTime;
            if (DataManager.instance.isMulti)
                RpcCanalizationObjectOnce(timerCanalisation, go);
            else
                CanalizationObjectOnce(timerCanalisation, go);

            return Spell1_Launched;
        });


        go.GetComponent<Canalised_Arrow>().charged = true;
        if (!DataManager.instance.isMulti)
        {
            animator.SetBool("CanalizedArrow", false);
            animator.SetBool("CanArrowEnd", true);
        }

        if (DataManager.instance.isMulti)
            RpcRealeaseCanalizedObjectOnce(go);

        else
            RealeaseCanalizedObjectOnce(go);
        
        
    }

    [ClientRpc]
    public void RpcCanalizedObjectOnce(GameObject attack)
    {
        animator.SetBool("CanalizedArrow", true);
        CanalizedObjectOnce(attack);
    }

    public void CanalizedObjectOnce(GameObject attack)
    {
        //attack.GetComponent<Canalised_Arrow>().archer = this;
        //Quaternion q = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        //attack.transform.parent = gameObject.transform;
        //attack.transform.position = transform.position + transform.forward * 1.5f;
        //attack.transform.rotation = transform.rotation * q;
        //attack.GetComponent<CapsuleCollider>().enabled = false;

        Slow(canalisedSlow, canalisedSlow, () => { return Spell1_Launched == true; });
        Vector3 tempPosition = transform.position + transform.forward * 1.5f;
        //GameObject attack = Instantiate(canalizedArrow, tempPosition, transform.rotation);
        //attack.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[colorIndex]);
        attack.transform.position = tempPosition + Vector3.up;
        attack.transform.rotation = transform.rotation;
        attack.GetComponentInChildren<Projectile>().caster = this;
        attack.transform.parent = gameObject.transform;
		attack.GetComponentInChildren<CapsuleCollider>().enabled = false;
    }

    [ClientRpc]
    public void RpcCanalizationObjectOnce(float timer, GameObject attack)
    {
        CanalizationObjectOnce(timer, attack);
    }

    public void CanalizationObjectOnce(float timer, GameObject attack)
    {
        if (timer > 0.01f && timer < 1.0f)
        {
            dmgArrow = false;
            //attack.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0, 0, 255));
            attack.GetComponent<Canalised_Arrow>().charged = false;
        }
        else if (timer > 1.0f)
        {
            dmgArrow = true;
            //attack.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", new Color(255, 0, 0));
            attack.GetComponent<Canalised_Arrow>().charged = true;
        }
    }

    [ClientRpc]
    public void RpcRealeaseCanalizedObjectOnce(GameObject attack)
    {
        animator.SetBool("CanalizedArrow", false);
        animator.SetBool("CanArrowEnd", true);
        RealeaseCanalizedObjectOnce(attack);
    }

    public void RealeaseCanalizedObjectOnce(GameObject attack)
    {
        attack.GetComponentInChildren<CapsuleCollider>().enabled = true;

        Rigidbody rb = attack.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY;
        if (rb != null)
        {
            attack.transform.parent = null;

            if (dmgArrow == false)
            {
                rb.velocity = transform.forward * 25.0f;
                //SoundManager.instance.PlaySound(7, 0.5f);
            }
            else
            {
                rb.velocity = transform.forward * 40.0f;
                //SoundManager.instance.PlaySound(7, 0.5f);
            }
        }

        attack.GetComponent<Canalised_Arrow>().launched = true;

        isAttacking = false;
        animator.SetBool("CanArrowEnd", true);
        Spell1_Launched = false;
    }

    // Coroutine Trapped_Arrow (Spell2)
    IEnumerator CoroutineTA()
    {
        tile = null;

        Tile playerTile;
        List<HexIndex> hexIndex = new List<HexIndex>();

        RaycastHit playerCurrentTile;
        RaycastHit hit;
        Ray rayCurrentTile;
        Ray ray;

        HexIndex origin = HexIndex.origin;
        HexIndex currentPos = HexIndex.origin;

        TerrainGenerator terrain = TerrainGenerator.instance;

        GameObject go = Instantiate(trap);
        go.GetComponent<Trap>().archer = this;
        bool isValid = false;

        yield return new WaitUntil(() =>
        {
            if (playWithGamepad)
            {
                rayCurrentTile = new Ray(transform.position + Vector3.up, Vector3.down);
                ray = new Ray(transform.position + Vector3.up + new Vector3(pc.direction.x, pc.direction.y, pc.direction.z) * rangeTrap, Vector3.down);
            }
            else
            {
                rayCurrentTile = new Ray(transform.position + Vector3.up, Vector3.down);
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            if (Physics.Raycast(rayCurrentTile, out playerCurrentTile, 10000, LayerMask.GetMask("Hexagon")))
            {
                playerTile = playerCurrentTile.collider.transform.parent.gameObject.GetComponent<Tile>();

                if (playerTile != null)
                {
                    terrain.GetHexIndex(playerTile.gameObject, out origin);
                }
            }

            if (Physics.Raycast(ray, out hit, 10000, LayerMask.GetMask("Hexagon")))
            {

                tile = hit.collider.transform.parent.gameObject.GetComponent<Tile>();

                if (tile != null)
                {

                    terrain.GetHexIndex(tile.gameObject, out currentPos);

                    isValid = false;

                    if (DataManager.instance.isMulti)
                    {
                        if (Vector3.Distance(Vector3.zero, go.transform.position) > 2.6f && Vector3.Distance(transform.position, go.transform.position) <= rangeTrap * 1.5f && (tile.Type == Tile.TileType.Empty || tile.Type == Tile.TileType.Liquid || tile.Type == Tile.TileType.SafeZone))
                        {
                            isValid = true;
                            go.SetActive(true);
                        }
                        else
                        {
                            go.SetActive(false);
                        }
                    }
                    else
                    {
                        if (HexIndex.Distance(HexIndex.origin, currentPos) > 1 && HexIndex.Distance(origin, currentPos) <= rangeTrap && (tile.Type == Tile.TileType.Empty || tile.Type == Tile.TileType.Liquid || tile.Type == Tile.TileType.SafeZone))
                        {
                            isValid = true;
                            go.SetActive(true);
                        }
                        else
                        {
                            go.SetActive(false);
                        }
                    }
                    Quaternion q = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    go.transform.position = hit.collider.transform.position;
                }
            }
            return TrapLaunched;
        });

        if (isValid)
        {
            go.GetComponent<Trap>().tile = tile;
            go.GetComponent<Trap>().placed = true;
            go.GetComponent<CapsuleCollider>().enabled = true;

            if (DataManager.instance.isMulti)
            {
                CmdTrap(go, go.transform.position, go.transform.rotation);
                Destroy(go);
            }
            else
            {
                LocalTrap(go, go.transform.position, go.transform.rotation);
            }

            counterTrap--;
            nbTrapPlaced++;

            if (DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 69, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
            }
            else
            {
                SoundCharacter(1, 69, 1.0f, AudioType.SFX);
            }
        }
        else
        {
            Destroy(go);
        }
    }

    // Spell3 (first click)
    void TP_Arrow_Launched()
    {
        animator.SetBool("TPArrow", true);

        if (DataManager.instance.isMulti)
        {
            CmdTP_Arrow_Launched();
        }
        else
        {
            LocalTP_Arrow_Launched();

        }


    }

    // Spell3 (second click)
    void TP_Arrow_TP()
    {
        /*if (tp_arrow_instantiated.GetComponent<TP_Arrow>().hit)
        {
            GetComponent<Player>().AddAttackInTab(tp_arrow_instantiated.GetComponent<TP_Arrow>().totalDamageDealt);
        }
        else
        {
            GetComponent<Player>().AddAttackInTab(0.0f);
        }*/

        if(prefabTPStartPointFX)
        {
            if (DataManager.instance.isMulti)
            {
                CmdTP_Arrow_StartPoint(transform.position);
            }
            else
            {
                GameObject fx = Instantiate(prefabTPStartPointFX);
                fx.transform.position = transform.position;
                fx.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[colorIndex]);
            }
        }

        gameObject.transform.position = tp_arrow_instantiated.transform.position - Vector3.up;

        if (DataManager.instance.isMulti)
        {
            CmdServerDestroyTp_Arrow(transform.position);
        }
        else
        {
            LocalServerDestroyTp_Arrow(transform.position);
        }
        tp_arrow_instantiated = null;
        timerTP_Arrow = 0.0f;
    }

    // Fonction de cooldown Spell3
    public void CooldownSpell3()
    {

        CastSpell("TP_Arrow");
        animator.SetBool("TPArrow", false);

    }

    // Ultimate
    void Shadow_Archer()
    {
        if(DataManager.instance.isMulti)
        {
            CmdShadow_Archer();
        }
        else
        {
            LocalShadow_Archer();
        }

        //if(go.GetComponent<InputController>().controller == InputController.Controller.Gamepad1 /*|| go.GetComponent<InputController>().controller == InputController.Controller.Gamepad2*/)
        //{
        // FAIRE EN SORTE QUE LE CODE MARCHE POUR PLUSIEURS ENNEMIS (PRODUITS SCALAIRES)


        //}
        
    }

    // Fonction de cooldown Ultimate
    public void CooldownUltimate()
    {
        CastSpell("Shadow_Archer");
    }

    [SerializeField]
    Sprite[] spriteSpell = new Sprite[7];

    //Spell information
    void ArcherSkillSet()
    {
        GetSpell("AutoAttack_Range").icon = spriteSpell[0];
        GetSpell("AutoAttack_Range").name = "Burst of Bolt";
        GetSpell("AutoAttack_Range").type = "Primary Attack";
        GetSpell("AutoAttack_Range").description = "Shoot a burst of three bolt.\n" +
                                                   "If the full burst hit the opponent, the passive is activated.\nEach bolt deals " + damageAA.ToString() + " damages.";

        GetSpell("Passive").icon = spriteSpell[1];
        GetSpell("Passive").name = "Exceptional Shooter";
        GetSpell("Passive").type = "Passive";
        GetSpell("Passive").description = "Increases attack speed when a full burst hits the enemy. This effect stacks up to 3 times.";


        GetSpell("Shadow_Archer").icon = spriteSpell[2];
        GetSpell("Shadow_Archer").name = "Shadow Ranger";
        GetSpell("Shadow_Archer").type = "Ultimate";
        GetSpell("Shadow_Archer").description = "Create a copy of the ranger who will shoot when the ranger does it during " + timerShadow.ToString() + " seconds. " +
                                                "On activation the auto-attack of the ranger shoot a burst of 8 arrows. " +
                                                "The shadow activates the passive of the ranger if it hit the opponent.";

        GetSpell("Canalised_Arrow").icon = spriteSpell[3];
        GetSpell("Canalised_Arrow").name = "Canalised Bolt";
        GetSpell("Canalised_Arrow").type = "Spell 1";
        GetSpell("Canalised_Arrow").description = "Not loaded : Shoot a bolt that knock back the opponent and deals " + damageSpell1.ToString() + " damages.\n" +
                                                  "Loaded : After 1 second of loading, shoot a bolt that deals " + damageSpell1_charged.ToString() + " damages. " +
                                                  "To load slow you by " + (canalisedSlow * 100).ToString() + "%.";

        GetSpell("Trapped_Arrow").icon = spriteSpell[4];
        GetSpell("Trapped_Arrow").name = "Trap";
        GetSpell("Trapped_Arrow").type = "Spell 2";
        GetSpell("Trapped_Arrow").description = "Place a trap at the targeted spot. The trap slows target hit by 50% for " + timerTrapped.ToString() +
                                                " seconds and deals him " + damageSpell2.ToString() + " damages. You gain one charge all " + cooldownSpell_2 + " seconds. " +
                                                "You can store 3 charges max.";

        GetSpell("TP_Arrow").icon = spriteSpell[5];
        GetSpell("TP_Arrow").name = "Teleportation";
        GetSpell("TP_Arrow").type = "Spell 3";
        GetSpell("TP_Arrow").description = "First activation : Shoot a bolt that goes through everything.\nSecond activation : Teleport the character to the place of the bolt.";

       

 

        GetSpell("Dodge").icon = spriteSpell[6];
    }




    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    private void LocalSpell1_Launched(bool launched)
    {
        Spell1_Launched = launched;
    }

    private void LocalArrowLaunched()
    {
        GameObject go = Instantiate(arrow);
        //SoundManager.instance.PlaySound(7,0.5f);
        if (DataManager.instance.isMulti)
        {
            go.GetComponent<Arrow>().caster = this;
            Vector3 tempPosition = transform.position + transform.forward * 1.5f;
            go.transform.position = tempPosition + Vector3.up;
            go.transform.rotation = transform.rotation;
            NetworkServer.Spawn(go);
            RpcAttackObjectOnce(go);
        }
        else
        {
            LocalAttackObjectOnce(go);
        }
    }


    private void LocalCanalisedArrow()
    {
        StartCoroutine(CoroutineSpell1());
    }

    private void LocalTrap(GameObject _go, Vector3 _pos, Quaternion _rot)
    {
        //GameObject go = Instantiate(trap, pos, rot);
        if (DataManager.instance.isMulti)
        {
            GameObject go = Instantiate(trap);
            NetworkServer.Spawn(go);
            RpcTrapObjectOnce(go, _pos, _rot);
        }
        else
        {
            LocalTrapObjectOnce(_go);
        }
    }

    private void LocalTP_Arrow_Launched()
    {
        GameObject go = Instantiate(TP_Arrow);
        if (DataManager.instance.isMulti)
        {
            LocalTpArrowObjectOnce(go);
            NetworkServer.Spawn(go);
            RpcTpArrowObjectOnce(go);
        }
        else
        {
            LocalTpArrowObjectOnce(go);
        }
    }

    [ClientRpc]
    private void RpcTP_Arrow_StartPoint(Vector3 _pos)
    {
        GameObject fx = Instantiate(prefabTPStartPointFX);
        fx.transform.position = _pos;
        fx.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[colorIndex]);
    }

    private void LocalServerDestroyTp_Arrow(Vector3 _pos)
    {
        if (DataManager.instance.isMulti)
        {
            NetworkServer.Destroy(tp_arrow_instantiated);
        }
        else
        {
            Destroy(tp_arrow_instantiated);
        }


        if (TPFX)
        {
            if (DataManager.instance.isMulti)
                RpcLocalServerPlayFXOnDestroyTp_Arrow(_pos);
            else
                LocalServerPlayFXOnDestroyTp_Arrow(_pos);
        }
    }

    void LocalServerPlayFXOnDestroyTp_Arrow(Vector3 _pos)
    {
        GameObject fx = Instantiate(TPFX);

        fx.transform.position = _pos;
        fx.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[colorIndex]);
        fx.GetComponent<ParticleSystem>().Play();
    }


     [ClientRpc]
    void RpcLocalServerPlayFXOnDestroyTp_Arrow(Vector3 _pos)
    {
        LocalServerPlayFXOnDestroyTp_Arrow(_pos);
    }

    private void LocalShadow_Archer()
    {

        GameObject go = Instantiate(shadow);
        if (DataManager.instance.isMulti)
        {
            go.GetComponent<Shadow>().caster = this;
            NetworkServer.Spawn(go);
            RpcShadowObjectOnce(go);
        }
        else
        {
            LocalShadowObjectOnce(go);
        }

    }

    private void LocalAttackObjectOnce(GameObject attack)
    {
        attack.GetComponent<Arrow>().caster = this;

        //if (attack.GetComponent<Rigidbody>() != null)
        //    attack.GetComponent<Rigidbody>().velocity = transform.forward * 25.0f;
        //Quaternion q = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        //attack.transform.position = transform.position + transform.forward * 1.5f;
        //attack.transform.rotation = transform.rotation * q;

        Vector3 tempPosition = transform.position + transform.forward * 1.5f;
        attack.transform.position = tempPosition + Vector3.up;
        attack.transform.rotation = transform.rotation;

        attack.GetComponent<Projectile>().caster = this;       
        SoundManager.instance.PlaySound(7,0.5f, AudioType.SFX);          

    }
    
    private void LocalTrapObjectOnce(GameObject attack)
    {
        animator.SetTrigger("Trap");
        attack.GetComponent<Trap>().archer = this;
        attack.GetComponent<Trap>().tile = tile;
        attack.GetComponent<CapsuleCollider>().enabled = true;
    }

    
    private void LocalShadowObjectOnce(GameObject attack)
    {
        animator.SetTrigger("Ulti");
        shadowActive = true;
        attack.GetComponent<Shadow>().caster = this;
        Quaternion q = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        attack.transform.position = transform.position;
        attack.transform.rotation = transform.rotation * q;

        objectShadow = attack.GetComponent<Shadow>();

        attack.GetComponent<InputController>().controller = GetComponent<InputController>().controller;
        attack.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        shadow_instantiated = attack;
        Destroy(shadow_instantiated, 20.0f);

        if(DataManager.instance.isMulti)
        {
            attack.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
        }

    }


    private void LocalTpArrowObjectOnce(GameObject attack)
    {
        //attack.GetComponent<TP_Arrow>().archer = this;
        //Quaternion q = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        //attack.transform.position = transform.position + transform.forward * 1.5f;
        //attack.transform.rotation = transform.rotation * q;
        //attack.GetComponent<MeshRenderer>().material.color = new Color(0, 255, 255);
        //Rigidbody rb = attack.GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    rb.velocity = transform.forward * 10.0f;
        //}

        //tp_arrow_instantiated = attack;

        Vector3 tempPosition = transform.position + transform.forward * 1.5f;
        //GameObject attack = Instantiate(TP_Arrow, tempPosition, transform.rotation);
        attack.GetComponent<Projectile>().caster = this;
        tp_arrow_instantiated = attack;
        attack.transform.position = tempPosition + Vector3.up;
        attack.transform.rotation = transform.rotation;

        attack.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[colorIndex]);
        attack.GetComponentInChildren<ParticleSystemRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[colorIndex]);
    }

    ///////////// PRIVATE NETWORK FUNCTIONS //////////////
    [Command]
    private void CmdSpell1_lauched(bool launched)
    {
        LocalSpell1_Launched(launched);
    }

    [Command]
    private void CmdArrowLaunched()
    {
        LocalArrowLaunched();
    }

    [Command]
    private void CmdCanalisedArrow()
    {
        StartCoroutine(CoroutineSpell1());
    }

    [Command]
    private void CmdTrap(GameObject go, Vector3 pos, Quaternion rot)
    {
        LocalTrap(go, pos, rot);
    }

    [Command]
    private void CmdTP_Arrow_Launched()
    {
        LocalTP_Arrow_Launched();
    }
    
    [Command]
    private void CmdTP_Arrow_StartPoint(Vector3 _pos)
    {
        RpcTP_Arrow_StartPoint(_pos);
    }

    [Command]
    private void CmdServerDestroyTp_Arrow(Vector3 _pos)
    {
        LocalServerDestroyTp_Arrow(_pos);
    }

    [Command]
    private void CmdShadow_Archer()
    {
        LocalShadow_Archer();
    }

    [ClientRpc]
    private void RpcAttackObjectOnce(GameObject attack)
    {
        LocalAttackObjectOnce(attack);
    }

    [ClientRpc]
    private void RpcTrapObjectOnce(GameObject attack, Vector3 _pos, Quaternion _rot)
    {
        attack.GetComponent<Trap>().placed = true;
        attack.transform.position = _pos;
        attack.transform.rotation = _rot;
        LocalTrapObjectOnce(attack);
    }

    [ClientRpc]
    private void RpcShadowObjectOnce(GameObject attack)
    {
        LocalShadowObjectOnce(attack);
    }

    [ClientRpc]
    private void RpcTpArrowObjectOnce(GameObject attack)
    {
        LocalTpArrowObjectOnce(attack);
    }
}

