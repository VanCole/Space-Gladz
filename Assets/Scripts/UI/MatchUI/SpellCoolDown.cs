using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellCoolDown : MonoBehaviour {
    [SerializeField] Image activeImage;
    [SerializeField] Text timerText;
    Image spellImage;
    public Spell attachedSpell;

    public bool isActive = false;

	// Use this for initialization
	void Start () {
        activeImage.fillAmount = 0.0f;
        spellImage = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (attachedSpell != null)
        {
            if (!attachedSpell.IsOffCooldown)
            {
                activeImage.fillAmount = 1.0f - (attachedSpell.timer / attachedSpell.cooldown);
                spellImage.color = new Color(0.7f, 0.7f, 0.7f);
                if(timerText != null)
                {
                    float value = attachedSpell.cooldown - attachedSpell.timer;
                    timerText.text = value.ToString((int)value >= 1.0f ? "F0" : "F1");
                }
            }
            else
            {
                activeImage.fillAmount = 0.0f;
                spellImage.color = Color.white;
                if (timerText != null)
                {
                    timerText.text = "";
                }
            }
        }
    }


    public void SetCooldown(float _time)
    {
        //cooldown = _time;
        //elapsedTime = 0.0f;
        isActive = false;
        spellImage.color = new Color(0.7f, 0.7f, 0.7f);
    }
}
