using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class InformationPanel : MonoBehaviour
{
    [SerializeField]
    PlayerSelection playerSelection;

    SkillsMenu skillsMenu;
    GiftsMenu giftsMenu;

    [SerializeField]
    Image glowCircle;

    GameObject currentSelectedButton;
    DataManager dt;

    int indexPlayer;
    int indexSpell;
    int indexGift;


    // Use this for initialization
    void Start()
    {
        skillsMenu = playerSelection.classScreen[(int)PlayerSelection.TypeScreen.screenClass].GetComponent<SkillsMenu>();
        giftsMenu = playerSelection.classScreen[(int)PlayerSelection.TypeScreen.screenGift].GetComponent<GiftsMenu>();

        indexPlayer = playerSelection.playerIndex;
        dt = DataManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (dt.gameState == DataManager.GameState.charSelection)
        {
            indexPlayer = playerSelection.playerIndex;
            indexSpell = (int)skillsMenu.selectedButton;
            indexGift = (int)giftsMenu.selectedGift;
            currentSelectedButton = playerSelection.playerEventSystem.currentSelectedGameObject;

            StartCoroutine(GetDataFromIcon());

            ChangeGlowCircle();
        }
        else
        {
            transform.Find("Name").GetComponent<Text>().text = "";
            transform.Find("Description").GetComponent<Text>().text = "";
            transform.Find("IconPack/Icon").GetComponent<Image>().sprite = null;
            transform.Find("Cooldown").GetComponent<Text>().text = "";
            transform.Find("SpellType").GetComponent<Text>().text = "";
        }
    }


    void GetDataFromSpell()
    {
        transform.Find("IconPack/OuterCircle").gameObject.SetActive(true);
        transform.Find("IconPack/GlowCircle").gameObject.SetActive(true);

        transform.Find("IconPack/Background").gameObject.SetActive(false);
        transform.Find("IconPack/FrameClean").gameObject.SetActive(false);


        if (indexSpell < (int)SkillsMenu.TypeButton.passive)
        {
            transform.Find("Name").GetComponent<Text>().text = dt.player[indexPlayer].spellList[indexSpell].name;
            transform.Find("Description").GetComponent<Text>().text = dt.player[indexPlayer].spellList[indexSpell].description;
            transform.Find("IconPack/Icon").GetComponent<Image>().sprite = dt.player[indexPlayer].spellList[indexSpell].icon;
            transform.Find("Cooldown").GetComponent<Text>().text = CooldownToString(indexSpell);
            transform.Find("SpellType").GetComponent<Text>().text = dt.player[indexPlayer].spellList[indexSpell].type;
        }
        else
        {
            transform.Find("Name").GetComponent<Text>().text = dt.player[indexPlayer].passive.name;
            transform.Find("Description").GetComponent<Text>().text = dt.player[indexPlayer].passive.description;
            transform.Find("IconPack/Icon").GetComponent<Image>().sprite = dt.player[indexPlayer].passive.icon;
            transform.Find("Cooldown").GetComponent<Text>().text = "";
            transform.Find("SpellType").GetComponent<Text>().text = dt.player[indexPlayer].passive.type;
        }
    }

    void GetDataFromGift()
    {
        transform.Find("IconPack/OuterCircle").gameObject.SetActive(false);
        transform.Find("IconPack/GlowCircle").gameObject.SetActive(false);

        transform.Find("IconPack/Background").gameObject.SetActive(true);
        transform.Find("IconPack/FrameClean").gameObject.SetActive(true);

        if (indexGift > (int)DataManager.TypeGift.none && indexGift <= (int)DataManager.TypeGift.VampShot)
        {
            for (int i = 1; i < giftsMenu.nbrGifts + 1; i++)
            {
                string iconName = "Gift " + i.ToString();
                if (currentSelectedButton.name == iconName)
                {
                    transform.Find("Name").GetComponent<Text>().text = currentSelectedButton.GetComponent<GiftUI>().name;
                    transform.Find("Description").GetComponent<Text>().text = currentSelectedButton.GetComponent<GiftUI>().description;
                    transform.Find("IconPack/Icon").GetComponent<Image>().sprite = currentSelectedButton.transform.Find("Icon").GetComponent<Image>().sprite;
                    transform.Find("Cooldown").GetComponent<Text>().text = "";
                    transform.Find("SpellType").GetComponent<Text>().text = "";

                }
            }
        }
    }


    IEnumerator GetDataFromIcon()
    {
        if (DataManager.instance.player[indexPlayer] != null)
        {
            if (DataManager.instance.gameState == DataManager.GameState.charSelection)
            {
                yield return new WaitUntil(() => skillsMenu.hasChanged);
                switch (playerSelection.typeScreen)
                {
                    case PlayerSelection.TypeScreen.screenClass:
                        {
                            GetDataFromSpell();
                            break;
                        }
                    case PlayerSelection.TypeScreen.screenGift:
                        {
                            GetDataFromGift();
                            break;
                        }
                    default: break;
                }
            }
        }     
    }



    string CooldownToString(int idSpell)
    {
        string cdText;

        cdText = "Cooldown: " + DataManager.instance.player[indexPlayer].spellList[idSpell].cooldown.ToString() + " sec";
        return cdText;
    }


    void ChangeGlowCircle()
    {
        switch (indexPlayer)
        {
            case 0: //RED
                {
                    glowCircle.GetComponent<Image>().color = new Color(1.0f, 0.4f, 0.4f, 1.0f);
                    break;
                }
            case 1: //BLUE
                {
                    glowCircle.GetComponent<Image>().color = new Color(0.4f, 0.4f, 1.0f, 1.0f);
                    break;
                }
            case 2: //YELLOW
                {
                    glowCircle.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0.4f, 1.0f);
                    break;
                }
            case 3: // GREEN
                {
                    glowCircle.GetComponent<Image>().color = new Color(0.4f, 1.0f, 0.4f, 1.0f);
                    break;
                }
            default: break;
        }
    }
}


