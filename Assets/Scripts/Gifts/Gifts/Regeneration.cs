using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Regeneration : Gift {

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                other.GetComponent<Player>().Healing(25.0f);
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
        name = "Revitalization";
        description = "Regenerate 250 hp on 25 seconds (10 by second)(does not exceed the maximum amount of life).";
    }
}
