/*
 * Last modification : 02/02/2018
 */
using System.Collections;
using UnityEngine;

public class Spell
{
    // callback that define behaviour of the spell
    public delegate int SpellBehaviour();
    SpellBehaviour callback;
    
    //Sprite
    public Sprite icon;
    public string name;
    public string description;
    public string type;


    //Cooldown
    public float cooldown;
    public float timer = 0.0f;

    //Condition spell can be casted
    private bool isOffCooldown = true;
    public bool IsOffCooldown { get { return isOffCooldown; } }

    // State of the spell
    private bool isActive = false;
    public bool IsActive { get { return isActive; } }
    
    // Create a spell with a given name and a cooldown
    public Spell(string _name, float _cooldown = 1.0f)
    {
        name = _name;
        cooldown = _cooldown;
    }

    public void SetCooldown(float _amount)
    {
        timer = _amount;
        isOffCooldown = (timer < cooldown) ? false : true;
    }

    // Coroutine that wait until cooldown time is reached
    public IEnumerator Cooldown()
    {
        // Lock the spell and run the behaviours
        isOffCooldown = false;
        timer = 0.0f;
        /*if (callback != null)
        {
            callback();
        }*/
        
        // Update cooldown and wait unitl it's reached
        yield return new WaitUntil(() => {
            timer += Time.deltaTime;
            return timer >= cooldown;
            });
        
        // Unlock the spell
        isOffCooldown = true;
    }

    // Call the callbacks attached to the spell and 
    public int Cast()
    {
        int damage = -1;
        //isActive = true;
        isOffCooldown = false;
        if (callback != null)
        {
            damage = callback();
        }
        return damage;
    }

    // Deactivate the spell
    public void Uncast()
    {
        //isActive = false;
        isOffCooldown = true;
    }

    // Add a behaviour when the spell is casted
    public Spell AddBehaviour(SpellBehaviour _behaviour)
    {
        callback += _behaviour;
        return this;
    }

    // Remove a behaviour when the spell is casted
    public Spell RemoveBehaviour(SpellBehaviour _behaviour)
    {
        callback -= _behaviour;
        return this;
    }
}

