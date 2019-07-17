using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeSteal : Gift {

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                other.GetComponent<Player>().LifeSteal(40.0f);
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
        name = "Vamp Shot";
        description = "Your auto-attacks heals you for the amount of damage inflicted (for 40 seconds).";
    }
}
