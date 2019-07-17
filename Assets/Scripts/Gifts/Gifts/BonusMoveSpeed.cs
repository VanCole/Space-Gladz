using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusMoveSpeed : Gift
{
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                other.GetComponent<Player>().SpeedBoost(3.5f, 15.0f);
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
        name = "Sprint";
        description = "Increases your speed movement by 70% for 15 seconds.";
    }
}
