using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


public class PersonalStatPanel : MonoBehaviour
{

    [SerializeField]
    int playerIndex;

    [SerializeField]
    Text[] stats;


    enum TypeStat
    {
        dmgDealt,
        dmgTaken,
        giftGathered,
        giftStolen,
        activatedTrap,
        supporter,
        name
    }

    // Use this for initialization
    void Start()
    {
        //if (DataManager.instance.isMulti)
        //{
            GameObject.Find("ArenaEventGenerator").GetComponent<ArenaEventManager>().setStatsNames();
            //stats[(int)TypeStat.name].text = DataManager.instance.PlayerNames[playerIndex];

        //}
        //else
        //{
        //    stats[(int)TypeStat.name].text = "Player " + (playerIndex + 1).ToString();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        GetDataFromPlayer();

        SwitchFrameColor();
    }

    void SwitchFrameColor()
    {
        GameObject.Find(name + "/Panel/PanelColor").GetComponent<Image>().color = DataManager.instance.playerColor[playerIndex];
    }

    void GetDataFromPlayer()
    {
        stats[(int)TypeStat.dmgDealt].text = DataManager.instance.player[playerIndex].totalDmgDealt.ToString();
        stats[(int)TypeStat.dmgTaken].text = DataManager.instance.player[playerIndex].totalDmgTaken.ToString();
        stats[(int)TypeStat.giftGathered].text = DataManager.instance.player[playerIndex].totalGiftTaken.ToString();
        stats[(int)TypeStat.giftStolen].text = DataManager.instance.player[playerIndex].totalGiftStolen.ToString();
        stats[(int)TypeStat.activatedTrap].text = DataManager.instance.player[playerIndex].totalTrapActivated.ToString();

        stats[(int)TypeStat.supporter].text = Mathf.Round(SupporterValueCalculator()).ToString();

    }


    float SupporterValueCalculator()
    {
        float value = 0.0f;

        float dmgDealt = DataManager.instance.player[playerIndex].totalDmgDealt;
        float dmgTaken = DataManager.instance.player[playerIndex].totalDmgTaken;
        float giftGathered = DataManager.instance.player[playerIndex].totalGiftTaken;
        float giftStolen = DataManager.instance.player[playerIndex].totalGiftStolen;
        float activatedTrap = DataManager.instance.player[playerIndex].totalTrapActivated;




        //Player played his best
        value = ((dmgDealt * 10) - dmgTaken) + (125 * giftGathered) + (50 * giftStolen) + (250 * activatedTrap);


        //Player did nothing
        if (dmgDealt == 0.0f && dmgTaken >= 1000.0f)
        {
            return 0.0f;
        }

        //Player played but not well
        if (dmgDealt < dmgTaken)
        {
            float value2 = 0.0f;

            value2 = Mathf.Abs(dmgDealt - dmgTaken) + (125 * giftGathered) + (50 * giftStolen) + (250 * activatedTrap);

            return value2 * 1.75f;
        }

        return value * 2.25f;
    }

    public void setName(string _name)
    {
        stats[(int)TypeStat.name].text = _name;
    }

}
