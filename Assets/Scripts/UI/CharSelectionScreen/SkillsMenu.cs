using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class SkillsMenu : MonoBehaviour
{
    [SerializeField]
    PlayerSelection playerSelection;

    [SerializeField]
    GameObject[] spell = new GameObject[6];

    GameObject[] icon = new GameObject[6];
    Image[] glowCircle = new Image[6];


    int colorIndex;

    public enum TypeButton
    {
        attack,
        spell1,
        spell2,
        spell3,
        ultimate,
        passive
    }
    public TypeButton selectedButton;

    public bool hasChanged = false;

    int playerIndex;


    private void Start()
    {
        playerIndex = playerSelection.playerIndex;

        for (int i = 0; i < icon.Length; i++)
        {
            icon[i] = spell[i];
        }
    }

    private void Update()
    {
        DisplayIcon();

        SetAsSelected();

        ChangeGlowCircle();


        if (playerSelection.playerEventSystem.currentSelectedGameObject == null && playerSelection.typeScreen == PlayerSelection.TypeScreen.screenClass)
        {
            string path = "Canvas CharSelection/CharSelect " + (playerIndex + 1) + "/SkillsScreen/Skills/AutoAttack";
            playerSelection.playerEventSystem.SetSelectedGameObject(GameObject.Find(path));
        }
    }

   
    void DisplayIcon()
    {
        if (DataManager.instance.player[playerIndex] != null)
        {
            for (int i = 0; i < icon.Length - 1; i++)
            {
                icon[i].GetComponent<Image>().sprite = DataManager.instance.player[playerIndex].spellList[i].icon;
            }
            icon[5].GetComponent<Image>().sprite = DataManager.instance.player[playerIndex].passive.icon;
        }    
    }

    
    public void SetAsSelected()
    {
        for (int i = 0; i < icon.Length; i++)
        {
            if (playerSelection.playerEventSystem.currentSelectedGameObject == icon[i].gameObject)
            {
                selectedButton = (TypeButton)i;
            }
        }
    }

    void ChangeGlowCircle()
    {
        colorIndex = playerSelection.championModel.GetComponentInChildren<Player>().colorIndex;

        for (int i = 0; i < icon.Length; i++)
        {
            glowCircle[i] = icon[i].transform.Find("GlowCircle").gameObject.GetComponent<Image>();

            switch (colorIndex)
            {
                case 0: //RED
                    {
                        if (selectedButton == (TypeButton)i)
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                        }
                        else
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(0.4f, 0.0f, 0.0f, 1.0f);
                        }
                        break;
                    }
                case 1: //BLUE
                    {
                        if (selectedButton == (TypeButton)i)
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(0.0f, 0.92f, 1.0f, 1.0f); 
                        }
                        else
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(0.0f, 0.3f, 0.4f, 1.0f);
                        }

                        break;
                    }
                case 2: //YELLOW
                    {
                        if (selectedButton == (TypeButton)i)
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
                        }
                        else
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.0f, 1.0f);
                        }
                        break;
                    }
                case 3: // GREEN
                    {
                        if (selectedButton == (TypeButton)i)
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                        }
                        else
                        {
                            glowCircle[i].GetComponent<Image>().color = new Color(0.0f, 0.4f, 0.0f, 1.0f);
                        }
                        break;
                    }
                default: break;
            }
        }     
    }
}
