using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Immune : Gift {

    [SerializeField]
    GameObject shield;

    GameObject shield_instantiated;

    private void Start()
    {
        shield_instantiated = shield.GetComponent<GameObject>();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                if(other.GetComponent<Player>().isShielded == false)
                {
                    other.GetComponent<Player>().isShielded = true;
                    shield_instantiated = Instantiate(shield);
                    shield_instantiated.transform.position = other.GetComponent<Player>().transform.position + Vector3.up;
                    shield_instantiated.transform.parent = other.transform;
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
        name = "Immune Shield";
        description = "Gives you a shield that immunize yourself against the next spell received (or an auto-attack with a crowd control).";
    }
}
