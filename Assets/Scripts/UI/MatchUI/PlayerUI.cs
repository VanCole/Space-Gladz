using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)] // executed after all other scripts
public class PlayerUI : MonoBehaviour
{
    [SerializeField] Player player;

    [SerializeField] GameObject[] spells;
    [SerializeField] GameObject dodge;
    [SerializeField] GameObject passive;

    [SerializeField] GameObject portrait;


    GameObject[] icons = new GameObject[7];
    Image[] glowCircle = new Image[7];

    [SerializeField] Image ultimate;

    [Header("Test")]
    [SerializeField]
    bool passiveActive = false;

    public int playerIndex;

    int colorIndex;


    // Use this for initialization
    IEnumerator Start()
    {
        // legacy 

        if (!DataManager.instance.isMulti)
        {
            yield return new WaitUntil(() => GameManager.instance.gameInitialized);
            player = DataManager.instance.player[playerIndex];


            //yield return new WaitForSeconds(0.1f);
            SetPlayer(player);
        }
    }


    public void SetPlayer(Player _player)
    {
        player = _player;
        for (int i = 0; i < spells.Length; i++)
        {
            spells[i].GetComponent<SpellCoolDown>().attachedSpell = player.spellList[i];
            spells[i].GetComponent<Image>().sprite = player.spellList[i].icon;
            icons[i] = spells[i];
        }
        // dodge
        dodge.GetComponent<SpellCoolDown>().attachedSpell = player.dodge;
        dodge.GetComponent<Image>().sprite = player.dodge.icon;
        icons[5] = dodge;

        // Passive
        passive.GetComponent<SpellCoolDown>().attachedSpell = player.passive;
        passive.GetComponent<Image>().sprite = player.passive.icon;
        icons[6] = passive;

        //Portrait
        portrait.transform.GetChild(0).GetComponent<Image>().sprite = player.portraitElements[0];
        portrait.transform.GetChild(1).GetComponent<Image>().sprite = player.portraitElements[2];
        portrait.transform.GetChild(2).GetComponent<Image>().sprite = player.portraitElements[1];

        portrait.transform.GetChild(2).GetComponent<Image>().color = new Color(portrait.transform.GetChild(2).GetComponent<Image>().color.r,
                                                                               portrait.transform.GetChild(2).GetComponent<Image>().color.g,
                                                                               portrait.transform.GetChild(2).GetComponent<Image>().color.b,
                                                                               0.20f);


        ChangeGlowCircle();
    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.isMulti && !player)
        {
            if (DataManager.instance.localPlayer)
            {
                // Change playerIndex to localIndex
                player = DataManager.instance.localPlayer.GetComponent<Player>();
                SetPlayer(player);
            }
        }

        if (player)
        {
            ultimate.fillAmount = player.currentPower / player.maxPower;
        }
    }



    void ChangeGlowCircle()
    {
        colorIndex = player.GetComponent<Player>().colorIndex;
        Color color = DataManager.instance.playerColor[colorIndex];

        for (int i = 0; i < glowCircle.Length; i++)
        {
            glowCircle[i] = icons[i].transform.Find("GlowCircle").gameObject.GetComponent<Image>();
            
            glowCircle[i].GetComponent<Image>().color = color;
            portrait.transform.GetChild(1).GetComponent<Image>().color = color;
        }
    }
}
