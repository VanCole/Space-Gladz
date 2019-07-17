using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerShield : Gift
{
    [SerializeField]
    GameObject pv_shield;

    GameObject pv_shield_instantiated;

    private void Start()
    {
        pv_shield_instantiated = pv_shield.GetComponent<GameObject>();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other != null && other.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                if (other.GetComponent<Player>().isPV_Shielded == true)
                {
                    other.GetComponentInChildren<PV_Shield>().newShield = true;
                    other.GetComponent<Player>().currentShieldPower = other.GetComponent<Player>().maxShieldPower;
                    IsTaken();
                    other.GetComponent<Player>().SoundCharacter(1, 61, 1.0f, AudioType.SFX);
                    Destroy(gameObject);
                }
                else
                {
                    other.GetComponent<Player>().isPV_Shielded = true;
                    other.GetComponent<Player>().currentShieldPower = other.GetComponent<Player>().maxShieldPower;
                    pv_shield_instantiated = Instantiate(pv_shield);
                    pv_shield_instantiated.transform.position = other.GetComponent<Player>().transform.position + Vector3.up;
                    pv_shield_instantiated.transform.parent = other.transform;
                    IsTaken();
                    other.GetComponent<Player>().SoundCharacter(1, 61, 1.0f, AudioType.SFX);
                    Destroy(gameObject);
                }
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
        name = "Life Shield";
        description = "Gives you a shield that disappears after receiving 150 damages (or after 15 seconds).";
    }
}
