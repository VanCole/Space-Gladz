using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : Gift
{
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                if (other.GetComponent<Player>().currentHealth < other.GetComponent<Player>().healthMax)
                {
                    other.GetComponent<Player>().currentHealth += 100.0f;
                    other.GetComponent<Player>().DamageText(100, true);
                    Debug.Log(other.GetComponent<Player>().currentHealth);
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
        name = "Heal";
        description = "Heals you with 100 hp (does not exceed the maximum amount of life).";
    }
}
