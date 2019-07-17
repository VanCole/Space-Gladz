using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUINetwork : MonoBehaviour {

    [SerializeField]
    Image healthBar;

    [SerializeField]
    public Image[] imageGift = new Image[3];
    //public List<Image> imageGift = new List<Image>();

    Player player = null;

    [SerializeField]
    public Sprite[] spriteGift = new Sprite[10];

    bool[] afficheGift = new bool[10];
    int[] positionImageGift = new int[10];

    // Use this for initialization
    void Start ()
    {
        player = GetComponentInParent<Player>();

        if (!DataManager.instance.isMulti)
        {
            if(DataManager.instance.localPlayer == player.gameObject)
            {
               // Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject.transform.GetChild(2));
        }

        for(int i = 0; i < 10; i++)
        {
            afficheGift[i] = false;
            positionImageGift[i] = -1;
        }
    }
	
	// Update is called once per frame
	void Update () {
        // Billboard
        //transform.LookAt(Camera.main.transform, -Camera.main.transform.up);
        //transform.Rotate(Vector3.forward * 180);

        healthBar.fillAmount = player.currentHealth / player.healthMax;

        CanvasGift();
        if (!player.isAlive || DataManager.instance.isMatchDone)
        {
            gameObject.SetActive(false);
        }
    }

    /*Image GetEmptyImage()
    {
        Image image = null;

        for (int i = 0; i < 3; i++)
        {
            if (imageGift[i].sprite == null)
            {
                //imageGift[i].sprite = spriteGift[0];
                image = imageGift[i];
                break;
            }
        }

        return image;
    }*/

    void CanvasGift()
    {
        /*if (player.canFlame)
        {
            if (!afficheGift[0])
            {
                Image image = GetEmptyImage();
                if (image != null)
                {
                    image.sprite = spriteGift[0];
                }
            }
        }*/

        //afficheGift[0] = player.canFlame;

        // FLAME SHOT
        if (player.canFlame)
        {
            if (!afficheGift[0])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[0];
                        positionImageGift[0] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[0] = player.canFlame;

        if(afficheGift[0] == false && player.canFlame == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if(positionImageGift[0] != -1)
            {
                imageGift[positionImageGift[0]].sprite = null;
                imageGift[positionImageGift[0]].gameObject.SetActive(false);
                positionImageGift[0] = -1;
            }
        }
        
        //VAMP SHOT
        if (player.canVamp)
        {
            if (!afficheGift[1])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[1];
                        positionImageGift[1] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[1] = player.canVamp;
        
        if (afficheGift[1] == false && player.canVamp == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[1] != -1)
            {
                imageGift[positionImageGift[1]].sprite = null;
                imageGift[positionImageGift[1]].gameObject.SetActive(false);
                positionImageGift[1] = -1;
            }
        }
        
        //STUN SHOT
        if (player.canStun)
        {
            Debug.Log("E");
            if (!afficheGift[2])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[2];
                        positionImageGift[2] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[2] = player.canStun;

        if (afficheGift[2] == false && player.canStun == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[2] != -1)
            {
                imageGift[positionImageGift[2]].sprite = null;
                imageGift[positionImageGift[2]].gameObject.SetActive(false);
                positionImageGift[2] = -1;
            }
        }
        
        //SLOW SHOT
        if (player.canSlow)
        {
            if (!afficheGift[3])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[3];
                        positionImageGift[3] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[3] = player.canSlow;

        if (afficheGift[3] == false && player.canSlow == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[3] != -1)
            {
                imageGift[positionImageGift[3]].sprite = null;
                imageGift[positionImageGift[3]].gameObject.SetActive(false);
                positionImageGift[3] = -1;
            }
        }

        //PANIC SHOT
        if (player.canPanic)
        {
            if (!afficheGift[4])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[4];
                        positionImageGift[4] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[4] = player.canPanic;

        if (afficheGift[4] == false && player.canPanic == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[4] != -1)
            {
                imageGift[positionImageGift[4]].sprite = null;
                imageGift[positionImageGift[4]].gameObject.SetActive(false);
                positionImageGift[4] = -1;
            }
        }

        //PV SHIELD
        if (player.isPV_Shielded)
        {
            if (!afficheGift[5])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[5];
                        positionImageGift[5] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[5] = player.isPV_Shielded;

        if (afficheGift[5] == false && player.isPV_Shielded == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[5] != -1)
            {
                imageGift[positionImageGift[5]].sprite = null;
                imageGift[positionImageGift[5]].gameObject.SetActive(false);
                positionImageGift[5] = -1;
            }
        }

        //IMMUNE
        if (player.isShielded)
        {
            if (!afficheGift[6])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[6];
                        positionImageGift[6] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[6] = player.isShielded;

        if (afficheGift[6] == false && player.isShielded == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[6] != -1)
            {
                imageGift[positionImageGift[6]].sprite = null;
                imageGift[positionImageGift[6]].gameObject.SetActive(false);
                positionImageGift[6] = -1;
            }
        }

        //REGENERATION
        if (player.isHealing)
        {
            if (!afficheGift[7])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[7];
                        positionImageGift[7] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[7] = player.isHealing;

        if (afficheGift[7] == false && player.isHealing == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[7] != -1)
            {
                imageGift[positionImageGift[7]].sprite = null;
                imageGift[positionImageGift[7]].gameObject.SetActive(false);
                positionImageGift[7] = -1;
            }
        }

        //BONUS MOVE SPEED
        if (player.isBonusMoveSpeed)
        {
            if (!afficheGift[8])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[8];
                        positionImageGift[8] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[8] = player.isBonusMoveSpeed;

        if (afficheGift[8] == false && player.isBonusMoveSpeed == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[8] != -1)
            {
                imageGift[positionImageGift[8]].sprite = null;
                imageGift[positionImageGift[8]].gameObject.SetActive(false);
                positionImageGift[8] = -1;
            }
        }

        //BONUS MOVE SPEED
        if (player.isBoostDamaged)
        {
            if (!afficheGift[9])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (imageGift[i].sprite == null)
                    {
                        imageGift[i].gameObject.SetActive(true);
                        imageGift[i].sprite = spriteGift[9];
                        positionImageGift[9] = i;
                        break;
                    }
                }
            }
        }

        afficheGift[9] = player.isBoostDamaged;

        if (afficheGift[9] == false && player.isBoostDamaged == false && DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            if (positionImageGift[9] != -1)
            {
                imageGift[positionImageGift[9]].sprite = null;
                imageGift[positionImageGift[9]].gameObject.SetActive(false);
                positionImageGift[9] = -1;
            }
        }
    }
}
