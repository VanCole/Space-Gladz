using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReducCooldown : Gift
{
    float reductionSpell = 0.0f;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                for (int i = 0; i < other.GetComponent<Player>().spellList.Count; i++)
                {
                    if (!other.GetComponent<Player>().spellList[i].IsOffCooldown)
                    {
                        reductionSpell = other.GetComponent<Player>().spellList[i].cooldown;
                        other.GetComponent<Player>().spellList[i].timer += reductionSpell;
                    }
                }

                if (!other.GetComponent<Player>().dodge.IsOffCooldown)
                {
                    reductionSpell = other.GetComponent<Player>().dodge.cooldown;
                    other.GetComponent<Player>().dodge.timer += reductionSpell;
                }

                IsTaken();
                other.GetComponent<Player>().SoundCharacter(1, 61, 1.0f, AudioType.SFX);
                Destroy(gameObject);
            }
            else
            {
                IsTaken();
                other.GetComponent<Player>().SoundCharacter(1, 62, 1.0f, AudioType.SFX);
                Destroy(gameObject);
            }
        }
    }

    [SerializeField]
    public Sprite sprite;

    void GiftSet()
    {
        icon = sprite;
        name = "Cooldown Reduction";
        description = "Reset all cooldown.";
    }
}
