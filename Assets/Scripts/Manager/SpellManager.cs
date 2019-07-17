/*
 * Last modification : 14/02/2018
 */
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SpellManager : MonoBehaviour {

    // Singleton
    static public SpellManager instance;
    
    Dictionary<string, Spell> spells = new Dictionary<string, Spell>();
    
	void Start () {
		if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
	}

    // Cast the spell from spell name if it exists
    public int CastSpell(string _spellName, bool _autoCooldown = true)
    {
        int dealedDamaged = -1;
        Spell spell = GetSpellFromName(_spellName);
        if (spell != null)
        {
            if (spell.IsOffCooldown)
            {
                dealedDamaged = spell.Cast();
                if (_autoCooldown)
                {
                    StartCoroutine(spell.Cooldown());
                }
                else
                {
                    spell.SetCooldown(0.0f);
                }
            }
        }
        else
        {
            Debug.LogWarning("Spell \"" + _spellName + "\" doesn't exists.");
        }
        return dealedDamaged;
    }

    // Return the spell from a given name
    public  Spell GetSpellFromName(string _name)
    {
        Spell s = null;
        if(spells.ContainsKey(_name))
        {
            s = spells[_name];
        }
        return s;
    }

    // Add a spell in the list with the given name and cooldown
    public Spell AddSpell(string _name, float _cooldown = 1.0f)
    {
        Spell s = new Spell("", _cooldown);
        spells.Add(_name, s);
        return s;
    }
}
