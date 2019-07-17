using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Warrior : Player
{
    [SerializeField]
    GameObject harpoon;

    [SerializeField]
    GameObject maulHitbox;

    [SerializeField]
    GameObject attackHitbox;

    [SerializeField]
    GameObject furiousThrow;

    [SerializeField]
    GameObject wallFXobj;

    [SerializeField]
    ParticleSystem wallFXParticles;


    //public bool isAttacking;
    public bool isHarpooning;
    public bool isChannelingUltimate;
    public bool isMauling;

    bool canCastSpell = true;

    public bool isCancelable = true;

    [HideInInspector] public int attackCount = 0;
    [HideInInspector] public int traitAttackMaxCount = 5;
    public bool isAttackBoosted;


    //Abilities Damage Value
    [HideInInspector] public float currentAttackDamage;
    [HideInInspector] public float initDamage;
    [HideInInspector] public float traitBuffedDamage;
    [HideInInspector] public float UltDamage;
    [HideInInspector] public float MaulDamagePerTick;

    //Abilies CoolDown
    float attackCD = 0.5f;
    [HideInInspector] public float harpoonCD; //8.0f
    float maulCD = 6.0f;
    float groundShakerCD = 8.0f;
    float furiousThrowCD = 50.0f; //50
    float furiousThrowChannelTime = 1.5f; //50

    public bool harpoonSoundPlayed = false;

    //Wall Lift 
    float wallUpDuration = 2.0f;
    float wallRiseSpeed = 0.05f;

    // Timer before anim win start
    float timerWin;

    AttackWarrior currentAttack;
    [HideInInspector] public Harpoon currentHarpoon;
    [HideInInspector] public Maul currentMaul;
    bool waitForHarpoon = false;
    bool waitForMaul = false;
    FuriousThrow currentFuriousThrow;

    // Use this for initialization
    override protected void Start()
    {
        timerFlaming = 0.0f;
        timerWin = 0.0f;
        harpoonCD = 8.0f; //8.0f

        //Damage Value
        currentAttackDamage = initDamage;
        initDamage = 10.0f;
        traitBuffedDamage = 50.0f;
        UltDamage = 230.0f;
        MaulDamagePerTick = 30.0f;


        base.Start();

        attackCount = 0;
        isAttackBoosted = false;

        //isAttacking = false;
        isHarpooning = false;
        isMauling = false;

        SpellList();
        WarriorSkillSet();
        passive.Cast(); // reset cooldown
    }

    void PassiveBehaviour()
    {
        //Reset attack count after buffed attack
        if (attackCount > traitAttackMaxCount)
        {
            attackCount = 0;
            passive.Cast(); // reset cooldown
        }

        //After X attack the next one deal bonus Damage
        if (attackCount == traitAttackMaxCount)
        {
            isAttackBoosted = true;
            GetSpell("Passive").SetCooldown(attackCount / (float)(traitAttackMaxCount));
        }
        //otherwise it deal normal damage
        else if (attackCount < traitAttackMaxCount)
        {
            isAttackBoosted = false;
            GetSpell("Passive").SetCooldown(attackCount / (float)(traitAttackMaxCount));
        }

        if (isAttackBoosted)
        {
            currentAttackDamage = traitBuffedDamage;
        }
        else
        {
            currentAttackDamage = initDamage;
        }
    }

    [ClientRpc]
    public void RpcUltCooldown(float CDR)
    {
        GetSpell("FuriousThrow").timer += CDR;
    }

    public void SpellList()
    {
        // SPELL LeftClick : Attack
        AddSpell("Attack", attackCD).AddBehaviour(AttackBehaviour);

        pc.autoAttack.AddListener(() =>
        {
            canCastSpell = true;

            if (!isAttacking && !isMauling && !isHarpooning && !isChannelingUltimate)
            {
                StartCoroutine(CoroutineCastSpell("Attack"));
                //CastSpell("Attack");
            }
        });
        pc.AAReleased.AddListener(() =>
        {
            canCastSpell = false;
        });

        //SPELL 1 : HARPOON
        AddSpell("Harpoon", harpoonCD).AddBehaviour(HarpoonBehaviour);

        pc.spell1.AddListener(() =>
        {
            if (!isAttacking && !isMauling && !isHarpooning && !isChannelingUltimate)
            {
                CastSpell("Harpoon");
            }
        });

        //SPELL 2 : Maul
        AddSpell("Maul", maulCD).AddBehaviour(MaulBehaviour);

        pc.spell2.AddListener(() =>
        {
            if (!isAttacking && !isHarpooning && !isChannelingUltimate)
            {
                MaulCast();
            }
        });

        //SPELL 3 : Wall
        AddSpell("GroundShaker", groundShakerCD).AddBehaviour(WallLift);

        pc.spell3.AddListener(() =>
        {
            if (!isAttacking && !isMauling && !isHarpooning && !isChannelingUltimate)
            {
                CastSpell("GroundShaker");
            }
        });

        //ULTIMATE : Furious Throw
        AddSpell("FuriousThrow", furiousThrowCD).AddBehaviour(FuriousThrowBehaviour);

        pc.ultimate.AddListener(() =>
        {
            if (!isAttacking && !isMauling && !isHarpooning && !isChannelingUltimate)
            {
                CastSpell("FuriousThrow");
            }
        });
    }

    public int AttackBehaviour()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 8, 0.3f, 0, 0, 0.0f, 0, false, AudioType.SFX);
            CmdAttackBehaviour();
        }
        else
        {
            currentAttack = Instantiate(attackHitbox, transform.position + Vector3.up, transform.rotation).GetComponent<AttackWarrior>();

            currentAttack.caster = gameObject;
            currentAttack.transform.parent = gameObject.transform;

            /*if(DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 8, 0.3f, 0, 0, 0.0f, 0);
            }
            else
            {*/
                SoundCharacter(1, 8, 0.3f, AudioType.SFX);
            //}
        }
        return 0;
    }

    [Command]
    void CmdAttackBehaviour()
    {
        currentAttack = Instantiate(attackHitbox).GetComponent<AttackWarrior>();
        NetworkServer.Spawn(currentAttack.gameObject);

        currentAttack.caster = gameObject;

        RpcParentObjectOnce(currentAttack.gameObject, gameObject);
        RpcAttackObjectOnce(currentAttack.gameObject);

    }

    [ClientRpc]
    public void RpcAttackObjectOnce(GameObject attackWarrior)
    {
        attackWarrior.GetComponent<AttackWarrior>().caster = gameObject;
        attackWarrior.transform.position = transform.position + Vector3.up;
        attackWarrior.transform.rotation = transform.rotation;
    }

    public int HarpoonBehaviour()
    {
        if (DataManager.instance.isMulti)
            CmdHarpoonBehaviour();
        else
        {
            Vector3 instantiatePosition = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);

            currentHarpoon = Instantiate(harpoon, instantiatePosition + transform.forward * 2.0f, transform.rotation).GetComponent<Harpoon>();
            currentHarpoon.transform.Rotate(90.0f, 0.0f, 0.0f);
            currentHarpoon.caster = gameObject;
        }
        StartCoroutine(IsHarpooning());

        return 0;
    }

    [Command]
    void CmdHarpoonBehaviour()
    {
        currentHarpoon = Instantiate(harpoon, new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z) + transform.forward * 2.0f, transform.rotation).GetComponent<Harpoon>();

        NetworkServer.Spawn(currentHarpoon.gameObject);

        currentHarpoon.caster = gameObject;
        currentHarpoon.gameObject.transform.rotation = transform.rotation;
        //RpcParentObjectOnce(currentHarpoon.gameObject, gameObject);

        RpcHarpoonObjectOnce(currentHarpoon.gameObject, new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z) + transform.forward * 2.0f, transform.rotation);
    }

    [ClientRpc]
    public void RpcHarpoonObjectOnce(GameObject harpoon, Vector3 _pos, Quaternion _rot)
    {  
        harpoon.transform.position = _pos;
        harpoon.transform.rotation = _rot;
        harpoon.transform.Rotate(90.0f, 0.0f, 0.0f);
        harpoon.GetComponent<Harpoon>().caster = gameObject;
        waitForHarpoon = true;
        //harpoon.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
        
    }

    public int MaulBehaviour()
    {
        if (DataManager.instance.isMulti)
            CmdMaulBehaviour();
        else
        {
            Vector3 tempPosition = transform.position + transform.forward * 2.0f;

            currentMaul = Instantiate(maulHitbox, tempPosition + Vector3.up, transform.rotation).GetComponent<Maul>();
            currentMaul.caster = gameObject;
            currentMaul.transform.parent = gameObject.transform;
        }

        StartCoroutine(IsChannelingMaul());
        StartCoroutine(MaulSoundAttack());
        StartCoroutine(IsCancelable());
        return 0;
    }

    [Command]
    void CmdMaulBehaviour()
    {
        currentMaul = Instantiate(maulHitbox).GetComponent<Maul>();

        NetworkServer.Spawn(currentMaul.gameObject);

        currentMaul.caster = gameObject;

        RpcParentObjectOnce(currentMaul.gameObject, gameObject);
        RpcMaulObjectOnce(currentMaul.gameObject);
    }

    [ClientRpc]
    public void RpcMaulObjectOnce(GameObject maul)
    {
        maul.transform.position = transform.position + transform.forward * 2.0f;
        maul.transform.rotation = transform.rotation;
        maul.GetComponent<Maul>().caster = gameObject;
        waitForMaul = true;
    }

    void MaulCast()
    {
        if (!isAttacking && !isMauling && !isHarpooning && !isChannelingUltimate)
        {
            CastSpell("Maul");
        }
        else if (isCancelable && isMauling)
        {
            Destroy(currentMaul.gameObject);

            StopCoroutine(IsChannelingMaul());
            isMauling = false;
        }
    }

    public int WallLift()
    {
        //animator.SetBool("WallW", true);

        if (DataManager.instance.isMulti)
        {
            CmdWallLift();
            animator.SetTrigger("Wall");
        }
        else
            WallLiftLocal();     
        return 0;
    }

    [Command]
    void CmdWallLift()
    {
        WallLiftLocal();
    }


    void WallLiftLocal()
    {
        if (DataManager.instance.isMulti)
        {
            RpcWallLift();
        }
        else
        {
            SoundCharacter(1, 51, 1.0f, AudioType.SFX);
            SoundCharacter(6, 24, 0.7f, AudioType.Voice);
            animator.SetTrigger("Wall");
        }

        //GameObject obj = Instantiate(wallFXobj, new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z), Quaternion.identity);
        // animator.SetTrigger("Wall");
        
       

        // Raycast to ground from in front of player
        Ray ray = new Ray(transform.position + Vector3.up, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Assume that there only one terrain generator in the scene
            TerrainGenerator terrain = TerrainGenerator.instance;

            HexIndex origin;
            terrain.GetHexIndex(hit.collider.transform.parent.gameObject, out origin);

            //cast a circle 2 tile from player
            foreach (HexIndex index in terrain.grid.GetRing(origin, 3))
            {
                GameObject obj;
                terrain.GetGameObject(index, out obj);
                Tile tile = obj.GetComponent<Tile>();

                //Create a vector between player and raycast target
                //From the point forward player calculate normalized angle
                //And run wallLift method
                Vector3 direction = obj.transform.position - transform.position;
                if (Vector3.Dot(transform.forward, direction.normalized) > 0.2f)
                {
                    tile.Wall(wallUpDuration, wallRiseSpeed);  //wall up duration,  delay till max height           

                }
            }
        }
        //animator.SetBool("WallW", false);
    }

    [ClientRpc]
    void RpcWallLift()
    {
        CmdPlaySound(1, 51, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
        CmdPlaySound(6, 24, 0.7f, 0, 0, 0.0f, 0, false, AudioType.Voice);
    }

    public int FuriousThrowBehaviour()
    {
        StartCoroutine(IsChannelingUltimate());
        return 0;
    }

    // Update is called once per frame
    override protected void Update()
    {
        PassiveBehaviour();

        if (Input.GetMouseButtonUp(0))
        {
            canCastSpell = false;
        }

        timerFlaming += Time.deltaTime;

        if (isOnFire == true && timerFlaming >= 10.0f)
        {
            PlayAflameSound();
            timerFlaming = 0.0f;
        }

        if (DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
            animator.SetBool("stanceMenu", true);
        }
        if (DataManager.instance.gameState == DataManager.GameState.inArena )
        {
            LaunchWinAnimation();
        }


        //Call update from mother class
        base.Update();
    }

    public IEnumerator IsHarpooning()
    {
        canMove = false;
        isHarpooning = true;
        isAttacking = true;

        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 9, 0.3f, 0, 0, 0.0f, 0, false, AudioType.SFX);
        }
        else
        {
            SoundCharacter(1, 9, 0.3f, AudioType.SFX);
        }

        animator.SetBool("Harpoon", isHarpooning);
        //yield return new WaitFor(event);
        yield return new WaitUntil(() =>
        {
            if (DataManager.instance.isMulti)
                return !currentHarpoon && waitForHarpoon;
            else
                return currentHarpoon.endHarpoon == true;
        });

        canMove = true;
        isHarpooning = false;
        isAttacking = false;
        waitForHarpoon = false;
        animator.SetBool("Harpoon", isHarpooning);

        if (harpoonSoundPlayed)
        {
            if (DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 20, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
            }
            else
            {
                SoundCharacter(1, 20, 1.0f, AudioType.Voice);
            }

            harpoonSoundPlayed = false;
        }
    }

    public IEnumerator IsChannelingMaul()
    {
        if (DataManager.instance.isMulti)
            Slow(0.3f, 0.3f, () => { return !currentMaul && waitForMaul; });
        else
            Slow(0.3f, 0.3f, () => { return currentMaul.endChannel == true; });

        isMauling = true;
        isAttacking = true;

        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(6, 22, 0.3f, 2, 23, 1.0f, 1, false, AudioType.Voice);
        }
        else
        {
            MultipleSoundCharacter(6, 22, 0.3f, 2, 23, 1.0f, AudioType.Voice);
        }

        animator.SetBool("Maul", isMauling);
        //yield return new WaitFor(event);
        yield return new WaitUntil(() =>
        {
            if (DataManager.instance.isMulti)
                return !currentMaul && waitForMaul;
            else
                return currentMaul.endChannel == true;


        });


        isMauling = false;
        isAttacking = false;
        waitForMaul = false;

        animator.SetBool("Maul", isMauling);

    }

    public IEnumerator MaulSoundAttack()
    {
        float timeStep = 0.25f;

        while (isMauling)
        {
            yield return new WaitForSeconds(timeStep);

            if (DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 8, 0.3f, 0, 0, 0.0f, 0, false, AudioType.SFX);
            }
            else
            {
                SoundCharacter(1, 8, 0.3f, AudioType.SFX);
            }
        }
    }

    public IEnumerator IsCancelable()
    {
        isCancelable = false;

        //yield return new WaitFor(event);
        yield return new WaitForSeconds(0.5f);
        isCancelable = true;
    }

    public IEnumerator IsChannelingUltimate()
    {
        animator.SetBool("Ulti", true);

        float timer = 0.0f;

        Slow(0.0f, 0.3f, () => { return timer >= furiousThrowChannelTime; });

        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 14, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
            CmdPlaySound(1, 68, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
        }
        else
        {
            SoundCharacter(1, 14, 1.0f, AudioType.Voice);
            SoundCharacter(1, 68, 1.0f, AudioType.SFX);
        }

        isChannelingUltimate = true;
        isAttacking = true;

        yield return new WaitUntil(() =>
        {
            timer += Time.deltaTime;
            if (isStunned)
            {
                timer = furiousThrowChannelTime;
            }
            return timer >= furiousThrowChannelTime;
        });

        if (!isStunned)
        {
            if (DataManager.instance.isMulti)
                CmdUltimate();
            else
            {
                Vector3 tempPosition = transform.position + transform.forward * 2.0f;
                currentFuriousThrow = Instantiate(furiousThrow, tempPosition + Vector3.up, transform.rotation).GetComponent<FuriousThrow>();
                currentFuriousThrow.caster = this;

                currentFuriousThrow.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", DataManager.instance.playerColor[colorIndex]);
                
            }
        }

        isChannelingUltimate = false;
        isAttacking = false;
        animator.SetBool("Ulti", isChannelingUltimate);

    }

    [Command]
    void CmdUltimate()
    {
        currentFuriousThrow = Instantiate(furiousThrow).GetComponent<FuriousThrow>();

        NetworkServer.Spawn(currentFuriousThrow.gameObject);

        currentFuriousThrow.caster = this;
        currentFuriousThrow.transform.position = transform.position + transform.forward * 2.0f + Vector3.up;
        currentFuriousThrow.transform.rotation = transform.rotation;
        RpcUltiObjectOnce(currentFuriousThrow.gameObject);
    }

    [ClientRpc]
    public void RpcUltiObjectOnce(GameObject furiousThrow)
    {
        furiousThrow.transform.position = transform.position + transform.forward * 2.0f + Vector3.up;
        furiousThrow.transform.rotation = transform.rotation;
        furiousThrow.GetComponent<FuriousThrow>().caster = this;
    }

    public IEnumerator CoroutineCastSpell(string spellName)
    {
        animator.SetBool("AutoAttack", true);
        while (canCastSpell)
        {
            isAttacking = true;
            CastSpell(spellName);

            //yield return new WaitFor(event);
            yield return new WaitUntil(() =>
            {
                return GetSpell(spellName).IsOffCooldown;
            });
            isAttacking = false;
        }
        animator.SetBool("AutoAttack", false);
    }



    [SerializeField]
    Sprite[] spriteSpell = new Sprite[7];

    void LaunchWinAnimation()
    {
        if (DataManager.instance.isMatchDone == true)
        {
            timerWin += Time.deltaTime;

            // Stop any other animations
            animator.SetFloat("X", 0.0f);
            animator.SetFloat("Y", 0.0f);
            animator.SetBool("AutoAttack", false);
            animator.SetBool("Harpoon", false);
            animator.SetBool("Maul", false);
            animator.SetBool("Ulti", false);
            animator.SetBool("Stuned", false);

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
        else
        {
            animator.SetBool("Win", false);
            animator.SetBool("stanceMenu", false);
        }
    }

    //Spell information
    void WarriorSkillSet()
    {
        GetSpell("Attack").icon = spriteSpell[0];
        GetSpell("Attack").type = "Primary Attack";
        GetSpell("Attack").name = "Thrust";
        GetSpell("Attack").description = "Quickly thrust the warrior's trident in front of him, dealing " + initDamage.ToString() + " damage to each enemy hit.";


        GetSpell("Passive").icon = spriteSpell[1];
        GetSpell("Passive").type = "Passive";
        GetSpell("Passive").name = "Battle Rage";
        GetSpell("Passive").description = "After 5 successful Attacks the next one deal " + (traitBuffedDamage - initDamage).ToString() + " bonus damage." +
                                          "\nThis empowered attack grants 5 seconds cooldown reduction to Furious Throw.";


        GetSpell("FuriousThrow").icon = spriteSpell[2];
        GetSpell("FuriousThrow").type = "Ultimate";
        GetSpell("FuriousThrow").name = "Furious Throw";
        GetSpell("FuriousThrow").description = "After " + furiousThrowChannelTime.ToString() + " seconds, launch a massive, power infused trident." +
                                               "\nExplodes upon hitting an enemy, dealing " + UltDamage.ToString() + " damage and stunning him for 0.5 seconds.";


        GetSpell("Harpoon").icon = spriteSpell[3];
        GetSpell("Harpoon").type = "Spell 1";
        GetSpell("Harpoon").name = "Harpoon";
        GetSpell("Harpoon").description = "Pull the first enemy hit toward the warrior, temporarily slowing them by 40% for 2 seconds." +
                                            "\nPull the warrior toward his harpoon if it get stuck in a wall.";

        GetSpell("Maul").icon = spriteSpell[4];
        GetSpell("Maul").type = "Spell 2";
        GetSpell("Maul").name = "Maul";
        GetSpell("Maul").description = "Channel a volley of blows up to 1.5 seconds, dealing " + MaulDamagePerTick.ToString() +
                                        " damage in front of the warrior every 0.25 seconds. \nEach tick reduce Furious Throw cooldown by 0.5 seconds.";


        GetSpell("GroundShaker").icon = spriteSpell[5];
        GetSpell("GroundShaker").type = "Spell 3";
        GetSpell("GroundShaker").name = "Ground Shaker";
        GetSpell("GroundShaker").description = "Lift an impassable wall in front of the player for " + wallUpDuration.ToString() + " seconds.";

            
        GetSpell("Dodge").icon = spriteSpell[6];
    }

    public override void PlayWinSound()
    {
        /*if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 15, 1.0f, 0, 0, 0.0f, 0);
        }
        else
        {*/
            SoundCharacter(1, 15, 1.0f, AudioType.Voice);
        //}

        base.PlayWinSound();
    }

    public override void PlayDeathSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 11, 0.3f, 0, 0, 0.0f, 0, false, AudioType.Voice);
            /*CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);
            CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);*/
            CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
            CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
        }
        else
        {
            SoundCharacter(1, 11, 0.3f, AudioType.Voice);
            /*SoundCharacter(1, 0, 1.0f, AudioType.Ambient);
            SoundCharacter(1, 54, 1.0f, AudioType.Ambient);*/
            SoundManager.instance.PlaySound(0, 1.0f, AudioType.Ambient);
            SoundManager.instance.PlaySound(54, 1.0f, AudioType.Ambient);
        }

        base.PlayDeathSound();
    }

    public override void PlayHitSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(6, 16, 0.3f, 2, 17, 0.3f, 1, false, AudioType.Voice);
        }
        else
        {
            MultipleSoundCharacter(6, 16, 0.3f, 2, 17, 0.3f, AudioType.Voice);
        }

        base.PlayHitSound();
    }

    public override void PlayFallSound()
    {
        if (isAlive)
        {
            if (DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 12, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
                //CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);
                //CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Ambient);
                CmdPlaySound(1, 0, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
                CmdPlaySound(1, 54, 1.0f, 0, 0, 0.0f, 2, false, AudioType.Ambient);
            }
            else
            {
                SoundCharacter(1, 12, 1.0f, AudioType.Voice);
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
            CmdPlaySound(1, 10, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
        }
        else
        {
            SoundCharacter(1, 10, 1.0f, AudioType.Voice);
        }

        base.PlayChosenSound();
    }

    public override void PlayDodgeSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(6, 19, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
        }
        else
        {
            SoundCharacter(6, 19, 1.0f, AudioType.Voice);
        }
        
        base.PlayDodgeSound();
    }

    public override void PlayAflameSound()
    {
        if (isAlive)
        {
            if (DataManager.instance.isMulti)
            {
                CmdPlaySound(1, 18, 1.0f, 0, 0, 0.0f, 0, false, AudioType.Voice);
            }
            else
            {
                SoundCharacter(1, 18, 1.0f, AudioType.Voice);
            }
        }

        base.PlayAflameSound();
    }

    public override void  PlayPanicSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 58, 0.3f, 0, 0, 0.0f, 0, false, AudioType.Voice);
        }
        else
        {
            SoundCharacter(1, 58, 0.3f, AudioType.Voice);
        }

        base.PlayPanicSound();
    }

    [ClientRpc]
    public void RpcParentObjectOnce(GameObject _obj, GameObject _parent)
    {
        _obj.transform.parent = _parent.transform;
    }
}
