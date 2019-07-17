using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assassin : Player
{

    //Abilities Damage Value
    public float currentAttackDamage;

    //Abilies CoolDown
    float attackCD = 1.0f;
    float spell1CD = 1.0f;
    float spell2CD = 1.0f;
    float spell3CD = 1.0f;
    float ultimateCD = 1.0f;



    // Use this for initialization
    override protected void Start()
    {
        base.Start();

        SpellList();
        AssassinSkillSet();
    }


    public void SpellList()
    {
        AddSpell("Attack", attackCD).AddBehaviour(AttackBehaviour);
        pc.spell1.AddListener(() =>
        {
            CastSpell("Attack");

        });

        AddSpell("Spell1", spell1CD).AddBehaviour(Spell1Behaviour);
        pc.spell1.AddListener(() =>
        {
            CastSpell("Spell1");

        });

        AddSpell("Spell2", spell2CD).AddBehaviour(Spell2Behaviour);
        pc.spell1.AddListener(() =>
        {
            CastSpell("Spell2");

        });

        AddSpell("Spell3", spell3CD).AddBehaviour(Spell3Behaviour);
        pc.spell1.AddListener(() =>
        {
            CastSpell("Spell3");

        });

        AddSpell("Ultimate", ultimateCD).AddBehaviour(UltimateBehaviour);
        pc.spell1.AddListener(() =>
        {
            CastSpell("Ultimate");

        });
    }


    public int AttackBehaviour()
    {
        Debug.Log("Assassin_Attack");
        return 0;
    }

    public int Spell1Behaviour()
    {
        Debug.Log("Assassin_Spell1");

        return 0;
    }
    public int Spell2Behaviour()
    {
        Debug.Log("Assassin_Spell2");

        return 0;
    }

    public int Spell3Behaviour()
    {
        Debug.Log("Assassin_Spell3");

        return 0;
    }

    public int UltimateBehaviour()
    {
        Debug.Log("Assassin_ultimate");

        return 0;
    }




    [SerializeField]
    Sprite[] spriteSpell = new Sprite[6];

    //Spell information
    void AssassinSkillSet()
    {
        GetSpell("Attack").icon = spriteSpell[0];
        GetSpell("Attack").name = "[PH]";
        GetSpell("Attack").description = "[PH]";

        GetSpell("Spell1").icon = spriteSpell[1];
        GetSpell("Spell1").name = "[PH]";
        GetSpell("Spell1").description = "[PH]";

        GetSpell("Spell2").icon = spriteSpell[2];
        GetSpell("Spell2").name = "[PH]";
        GetSpell("Spell2").description = "[PH]";

        GetSpell("Spell3").icon = spriteSpell[3];
        GetSpell("Spell3").name = "[PH]";
        GetSpell("Spell3").description = "[PH]";

        GetSpell("Ultimate").icon = spriteSpell[4];
        GetSpell("Ultimate").name = "[PH]";
        GetSpell("Ultimate").description = "[PH]";

        GetSpell("Passive").icon = spriteSpell[5];
        GetSpell("Passive").name = "[PH]";
        GetSpell("Passive").description = "[PH]";

    }

}
