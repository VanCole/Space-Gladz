using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gift : MonoBehaviour
{
    //Reference
    public GiftSpawner gs;

    //Gift index
    public int index;

    public int playerIndex;

    //Sprite
    public Sprite icon;
    public string name;
    public string description;

    public MeshRenderer contourBox;

    protected void IsTaken()
    {
        gs.tabOccuped[index] = false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (playerIndex == other.GetComponent<Player>().colorIndex)
            {
                other.GetComponent<Player>().totalGiftTaken++;
            }
            else
            {
                other.GetComponent<Player>().totalGiftStolen++;
            }
        }
    }

    private void Update()
    {
        DestroyOnPlayerDeath();
    }


    void DestroyOnPlayerDeath()
    {
        if (!DataManager.instance.player[playerIndex].isAlive)
        {
            IsTaken();
            Destroy(gameObject);
        }
    }
}
