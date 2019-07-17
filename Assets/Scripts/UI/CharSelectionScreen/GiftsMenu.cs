using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

//[DefaultExecutionOrder(-1)]

public class GiftsMenu : MonoBehaviour
{
    [SerializeField]
    PlayerSelection playerSelection;

    [SerializeField]
    Text missingGift;
    bool displayWarning;
    Coroutine currentCoroutine;

    public GameObject[] gifts = new GameObject[12];

    GameObject informationPanel;

    public DataManager.TypeGift selectedGift;

    public int nbrGifts = 12;

    int playerIndex;


    // Use this for initialization
    private void Start()
    {
        playerIndex = playerSelection.playerIndex;

        informationPanel = playerSelection.informationPanel;
    }

    // Update is called once per frame
    void Update()
    {
        LobbyDisplay();

        GiftInformations();

        DisplayInformationPanel();

        SetAsSelected();


        

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(SetGiftPreset(DataManager.TypeGift.Heal, DataManager.TypeGift.Immune, DataManager.TypeGift.PowerShield));
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ResetGift();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutineShowWarning();
        }



        MissingGiftWarning();
    }


    void MissingGiftWarning()
    {
        missingGift.text = "Gifts Selected : " + playerSelection.giftCount + "/3";

        if (displayWarning)
        {
            missingGift.color = new Color(missingGift.color.r, missingGift.color.g, missingGift.color.b, 1.0f);
        }
        else
        {
            missingGift.color = new Color(missingGift.color.r, missingGift.color.g, missingGift.color.b, 0.0f);
        }
    }

    public void StartCoroutineShowWarning()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ShowWarning());
    }

    IEnumerator ShowWarning()
    {
        displayWarning = true;
        yield return new WaitForSeconds(1.5f);
        displayWarning = false;
    }


    void LobbyDisplay()
    {
        if (DataManager.instance.isMulti)
        {
            if (playerSelection.transform.GetSiblingIndex() != DataManager.instance.localPlayerIndex - 1)
            {
                transform.Find("Gifts").gameObject.SetActive(false);
                transform.Find("ReadyButton").GetComponent<Button>().enabled = false;
            }
            else
            {
                transform.Find("Gifts").gameObject.SetActive(true);
                transform.Find("ReadyButton").GetComponent<Button>().enabled = true;
            }
        }
        else
        {
            transform.Find("Gifts").gameObject.SetActive(true);
            transform.Find("ReadyButton").GetComponent<Button>().enabled = true;
        }
    }
    void DisplayInformationPanel()
    {
        if (DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
            if (playerSelection.isToolTipsToggled)
            {
                informationPanel.GetComponent<CanvasGroup>().alpha = 1.0f;
            }
            else
            {
                informationPanel.GetComponent<CanvasGroup>().alpha = 0.0f;
            }

            if (selectedGift > DataManager.TypeGift.none && selectedGift <= DataManager.TypeGift.VampShot)
            {
                for (int i = 1; i <= gifts.Length; i++)
                {
                    string iconName = "Gift " + i.ToString();
                    if (playerSelection.playerEventSystem.currentSelectedGameObject != null && playerSelection.playerEventSystem.currentSelectedGameObject.name == iconName)
                    {
                        informationPanel.transform.Find("Name").GetComponent<Text>().text = playerSelection.playerEventSystem.currentSelectedGameObject.GetComponent<GiftUI>().name;
                        informationPanel.transform.Find("Description").GetComponent<Text>().text = playerSelection.playerEventSystem.currentSelectedGameObject.GetComponent<GiftUI>().description;
                    }
                }
            }
        }
    }


    public void SetAsSelected()
    {
        for (int i = 0; i < gifts.Length; i++)
        {
            if (playerSelection.playerEventSystem.currentSelectedGameObject == gifts[i].gameObject)
            {
                selectedGift = (DataManager.TypeGift)i + 1;
            }
        }
    }


    public void ResetGift()
    {
        for (int i = 0; i < gifts.Length; i++)
        {
            if (gifts[i].GetComponent<GiftUI>().isSelected)
            {
                if (playerSelection.giftCount != 0)
                {
                    playerSelection.selectedGifts[gifts[i].GetComponent<GiftUI>().arrayPos - 1] = DataManager.TypeGift.none;
                    gifts[i].GetComponent<GiftUI>().arrayPos = 0;
                    gifts[i].GetComponent<GiftUI>().isSelected = false;
                    playerSelection.giftCount--;
                }
            }
        }
        playerSelection.giftsPreselected = false;
    }


    public IEnumerator SetGiftPreset(DataManager.TypeGift gift1, DataManager.TypeGift gift2, DataManager.TypeGift gift3)
    {
        yield return new WaitUntil(() => playerSelection.typeScreen == PlayerSelection.TypeScreen.screenGift);

        ResetGift();

        DataManager.TypeGift[] tempGiftArray = new DataManager.TypeGift[] { gift1, gift2, gift3 };

        for (int i = 0; i < playerSelection.selectedGifts.Length; i++)
        {
            playerSelection.selectedGifts[i] = tempGiftArray[i];

            gifts[(int)tempGiftArray[i] - 1].GetComponent<GiftUI>().arrayPos = i + 1;
            gifts[(int)tempGiftArray[i] - 1].GetComponent<GiftUI>().isSelected = true;
            playerSelection.giftCount++;
        }
        playerSelection.giftsPreselected = true;

        playerSelection.giftCount = 3;
    }


    //Gift information
    void GiftInformations()
    {
        gifts[0].GetComponent<GiftUI>().name = "Flash Heal";
        gifts[0].GetComponent<GiftUI>().description = "Instantly regenerate 100 hp, cannot exceed maximum life.";

        gifts[1].GetComponent<GiftUI>().name = "Bulwark";
        gifts[1].GetComponent<GiftUI>().description = "Provide a shield that completely absorb the next spell damage and crowd control effect.";

        gifts[2].GetComponent<GiftUI>().name = "Life Support";
        gifts[2].GetComponent<GiftUI>().description = "provide a shield that disappears after absorbing 150 damage or after 15 seconds.";

        gifts[3].GetComponent<GiftUI>().name = "Sprint";
        gifts[3].GetComponent<GiftUI>().description = "Increases movement speed by 70% for 15 seconds.";

        gifts[4].GetComponent<GiftUI>().name = "Strong Synapses";
        gifts[4].GetComponent<GiftUI>().description = "Instantly reduce all cooldown by 25%.";

        gifts[5].GetComponent<GiftUI>().name = "Berserk";
        gifts[5].GetComponent<GiftUI>().description = "Increases all damage dealt by 20% for 15 seconds.";

        gifts[6].GetComponent<GiftUI>().name = "Shocking Touch";
        gifts[6].GetComponent<GiftUI>().description = "Your next successful basic attack stun your opponent for 2 seconds.";

        gifts[7].GetComponent<GiftUI>().name = "Numbing Poison";
        gifts[7].GetComponent<GiftUI>().description = "Your next successful basic attack slow your opponent by 70% for 4 seconds.";

        gifts[8].GetComponent<GiftUI>().name = "Feared Weapon";
        gifts[8].GetComponent<GiftUI>().description = "Your next successful basic attack leave your opponent in panic, forcing him to flee in the opposite direction for 1.5 seconds.";

        gifts[9].GetComponent<GiftUI>().name = "Revitalization";
        gifts[9].GetComponent<GiftUI>().description = "Regenerate 250 hp over 25 seconds, cannot exceed maximum life.";

        gifts[10].GetComponent<GiftUI>().name = "Ignited Weapon";
        gifts[10].GetComponent<GiftUI>().description = "Fire infuse your basic attack for 40 seconds. Each successful basic attack will apply damage over time to your opponent";

        gifts[11].GetComponent<GiftUI>().name = "Vampiric Curse";
        gifts[11].GetComponent<GiftUI>().description = "For the next 40 seconds each succesful basic attack will heal you for 50% of the damage dealt.";
    }
}
