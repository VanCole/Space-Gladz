using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    //Reference
    [SerializeField] Text playerName;

    public GameObject harponned;
    public InputController pc;
    public Rigidbody rb;
    public Vector3 velocity = Vector3.zero;
    ParticleSystem fireParticle;
    GameObject stunFX;
    GameObject slowFX;
    GameObject panicFX;
    GameObject healFX;
    [SerializeField] GameObject dashFX;
    [SerializeField] GameObject prefabDamageText;
    public Animator animator;
    public NetworkAnimator NetAnimator;
    public float animatorSpeed;
    //public AnimatorClipInfo[] animClipInfo;
    public Transform opponent;
    public Coroutine regenCoroutine = null;
    public Coroutine flameCoroutine = null;
    public Coroutine vampCoroutine = null;

    public delegate bool Condition();

    public int indexLastAttacks = 0;

    //Player index
    public int index;

    //List used to compare Slow Value
    [HideInInspector]
    public List<float> SlowSpeedList = new List<float>();
    [HideInInspector]
    public List<float> SlowRotateSpeedList = new List<float>();

    //Class Chosen by player
    public DataManager.TypeClass selectedClass;

    //Gifts chosen by player
    public DataManager.TypeGift[] gifts = new DataManager.TypeGift[3];

    //Stats 
    [SyncVar]
    public float currentHealth;
    public float healthMax;
    public float maxShieldPower;
    [SyncVar]
    public float currentShieldPower;

    public float currentPower;
    public float maxPower;
    public float ultimateMultiplier;

    public float currentSpeed;
    public float SpeedMax;
    public float currentRotateSpeed;
    public float RotateSpeedMax;
    public float bonusMoveSpeed;

    public float damageMultiplier;
    public float knockBackPower;

    //EndGame Stats
    [SyncVar] public float totalDmgDealt = 0.0f;
    [SyncVar] public float totalDmgTaken = 0.0f;
    [SyncVar] public int totalGiftTaken = 0;
    [SyncVar] public int totalGiftStolen = 0;
    [SyncVar] public int totalTrapActivated = 0;


    //Debuff
    float slowMultiplier;
    float slowRotateMultiplier;
    public int isSlowed = 0;
    public bool isStunned;
    public bool isPanic;
    public bool isKnockBack;
    public bool isOnFire;
    int triggerFlames = 0;
    Coroutine flamingTimer; // keep reference to flaming timer

    //State
    public bool isAlive;
    public bool isHealing;
    public bool isFlamingOthers;
    public bool isStealingLife;
    public bool isBoostDamaged;
    public bool canMove;
    public bool canDodge;
    public bool canStun;
    public bool canSlow;
    public bool canPanic;
    public bool canFlame;
    public bool canVamp;
    public bool canAttack;

    public bool isAttacking;
    public bool isDodging;
    public bool isShielded;
    public bool isPV_Shielded;
    public bool isBonusMoveSpeed;

    //Spell variable
    public List<Spell> spellList = new List<Spell>();
    public Spell interaction;
    public Spell passive;
    public Spell dodge;

    static int count = 0;
    private const int nbMaxSpell = 5;

    public float pv_lost = 0.0f;
    public float damageDeals = 0.0f;

    // Sounds variables
    public bool isDeadByFalling = false;
    public float timerFlaming;

    [HideInInspector] public Color color;
    [SerializeField] public int colorIndex;

    public Sprite[] portraitElements = new Sprite[3];

    int showSlowFX = 0;

    Coroutine damageColorCoroutine = null;


    //INITIATLISATION ============================================================================================
    // Use this for initialization
    protected virtual void Start()
    {

        isAlive = true;
        isHealing = false;
        isFlamingOthers = false;
        isBoostDamaged = false;
        canMove = false;
        canDodge = false;
        canAttack = false;
        canStun = false;
        canSlow = false;
        canPanic = false;
        canFlame = false;
        canVamp = false;
        isKnockBack = false;

        // Set unique index to this player
        index = count;
        count++;

        //if(DataManager.instance.isMulti)
        //{
        //    colorIndex = DataManager.instance.localPlayerIndex - 1;
        //}

        if (colorIndex >= 0 && colorIndex < DataManager.instance.playerColor.Length)
        {
            SetColor(DataManager.instance.playerColor[colorIndex]);
        }

        pc = GetComponent<InputController>();
        rb = GetComponent<Rigidbody>();
        fireParticle = GetComponentInChildren<ParticleSystem>();
        animator = GetComponent<Animator>();

        if (DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            playerName.text = DataManager.instance.PlayerNames[colorIndex];

            try { stunFX = transform.Find("StunFX").gameObject; }
            catch { Debug.LogWarning("No StunFX found for the player \"" + gameObject.name + "\""); }
            try { slowFX = transform.Find("SlowFX").gameObject; }
            catch { Debug.LogWarning("No SlowFX found for the player \"" + gameObject.name + "\""); }
            try { panicFX = transform.Find("PanicFX").gameObject; }
            catch { Debug.LogWarning("No PanicFX found for the player \"" + gameObject.name + "\""); }
            try { healFX = transform.Find("HealFX").gameObject; }
            catch { Debug.LogWarning("No HealFX found for the player \"" + gameObject.name + "\""); }
            //try { dashFX = transform.Find("DashFX").gameObject; }
            //catch { Debug.LogError("No DashFX found for the player \"" + gameObject.name + "\""); }
        }



        // animClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        animatorSpeed = GetComponent<Animator>().speed;
        GameObject quad = transform.Find("Quad").gameObject;
        Color c = DataManager.instance.playerColor[colorIndex];
        c += Color.white * 0.2f;
        c.a = 0.5f;
        quad.GetComponent<MeshRenderer>().material.SetColor("_Color", c);

        //Init fixed max value
        SpeedMax = 5.0f;
        RotateSpeedMax = 10.0f;
        healthMax = 1000.0f;
        maxPower = 100.0f;
        ultimateMultiplier = 2.0f;
        maxShieldPower = 150.0f;
        bonusMoveSpeed = 0.0f;

        slowMultiplier = 1.0f;
        slowRotateMultiplier = 1.0f;

        damageMultiplier = 1.0f;

        knockBackPower = 5.0f;

        isStunned = false;
        isPanic = false;
        isOnFire = false;
        isDodging = false;
        isAttacking = false;

        isShielded = false;
        isPV_Shielded = false;
        isBonusMoveSpeed = false;

        //Init current value
        currentSpeed = SpeedMax;
        currentRotateSpeed = RotateSpeedMax;
        currentHealth = healthMax;
        currentPower = maxPower;
        currentShieldPower = maxShieldPower;

        // Spell interaction
        interaction = CreateSpell("Interaction", 0.1f).AddBehaviour(InteractionBehaviour);
        pc.interact.AddListener(() =>
        {

            CastSpell("Interaction");
        });

        // Spell passive
        passive = CreateSpell("Passive", 1.0f);

        // Spell dodge
        dodge = CreateSpell("Dodge", 2f).AddBehaviour(DodgeBehaviour);
        pc.dodge.AddListener(() =>
        {
            if (!isAttacking && canDodge)
            {
                CastSpell("Dodge");
            }
        });


    }

    public void SetColor(Color _c)
    {
        color = _c;
        foreach (SkinnedMeshRenderer mr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            foreach (Material m in mr.materials)
            {
                m.SetColor("_EmissionColor", _c);
            }
        }
    }


    //SPELL MANAGEMENT =====================================================================================================
    //Creation and gestion
    private Spell CreateSpell(string _name, float _cooldown = 1.0f)
    {
        Spell spell = null;
        spell = SpellManager.instance.AddSpell(_name + index, _cooldown);
        return spell;
    }

    protected Spell AddSpell(string _name, float _cooldown = 1.0f)
    {
        Spell spell = null;
        if (spellList.Count >= nbMaxSpell)
        {
            Debug.LogError("Couldn't add spell " + _name + " in player " + index + " : There are too many spell for this player");
        }
        else
        {
            spell = SpellManager.instance.AddSpell(_name + index, _cooldown);
            spellList.Add(spell);
        }
        return spell;
    }

    public void CastSpell(string _name)
    {
        int damage = -1;
        if (!isStunned && canAttack && isAlive)
        {
            damage = SpellManager.instance.CastSpell(_name + index);
            //Debug.Log("Dealed damage = " + damage);
        }
    }

    public Spell GetSpell(string _name)
    {
        return SpellManager.instance.GetSpellFromName(_name + index);
    }

    //Spells Behaviour
    public int InteractionBehaviour()
    {
        if (DataManager.instance.isMulti)
        {
            CmdEnvInteraction();
        }
        else
        {
            LocalEnvInteraction();
        }
        return 0;
    }

    public int DodgeBehaviour()
    {
        //Debug.Log("dodge");

        PlayDodgeSound();

        StartCoroutine(CoroutineDodge());

        return 0;
    }

    public void SoundCharacter(int randomValue, int soundNumber, float soundVolume, AudioType type)
    {
        int random = UnityEngine.Random.Range(0, randomValue);

        if (random == 0)
        {
            SoundManager.instance.PlaySound(soundNumber, soundVolume, type);
        }
    }

    public void MultipleSoundCharacter(int randomValue1, int soundNumber1, float soundVolume1, int randomValue2, int soundNumber2, float soundVolume2, AudioType type)
    {
        int random1 = UnityEngine.Random.Range(0, randomValue1);

        if (random1 == 0)
        {
            int random2 = UnityEngine.Random.Range(0, randomValue2);

            if (random2 == 0)
            {
                SoundManager.instance.PlaySound(soundNumber1, soundVolume1, type);
            }
            else
            {
                SoundManager.instance.PlaySound(soundNumber2, soundVolume2, type);
            }
        }
    }

    public virtual void PlayWinSound() { }
    public virtual void PlayDeathSound() { }
    public virtual void PlayFallSound() { }
    public virtual void PlayChosenSound() { }
    public virtual void PlayHitSound() { }
    public virtual void PlayDodgeSound()
    {
        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(1, 77, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
        }
        else
        {
            SoundCharacter(1, 77, 1.0f, AudioType.SFX);
        }
    }
    public virtual void PlayAflameSound() { }
    public virtual void PlayPanicSound() { }


    //UPDATE =============================================================================================================
    // Update is called once per frame
    protected virtual void Update()
    {
        if (DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (isAlive)
            {
                //Manager player state such as slow / stun
                DebuffManager();

                if (canMove)
                {
                    Movement();
                }
                else if (isKnockBack)
                {
                    if (opponent != null)
                    {
                        Vector3 direction = (transform.position - opponent.position).normalized;

                        velocity = rb.velocity;
                        velocity.x = direction.x * currentSpeed * knockBackPower;
                        velocity.z = direction.z * currentSpeed * knockBackPower;
                        rb.velocity = velocity;
                    }
                }
                else if (isPanic)
                {
                    if (opponent != null)
                    {
                        Vector3 direction = (transform.position - opponent.position).normalized;
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, currentRotateSpeed * Time.deltaTime);

                        velocity = rb.velocity;
                        velocity.x = direction.x * currentSpeed;
                        velocity.z = direction.z * currentSpeed;
                        rb.velocity = velocity;
                    }
                }
                else
                {
                    rb.velocity = Vector3.zero;
                }

                Death();

                //prevent player from being on top of hexagons
                if (DataManager.instance.gameState == DataManager.GameState.inArena)
                {
                    Recalage();
                }

                if (DataManager.instance.isMulti)
                {
                    if (harponned)
                        transform.position = harponned.transform.position - Vector3.up;
                }
            }
            else
            {
                rb.velocity = Vector3.zero;
                pc.enabled = false;

                animator.SetBool("Dead", true);
                animator.SetTrigger("Dead");


                //Disable debuff fx
                stunFX.SetActive(false);
                slowFX.SetActive(false);
                panicFX.SetActive(false);
                healFX.SetActive(false);

            }
        }




        if (DataManager.instance.gameState == DataManager.GameState.inArena && DataManager.instance.isMatchDone == true)
        {
            canMove = false;
            transform.Find("Quad").gameObject.SetActive(false);

            isShielded = false;
            isPV_Shielded = false;

            //Player will face target as if looking 
            if (isAlive)
            {
                Vector3 direction = (GameObject.Find("WinnerLookAtTarget").transform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, currentRotateSpeed * Time.deltaTime);
            }
        }
    }



    void Death()
    {
        if (isAlive && currentHealth <= 0.0f)
        {
            //ShowDirector come film the death

            if (!isDeadByFalling)
            {
                ShowDirector.instance.StartCoroutineSwitchToDeadPlayer(transform.position, ShowDirector.instance.tracksEvent[2], 30.0f, gameObject, 4.0f, false);
                PlayDeathSound();
            }
            else
            {
                ShowDirector.instance.StartCoroutineSwitchToDeadPlayer(transform.position, ShowDirector.instance.tracksEvent[2], 30.0f, gameObject, 4.0f, true);
            }
            isAlive = false;

            isShielded = false;
            isPV_Shielded = false;
            isStunned = false;
            isOnFire = false;

            GameManager.instance.nbrAlivePlayer--;
            tag = "Untagged";
            //Destroy(gameObject);
        }
    }


    //POSITION MANAGEMENT ==========================================================================================================
    void Movement()
    {
        if (!isDodging)
        {
            velocity = rb.velocity;
            velocity.x = pc.directionToMove.x * currentSpeed;
            velocity.z = pc.directionToMove.z * currentSpeed;
            rb.velocity = velocity;
        }

        if (pc.direction.sqrMagnitude != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(pc.direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, currentRotateSpeed * Time.deltaTime);
        }

        if (!DataManager.instance.isMulti || (DataManager.instance.isMulti && isLocalPlayer))
        {
            animator.SetFloat("X", Vector3.Dot(transform.right, pc.directionToMove));
            animator.SetFloat("Y", Vector3.Dot(transform.forward, pc.directionToMove));
        }


        if (isSlowed > 0 || isStunned)
        {
            animator.SetFloat("Multiplayer", currentSpeed);
        }
    }

    //prevent player from being on top of hexagons
    void Recalage()
    {
        if (DataManager.instance.isMulti)
        {
            CmdRecalage();
        }
        else
        {
            LocalRecalage();
        }
    }

    //STATE BEHAVIOUR ==============================================================================================================
    //Manager player state such as slow / stun

    public void DebuffManager()
    {
#if __DEBUG
        //DEBUG
        if (Input.GetKeyDown(KeyCode.R))
        {
            Stun(1.5f);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Slow(0.1f, 0.2f, 1.0f, true);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            Panic(2.0f);
        }
#endif

        //State Hierarchy    
        if (isSlowed > 0)
        {
            //Find the most powerful slow and apply it to the player
            currentSpeed = (SpeedMax + bonusMoveSpeed) * SlowSpeedList[0];
            currentRotateSpeed = RotateSpeedMax * SlowRotateSpeedList[0];
        }
        else
        {
            currentSpeed = SpeedMax + bonusMoveSpeed;
            currentRotateSpeed = RotateSpeedMax;
        }


        if (stunFX) stunFX.SetActive(isAlive && isStunned);
        if (panicFX) panicFX.SetActive(isAlive && isPanic);
        if (slowFX) slowFX.GetComponentInChildren<MeshRenderer>().enabled = (showSlowFX > 0);

        if (DataManager.instance.isMatchDone)
        {
            isOnFire = false;
            fireParticle.Stop();

            isSlowed = 0;
            isStunned = false;
            isHealing = false;

            stunFX.SetActive(false);
            slowFX.SetActive(false);
            panicFX.SetActive(false);
            healFX.SetActive(false);
        }
    }

    // DAMAGED
    public void GetDamage(float amount, bool isUnshieldable = true)
    {
        DamageColor();
        DamageText(amount);

        // only server apply damages to player
        if (DataManager.instance.isMulti && !isServer)
        {
            return;
        }

        if (isShielded && isPV_Shielded)
        {
            if (!isUnshieldable)
            {
                isShielded = false;

                if (DataManager.instance.isMulti)
                {
                    CmdPlaySound(1, 57, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    SoundCharacter(1, 57, 1.0f, AudioType.SFX);
                }
            }
            else if (currentShieldPower < amount)
            {
                amount -= currentShieldPower;
                currentHealth -= amount;
                pv_lost += amount;
                totalDmgTaken += amount;
                isPV_Shielded = false;

                if (DataManager.instance.isMulti)
                {
                    CmdPlaySound(1, 57, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    SoundCharacter(1, 57, 1.0f, AudioType.SFX);
                }
            }
            else if (currentShieldPower == amount)
            {
                isPV_Shielded = false;

                if (DataManager.instance.isMulti)
                {
                    CmdPlaySound(1, 57, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    SoundCharacter(1, 57, 1.0f, AudioType.SFX);
                }
            }
            else
            {
                currentShieldPower -= amount;
            }
        }
        else if (isShielded)
        {
            if (!isUnshieldable)
            {
                isShielded = false;

                if (DataManager.instance.isMulti)
                {
                    CmdPlaySound(1, 57, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    SoundCharacter(1, 57, 1.0f, AudioType.SFX);
                }
            }
            else
            {
                currentHealth -= amount;
                pv_lost += amount;
                totalDmgTaken += amount;

                //damageDeals += amount;
            }
        }
        else if (isPV_Shielded)
        {
            if (currentShieldPower < amount)
            {
                //damageDeals += amount;
                amount -= currentShieldPower;
                currentHealth -= amount;
                pv_lost += amount;
                totalDmgTaken += amount;

                isPV_Shielded = false;

                if (DataManager.instance.isMulti)
                {
                    CmdPlaySound(1, 57, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    SoundCharacter(1, 57, 1.0f, AudioType.SFX);
                }
            }
            else if (currentShieldPower == amount)
            {
                isPV_Shielded = false;

                if (DataManager.instance.isMulti)
                {
                    CmdPlaySound(1, 57, 1.0f, 0, 0, 0.0f, 0, false, AudioType.SFX);
                }
                else
                {
                    SoundCharacter(1, 57, 1.0f, AudioType.SFX);
                }
            }
            else
            {
                currentShieldPower -= amount;
                //damageDeals += amount;
            }
        }
        else
        {
            currentHealth -= amount;
            pv_lost += amount;
            totalDmgTaken += amount;

            //damageDeals += amount;

            if (!isOnFire)
            {
                PlayHitSound();
            }
        }
    }

    public void DamageText(float value, bool heal = false)
    {
        GameObject damageText = Instantiate(prefabDamageText, transform);
        damageText.transform.localPosition = 1.5f * Vector3.up;
        damageText.GetComponent<TextMesh>().text = value.ToString();
        damageText.GetComponent<TextMesh>().color = heal ? Color.green : Color.yellow;
    }

    // BOOST SPEED
    public void SpeedBoost(float bonusSpeed, float duration)
    {
        StartCoroutine(IsBoostSpeed(bonusSpeed, duration));
    }

    // BOOST DAMAGE
    public void DamageBoost(float multiplierDamage, float duration)
    {
        StartCoroutine(IsDamageBoost(multiplierDamage, duration));
    }

    //SLOW
    public void Slow(float speedSlow, float rotateSlow, Condition condition, bool showFX = false)
    {
        StartCoroutine(IsSlowed(speedSlow, rotateSlow, condition, showFX));
    }

    public void Slow(float speedSlow, float rotateSlow, float duration, bool showFX = false)
    {
        float timer = 0.0f;

        StartCoroutine(IsSlowed(speedSlow, rotateSlow, () =>
        {
            timer += Time.deltaTime;
            return timer >= duration;
        }, showFX));
    }

    //STUN
    public void Stun(Condition condition)
    {
        StartCoroutine(IsStunned(condition));
    }

    public void Stun(float duration)
    {
        float timer = 0.0f;

        StartCoroutine(IsStunned(() =>
        {
            timer += Time.deltaTime;
            return timer >= duration;
        }));
    }

    //PANIC
    public void Panic(float duration)
    {
        StartCoroutine(IsPanic(duration));
    }

    //KNOCK BACK
    public void KnockBack(float duration)
    {
        StartCoroutine(IsKnockBack(duration));
    }

    //HEALING
    public void Healing(float duration)
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }

        regenCoroutine = StartCoroutine(IsRegen(duration, 250.0f));
    }

    //AFLAME
    public void Flaming(float duration)
    {
        isFlamingOthers = true;

        if (flameCoroutine != null)
        {
            StopCoroutine(flameCoroutine);
        }

        flameCoroutine = StartCoroutine(IsFlaming(duration));

        isFlamingOthers = false;
    }

    //LIFESTEAL
    public void LifeSteal(float duration)
    {
        isStealingLife = true;

        if (vampCoroutine != null)
        {
            StopCoroutine(vampCoroutine);
        }

        vampCoroutine = StartCoroutine(IsVamping(duration));

        isStealingLife = false;
    }


    //COROUTINE ======================================================================================
    public IEnumerator IsStunned(Condition condition)
    {

        isStunned = true;
        animator.SetBool("Stuned", isStunned);
        yield return StartCoroutine(IsSlowed(0.0f, 0.0f, condition, false));
        isStunned = false;
        animator.SetBool("Stuned", isStunned);
    }

    public IEnumerator IsPanic(float duration) // Code de panic dans l'Update car s'active quand !canMove
    {
        canMove = false;
        isPanic = true;
        PlayPanicSound();
        yield return new WaitForSeconds(duration);
        isPanic = false;
        canMove = true;
    }

    public IEnumerator IsKnockBack(float duration) // Code de panic dans l'Update car s'active quand !canMove
    {
        canMove = false;
        isKnockBack = true;
        yield return new WaitForSeconds(duration);
        isKnockBack = false;
        canMove = true;
    }

    public IEnumerator IsFlaming(float duration)
    {
        canFlame = true;
        yield return new WaitForSeconds(duration);
        canFlame = false;
    }

    public IEnumerator IsVamping(float duration)
    {
        canVamp = true;
        yield return new WaitForSeconds(duration);
        canVamp = false;
    }

    public IEnumerator IsRegen(float duration, float maxPV_Healed)
    {
        isHealing = true;

        float pv_healed = maxPV_Healed / duration;
        float timerHeal = 0.0f;

        healFX.GetComponent<ParticleSystem>().Play();

        while (timerHeal < duration)
        {
            if (currentHealth < healthMax)
            {
                currentHealth += pv_healed;
                DamageText(pv_healed, true);

                if (currentHealth > healthMax)
                {
                    currentHealth = healthMax;
                }
            }

            timerHeal++;

            yield return new WaitForSeconds(1.0f);
        }
        healFX.GetComponent<ParticleSystem>().Stop();

        regenCoroutine = null;
        isHealing = false;
    }

    public IEnumerator IsBoostSpeed(float bonusSpeed, float duration)
    {
        isBonusMoveSpeed = true;
        bonusMoveSpeed = bonusSpeed;
        yield return new WaitForSeconds(duration);
        bonusMoveSpeed = 0.0f;
        isBonusMoveSpeed = false;
    }

    public IEnumerator IsDamageBoost(float multiplierDamage, float duration)
    {
        isBoostDamaged = true;
        damageMultiplier = multiplierDamage;
        yield return new WaitForSeconds(duration);
        damageMultiplier = 1.0f;
        isBoostDamaged = false;
    }

    public IEnumerator IsSlowed(float speedSlow, float rotateSlow, Condition condition, bool showFX)
    {
        isSlowed++;
        SlowSpeedList.Add(speedSlow);
        SlowRotateSpeedList.Add(rotateSlow);

        SlowRotateSpeedList.Sort();
        SlowSpeedList.Sort();

        if (showFX) showSlowFX++;

        yield return new WaitUntil(() =>
        {
            return condition();
        });

        SlowSpeedList.Remove(speedSlow);
        SlowRotateSpeedList.Remove(rotateSlow);
        isSlowed--;

        if (showFX) showSlowFX--;
    }

    IEnumerator CoroutineDodge(float _dodgeTime = 0.1f)
    {
        currentSpeed = 0;
        float distance = 2.0f;
        isDodging = true;
        float dodgeSpeed = distance / _dodgeTime;
        rb.velocity = pc.directionToMove * dodgeSpeed;

        if (DataManager.instance.isMulti)
        {
            CmdPlaySound(6, 19, 0.5f, 0, 0, 0.0f, 0, false, AudioType.Voice);
        }
        else
        {
            SoundCharacter(6, 19, 0.5f, AudioType.Voice);
        }

        if (dashFX)
        {
            dashFX.SetActive(true);
            dashFX.GetComponent<Animator>().Play("DashFX", -1, 0);

            Debug.Log("Dash");

            GameObject dashObj = Instantiate(dashFX);
            dashObj.transform.position = transform.position + Vector3.up;
            dashObj.transform.rotation = Quaternion.LookRotation(pc.directionToMove, Vector3.up) * Quaternion.Euler(0, -90, 0);
            Destroy(dashObj, 0.15f);
        }

        float timer = 0.0f;
        yield return new WaitUntil(() =>
        {
            timer += Time.deltaTime;
            return timer > _dodgeTime;
        });

        currentSpeed = SpeedMax;
        isDodging = false;
    }


    public void DamageColor()
    {
        if (damageColorCoroutine != null)
        {
            StopCoroutine(damageColorCoroutine);
        }
        damageColorCoroutine = StartCoroutine(DamageColorCoroutine(0.1f));
    }

    IEnumerator DamageColorCoroutine(float _duration)
    {
        SetOverwriteColor(new Color(0.3f, 0.2f, 0.1f, 1));
        yield return new WaitForSeconds(_duration);
        SetOverwriteColor(new Color(0.3f, 0.2f, 0.1f, 0));
    }

    public void SetOverwriteColor(Color _c)
    {
        foreach (SkinnedMeshRenderer mr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            foreach (Material m in mr.materials)
            {
                m.SetColor("_OverColor", _c);
            }
        }
    }

    public void Aflame(float _duration)
    {
        if (isOnFire && flamingTimer != null)
        {
            StopCoroutine(flamingTimer);
        }
        else if (!isOnFire)
        {
            isOnFire = true;
            StartCoroutine(FlamingDamages());
        }

        flamingTimer = StartCoroutine(FlamingTimer(_duration));
    }

    IEnumerator FlamingDamages()
    {
        float timeStep = 0.25f;
        fireParticle.Play();
        AudioSource source = SoundManager.instance.PlaySound(56, 0.2f, true, AudioType.SFX);

        while (isOnFire)
        {
            yield return new WaitForSeconds(timeStep);
            GetDamage(5.0f);
            pv_lost += 5.0f;
            totalDmgTaken += 5.0f;

        }
        fireParticle.Stop();
        SoundManager.instance.StopSound(source);
    }

    IEnumerator FlamingTimer(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        isOnFire = false;
        flamingTimer = null;
    }

    void OnChangeHealth(float health)
    {
        //healthBar.fillAmount = health / healthMax;
        //currentHealth = health;
    }

    //TRIGGER INTERACTION=====================================================================================
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Finish"))
        {
            GetDamage(currentHealth, false);

            isDeadByFalling = true;
            PlayFallSound();
            currentHealth = 0;

            rb.isKinematic = true;
        }
        else if (other.gameObject.CompareTag("Explosion"))
        {
            GetDamage(50.0f, false);

            //currentHealth -= 50;
            //rb.AddForce(1500 * Vector3.Scale(transform.position - other.transform.position, new Vector3(1, 0, 1)).normalized);
            //Debug.Log(currentHealth);
        }
        else if (other.gameObject.CompareTag("Flames"))
        {
            triggerFlames++;
            if (flamingTimer != null)
            {
                StopCoroutine(flamingTimer);
            }
            if (!isOnFire)
            {
                isOnFire = true;
                StartCoroutine(FlamingDamages());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Flames"))
        {
            triggerFlames--;
            if (triggerFlames == 0)
            {
                //Debug.Log("Exit flames");
                flamingTimer = StartCoroutine(FlamingTimer(3.0f));
            }
        }
    }


    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    //prevent player from being on top of hexagons
    private void LocalRecalage()
    {
        // Recalage of character when he is on a wall
        float seuil = 0.2f;
        if (transform.position.y > seuil)
        {
            TerrainGenerator terrain = GameObject.FindObjectOfType<TerrainGenerator>();

            // Get player hex index
            Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
            RaycastHit hit;
            Tile playerTile;
            HexIndex origin = HexIndex.origin;
            string s = "";
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask.GetMask("Hexagon")))
            {
                s += "Raycast success on " + hit.collider.name;
                //playerTile = hit.collider.transform.parent.gameObject.GetComponent<Tile>();
                playerTile = hit.collider.GetComponentInParent<Tile>();
                s += " | PlayerTile : " + playerTile;
                if (playerTile != null)
                {
                    terrain.GetHexIndex(playerTile.gameObject, out origin);
                }
            }

            Debug.Log("Recalage..." + s);

            // Get the first encountered empty tile
            GameObject tileToTP = null;
            int radius = 1;
            float SqDistFromPlayer = float.MaxValue;
            while (tileToTP == null)
            {
                foreach (HexIndex t in terrain.grid.GetRing(origin, radius))
                {
                    GameObject temp;
                    terrain.GetGameObject(t, out temp);
                    if (temp != null && temp.transform.position.y > -2.0f && temp.transform.position.y < seuil)
                    {
                        float dist = (transform.position - temp.transform.position).sqrMagnitude;
                        if (dist < SqDistFromPlayer)
                        {
                            SqDistFromPlayer = dist;
                            tileToTP = temp;
                        }
                    }
                }
                radius++;
            }

            // TP Player to that tile
            if (DataManager.instance.isMulti)
            {
                RpcTpToTile(tileToTP);
            }
            else
            {
                LocalTpToTile(tileToTP);
            }
        }
    }

    private void LocalTpToTile(GameObject tileToTP)
    {
        transform.position = tileToTP.transform.position;
        rb.velocity = Vector3.Scale(rb.velocity, new Vector3(1, 0, 1));
    }

    private void LocalEnvInteraction()
    {

        // Raycast to ground from in front of player
        float distanceFromPlayer = 1.5f;
        Ray ray = new Ray(transform.position + transform.forward * distanceFromPlayer + Vector3.up, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20, LayerMask.GetMask("Hexagon")))
        {
            Tile tile = hit.collider.transform.parent.gameObject.GetComponent<Tile>();
            if (tile)
            {
                if (!tile.isActive && tile.Enable)
                {
                    if (DataManager.instance.isMulti)
                    {
                        RpcSetTargetInteraction(hit.collider.transform.parent.gameObject);
                    }
                    else
                    {
                        LocalSetTargetInteraction(hit.collider.transform.parent.gameObject);
                    }
                    tile.Activate();

                    if (tile.Type != Tile.TileType.Empty && tile.Type != Tile.TileType.Wall && tile.Type != Tile.TileType.SafeZone)
                    {
                        totalTrapActivated++;
                    }
                }
            }
        }
    }

    private void LocalSetTargetInteraction(GameObject _tile)
    {
        _tile.GetComponent<Tile>().caster = gameObject;
    }

    //////////// PRIVATE NETWORK FUNCTIONS /////////////
    public override void OnStartLocalPlayer()
    {
        //Camera.main.GetComponent<CameraPath>().players[0] = gameObject;
        colorIndex = DataManager.instance.localPlayerIndex - 1;
        CmdSetColor(colorIndex);
        DataManager.instance.localPlayer = gameObject;
        CmdSetName(DataManager.instance.localPlayerIndex - 1);
        StartCoroutine(setName());

        CmdReady(DataManager.instance.localPlayerIndex);
    }

    [Command]
    private void CmdReady(int index)
    {
        if (GameManager.instance)
        {
            GameManager.instance.isPlayerInGame.Add(true);
        }
        else
        {
            Debug.Log("No gamemanager");
        }
        Debug.Log("Player in game : " + GameManager.instance.isPlayerInGame.Count + " | index : " + index);
    }

    //prevent player from being on top of hexagons
    [Command]
    private void CmdRecalage()
    {
        LocalRecalage();
    }

    [ClientRpc]
    private void RpcTpToTile(GameObject tileToTP)
    {
        LocalTpToTile(tileToTP);
    }

    [Command]
    private void CmdEnvInteraction()
    {
        LocalEnvInteraction();
    }

    [ClientRpc]
    private void RpcSetTargetInteraction(GameObject _tile)
    {
        LocalSetTargetInteraction(_tile);
    }

    //--------------------- Network Crowd Control ------------------------

    [ClientRpc]
    public void RpcStun(float _duration)
    {
        Stun(_duration);
    }

    [ClientRpc]
    public void RpcSlow(float _speedSlow, float _rotateSlow, float _duration, bool _showFX)
    {
        Slow(_speedSlow, _rotateSlow, _duration, _showFX);
    }

    [ClientRpc]
    public void RpcPanic(float _duration, GameObject _opponent)
    {
        opponent = _opponent.transform;
        Panic(_duration);
    }

    [ClientRpc]
    public void RpcAflame(float _duration)
    {
        Aflame(_duration);
    }
    //--------------------- Network Gifts Bools ------------------------

    [ClientRpc]
    public void RpcCanStun(bool _active)
    {
        canStun = _active;
    }

    [ClientRpc]
    public void RpcCanSlow(bool _active)
    {
        canSlow = _active;
    }

    [ClientRpc]
    public void RpcCanPanic(bool _active)
    {
        canPanic = _active;
    }

    [ClientRpc]
    public void RpcIsShielded(bool _active)
    {
        isShielded = _active;
    }
    //--------------------- Network Name Setting ------------------------

    IEnumerator setName()
    {
        ///Remplace with "allPlayerConnected" bool
        yield return new WaitUntil(() => GameManager.instance.gameInitialized);
        CmdSetName(DataManager.instance.localPlayerIndex - 1);
    }

    [Command]
    void CmdSetName(int _index)
    {
        playerName.text = DataManager.instance.PlayerNames[_index];
        RpcSetName(DataManager.instance.PlayerNames[_index]);
    }

    [ClientRpc]
    void RpcSetName(string _name)
    {
        playerName.text = _name;
    }

    //--------------------- Network Color Setting ------------------------

    [Command]
    void CmdSetColor(int _colorIndex)
    {
        RpcSetColor(_colorIndex);
    }

    [ClientRpc]
    void RpcSetColor(int _colorIndex)
    {
        SetColor(DataManager.instance.playerColor[_colorIndex]);
        GameObject quad = transform.Find("Quad").gameObject;
        Color c = DataManager.instance.playerColor[_colorIndex];
        c += Color.white * 0.2f;
        c.a = 0.5f;
        quad.GetComponent<MeshRenderer>().material.SetColor("_Color", c);
        name = "Challenger " + (_colorIndex + 1);
        colorIndex = _colorIndex;
    }

    //--------------------- Network Sounds Player ------------------------

    [Command]
    public void CmdPlaySound(int randomValue1, int soundNumber1, float soundVolume1, int randomValue2, int soundNumber2, float soundVolume2, int soundFunction, bool loop, AudioType type)
    {
        RpcPlaySound(randomValue1, soundNumber1, soundVolume1, randomValue2, soundNumber2, soundVolume2, soundFunction, loop, type);
    }

    [ClientRpc]
    void RpcPlaySound(int randomValue1, int soundNumber1, float soundVolume1, int randomValue2, int soundNumber2, float soundVolume2, int soundFunction, bool loop, AudioType type)
    {
        switch (soundFunction)
        {
            case 0:
                SoundCharacter(randomValue1, soundNumber1, soundVolume1, type);
                break;
            case 1:
                MultipleSoundCharacter(randomValue1, soundNumber1, soundVolume1, randomValue2, soundNumber2, soundVolume2, type);
                break;
            case 2:
                SoundManager.instance.PlaySound(soundNumber1, soundVolume1, loop, type);
                break;
        }
    }

}


